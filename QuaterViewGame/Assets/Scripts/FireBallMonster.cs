using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

public class FireBallMonster : BoombMonster
{
    //private void Awake()
    //{
    //    mainColider = GetComponentInChildren<SphereCollider>();
    //    rigid = GetComponent<Rigidbody>();
    //    nav = rigid.GetComponent<NavMeshAgent>();
    //    animator = GetComponentInChildren<Animator>();
    //    meshs = GetComponentsInChildren<SkinnedMeshRenderer>();

    //    if (enemyType != Type.D)
    //        Invoke("ChaseStart", spawnTime);
    //    AnimationGetTime(animator.runtimeAnimatorController.animationClips);
    //}

    private void Start()
    {
        transform.localScale = new Vector3(3f, 3f, 3f);
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

    public override void AnimationGetTime(AnimationClip[] clips)
    {
        foreach (AnimationClip clip in clips)
        {
            switch (clip.name)
            {
                case "Anim_Attack":
                    attackTime = clip.length;
                    break;
                case "Anim_Damage":
                    damageHitTime = clip.length;
                    break;
                case "Anim_Idle":
                    idleTime = clip.length;
                    break;
                case "Anim_Run":
                    walkTime = clip.length;
                    break;
                case "Anim_Death":
                    dieTime = clip.length;
                    break;
                default:
                    break;
            }
        }
    }

    protected override IEnumerator OnDamage(Vector3 reactVector, bool isGrenade, int damage)
    {
        // 피격을 당했을 때 색변하기
        foreach (SkinnedMeshRenderer mesh in meshs)
        {
            mesh.material.color = Color.red;
        }

        DamageText damageText = manager.GetDamageText().GetComponent<DamageText>();
        damageText.SetTarget(this.transform);
        damageText.print(damage.ToString());

        animator.SetTrigger("doDamage");

        yield return new WaitForSeconds(0.1f);

        if (curHealth > 0)
        {
            foreach (SkinnedMeshRenderer mesh in meshs)
                mesh.material.color = Color.white;
        }
        else if (curHealth <= 0)
        {
            isDead = true;
            rigid.constraints = RigidbodyConstraints.FreezeRotationX |
                                RigidbodyConstraints.FreezeRotationY |
                                RigidbodyConstraints.FreezeRotationZ;

            foreach (SkinnedMeshRenderer mesh in meshs)
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

            int fireBallEnemyCnt = manager.FireBallMonsterCnt;
            manager.DecreaseEnemyCount(Type.FireBallMonster, --fireBallEnemyCnt);
            Invoke("DieAfterTime", 4f);
        }
    }

    protected override IEnumerator Attack()
    {
        isChase = false;
        isAttack = true;

        if (isDead)
        {
            yield break;
        }

        yield return new WaitForSeconds(attackTime);

        animator.SetBool("isAttack", true);

        // 공격 구현

        meleeArea.enabled = true;
        yield return new WaitForSeconds(attackTime);

        if (meleeArea == null)
            yield break;

        meleeArea.enabled = false; // 공격 범위

        yield return null;

        isChase = true;
        isAttack = false;
        animator.SetBool("isAttack", false);
    }

    protected override void Targetting()
    {
        float targetRadius = 1.5f;
        float targetRange = 3f;

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
