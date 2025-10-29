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
    }

    public void SetTarget(Transform target)
    {
        this.target = target;
    }
}
