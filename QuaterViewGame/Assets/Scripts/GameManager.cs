using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    [SerializeField] private GameObject menuCam; // 메뉴 카메라
    [SerializeField] private GameObject gameCam; // 게임 카메라
    [SerializeField] private Player player;
    [SerializeField] private Boss boss;
    [SerializeField] private GameObject itemShop;
    [SerializeField] private GameObject weaponShop;
    [SerializeField] private GameObject startZone;


    [SerializeField] private int stage;
    [SerializeField] private float playTime;
    [SerializeField] private bool isBattle;
    [SerializeField] private int enemyCntA;
    [SerializeField] private int enemyCntB;
    [SerializeField] private int enemyCntC;
    [SerializeField] private int enemyCntD;

    [SerializeField] private Transform[] enemyZones;
    [SerializeField] private GameObject[] enemies;
    [SerializeField] private List<int> enemyList;

    [SerializeField] private GameObject menuPanel;
    [SerializeField] private GameObject gamePanel;
    [SerializeField] private TextMeshProUGUI maxScoreText;
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI stageText;
    [SerializeField] private TextMeshProUGUI playTimeText;
    [SerializeField] private TextMeshProUGUI playerHealthText;
    [SerializeField] private TextMeshProUGUI playerAmmoText;
    [SerializeField] private TextMeshProUGUI playerCoinText;

    [SerializeField] private Image weapon1Img;
    [SerializeField] private Image weapon2Img;
    [SerializeField] private Image weapon3Img;
    [SerializeField] private Image weaponRImg;

    [SerializeField] private TextMeshProUGUI enemyAText;
    [SerializeField] private TextMeshProUGUI enemyBText;
    [SerializeField] private TextMeshProUGUI enemyCText;

    [SerializeField] private RectTransform bossHealthGroup; // 보스 체력 UI를 표시하기 위한 변수
    [SerializeField] private RectTransform bossHealthBar;

    private void Awake()
    {
        enemyList = new List<int>();
        maxScoreText.text = string.Format("{0:n0}", PlayerPrefs.GetInt("MaxScore"));
    }

    private void Update()
    {
        if (isBattle)
            playTime += Time.deltaTime;
    }

    public void StageStart()
    {
        itemShop.SetActive(false);
        weaponShop.SetActive(false);
        startZone.SetActive(false);

        foreach(Transform zone in enemyZones)
        {
            zone.gameObject.SetActive(true);
        }

        isBattle = true;
        StartCoroutine(InBattle());
    }

    public void StageEnd()
    {
        player.transform.position = new Vector3(-28, -6, -50);

        itemShop.SetActive(true);
        weaponShop.SetActive(true);
        startZone.SetActive(true);

        foreach (Transform zone in enemyZones)
        {
            zone.gameObject.SetActive(false);
        }

        stage++;
        isBattle = false;
    }

    private IEnumerator InBattle()
    {
        if(stage % 5 == 0)
        {
            enemyCntD++;
            GameObject instantEnemy = Instantiate(enemies[enemyList[3]], enemyZones[0].position, enemyZones[0].rotation);
            Enemy target = instantEnemy.GetComponent<Enemy>();
            target.Initialize(player.transform);
            boss = instantEnemy.GetComponent<Boss>();
        }
        else
        {
            for (int index = 0; index < stage; index++)
            { 
                int ran = Random.Range(0, 3);
                enemyList.Add(ran);
                switch (ran)
                {
                    case 0:
                        enemyCntA++;
                        break;
                    case 1:
                        enemyCntB++;
                        break;
                    case 2:
                        enemyCntC++;
                        break;
                }
                Debug.Log("ran : " + ran);
            }

            while(enemyList.Count > 0)
            {
                int ranZone = Random.Range(0, 4);
                GameObject instantEnemy = Instantiate(enemies[enemyList[0]], enemyZones[ranZone].position, enemyZones[ranZone].rotation);
                Enemy target = instantEnemy.GetComponent<Enemy>();
                target.Initialize(player.transform);
                enemyList.RemoveAt(0);
                yield return new WaitForSeconds(4f);
            }
        }

        while (enemyCntA + enemyCntB + enemyCntC + enemyCntD > 0)
        {
            yield return null;
        }

        yield return new WaitForSeconds(4f);
        StageEnd();
    }

    public void GameStart()
    {
        menuCam.SetActive(false);
        gameCam.SetActive(true);

        menuPanel.SetActive(false);
        gamePanel.SetActive(true);

        player.gameObject.SetActive(true);
    }


    // Update가 끝난 후 호출되는 생명주기 함수.
    private void LateUpdate()
    {
        // 상단 UI
        scoreText.text = string.Format("{0:n0}", player.Score);
        stageText.text = "STAGE " + stage;

        int hour = (int)(playTime / 3600); // 시간
        int min = (int)((playTime - hour * 3600) / 60); // 분
        int second = (int)(playTime % 60);

        playTimeText.text = string.Format("{0:00}", hour) + ":" + string.Format("{0:00}", min) + ":"
                                + string.Format("{0:00}", second);

        // 플레이어 UI
        playerHealthText.text = player.Health + " / " + player.MaxHealth;
        playerCoinText.text = string.Format("{0:n0}", player.Coin);
        if (player.EquipWeapon == null)
            playerAmmoText.text = "- / " + player.Ammo;
        else if (player.EquipWeapon.GetWeaponType() == Weapon.Type.Melee)
            playerAmmoText.text = "- / " + player.Ammo;
        else
            playerAmmoText.text = player.EquipWeapon.CurAmmo + " / " + player.Ammo;

        // 무기 UI
        weapon1Img.color = new Color(1, 1, 1, player.HasWeapons[0] ? 1 : 0);
        weapon2Img.color = new Color(1, 1, 1, player.HasWeapons[1] ? 1 : 0);
        weapon3Img.color = new Color(1, 1, 1, player.HasWeapons[2] ? 1 : 0);
        weaponRImg.color = new Color(1, 1, 1, (player.HasGrenades > 0) ? 1 : 0);

        // 몬스터 숫자 UI
        enemyAText.text = enemyCntA.ToString();
        enemyBText.text = enemyCntB.ToString();
        enemyCText.text = enemyCntC.ToString();


        // 체력 바를 줄이기
        if(boss != null)
            bossHealthBar.localScale = new Vector3((float)boss.CurHealth / boss.MaxHealth, 1, 1);
    }
}
