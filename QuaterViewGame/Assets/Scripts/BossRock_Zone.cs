using UnityEngine;

public class BossRock_Zone : MonoBehaviour
{
    // Boss가 doRockShot을 사용을 한다고 받을 때 처리
    [SerializeField] private GameObject targetObj;
    [SerializeField] private GameManager gm;

    private float targetY;

    void Update()
    {
        //Debug.Log("gm.Boss.DoRockShot : " + gm.Boss.DoRockShot);
        //if (gm.Boss.DoRockShot)
        //{
        //    targetY = gm.Boss.BossRock.TarGetY;
        //    gameObject.SetActive(true);
        //    transform.position = targetObj.transform.position + new Vector3(0f, targetY, 0f);
        //}
    }
}
