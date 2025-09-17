using UnityEngine;
using UnityEngine.Pool;

public class ObjectPool : MonoBehaviour
{
    public enum PoolType
    {
        Stack,
        Queue
    }

    [SerializeField] private GameObject prefab;
    public GameObject Prefab => prefab;
    public int maxPoolSize = 10;
    IObjectPool<ParticleSystem> pool;
}

