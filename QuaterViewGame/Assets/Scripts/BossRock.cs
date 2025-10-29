using System.Collections;
using UnityEngine;

public class BossRock : Bullet
{
    [Header("Options")]
    [SerializeField] private float speed; // 오브젝트가 움직이는 속도
    [SerializeField] private float rollingStartTime; // speed만큼 움직이기 시작하는 시간
    [SerializeField] private float scaleUpTime; // 커지는데 걸리는 시간

    private Rigidbody rigid;
    private float angularPower = 2f;
    private float scaleValue = 0.1f;
    private bool isShot;

    public Transform player;
    [SerializeField] private Transform meshObj; //Mesh Object 연결 (자식)

    private void Awake()
    {
        isRock = true;
        rigid = GetComponent<Rigidbody>();
        player = GameObject.FindGameObjectWithTag("Player").transform;
        //자식 Mesh Object 자동 찾기 (없으면 수동 연결)
        if (meshObj == null && transform.childCount > 0)
            meshObj = transform.GetChild(0);
        //플레이어 바라보기
        if (player != null)
            transform.LookAt(player.position);
        StartCoroutine(GainPowerTimer());
        StartCoroutine(GainPower()); // scaleUpTime 후 Scale이 점점 커지면서 회전하도록 설정.
        StartCoroutine(StartRolling()); // rollingStartTime 뒤에 움직이게 설정
    }

    public IEnumerator GainPowerTimer()
    {
        yield return new WaitForSeconds(scaleUpTime);
        isShot = true;
    }

    public IEnumerator GainPower()
    {
        while (!isShot)
        {
            angularPower += 0.02f;
            scaleValue += 0.005f;
            //transform.localScale = Vector3.one * scaleValue;
            //rigid.AddTorque(transform.right * angularPower, ForceMode.Acceleration);
            if (meshObj != null)
                meshObj.localScale = Vector3.one * scaleValue;
            yield return null;
        }
    }
    public IEnumerator StartRolling()
    {
        yield return new WaitForSeconds(rollingStartTime);
        //rigid.linearVelocity = new Vector3(0f, 0f, speed);
        Vector3 dir = (player.position - transform.position).normalized;
        rigid.linearVelocity = dir * speed; //부모 이동
    }


    private void Update()
    {
        //자식 Mesh만 계속 회전 (시각 효과)
        if (meshObj != null)
        {
            meshObj.Rotate(Vector3.right * angularPower * 50f * Time.deltaTime);
        }
    }
}