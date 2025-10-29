using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour
{
    public enum Type
    {
        A, B, C, D
    };

    [Header("Options")]
    [SerializeField] private Type enemyType;
    [SerializeField] private int maxHealth;
    [SerializeField] private int curHealth;
    [SerializeField] private float spawnTime;

    [Header("Components")]
    [SerializeField] protected BoxCollider meleeArea;
    [SerializeField] private BoxCollider mainColider;
    [SerializeField] protected GameObject bullet;

    protected MeshRenderer[] meshs;
    public Transform target; // 추적 할 오브젝트 
    protected BoxCollider boxCollider;
    protected Rigidbody rigid;
    protected NavMeshAgent nav;
    protected Animator animator;
    protected float time = 0f;

    private bool isChase; // 추적하고 있는가?
    private bool isAttack; // 공격을 하고 있는가?
    private bool isTime;
    protected bool isDead;
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
                GameObject instanceBullet = Instantiate(bullet, transform.position, transform.rotation);
                Rigidbody rigidBullet = instanceBullet.GetComponent<Rigidbody>();
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
        if (isChase)
        {
            rigid.linearVelocity = Vector3.zero;
            rigid.angularVelocity = Vector3.zero;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.tag != "Wall")
        {
            rigid.constraints = RigidbodyConstraints.FreezePositionX |
                                RigidbodyConstraints.FreezePositionY |
                                RigidbodyConstraints.FreezePositionZ |
                                RigidbodyConstraints.FreezeRotationX |
                                RigidbodyConstraints.FreezeRotationY |
                                RigidbodyConstraints.FreezeRotationZ;
        }
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
            curHealth -= bullet.GetDamage();
            Vector3 reactVector = transform.position - other.transform.position;
            Destroy(other.gameObject);
            StartCoroutine(OnDamage(reactVector, false));
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
            //mat.color = Color.red;
        }

        yield return new WaitForSeconds(0.1f);
        //rigid.constraints = RigidbodyConstraints.None;
        
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
            if(enemyType != Type.D)
                Destroy(gameObject, 4f);
        }
    }

    public void Initialize(Transform target)
    {
        this.target = target;
    }
}
