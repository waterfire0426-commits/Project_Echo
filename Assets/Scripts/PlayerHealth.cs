using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    public int maxHealth = 10;
    private int currentHealth;

    [Header("UI References")]
    public Slider hpSlider;       // 체력바
    public GameObject gameOverUI; // Game Over UI
    public GameObject hudUI;      // 평소 HUD (방호복 UI)

    [Header("Scene Settings")]
    public string mainMenuSceneName = "MainMenu"; // 메인 메뉴 씬 이름

    void Start()
    {
        currentHealth = maxHealth;

        // HP Slider 초기화
        if (hpSlider != null)
        {
            hpSlider.minValue = 0;
            hpSlider.maxValue = maxHealth;
            hpSlider.value = currentHealth;
        }

        gameOverUI.SetActive(false);

        // 시작할 때 마우스 숨기기
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        // 테스트용: L 키 누르면 체력 1 감소
        if (Input.GetKeyDown(KeyCode.L))
        {
            TakeDamage(1);
            Debug.Log("체력 감소! 현재 체력: " + currentHealth);
        }
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        if (currentHealth < 0) currentHealth = 0;

        if (hpSlider != null)
            hpSlider.value = currentHealth;

        if (currentHealth <= 0)
        {
            GameOver();
        }
    }

    void GameOver()
    {
        // HUD 꺼짐
        if (hudUI != null)
            hudUI.SetActive(false);

        // Game Over UI 표시
        gameOverUI.SetActive(true);

        // 게임 멈춤
        Time.timeScale = 0f;

        // 마우스 보이게 + UI 클릭 가능하게
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    // 강제로 Game Over 시킬 때 (예: 오염 게이지 5단계)
    public void ForceGameOver()
    {
        if (currentHealth > 0) // 중복 호출 방지
            currentHealth = 0;

        GameOver();
    }

    // UI 버튼에서 호출할 함수
    public void OnClickRestart()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenuScene"); // 메인 메뉴로 이동
    }

    public void OnClickExit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
