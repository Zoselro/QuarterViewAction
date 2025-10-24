using System.Collections;
using UnityEngine;

public class Boss : Enemy
{
    [SerializeField] GameObject missile;
    [SerializeField] Transform missilePortA;
    [SerializeField] Transform missilePortB;

    private Vector3 lookVec; // 플레이어가 가는 방향을 미리 예측하는 벡터
    private Vector3 tauntVec; // 어디로 내려찍을 지 미리 에측하는 벡터
    private bool isLook; // 플레이어를 바라보는 플래그 변수

    private void Awake()
    {

        transform.localScale = new Vector3(3f, 3f, 3f);
    }

    private void Update()
    {
        if (isLook)
        {
            float h = Input.GetAxisRaw("Horizontal");
            float v = Input.GetAxisRaw("Vertical");
            lookVec = new Vector3(h, 0, v) * 5f;

            transform.LookAt(target.position + lookVec);
        }
    }

    // 보스가 행동패턴을 결정해주는 코루틴
    private IEnumerator Thick()
    {
        yield return null;
    }
}
