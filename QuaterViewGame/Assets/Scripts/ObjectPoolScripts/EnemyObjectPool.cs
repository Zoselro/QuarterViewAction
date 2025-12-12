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
    [SerializeField] private BoombMonster boombMonsterPrefab;

    private Dictionary<Enemy.Type, IObjectPool<Enemy>> enemyPools;
    private Enemy enemy;

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
        enemyPools.Add(Enemy.Type.BoombMonster, CreatePool(boombMonsterPrefab));
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
                if (b is BoombMonster boombMonster)
                    boombMonster.ResetState();
                else
                    b.ResetState();

                if(b is Boss boss)
                {
                    boss.ResetBoss();
                    boss.SetIsHpBar(true);
                }
            },
            actionOnRelease: (b) =>
            {
                b.transform.SetParent(Instance.transform);
                b.gameObject.SetActive(false);

                if (b is Boss boss)
                    boss.SetIsHpBar(false);
            },
            maxSize: 5
        );
    }

    public Enemy GetEnemy(Enemy.Type type)
    {
        enemy = enemyPools[type].Get();
        return enemy;
    }

    public Enemy GetEnemyType(Enemy.Type type)
    {
        return enemy;
    }

    public void ReturnEnemy(Enemy enemy)
    {
        enemyPools[enemy.GetEnemyType()].Release(enemy);
    }
}