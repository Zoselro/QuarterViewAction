using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    [SerializeField] private GameObject menuCam; // 메뉴 카메라
    [SerializeField] private GameObject gameCam; // 게임 카메라
    [SerializeField] private Player player;
    [SerializeField] private Boss boss;
    [SerializeField] private int stage;
    [SerializeField] private float playTime;
    [SerializeField] private bool isBattle;
    [SerializeField] private int enemyCntA;
    [SerializeField] private int enemyCntB;
    [SerializeField] private int enemyCntC;

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
        maxScoreText.text = string.Format("{0:n0}", PlayerPrefs.GetInt("MaxScore"));
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
        scoreText.text = string.Format("{0:n0}", player.Score);
        playerHealthText.text = player.Health + " / " + player.MaxHealth;
        playerCoinText.text = string.Format("{0:n0}", player.Coin);
        if (player.EquipWeapon == null)
            playerAmmoText.text = "- / " + player.Ammo;
        else if (player.EquipWeapon.GetWeaponType() == Weapon.Type.Melee)
            playerAmmoText.text = "- / " + player.Ammo;
        else
            playerAmmoText.text = player.EquipWeapon.CurAmmo + " / " + player.Ammo;

        weapon1Img.color = new Color(1, 1, 1, player.HasWeapons[0] ? 1 : 0);
        weapon1Img.color = new Color(1, 1, 1, player.HasWeapons[1] ? 1 : 0);
        weapon1Img.color = new Color(1, 1, 1, player.HasWeapons[2] ? 1 : 0);
        weapon1Img.color = new Color(1, 1, 1, player.HasGrenades > 0 ? 1 : 0);

        enemyAText.text = enemyCntA.ToString();
        enemyBText.text = enemyCntB.ToString();
        enemyCText.text = enemyCntC.ToString();
    }
}
