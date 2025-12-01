using UnityEngine;
using UnityEngine.Pool;

public class ThrowGrenadeObjectPool : MonoBehaviour
{
    [SerializeField] private GameObject throwGrenade;

    public static ThrowGrenadeObjectPool Instance = null;

    private IObjectPool<GameObject> grenadeObjectPool;

    private void Awake()
    {
        Instance = this;
        grenadeObjectPool = new ObjectPool<GameObject>(
            createFunc: () =>
            {
                GameObject newObj = Instantiate(throwGrenade);
                newObj.SetActive(false);
                return newObj;
            },
            actionOnGet : (g) =>
            {
                g.transform.SetParent(Instance.transform);
                g.gameObject.SetActive(true);
                g.GetComponent<Grenade>().ReSetState();
                
            },
            actionOnRelease: (g) =>
            {
                g.transform.SetParent(Instance.transform);
                g.SetActive(false);
            },
            maxSize : 10
        );
    }

    public static GameObject GetThrowGrenade()
    {
        return Instance.grenadeObjectPool.Get();
    }

    public static void ReleaseThrowGrenade(GameObject grenade)
    {
        Instance.grenadeObjectPool.Release(grenade);
    }
}
