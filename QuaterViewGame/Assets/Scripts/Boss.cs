using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class Boss : Enemy
{
    [Header("Object")]
    [SerializeField] GameObject missile;
    [SerializeField] Transform missilePortA;
    [SerializeField] Transform missilePortB;

    [Header("Options")]
    [SerializeField] private bool isLook; // �÷��̾ �ٶ󺸴� �÷��� ����
    
    private Vector3 lookVec; // �÷��̾ ���� ������ �̸� �����ϴ� ����
    private Vector3 tauntVec; // ���� �������� �� �̸� �����ϴ� ����

    private float doShotTime;
    private float doBigShotTime;
    private float tauntTime;

    private void Awake()
    {
        rigid = GetComponent<Rigidbody>();
        boxCollider = GetComponent<BoxCollider>();
        nav = rigid.GetComponent<NavMeshAgent>();
        animator = GetComponentInChildren<Animator>();
        meshs = GetComponentsInChildren<MeshRenderer>();

        nav.isStopped = true;
        DoActionTime();

        StartCoroutine(Think());
        transform.localScale = new Vector3(3f, 3f, 3f);
    }

    private void Start()
    {
        mainColider.enabled = false;
        curHealth = maxHealth;
    }

    private void Update()
    {
        time += Time.deltaTime;
        isTime = spawnTime <= time;
        if (isTime)
        {
            mainColider.enabled = true;
        }
        else
        {
            // spawnTime ���� ������ �ƹ��͵� ���ϰԲ� �ϴ� �ڵ� �ۼ��ϱ�
            //StopAllCoroutines();
            return;
        }

        if (isDead)
        {
            StopAllCoroutines();
            return;
        }

        if (isLook)
        {
            float h = Input.GetAxisRaw("Horizontal");
            float v = Input.GetAxisRaw("Vertical");
            lookVec = new Vector3(h, 0, v) * 5f;

            transform.LookAt(target.position + lookVec);
        }
        else
        {
            nav.SetDestination(tauntVec);
        }
    }

    private void FixedUpdate()
    {
        rigid.linearVelocity = Vector3.zero;
        rigid.angularVelocity = Vector3.zero;
    }

    // ������ �ൿ������ �������ִ� �ڷ�ƾ
    private IEnumerator Think()
    {
        yield return new WaitForSeconds(0.1f);

        int randomAction = Random.Range(0, 5);

        switch (randomAction)
        {
            // �̻��� �߻� ����
            case 0:
            case 1:
                StartCoroutine(MissileShot());
                break;
            // �� �������� ����
            case 2:
            case 3:
                StartCoroutine(RockShot());
                break;
            // ���� ���� ����
            case 4:
                StartCoroutine(Taunt());
                break;
        }
    }

    public void DoActionTime()
    {
        AnimationClip[] clips = animator.runtimeAnimatorController.animationClips;

        foreach (AnimationClip clip in clips)
        {
            if (clip.name == "Shot")
            {
                doShotTime = clip.length;
                Debug.Log("doShotTime : " + doShotTime);
            }
            else if (clip.name == "BigShot")
            {
                doBigShotTime = clip.length;
                Debug.Log("doBigShotTime : " + doBigShotTime);
            }
            else if (clip.name == "Taunt")
            {
                tauntTime = clip.length;
                Debug.Log("tauntTime : " + tauntTime);
            }
        }
    }

    private IEnumerator MissileShot()
    {
        animator.SetTrigger("doShot");
        
        // ù ��° �̻��� �߻�
        yield return new WaitForSeconds(0.2f);
        GameObject instantMissileA = Instantiate(missile, missilePortA.position, missilePortA.rotation);
        // �̻��� ��ũ��Ʈ �����Ͽ�, �� �̻����� �÷��̾ ������ �� �ֵ��� �ϴ� �ڵ�
        BossMissile bossMissileA = instantMissileA.GetComponent<BossMissile>();
        bossMissileA.SetTarget(target);

        yield return new WaitForSeconds(0.3f);
        GameObject instantMissileB = Instantiate(missile, missilePortB.position, missilePortB.rotation);
        // �̻��� ��ũ��Ʈ �����Ͽ�, �� �̻����� �÷��̾ ������ �� �ֵ��� �ϴ� �ڵ�
        BossMissile bossMissileB = instantMissileB.GetComponent<BossMissile>();
        bossMissileB.SetTarget(target);

        //yield return new WaitForSeconds(2.5f);
        yield return new WaitForSeconds(doShotTime - 0.5f);

        StartCoroutine(Think());
    }

    private IEnumerator RockShot()
    {
        // �� ���� ���� �ٶ󺸱� ����
        isLook = false;
        animator.SetTrigger("doBigShot");

        GameObject obj = Instantiate(bullet);
        obj.transform.position = transform.position;
        obj.transform.rotation = transform.rotation;
        //yield return new WaitForSeconds(3f);
        yield return new WaitForSeconds(doBigShotTime);
        isLook = true;

        StartCoroutine(Think());
    }

    private IEnumerator Taunt()
    {
        tauntVec = target.position + lookVec;

        isLook = false;
        nav.isStopped = false;
        boxCollider.enabled = false;
        animator.SetTrigger("doTaunt");

        yield return new WaitForSeconds(tauntTime / 2f);
        meleeArea.enabled = true;

        yield return new WaitForSeconds(0.5f);
        meleeArea.enabled = false;

        //yield return new WaitForSeconds(3f);
        yield return new WaitForSeconds(tauntTime - ((tauntTime / 2f) + 0.5f));
        isLook = true;
        nav.isStopped = true;
        boxCollider.enabled = true;
        StartCoroutine(Think());
    }
}
