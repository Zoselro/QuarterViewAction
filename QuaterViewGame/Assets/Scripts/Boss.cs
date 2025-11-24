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
    [SerializeField] private bool isLook; // 플레이어를 바라보는 플래그 변수
    
    private Vector3 lookVec; // 플레이어가 가는 방향을 미리 예측하는 벡터
    private Vector3 tauntVec; // 어디로 내려찍을 지 미리 에측하는 벡터

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
        
        if(nav.isOnNavMesh)
            nav.isStopped = true;
        
        DoActionTime();

        StartCoroutine(Think());
        transform.localScale = new Vector3(3f, 3f, 3f);
    }

    private void Start()
    {
        mainColider.enabled = false;
        curHealth = maxHealth;
        ResetBoss();
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
            // spawnTime 동안 보스가 아무것도 안하게끔 하는 코드 작성하기
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

    // 보스가 행동패턴을 결정해주는 코루틴
    private IEnumerator Think()
    {
        yield return new WaitForSeconds(0.1f);

        int randomAction = Random.Range(0, 5);

        switch (randomAction)
        {
            // 미사일 발사 패턴
            case 0:
            case 1:
                StartCoroutine(MissileShot());
                break;
            // 돌 굴러가는 패턴
            case 2:
            case 3:
                StartCoroutine(RockShot());
                break;
            // 점프 공격 패턴
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
            }
            else if (clip.name == "BigShot")
            {
                doBigShotTime = clip.length;
            }
            else if (clip.name == "Taunt")
            {
                tauntTime = clip.length;
            }
        }
    }

    private IEnumerator MissileShot()
    {
        animator.SetTrigger("doShot");
        
        // 첫 번째 미사일 발사
        yield return new WaitForSeconds(0.2f);
        //GameObject instantMissileA = Instantiate(missile, missilePortA.position, missilePortA.rotation);
        // 미사일 스크립트 접근하여, 그 미사일이 플레이어를 추적할 수 있도록 하는 코드
        //BossMissile bossMissileA = instantMissileA.GetComponent<BossMissile>();
        //bossMissileA.SetTarget(target);
        BossMissile instantMissileA = EnemyBulletObejctPool.Instance.GetBossBulletPool();
        instantMissileA.SetTarget(target);
        instantMissileA.transform.position = missilePortA.position;
        instantMissileA.transform.rotation = missilePortA.rotation;

        yield return new WaitForSeconds(0.3f);
        //GameObject instantMissileB = Instantiate(missile, missilePortB.position, missilePortB.rotation);
        // 미사일 스크립트 접근하여, 그 미사일이 플레이어를 추적할 수 있도록 하는 코드
        //BossMissile bossMissileB = instantMissileB.GetComponent<BossMissile>();
        //bossMissileB.SetTarget(target);

        BossMissile instantMissileB = EnemyBulletObejctPool.Instance.GetBossBulletPool();
        instantMissileB.SetTarget(target);
        instantMissileB.transform.position = missilePortB.position;
        instantMissileB.transform.rotation = missilePortB.rotation;

        //yield return new WaitForSeconds(2.5f);
        yield return new WaitForSeconds(doShotTime - 0.5f);

        StartCoroutine(Think());
    }

    private IEnumerator RockShot()
    {
        // 기 모을 때는 바라보기 중지
        isLook = false;
        animator.SetTrigger("doBigShot");

        //GameObject obj = Instantiate(bullet);
        BossRock obj = EnemyBulletObejctPool.Instance.GetBossRockPool();
        obj.transform.position = transform.position;
        obj.transform.rotation = transform.rotation;
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

    public void ResetBoss()
    {
        if (nav.isOnNavMesh)
            nav.isStopped = true;
        meleeArea.enabled = false;
        transform.localScale = new Vector3(3f, 3f, 3f);

        DoActionTime();
        StopAllCoroutines();
        StartCoroutine(Think());
    }
}
