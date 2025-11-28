using System.Collections;
using TMPro;
using UnityEngine;

public class Shop : MonoBehaviour
{
    [Header("■ GameObject")]
    [SerializeField] private RectTransform uiGroup;
    [SerializeField] private Animator anim;
    [SerializeField] private GameObject[] itemObj; // 프리팹을 저장할 아이템 오브젝트들
    [SerializeField] private int[] itemPrice; // 아이템 오브젝트에 대한 가격
    [SerializeField] private Transform[] itemPos; // 아이템 소환되는 위치
    [SerializeField] private string[] talkData; // player가 물품을 살 때 coin이 부족할 때 나오는 대사를 저장하기 위한 변수
    [SerializeField] TextMeshProUGUI talkText; 

    private Player enterPlayer;

    public void Enter(Player player)
    {
        enterPlayer = player;
        uiGroup.anchoredPosition = Vector3.zero; // 화면 정 중앙에 오도록 하기
    }

    public void Exit()
    {
        anim.SetTrigger("doHello");
        uiGroup.anchoredPosition = Vector3.down * 1000; // 내렸을 때 그 위치로 복귀
    }

    // 물건 사기
    public void Buy(int index) // 물건에 대한 index
    {
        int price = itemPrice[index];
        if(price > enterPlayer.Coin) // 만약 플레이어가 돈이 모자르다면
        {
            StopCoroutine(Talk());
            StartCoroutine(Talk());
            return;
        }

        enterPlayer.SetCoin(enterPlayer.Coin - price);
        Vector3 ranVec = Vector3.right * Random.Range(-3, 3)
                            + Vector3.forward * Random.Range(-3, 3);
        GameObject item = WeaponObjectPool.GetItem(index);
        item.transform.position = itemPos[index].position + ranVec;
        item.transform.rotation = itemPos[index].rotation;
    }

    IEnumerator Talk()
    {
        talkText.text = talkData[1];
        yield return new WaitForSeconds(2);
        talkText.text = talkData[0];
    }
}
