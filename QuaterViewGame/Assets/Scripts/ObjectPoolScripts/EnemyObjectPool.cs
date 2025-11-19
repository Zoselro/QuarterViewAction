using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class EnemyObjectPool : MonoBehaviour
{
    public static EnemyObjectPool Instance = null;

    [SerializeField] private Enemy enemyAPrefab;
    [SerializeField] private Enemy enemyBPrefab;
    [SerializeField] private Enemy enemyCPrefab;
    [SerializeField] private Enemy enemyDPrefab;

    private Dictionary<Enemy.Type, IObjectPool<Enemy>> enemyPools;

    private void Awake()
    {
        Instance = this;
        enemyPools = new Dictionary<Enemy.Type, IObjectPool<Enemy>>();
    }

    private void Start()
    {
        enemyPools.Add(Enemy.Type.A, CreatePool(enemyAPrefab));
        enemyPools.Add(Enemy.Type.B, CreatePool(enemyBPrefab));
        enemyPools.Add(Enemy.Type.C, CreatePool(enemyCPrefab));
        enemyPools.Add(Enemy.Type.D, CreatePool(enemyDPrefab));
    }

    private IObjectPool<Enemy> CreatePool(Enemy prefab)
    {
        return new ObjectPool<Enemy>(
            createFunc: () =>
            {
                Enemy newObj = Instantiate(prefab, transform);
                newObj.gameObject.SetActive(false);
                return newObj;
            },
            actionOnGet: (b) =>
            {
                b.transform.SetParent(Instance.transform);
                b.gameObject.SetActive(true);
                b.ResetState();

                if(b is Boss boss)
                {
                    boss.ResetBoss();
                }
            },
            actionOnRelease: (b) =>
            {
                b.transform.SetParent(Instance.transform);
                b.gameObject.SetActive(false);
            },
            maxSize: 5
        );
    }

    public Enemy GetEnemy(Enemy.Type type)
    {
        return enemyPools[type].Get();
    }

    public void ReturnEnemy(Enemy enemy)
    {
        enemyPools[enemy.GetEnemyType()].Release(enemy);
    }
}