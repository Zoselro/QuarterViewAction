using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.Pool;

public class ItemObjectPool : MonoBehaviour
{
    [SerializeField] private GameObject[] weaponPrefabs;
    [SerializeField] private GameObject[] itemPrefabs;
    [SerializeField] private GameObject[] coinPrefabs;

    public static ItemObjectPool Instance = null;

    private IObjectPool<GameObject>[] weaponPools;
    private IObjectPool<GameObject>[] itemPools;
    private IObjectPool<GameObject>[] coinPools;

    private void Awake()
    {
        Instance = this;
        weaponPools = new IObjectPool<GameObject>[weaponPrefabs.Length];
        itemPools = new IObjectPool<GameObject>[itemPrefabs.Length];
        coinPools = new IObjectPool<GameObject>[coinPrefabs.Length];

        makeObjectPool(weaponPools, weaponPrefabs);
        makeObjectPool(itemPools, itemPrefabs);
        makeObjectPool(coinPools, coinPrefabs);
    }

    public void makeObjectPool(IObjectPool<GameObject>[] pools, GameObject[] objectPrefab)
    {
        for (int i = 0; i < pools.Length; i++)
        {
            int index = i;

            pools[i] = new ObjectPool<GameObject>(

                createFunc: () =>
                {
                    GameObject obj = Instantiate(objectPrefab[index]);
                    obj.gameObject.SetActive(false);
                    return obj;
                },
                actionOnGet: (i) =>
                {
                    i.transform.SetParent(Instance.transform);
                    i.gameObject.SetActive(true);
                    Item item = i.GetComponent<Item>();
                },
                actionOnRelease: (i) =>
                {
                    i.transform.SetParent(Instance.transform);
                    i.gameObject.SetActive(false);
                },
                maxSize: 3
            );
        }
    }

    public static void ReturnItem(GameObject item, int index, bool isWeaponShop)
    {
        if(isWeaponShop)
            Instance.weaponPools[index].Release(item);
        else
            Instance.itemPools[index].Release(item);
    }

    public static GameObject GetItem(int index, bool isWeaponShop)
    {
        if(isWeaponShop)
            return Instance.weaponPools[index].Get();
        else
            return Instance.itemPools[index].Get();
    }

    public static void ReturnCoin(GameObject coin, int index)
    {
        Instance.coinPools[index].Release(coin);
    }

    public static GameObject GetCoin(int index)
    {
        return Instance.coinPools[index].Get();
    }
}
