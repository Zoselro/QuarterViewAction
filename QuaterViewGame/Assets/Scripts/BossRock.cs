using System.Collections;
using UnityEngine;

public class BossRock : Bullet
{
    [Header("Options")]
    [SerializeField] private float speed; // 오브젝트가 움직이는 속도
    [SerializeField] private float rollingStartTime; // speed만큼 움직이기 시작하는 시간
    [SerializeField] private float scaleUpTime; // 커지는데 걸리는 시간
    [SerializeField] private GameManager gm;

    private Rigidbody rigid;
    private float angularPower = 2f;
    private bool isFall;
    private float spawnTime;
    private float realSpawnTime;

    [SerializeField] private Transform player;
    [SerializeField] private Transform meshObj; //Mesh Object 연결 (자식)
    [SerializeField] private float fallSpeed;
    [SerializeField] private float targetY;

    public float TarGetY => targetY;
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
        transform.position = player.position + new Vector3(0f, targetY, 0f);
        isRock = true;
        isFall = false;
        realSpawnTime = 0f;
        StartCoroutine(GainPowerTimer());
    }

    public IEnumerator GainPowerTimer()
    {
        yield return new WaitForSeconds(scaleUpTime);
    }


    private void Update()
    {
        if (!isFall)
        {
            if (player != null)
                transform.position = player.position + new Vector3(0f, targetY, 0f);
        }
        else
        {
            transform.position += Vector3.down * fallSpeed * Time.deltaTime;
        }

        //자식 Mesh만 계속 회전 (시각 효과)
        if (meshObj != null)
        {
            meshObj.Rotate(Vector3.right * angularPower * 50f * Time.deltaTime);
        }

        if (EnemyObjectPool.Instance.GetEnemyType(Enemy.Type.D).CurHealth == 0)
        {
            EnemyBulletObejctPool.Instance.ReturnBossRockPool(gameObject.GetComponent<BossRock>());
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
        }
    }

    public void SetGameManager(GameManager gm)
    {
        this.gm = gm;
    }
}