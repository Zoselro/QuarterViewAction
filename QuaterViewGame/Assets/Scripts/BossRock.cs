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

    private void Awake()
    {
        isRock = true;
        rigid = GetComponent<Rigidbody>();
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
        while(!isShot)
         {
            angularPower += 0.02f;
            scaleValue += 0.005f;
            transform.localScale = Vector3.one * scaleValue;
            rigid.AddTorque(transform.right * angularPower, ForceMode.Acceleration);
            yield return null;
        }
    }
    public IEnumerator StartRolling()
    {
        yield return new WaitForSeconds(rollingStartTime);
        rigid.linearVelocity = new Vector3(0f, 0f, speed);
    }
}
