using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.PlayerLoop;

public class BossRock : Bullet
{
    [Header("Options")]
    [SerializeField] private float speed; // 오브젝트가 움직이는 속도
    [SerializeField] private float rollingStartTime; // speed만큼 움직이기 시작하는 시간
    [SerializeField] private float scaleUpTime; // 커지는데 걸리는 시간
    [SerializeField] private GameManager gm;
    [SerializeField] private GameObject bossRockZone;

    private Rigidbody rigid;
    private float angularPower = 2f;
    private bool isFall;
    private float spawnTime;
    private float realSpawnTime;
    private bool isSpawnBossRock;
    private bool _returned;

    [SerializeField] private Transform player;
    [SerializeField] private Transform meshObj; //Mesh Object 연결 (자식)
    [SerializeField] private float fallSpeed;
    [SerializeField] private float targetY;

    public GameManager Gm => gm;
    public float TarGetY => targetY;

    private void OnEnable()
    {
        //_returned = false;
        //Debug.Log("_returned : " + _returned);
    }
    public void ReturnToPoolOnce()
    {
        if (_returned) return;
        _returned = true;

        // 필요하면 여기서 상태 정리
        StopAllCoroutines();
        if (bossRockZone != null)
            bossRockZone.SetActive(false);

        EnemyBulletObejctPool.Instance.ReturnBossRockPool(this);
    }

    private void Awake()
    {
        rigid = GetComponent<Rigidbody>();
        player = GameObject.FindGameObjectWithTag("Player").transform;
        realSpawnTime = spawnTime;
        //자식 Mesh Object 자동 찾기 (없으면 수동 연결)
        if (meshObj == null && transform.childCount > 0)
            meshObj = transform.GetChild(0);
        ReSetState();
    }

    public void ReSetState()
    {
        //_returned = false;
        transform.position = player.position + new Vector3(0f, targetY, 0f);
        isRock = true;
        isFall = false;
        realSpawnTime = 0f;
        StartCoroutine(GainPowerTimer());
    }

    public void StartBossRockZone()
    {
        if (bossRockZone == null || player == null) return;

        Vector3 targetPosition = player.position + new Vector3(0f, targetY - 3, 0f);
        bossRockZone.gameObject.SetActive(true);
        bossRockZone.transform.position = targetPosition;
    }

    public IEnumerator GainPowerTimer()
    {
        yield return new WaitForSeconds(scaleUpTime);
    }


    private void Update()
    {
        if(bossRockZone != null)
            StartBossRockZone();

        if (!isFall)
        {
            if (player != null)
                transform.position = player.position + new Vector3(0f, targetY, 0f);
        }
        else
        {
            var v = rigid.linearVelocity;
            v.y = Mathf.Max(v.y, -20f); // 최대 낙하 속도 10 제한
            rigid.linearVelocity = v;
        }

        //자식 Mesh만 계속 회전 (시각 효과)
        if (meshObj != null)
        {
            meshObj.Rotate(Vector3.right * angularPower * 50f * Time.deltaTime);
        }

        if (EnemyObjectPool.Instance.GetEnemyType(Enemy.Type.D).CurHealth == 0)
        {
            EnemyBulletObejctPool.Instance.ReturnBossRockPool(gameObject.GetComponent<BossRock>());
            //ReturnToPoolOnce();
            return;
        }

        realSpawnTime += Time.deltaTime;

        // 5초 시점에 카메라 회전 알림
        if (realSpawnTime >= 5f)
        {
            if (gm != null)
            {
                gm.SetCameraX(); // GameManager에서 Player 카메라를 45도로 바로 변경
            }
        }

        if (realSpawnTime >= 7f)
        {
            isFall = true;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Floor"))
        {
            Debug.Log("실행 BossRock");
            EnemyBulletObejctPool.Instance.ReturnBossRockPool(gameObject.GetComponent<BossRock>());
            //ReturnToPoolOnce();
        }
    }

    public void SetGameManager(GameManager gm)
    {
        this.gm = gm;
    }

    public void SetBossRockZone(GameObject bossRockZone)
    {
        this.bossRockZone = bossRockZone;
    }

    public void SetIsSpawnBossRock(bool isSpawnBossRock)
    {
        this.isSpawnBossRock = isSpawnBossRock;
    }

    public GameObject GetBossRockZone()
    {
        return bossRockZone;
    }
}