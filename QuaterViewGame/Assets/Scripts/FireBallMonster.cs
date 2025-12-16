using System;
using UnityEngine;
using UnityEngine.AI;

public class FireBallMonster : BoombMonster
{
    private void Awake()
    {
        mainColider = GetComponentInChildren<SphereCollider>();
        rigid = GetComponent<Rigidbody>();
        nav = rigid.GetComponent<NavMeshAgent>();
        animator = GetComponentInChildren<Animator>();
        SkinnedMeshRenderers = GetComponentsInChildren<SkinnedMeshRenderer>();

        if (enemyType != Type.D)
            Invoke("ChaseStart", spawnTime);
        Debug.Log("mainColider : " + mainColider);
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
}
