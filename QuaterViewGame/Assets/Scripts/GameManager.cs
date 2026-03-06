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
    [SerializeField] private GameObject bossRockZone;

    [Header("■ Options")]
    [SerializeField] private int stage;
    [SerializeField] private float playTime;
    [SerializeField] private bool isBattle;
    [SerializeField] private int enemyCntA;
    [SerializeField] private int enemyCntB;
    [SerializeField] private int enemyCntC;
    [SerializeField] private int enemyCntD;
    [SerializeField] private int boombEnemyCnt;
    [SerializeField] private int fireBallMonsterCnt;
    [SerializeField] private float bossRockDuration = 1.5f;// BossRock 변화 시간


    [Header("■ 배열 및 리스트")]
    [SerializeField] private Transform[] enemyZones;
    [SerializeField] private GameObject[] enemies;
    [SerializeField] private List<int> enemyList;
    [SerializeField] private List<int> speacialEnemyList;

    [Header("■ GameObject")]
    [SerializeField] private GameObject menuPanel;
    [SerializeField] private GameObject gamePanel;
    [SerializeField] private GameObject overPanel;
    [SerializeField] private GameObject damagePanel;
    [SerializeField] private TextMeshProUGUI damageText;
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
    [SerializeField] private TextMeshProUGUI boombEnemyText;
    [SerializeField] private TextMeshProUGUI fireBallMonsterText;

    [Header("■ Boss UI")]
    [SerializeField] private RectTransform bossHealthGroup; // 보스 체력 UI를 표시하기 위한 변수
    [SerializeField] private RectTransform bossHealthBar;

    [Header("■ Score UI")]
    [SerializeField] private TextMeshProUGUI curScoreText;
    [SerializeField] private TextMeshProUGUI bestScoreText;

    private Coroutine rockCamCo;

    public int EnemyCntA => enemyCntA;
    public int EnemyCntB => enemyCntB;
    public int EnemyCntC => enemyCntC;
    public int EnemyCntD => enemyCntD;

    public int FireBallMonsterCnt => fireBallMonsterCnt;

    public int BoombEnemyCnt => boombEnemyCnt;

    public Boss Boss => boss;

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

        foreach (Transform zone in enemyZones)
        {
            zone.gameObject.SetActive(true);
        }

        isBattle = true;
        StartCoroutine(InBattle());
    }

    public void StageEnd()
    {
        player.transform.position = new Vector3(-28, -3, -50);

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

    // Enemy A, B C 에 대한 1~9스테이지까지 가중치
    private (float a, float b, float c) GetWeights(int stage)
    {
        // 보스 스테이지는 여기로 오면 안 되게 설계 권장
        if (stage % 5 == 0)
            return (0f, 0f, 0f);

        // 11 이후부터 9 스테이지 기반 유지
        if (stage >= 11) stage = 9;

        // 보스(5의 배수)를 제외한 "일반 스테이지 순번"
        int n = stage - stage / 5;

        float a = 11f - n;                 // 10,9,8,7,6,5,4,3...
        float b = (n == 1) ? 0f : 0.5f * n;  // 0,1,1.5,2,2.5...
        float c = (n <= 2) ? 0f : 0.5f * (n - 2); // 0,0,0.5,1,1.5...

        return (a, b, c);
    }

    // Enemy A, B C 에 대한 가중치 기반 랜덤으로 뽑기
    private int GetWeightedEnemyIndex(int stage)
    {
        var (a, b, c) = GetWeights(stage);
        float total = a + b + c;
        if (total <= 0f) return 0;

        float roll = Random.Range(0f, total);

        if (roll < a) return 0;
        if (roll < a + b) return 1;
        return 2;
    }

    // BoombEnemy 스폰
    private IEnumerator SpawnSpecialEnemy(int stage, int count, Enemy.Type type)
    {
        for (int i = 0; i < count; i++)
        {
            int ranZone = Random.Range(0, 4);

            Enemy enemy = EnemyObjectPool.Instance.GetEnemy(type);

            enemy.transform.position = enemyZones[ranZone].position;
            enemy.transform.rotation = enemyZones[ranZone].rotation;

            enemy.Initialize(player.transform, this);

            if (enemy.GetEnemyType() == Enemy.Type.BoombMonster)
                boombEnemyCnt++;
            else if (enemy.GetEnemyType() == Enemy.Type.FireBallMonster)
                fireBallMonsterCnt++;

            yield return new WaitForSeconds(4f); // 한꺼번에 겹쳐 나오면 보기 안 좋아서(선택)
        }
    }


    private IEnumerator SpawnEnemy(int cnt, Enemy enemy)
    {
        if (enemy.GetEnemyType() == Enemy.Type.BoombMonster)
        {
            cnt = GetBoombSpawnCount(stage);
            if (cnt == 0) yield break;
        }

        if (enemy.GetEnemyType() == Enemy.Type.FireBallMonster)
        {
            cnt = GetFireBallSpawnCount(stage);
            if (cnt == 0) yield break;
        }

        for (int i = 0; i < cnt; i++)
        {
            speacialEnemyList.Add(4);
        }
        //(SpawnSpecialEnemy(stage, cnt, Enemy.Type));
        speacialEnemyList.RemoveAt(0);
        yield return new WaitForSeconds(4f);
    }

    // BoombEnemy 스폰 규칙
    private int GetBoombSpawnCount(int stage)
    {
        if (stage <= 4) return 1;        // 1~4
        if (stage <= 9) return 2;        // 6~9 (5는 위에서 제외됨)
        if (stage <= 14) return 3;       // 11~14 (15는 보스라 제외됨)
        if (stage <= 20) return 4;       // 16~20

        return 4; // 그 이후도 4마리 유지
    }

    // FireBallEnemy 스폰 규칙
    private int GetFireBallSpawnCount(int stage)
    {
        // FireBallEnemy는 3,6,9 스테이지에서는 1마리씩
        // 12, 18 스테이지에서는 2마리씩 스폰이 된다.
        // 단, 15스테이지는 보스 스테이지 이므로, 스폰이 되면 안된다.

        if (stage < 10)
        {
            if (stage % 3 == 0)
                return 1;
        }
        else if (stage < 20)
        {
            if (stage == 15 || stage == 10)
                return 0;
            if (stage % 3 == 0)
                return 2;
        }
        return 0;
    }

    private IEnumerator InBattle()
    {
        if (stage % 5 == 0)
        {
            enemyCntD++;
            int ranzone = Random.Range(0, 4);
            Enemy instantEnemy = EnemyObjectPool.Instance.GetEnemy(Enemy.Type.D);

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
                //int ran = Random.Range(0, 3);
                int ran = GetWeightedEnemyIndex(stage);

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
            
            while (enemyList.Count > 0)
            {
                int ranZone = Random.Range(0, 4);
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

            int boombCount = GetBoombSpawnCount(stage);
            for (int i = 0; i < boombCount; i++)
            {
                speacialEnemyList.Add(3);
            }
            StartCoroutine(SpawnSpecialEnemy(stage, boombCount, Enemy.Type.BoombMonster));
            yield return new WaitForSeconds(4f);
            //speacialEnemyList.RemoveAt(0);

            //SpawnEnemy(GetBoombSpawnCount(stage), EnemyObjectPool.Instance.GetEnemy(Enemy.Type.BoombMonster));
            //SpawnEnemy(GetFireBallSpawnCount(stage), EnemyObjectPool.Instance.GetEnemy(Enemy.Type.FireBallMonster));

            int fireCount = GetFireBallSpawnCount(stage);
            for (int i = 0; i < fireCount; i++)
            {
                speacialEnemyList.Add(4);
            }
            StartCoroutine(SpawnSpecialEnemy(stage, fireCount, Enemy.Type.FireBallMonster));
            //speacialEnemyList.RemoveAt(0);
        }

        while (enemyCntA + enemyCntB + enemyCntC + enemyCntD + boombEnemyCnt + fireBallMonsterCnt > 0)
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
        fireBallMonsterCnt = fireBallMonsterCnt <= 0 ? 0 : fireBallMonsterCnt;
        boombEnemyCnt = boombEnemyCnt <= 0 ? 0 : boombEnemyCnt;
        enemyCntD = enemyCntD <= 0 ? 0 : enemyCntD;

        // 몬스터 숫자 UI
        enemyAText.text = enemyCntA.ToString();
        enemyBText.text = enemyCntB.ToString();
        enemyCText.text = enemyCntC.ToString();
        boombEnemyText.text = boombEnemyCnt.ToString();
        fireBallMonsterText.text = fireBallMonsterCnt.ToString();

        if (boss == null)
            return;
        else if (boss.IsHpBar == true)
        {
            bossHealthGroup.gameObject.SetActive(true);
            float bossHp = (float)boss.CurHealth / boss.MaxHealth;
            bossHealthBar.localScale = new Vector3(bossHp, 1, 1);
            if(bossHp <= 0)
            {
                StopRockFlow();
                SetCameraX();
            }

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
            case Enemy.Type.BoombMonster:
                boombEnemyCnt = enemyCnt;
                break;
            case Enemy.Type.FireBallMonster:
                fireBallMonsterCnt = enemyCnt;
                break;
        }
    }

    public void StopRockFlow()
    {
        if (rockCamCo != null) StopCoroutine(rockCamCo);
        rockCamCo = null;
    }

    public void StartRockFlow()
    {
        if (rockCamCo != null) StopCoroutine(rockCamCo);
        rockCamCo = StartCoroutine(RotateCameraXSmooth());
        //StartCoroutine(RotateCameraXSmooth());
    }

    public void SetCameraX()
    {
        //if(boss.GetIsDead())
        //    StopRockFlow();
        
        Transform cam = Camera.main.transform;
        Vector3 rot = cam.eulerAngles;
        rot.x = 60f;
        cam.eulerAngles = rot;
    }

    IEnumerator RotateCameraXSmooth()
    {
        Transform cam = Camera.main.transform;
        float startX = 60f;
        float endX = 45f;

        float elapsed = 0f;
       
            while (elapsed < bossRockDuration)
            {
                elapsed += Time.deltaTime;

                float t = elapsed / bossRockDuration;

                float currentX = Mathf.Lerp(startX, endX, t);

                Vector3 rot = cam.eulerAngles;
                rot.x = currentX;
                cam.eulerAngles = rot;

                yield return null;
            }

        // 마지막 값 정확하게 고정
        Vector3 finalRot = cam.eulerAngles;
        finalRot.x = endX;
        cam.eulerAngles = finalRot;
    }


    public Boss GetBoss()
    {
        return boss;
    }

    public GameObject GetBossRockZone()
    {
        return bossRockZone;
    }

    public Player GetPlayer()
    {
        return player;
    }

    public TextMeshProUGUI GetDamageText()
    {
        //TextMeshProUGUI target = Instantiate(damageText, damagePanel.transform);
        TextMeshProUGUI target = DamageTextObejctPool.GetDamageText();
        return target;
    }
}
