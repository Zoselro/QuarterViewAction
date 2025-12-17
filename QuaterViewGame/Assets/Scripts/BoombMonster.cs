using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class BoombMonster : Enemy
{
    [SerializeField] private float targetRange;
    [SerializeField] private float targetRadius;
    [SerializeField] protected int damage;

    protected SkinnedMeshRenderer[] SkinnedMeshRenderers;

    protected float attackTime;
    protected float damageHitTime;
    protected float idleTime;
    protected float walkTime;
    protected float dieTime;

    private void Awake()
    {
        rigid = GetComponent<Rigidbody>();
        boxCollider = GetComponent<BoxCollider>();
        nav = rigid.GetComponent<NavMeshAgent>();
        animator = GetComponentInChildren<Animator>();
        SkinnedMeshRenderers = GetComponentsInChildren<SkinnedMeshRenderer>();
        if (enemyType != Type.D)
            Invoke("ChaseStart", spawnTime);

        AnimationGetTime(animator.runtimeAnimatorController.animationClips);
    }

    public virtual void AnimationGetTime(AnimationClip[] clips)
    {
        foreach (AnimationClip clip in clips)
        {
            switch (clip.name)
            {
                case "mon00_attack01":
                    attackTime = clip.length;
                    break;
                case "mon00_damage":
                    damageHitTime = clip.length;
                    break;
                case "mon00_idle":
                    idleTime = clip.length;
                    break;
                case "mon00_walk":
                    walkTime = clip.length;
                    break;
                case "mon00_Die":
                    dieTime = clip.length;
                    break;
                default:
                    break;
            }
        }
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

    IEnumerator Explosion()
    {
        // 오브젝트 주위에 있는 "Player" 레이어를 가진 오브젝트를 가져오는 메서드
        // Physics.SphereCastAll([내 위치], [범위], [방향], [레이어를 쏘는 길이], [가져올 레이어])
        
        yield return new WaitForSeconds(attackTime - 0.5f);

        RaycastHit[] raycastHits =
                Physics.SphereCastAll(transform.position,
                targetRadius, transform.forward, targetRange, LayerMask.GetMask("Player"));

        foreach (RaycastHit hitObj in raycastHits)
        {
            StartCoroutine(hitObj.transform.gameObject.GetComponent<Player>().OnHitDamage(damage));
        }

        yield return null;
    }

    protected override IEnumerator Attack()
    {
        isChase = false;
        isAttack = true;
        animator.SetBool("isAttack", true);

        StartCoroutine(Explosion());
        
        if (isDead)
        {
            yield break;
        }

        yield return new WaitForSeconds(2.2f);

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
        yield return new WaitForSeconds(0.1f);

        if (curHealth > 0)
        {
            animator.SetTrigger("doDamage");
        }
        else if (curHealth <= 0)
        {
            isDead = true;
            //manager.SetCameraX();
            rigid.constraints = RigidbodyConstraints.FreezeRotationX |
                                RigidbodyConstraints.FreezeRotationY |
                                RigidbodyConstraints.FreezeRotationZ;

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

            int boombEnemyCnt = manager.BoombEnemyCnt;
            manager.DecreaseEnemyCount(Type.BoombMonster, --boombEnemyCnt);
            Invoke("DieAfterTime", 4f);
        }
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

