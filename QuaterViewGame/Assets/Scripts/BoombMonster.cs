using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class BoombMonster : Enemy
{
    [SerializeField] private float targetRange;
    [SerializeField] private float targetRadius;

    private SkinnedMeshRenderer[] SkinnedMeshRenderers;

    private void Awake()
    {
        rigid = GetComponent<Rigidbody>();
        boxCollider = GetComponent<BoxCollider>();
        nav = rigid.GetComponent<NavMeshAgent>();
        animator = GetComponentInChildren<Animator>();
        SkinnedMeshRenderers = GetComponentsInChildren<SkinnedMeshRenderer>();
        if (enemyType != Type.D)
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
        if (!isDead)
            FreezeVelocity();
    }

    private void Update()
    {
        if (nav == null || !nav.enabled || !nav.isOnNavMesh) return;
        if (target == null) return;

        if (nav.enabled && enemyType != Type.D)
        {
            nav.SetDestination(target.position);
            nav.isStopped = !isChase;
        }
    }


    protected override IEnumerator Attack()
    {
        isChase = false;
        isAttack = true;
        animator.SetBool("isAttack", true);

        if (isDead)
        {
            yield break;
        }
        Debug.Log("Attack 실행1");
        yield return new WaitForSeconds(2.2f);
        Debug.Log("Attack 실행2");

        isChase = true;
        isAttack = false;
        animator.SetBool("isAttack", false);
    }

    protected override void Targetting()
    {
        RaycastHit[] raycastHits =
                        Physics.SphereCastAll(transform.position,
                        targetRadius, transform.forward, targetRange, LayerMask.GetMask("Player"));

        // 공격중이 아닌데, 범위 안에 플레이어가 타겟팅이 되었을 경우
        if (raycastHits.Length > 0 && !isAttack && isTime)
        {
            StartCoroutine(Attack());
        }
        //base.Targetting();
    }

    protected override IEnumerator OnDamage(Vector3 reactVector, bool isGrenade)
    {
        foreach (SkinnedMeshRenderer mesh in SkinnedMeshRenderers)
        {
            foreach(Material material in mesh.materials)
            {
                material.color = Color.red;
            }
            //mesh.material.color = Color.red;
        }

        yield return new WaitForSeconds(0.1f);

        if (curHealth > 0)
        {
            animator.SetTrigger("doDamage");
            foreach (SkinnedMeshRenderer mesh in SkinnedMeshRenderers)
                mesh.material.color = Color.white;
        }
        else if (curHealth <= 0)
        {
            isDead = true;
            manager.SetCameraX();
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
            ranCoin = Random.Range(0, 3);
            GameObject coin = ItemObjectPool.GetCoin(ranCoin);
            coin.transform.position = transform.position;
            coin.transform.rotation = Quaternion.identity;

            Item coinItem = coin.GetComponent<Item>();
            if (coinItem != null)
            {
                coinItem.SetCoinIndexPool(ranCoin);
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

    public override void ResetState()
    {
        time = 0;
        curHealth = maxHealth;

        foreach (SkinnedMeshRenderer mesh in SkinnedMeshRenderers)
            mesh.material.color = Color.white;

        gameObject.layer = 11;

        isChase = false;
        isAttack = false; // 공격을 하고 있는가?
        isTime = false;
        isDead = false;
        nav.enabled = true;

        rigid.constraints = RigidbodyConstraints.FreezeAll;

        mainColider.enabled = false;

        if (enemyType != Type.D)
            Invoke("ChaseStart", spawnTime);
    }
}

