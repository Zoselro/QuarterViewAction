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
        Debug.Log(EnemyObjectPool.Instance.GetEnemyType(Enemy.Type.D).CurHealth);
        if (EnemyObjectPool.Instance.GetEnemyType(Enemy.Type.D).CurHealth == 0)
        {
            Destroy(gameObject);
        }
    }

    public void SetTarget(Transform target)
    {
        this.target = target;
    }
}
