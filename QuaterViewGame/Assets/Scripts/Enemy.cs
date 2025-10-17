using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour
{
    public enum Type
    {
        A, B, C
    };

    [Header("Options")]
    [SerializeField] private Type enemyType;
    [SerializeField] private int maxHealth;
    [SerializeField] private int curHealth;
    [SerializeField] private float spawnTime;

    [Header("Components")]
    [SerializeField] private BoxCollider meleeArea;
    [SerializeField] private BoxCollider mainColider;

    private Transform target; // 추적 할 오브젝트 
    private bool isChase; // 추적하고 있는가?
    private bool isAttack; // 공격을 하고 있는가?
    private Material mat;
    private BoxCollider boxCollider;
    private Rigidbody rigid;
    private NavMeshAgent nav;
    private float checkDistance = 1f; // 바닥 감지 거리 (필요시 조절)
    private Animator animator;
    private float time = 0f;
    private bool isTime;
    private void Awake()
    {
        rigid = GetComponent<Rigidbody>();
        boxCollider = GetComponent<BoxCollider>();
        mat = GetComponentInChildren<MeshRenderer>().material;
        nav = rigid.GetComponent<NavMeshAgent>();
        animator = GetComponentInChildren<Animator>();
        Invoke("ChaseStart", spawnTime);
    }
    private void Start()
    {
        mainColider.enabled = false;

        curHealth = maxHealth;
        rigid.isKinematic = true;
    }
    private void FixedUpdate()
    {
        time += Time.fixedDeltaTime;
        isTime = spawnTime <= time;
        if (isTime)
        {
            Debug.Log("2초");
            mainColider.enabled = true;
        }
        FreezeVelocity();
        Targetting();
    }

    public void Targetting()
    {
        float targetRadius = 0f;
        float targetRange = 0f;

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

    private IEnumerator Attack()
    {
        isChase = false;
        isAttack = true;
        animator.SetBool("isAttack", true);

        switch (enemyType)
        {
            case Type.A:
                yield return new WaitForSeconds(0.2f);
                meleeArea.enabled = true;
                //rigid.isKinematic = true;

                yield return new WaitForSeconds(1f);
                meleeArea.enabled = false; // 공격 범위

                yield return new WaitForSeconds(1f);

                //rigid.isKinematic = false;
                break;
            case Type.B:
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
        //rigid.isKinematic = false;
    }

    private void Update()
    {
        Debug.DrawRay(transform.position, transform.forward * 5, Color.green);
        if (nav.enabled)
        {
            nav.SetDestination(target.position);
            nav.isStopped = !isChase;
        }
    }

    public void FreezeVelocity()
    {
        if (isChase)
        {
            rigid.linearVelocity = Vector3.zero;
            rigid.angularVelocity = Vector3.zero;
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

        mat.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        
        if(curHealth > 0)
        {
            mat.color = Color.white;
        }
        else if(curHealth <= 0)
        {
            mat.color = Color.gray;
            curHealth = 0;
            gameObject.layer = 12;
            isChase = false;
            nav.enabled = false;
            animator.SetTrigger("doDie");

            rigid.isKinematic = false;

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
            Destroy(gameObject, 4f);
        }
    }

    public void Initialize(Transform target)
    {
        this.target = target;
    }
}
