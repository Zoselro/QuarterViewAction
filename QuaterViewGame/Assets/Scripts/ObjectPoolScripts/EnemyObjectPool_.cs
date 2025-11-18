using UnityEngine;
using UnityEngine.Pool;

public class EnemyObjectPool_ : MonoBehaviour
{
    public static EnemyObjectPool_ Instance = null;

    private IObjectPool<Enemy> enemyAPool;
    private IObjectPool<Enemy> enemyBPool;
    private IObjectPool<Enemy> enemyCPool;
    private IObjectPool<Enemy> enemyDPool;

    [SerializeField] private Enemy enemyAPrefab;
    [SerializeField] private Enemy enemyBPrefab;
    [SerializeField] private Enemy enemyCPrefab;
    [SerializeField] private Enemy enemyDPrefab;
    private void Awake()
    {
        Instance = this;
    }

    public void CreateEnemy(Enemy.Type type, IObjectPool<Enemy> enemyPool)
    {
        enemyPool = new ObjectPool<Enemy>(
            createFunc: () =>
            {
                Enemy newObj = Instantiate(enemyAPrefab, transform);
                newObj.gameObject.SetActive(false);
                return newObj;
            },
            actionOnGet: (b) =>
            {
                b.transform.SetParent(Instance.transform);
                b.gameObject.SetActive(true);
            },
            actionOnRelease: (b) =>
            {
                b.transform.SetParent(Instance.transform);
                b.gameObject.SetActive(false);
            },
            maxSize: 5
        );
    }

    public static void ReturnEnemy(Enemy.Type type, Enemy enemy)
    {
        Instance.enemyAPool.Release(enemy);
    }

    public static Enemy GetEnemy()
    {
        Enemy enemy = Instance.enemyAPool.Get();
        return enemy;
    }
}
