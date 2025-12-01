using System.Collections;
using UnityEngine;

public class Grenade : MonoBehaviour
{
    [SerializeField] GameObject meshObj;
    [SerializeField] GameObject effectObj;
    [SerializeField] Rigidbody rigid;
    [SerializeField] private float explosionTime;

    public void ReSetState()
    {
        StopAllCoroutines();

        // 물리 상태 초기화 (안 해도 되지만 안전하게)
        rigid.linearVelocity = Vector3.zero;
        rigid.angularVelocity = Vector3.zero;

        // 외형 상태 초기화
        meshObj.SetActive(true);
        effectObj.SetActive(false);

        // 다시 타이머 시작
        StartCoroutine(Explosion());
    }

    IEnumerator Explosion()
    {
        yield return new WaitForSeconds(explosionTime);
        rigid.linearVelocity = Vector3.zero;
        rigid.angularVelocity = Vector3.zero;
        meshObj.SetActive(false);
        effectObj.SetActive(true);

        // 오브젝트 주위에 있는 "Enemy" 레이어를 가진 오브젝트를 가져오는 메서드
        // Physics.SphereCastAll([내 위치], [범위], [방향], [레이어를 쏘는 길이], [가져올 레이어])
        RaycastHit[] raycastHits =
            Physics.SphereCastAll(transform.position,
                                    15, Vector3.up, 0f, LayerMask.GetMask("Enemy"));

        foreach (RaycastHit hitObj in raycastHits)
        {
            hitObj.transform.GetComponent<Enemy>().HitByGrenade(transform.position);
        }

        yield return new WaitForSeconds(5f);
        ReturnToPool();
    }

    public void ReturnToPool()
    {
        ThrowGrenadeObjectPool.ReleaseThrowGrenade(gameObject);
    }
}
