using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour
{
    [Header("Options")]
    [SerializeField] private int maxHealth;
    [SerializeField] private int curHealth;

    private Transform target; // ���� �� ������Ʈ 
    private bool isChase;
    private Material mat;
    private BoxCollider boxCollider;
    private Rigidbody rigid;
    private NavMeshAgent nav;
    private float checkDistance = 1f; // �ٴ� ���� �Ÿ� (�ʿ�� ����)
    private Animator animator;
    private void Awake()
    {
        rigid = GetComponent<Rigidbody>();
        boxCollider = GetComponent<BoxCollider>();
        mat = GetComponentInChildren<MeshRenderer>().material;
        nav = rigid.GetComponent<NavMeshAgent>();
        animator = GetComponentInChildren<Animator>();

        Invoke("ChaseStart", 2f);
    }
    private void Start()
    {   
        curHealth = maxHealth;
    }

    private void FixedUpdate()
    {
        FreezeVelocity();
    }

    public void ChaseStart()
    {
        isChase = true;
        animator.SetBool("isWalk", true);
        rigid.isKinematic = false;
    }

    private void Update()
    {
        Debug.DrawRay(transform.position, transform.forward * 5, Color.green);
        if(isChase)
            nav.SetDestination(target.position);
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
            // �ǰݽ� �ݵ��ϱ� ���� ����
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


    // �ǰݽ� �̺�Ʈ �Լ�
    IEnumerator OnDamage(Vector3 reactVector, bool isGrenade)
    {
        // �ǰ��� ������ �� �����ϱ�

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
