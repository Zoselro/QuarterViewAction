using System.Collections;
using UnityEngine;

public class BossRock : Bullet
{
    [Header("Options")]
    [SerializeField] private float speed; // ������Ʈ�� �����̴� �ӵ�
    [SerializeField] private float rollingStartTime; // speed��ŭ �����̱� �����ϴ� �ð�
    [SerializeField] private float scaleUpTime; // Ŀ���µ� �ɸ��� �ð�

    private Rigidbody rigid;
    private float angularPower = 2f;
    private float scaleValue = 0.1f;
    private bool isShot;

    private void Awake()
    {
        isRock = true;
        rigid = GetComponent<Rigidbody>();
        StartCoroutine(GainPowerTimer());
        StartCoroutine(GainPower()); // scaleUpTime �� Scale�� ���� Ŀ���鼭 ȸ���ϵ��� ����.
        StartCoroutine(StartRolling()); // rollingStartTime �ڿ� �����̰� ����
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
