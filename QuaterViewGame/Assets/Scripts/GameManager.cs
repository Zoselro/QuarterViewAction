using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    [Header("■ GameObject")]
    [SerializeField] private GameObject menuCam; // 메뉴 카메라
    [SerializeField] private GameObject gameCam; // 게임 카메라
    [SerializeField] private Player player;
    [SerializeField] private Boss boss;
    [SerializeField] private GameObject itemShop;
    [SerializeField] private GameObject weaponShop;
    [SerializeField] private GameObject startZone;

    [Header("■ Options")]
    [SerializeField] private int stage;
    [SerializeField] private float playTime;
    [SerializeField] private bool isBattle;
    [SerializeField] private int enemyCntA;
    [SerializeField] private int enemyCntB;
    [SerializeField] private int enemyCntC;
    [SerializeField] private int enemyCntD;

    [Header("■ 배열 및 리스트")]
    [SerializeField] private Transform[] enemyZones;
    [SerializeField] private GameObject[] enemies;
    [SerializeField] private List<int> enemyList;

    [Header("■ GameObject")]
    [SerializeField] private GameObject menuPanel;
    [SerializeField] private GameObject gamePanel;
    [SerializeField] private GameObject overPanel;
    [SerializeField] private TextMeshProUGUI maxScoreText;
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI stageText;
    [SerializeField] private TextMeshProUGUI playTimeText;
    [SerializeField] private TextMeshProUGUI playerHealthText;
    [SerializeField] private TextMeshProUGUI playerAmmoText;
    [SerializeField] private TextMeshProUGUI playerCoinText;

    [Header("■ WeaponImage")]
    [SerializeField] private Image weapon1Img;
    [SerializeField] private Image weapon2Img;
    [SerializeField] private Image weapon3Img;
    [SerializeField] private Image weaponRImg;

    [Header("■ EnemyText")]
    [SerializeField] private TextMeshProUGUI enemyAText;
    [SerializeField] private TextMeshProUGUI enemyBText;
    [SerializeField] private TextMeshProUGUI enemyCText;

    [Header("■ Boss UI")]
    [SerializeField] private RectTransform bossHealthGroup; // 보스 체력 UI를 표시하기 위한 변수
    [SerializeField] private RectTransform bossHealthBar;

    [Header("■ Score UI")]
    [SerializeField] private TextMeshProUGUI curScoreText;
    [SerializeField] private TextMeshProUGUI bestScoreText;

    public int EnemyCntA => enemyCntA;
    public int EnemyCntB => enemyCntB;
    public int EnemyCntC => enemyCntC;
    public int EnemyCntD => enemyCntD;

    private bool isSpawn;
    private bool isBossSpawn;

    private void Awake()
    {
        enemyList = new List<int>();
        maxScoreText.text = string.Format("{0:n0}", PlayerPrefs.GetInt("MaxScore"));
    }
    private void Start()
    {
        bossHealthGroup.gameObject.SetActive(false);
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
            int ranzone = Random.Range(0, 4);
            //GameObject instantEnemy = Instantiate(enemies[3], enemyZones[0].position, enemyZones[0].rotation);
            Enemy instantEnemy = EnemyObjectPool.Instance.GetEnemy(Enemy.Type.D);
            //Boss instantEnemy = EnemyObjectPool.Instance.GetBoss();


            instantEnemy.transform.position = enemyZones[ranzone].position;
            instantEnemy.transform.rotation = enemyZones[ranzone].rotation;
            Enemy target = instantEnemy.GetComponent<Enemy>();
            target.Initialize(player.transform, this);
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
            }

            while(enemyList.Count > 0)
            {
                int ranZone = Random.Range(0, 4);
                //GameObject instantEnemy = Instantiate(enemies[enemyList[0]], enemyZones[ranZone].position, enemyZones[ranZone].rotation);
                Enemy instantEnemy = null;
                switch (enemyList[0])
                {
                    case 0:
                        instantEnemy = EnemyObjectPool.Instance.GetEnemy(Enemy.Type.A);
                        break;
                    case 1:
                        instantEnemy = EnemyObjectPool.Instance.GetEnemy(Enemy.Type.B);
                        break;
                    case 2:
                        instantEnemy = EnemyObjectPool.Instance.GetEnemy(Enemy.Type.C);
                        break;
                }
                instantEnemy.transform.position = enemyZones[ranZone].position;
                instantEnemy.transform.rotation = enemyZones[ranZone].rotation;

                Enemy target = instantEnemy.GetComponent<Enemy>();
                target.Initialize(player.transform, this);
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

    public void GameOver()
    {
        gamePanel.SetActive(false);
        overPanel.SetActive(true);
        curScoreText.text = scoreText.text;

        int maxScore = PlayerPrefs.GetInt("MaxScore");
        if (player.Score > maxScore)
        {
            bestScoreText.gameObject.SetActive(true);
            PlayerPrefs.SetInt("MaxScore", player.Score);
        }
    }

    public void ReStart()
    {
        SceneManager.LoadScene(0);
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

        enemyCntA = enemyCntA <= 0 ? 0 : enemyCntA;
        enemyCntB = enemyCntB <= 0 ? 0 : enemyCntB;
        enemyCntC = enemyCntC <= 0 ? 0 : enemyCntC;
        enemyCntD = enemyCntD <= 0 ? 0 : enemyCntD;

        // 몬스터 숫자 UI
        enemyAText.text = enemyCntA.ToString();
        enemyBText.text = enemyCntB.ToString();
        enemyCText.text = enemyCntC.ToString();

        if (boss == null)
            return;
        else if (boss.IsHpBar == true)
        {
            bossHealthGroup.gameObject.SetActive(true);
            bossHealthBar.localScale = new Vector3((float)boss.CurHealth / boss.MaxHealth, 1, 1);
        }
        else
        {
            bossHealthGroup.gameObject.SetActive(false);
        }
    }

    public void DecreaseEnemyCount(Enemy.Type type, int enemyCnt)
    {
        switch (type)
        {
            case Enemy.Type.A:
                enemyCntA = enemyCnt;
                break;
            case Enemy.Type.B:
                enemyCntB = enemyCnt;
                break;
            case Enemy.Type.C:
                enemyCntC = enemyCnt;
                break;
            case Enemy.Type.D:
                enemyCntD = enemyCnt;
                break;
        }
    }

    public Boss GetBoss()
    {
        return boss;
    }
}
