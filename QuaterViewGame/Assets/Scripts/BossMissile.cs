using UnityEngine;
using UnityEngine.AI;

public class BossMissile : Bullet
{
    [SerializeField] private Transform target;
    private NavMeshAgent nav;


    private void Awake()
    {
        nav = GetComponent<NavMeshAgent>();
    }

    private void Update()
    {
        nav.SetDestination(target.position);
        if (EnemyObjectPool.Instance.GetEnemyType(Enemy.Type.D).CurHealth == 0)
        {
            EnemyBulletObejctPool.Instance.ReturnBossBulletPool(gameObject.GetComponent<BossMissile>());
        }
    }

    public void SetTarget(Transform target)
    {
        this.target = target;
    }
}
