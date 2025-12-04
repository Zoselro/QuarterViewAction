using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static UnityEditor.Experimental.GraphView.GraphView;

public class SpawnTreeGenerator : MonoBehaviour
{
    // 프레임 드랍 해결방안
    // 1. Player가 이동을 할 때마다 전방 30m 범위에
    // 2. 랜덤 위치에 나무 오브젝트 자동 생성

    private List<GameObject> spawnedObjects;

    [SerializeField] private GameObject[] prefabs;
    [SerializeField] private float spawnInterval = 0.5f;

    private float timer = 0;

    private void Awake()
    {
        spawnedObjects = new List<GameObject>();
    }

    void Update()
    {
        timer += Time.deltaTime;

        if (timer >= spawnInterval)
        {
            timer = 0;
            SpawnInsideCamera();
            Cleanup();
        }
    }

    void SpawnInsideCamera()
    {
        Camera cam = Camera.main;
        float depth = cam.transform.position.y;

        Vector3 bottomLeft = cam.ScreenToWorldPoint(new Vector3(0, 0, depth));
        Vector3 bottomRight = cam.ScreenToWorldPoint(new Vector3(Screen.width, 0, depth));
        Vector3 topLeft = cam.ScreenToWorldPoint(new Vector3(0, Screen.height, depth));

        float minX = bottomLeft.x;
        float maxX = bottomRight.x;
        float minZ = bottomLeft.z;
        float maxZ = topLeft.z;

        // 화면 내 랜덤 위치
        Vector3 pos = new Vector3(Random.Range(minX, maxX), 100f, Random.Range(minZ, maxZ));

        if (Physics.Raycast(pos, Vector3.down, out RaycastHit hit, 300f))
        {
            pos = hit.point;
            int preFabCount = prefabs.Length;
            int preFabRan = Random.Range(0, preFabCount);
            Debug.Log(preFabRan);
            GameObject obj = prefabs[preFabRan];
            GameObject tree = Instantiate(obj, pos, Quaternion.identity);
            spawnedObjects.Add(tree);
        }
    }

    void Cleanup()
    {
        Camera cam = Camera.main;
        Vector3 camPos = cam.transform.position;

        for (int i = spawnedObjects.Count - 1; i >= 0; i--)
        {
            if (spawnedObjects[i] == null)
            {
                spawnedObjects.RemoveAt(i);
                continue;
            }

            float dist = Vector3.Distance(camPos, spawnedObjects[i].transform.position);
            if (dist > 100f)
            {
                Debug.Log("지워짐");
                Destroy(spawnedObjects[i]);
                spawnedObjects.RemoveAt(i);
            }
        }
    }
}
