using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour
{
    public enum Type
    {
        A, B, C, D
    };

    [Header("Options")]
    [SerializeField] protected Type enemyType;
    [SerializeField] protected int maxHealth;
    [SerializeField] protected int curHealth;
    [SerializeField] protected float spawnTime;
    [SerializeField] protected int score;

    [Header("Components")]
    [SerializeField] protected BoxCollider meleeArea;
    [SerializeField] protected BoxCollider mainColider;
    [SerializeField] protected GameObject bullet;
    [SerializeField] protected Transform target; // 추적 할 오브젝트 
    [SerializeField] protected GameObject[] coins;
    [SerializeField] protected GameManager manager;

    protected MeshRenderer[] meshs;
    protected BoxCollider boxCollider;
    protected Rigidbody rigid;
    protected NavMeshAgent nav;
    protected Animator animator;
    protected float time = 0f;

    protected bool isChase; // 추적하고 있는가?
    protected bool isAttack; // 공격을 하고 있는가?
    protected bool isTime;
    protected bool isDead;
    protected bool isHpBar;

    public int CurHealth => curHealth;
    public int MaxHealth => maxHealth;
    public bool IsHpBar => isHpBar;
    private void Awake()
    {
        rigid = GetComponent<Rigidbody>();
        boxCollider = GetComponent<BoxCollider>();
        nav = rigid.GetComponent<NavMeshAgent>();
        animator = GetComponentInChildren<Animator>();
        meshs = GetComponentsInChildren<MeshRenderer>();

        if(enemyType != Type.D)
            Invoke("ChaseStart", spawnTime);
    }

    private void Start()
    {
        mainColider.enabled = false;
        curHealth = maxHealth;
    }
    private void FixedUpdate()
    { 
        time += Time.fixedDeltaTime;
        isTime = spawnTime <= time;
        if (isTime)
        {
            mainColider.enabled = true;
        }
        Targetting();
        if(!isDead)
            FreezeVelocity();
    }
    private void Update()
    {
        if (nav.enabled && enemyType != Type.D)
        {
            nav.SetDestination(target.position);
            nav.isStopped = !isChase;
        }
    }

    public void Targetting()
    {
        float targetRadius = 0f;
        float targetRange = 0f;
        if(!isDead && enemyType != Type.D)
        {
            switch (enemyType)
            {
                case Type.A:
                    targetRadius = 1.5f;
                    targetRange = 3f;
                    break;
                case Type.B:
                    targetRadius = 1f;
                    targetRange = 6f;
                    break;
                case Type.C:
                    targetRadius = 0.5f;
                    targetRange = 25f;
                    break;
            }

            RaycastHit[] raycastHits =
            Physics.SphereCastAll(transform.position,
                                    targetRadius, transform.forward, targetRange, LayerMask.GetMask("Player"));

            // 공격중이 아닌데, 범위 안에 플레이어가 타겟팅이 되었을 경우
            if (raycastHits.Length > 0 && !isAttack && isTime)
            {
                StartCoroutine(Attack());
            }
        }

    }

    private IEnumerator Attack()
    {
        isChase = false;
        isAttack = true;
        animator.SetBool("isAttack", true);

        switch (enemyType)
        {
            case Type.A:
                if (isDead)
                    break;
                yield return new WaitForSeconds(0.2f);
                meleeArea.enabled = true;

                if (meleeArea == null)
                    break;
                
                yield return new WaitForSeconds(1f);
                meleeArea.enabled = false; // 공격 범위
                yield return new WaitForSeconds(1f);
                break;
            case Type.B:
                if (isDead)
                    break;
                // 돌격 구현
                yield return new WaitForSeconds(0.1f);
                rigid.AddForce(transform.forward * 20, ForceMode.Impulse);
                meleeArea.enabled = true;

                yield return new WaitForSeconds(0.5f);
                rigid.linearVelocity = Vector3.zero;

                meleeArea.enabled = false;

                yield return new WaitForSeconds(2f);
                break;
            case Type.C:
                if (isDead)
                    break;
                yield return new WaitForSeconds(0.5f);
                Bullet enemyCBullet = EnemyBulletObejctPool.Instance.GetEnemyCBulletPool();
                enemyCBullet.transform.position = transform.position;
                enemyCBullet.transform.rotation = transform.rotation;
                Rigidbody rigidBullet = enemyCBullet.GetComponent<Rigidbody>();
                rigidBullet.linearVelocity = transform.forward * 20;
                
                yield return new WaitForSeconds(2f);
                break;
        }
        isChase = true;
        isAttack = false;
        animator.SetBool("isAttack", false);
    }


    public void ChaseStart()
    {
        isChase = true;
        animator.SetBool("isWalk", true);
    }


    public void FreezeVelocity()
    {
        //if (isChase)
        //{
            rigid.linearVelocity = Vector3.zero;
            rigid.angularVelocity = Vector3.zero;
        //}
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Melee")
        {
            Weapon weapon = other.GetComponent<Weapon>();
            curHealth -= weapon.GetDamage();
            // 피격시 반동하기 위한 벡터
            Vector3 reactVector = transform.position - other.transform.position;
            StartCoroutine(OnDamage(reactVector, false));
        }
        else if (other.tag == "Bullet")
        {
            Bullet bullet = other.GetComponent<Bullet>();

            if(bullet != null)
            {
                curHealth -= bullet.GetDamage();
                Vector3 reactVector = transform.position - other.transform.position;
                BulletObjectPool.ReturnBullet(bullet);
                StartCoroutine(OnDamage(reactVector, false));
            }

        }
    }

    public void HitByGrenade(Vector3 explosionPos)
    {
        curHealth -= 100;
        Vector3 reactVec = transform.position - explosionPos;
        StartCoroutine(OnDamage(reactVec, true));
    }


    // 피격시 이벤트 함수
    IEnumerator OnDamage(Vector3 reactVector, bool isGrenade)
    {
        // 피격을 당했을 때 색변하기
        foreach(MeshRenderer mesh in meshs)
        {
            mesh.material.color = Color.red;
        }

        yield return new WaitForSeconds(0.1f);
        
        if(curHealth > 0)
        {
            foreach(MeshRenderer mesh in meshs)
                mesh.material.color = Color.white;
        }
        else if(curHealth <= 0)
        {
            isDead = true;

            rigid.constraints = RigidbodyConstraints.FreezeRotationX |
                                RigidbodyConstraints.FreezeRotationY |
                                RigidbodyConstraints.FreezeRotationZ;
            
            foreach (MeshRenderer mesh in meshs)
                mesh.material.color = Color.gray;

            curHealth = 0;
            gameObject.layer = 12;
            isChase = false;
            nav.enabled = false;
            animator.SetTrigger("doDie");

            Player player = target.GetComponent<Player>();
            player.SetScore(player.Score + score);
            int ranCoin = Random.Range(0, 3);
            Instantiate(coins[ranCoin], transform.position, Quaternion.identity);

            switch (enemyType)
            {
                // 다른 방법 알아보기.
                case Type.A:
                    //manager.enemyCntA--;
                    int enemyCntA = manager.EnemyCntA;
                    manager.DecreaseEnemyCount(Type.A, --enemyCntA);
                    break;
                case Type.B:
                    int enemyCntB = manager.EnemyCntB;
                    manager.DecreaseEnemyCount(Type.B, --enemyCntB);
                    break;
                case Type.C:
                    int enemyCntC = manager.EnemyCntC;
                    manager.DecreaseEnemyCount(Type.C, --enemyCntC);
                    break;
                case Type.D:
                    int enemyCntD = manager.EnemyCntD;
                    manager.DecreaseEnemyCount(Type.D, --enemyCntD);
                    break;
            }


            if (isGrenade)
            {
                reactVector = reactVector.normalized;
                reactVector += Vector3.up * 3;

                rigid.freezeRotation = false;
                rigid.AddForce(reactVector * 5, ForceMode.Impulse);
                rigid.AddTorque(reactVector * 15, ForceMode.Impulse);
            }
            else
            {
                reactVector = reactVector.normalized;
                reactVector += Vector3.up;
                rigid.AddForce(reactVector * 5, ForceMode.Impulse);
            }
            rigid.freezeRotation = false;

            Invoke("DieAfterTime", 4f);
        }
    }

    private void DieAfterTime()
    {
        EnemyObjectPool.Instance.ReturnEnemy(this);
    }

    public void Initialize(Transform target, GameManager manager)
    {
        this.target = target;
        this.manager = manager;
    }

    public void SetIsHpBar(bool isHpBar)
    {
        this.isHpBar = isHpBar;
    }

    public Type GetEnemyType()
    {
        return enemyType;
    }

    public void ResetState()
    {
        time = 0;
        curHealth = maxHealth;

        foreach (MeshRenderer mesh in meshs)
            mesh.material.color = Color.white;
        
        gameObject.layer = 11;
        
        isChase = false;
        isAttack = false; // 공격을 하고 있는가?
        isTime = false;
        isDead = false;
        nav.enabled = true;

        rigid.constraints = RigidbodyConstraints.FreezePositionX |
                    RigidbodyConstraints.FreezePositionY |
                    RigidbodyConstraints.FreezePositionZ;

        mainColider.enabled = false;

        if (enemyType != Type.D)
            Invoke("ChaseStart", spawnTime);
    }   
}
