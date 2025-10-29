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

    public Transform player;
    [SerializeField] private Transform meshObj; //Mesh Object ���� (�ڽ�)

    private void Awake()
    {
        isRock = true;
        rigid = GetComponent<Rigidbody>();
        player = GameObject.FindGameObjectWithTag("Player").transform;
        //�ڽ� Mesh Object �ڵ� ã�� (������ ���� ����)
        if (meshObj == null && transform.childCount > 0)
            meshObj = transform.GetChild(0);
        //�÷��̾� �ٶ󺸱�
        if (player != null)
            transform.LookAt(player.position);
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
        rigid.linearVelocity = dir * speed; //�θ� �̵�
    }


    private void Update()
    {
        //�ڽ� Mesh�� ��� ȸ�� (�ð� ȿ��)
        if (meshObj != null)
        {
            meshObj.Rotate(Vector3.right * angularPower * 50f * Time.deltaTime);
        }
    }
}