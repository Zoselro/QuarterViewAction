using UnityEngine;

public class MiniMapCamera : MonoBehaviour
{
    [SerializeField] private GameObject target;
    float tarGetX;
    float tarGetZ;
    Vector3 currentPos = Vector3.zero;

    void Update()
    {
        transform.Rotate(0f, 0f, 0f);

        tarGetX = target.transform.position.x; // 플레이어의 위치 
        tarGetZ = target.transform.position.z; 
        currentPos = transform.position; 
        currentPos.x = tarGetX;
        currentPos.z = tarGetZ;
        transform.position = currentPos;
    }
}
