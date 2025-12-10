using System.Collections;
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
    private int cntMissile = 0;

    private BossRock bossRock;
    public BossRock BossRock => bossRock;

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
    private void LateUpdate()
    {
        Vector3 rot = transform.rotation.eulerAngles;

        // Unity EulerAngles를 -180~180도로 변환
        if (rot.x > 180f) rot.x -= 360f;

        // X축이 -20보다 작으면 -20으로 고정
        if (rot.x < -20f)
            rot.x = -20f;

        // 다시 Quaternion 변환하여 적용
        transform.rotation = Quaternion.Euler(rot);
    }

    private void FixedUpdate()
    {
        rigid.linearVelocity = Vector3.zero;
        rigid.angularVelocity = Vector3.zero;
    }

    public void RockShotTest()
    {
        StartCoroutine(RockShot());
    }

    // 보스가 행동패턴을 결정해주는 코루틴
    private IEnumerator Think()
    {
        yield return new WaitForSeconds(0.1f);

        // Boss Rock, Missile, taunt 패턴 나오는 방식

        // 1.Missile 3번 발사 후 taunt 패턴 1회 발동
        // 2.Missile 6번 발사 후 Boss Rock 패턴 1회 발동
        // 이후 Missile 6번 발사 한 횟수 초기화.

        //Invoke("RockShotTest", 1f);

        if (cntMissile == 3 || cntMissile == 6 || cntMissile == 9)
            StartCoroutine(Taunt());
        else if (cntMissile == 12)
        {
            StartCoroutine(RockShot());
            cntMissile = -1;
        }
        else
            StartCoroutine(MissileShot());
        cntMissile++;
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

        // 미사일 스크립트 접근하여, 그 미사일이 플레이어를 추적할 수 있도록 하는 코드
        BossMissile instantMissileA = EnemyBulletObejctPool.Instance.GetBossBulletPool();
        instantMissileA.SetTarget(target);
        instantMissileA.transform.position = missilePortA.position;
        instantMissileA.transform.rotation = missilePortA.rotation;

        yield return new WaitForSeconds(0.3f);

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
        //doRockShot = true;
        manager.StartRockFlow();
        animator.SetTrigger("doBigShot");
        yield return new WaitForSeconds(2f);
        bossRock = EnemyBulletObejctPool.Instance.GetBossRockPool();
        bossRock.SetGameManager(manager);
        BossRock.SetBossRockZone(bossRock.Gm.GetBossRockZone());
        //obj.transform.position = transform.position;
        //obj.transform.rotation = transform.rotation;
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
        //doRockShot = false;
        transform.localScale = new Vector3(3f, 3f, 3f);
        DoActionTime();
        StopAllCoroutines();
        StartCoroutine(Think());
    }

    public void SetBossRock(BossRock bossRock) 
    {
        this.bossRock = bossRock;
    }
}
