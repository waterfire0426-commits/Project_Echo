using UnityEngine;
public class PauseMenu : MonoBehaviour
{
    public GameObject pausePanel; // 서브 메뉴 패널

    private bool isPaused = false;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            isPaused = !isPaused;
            pausePanel.SetActive(isPaused);

            // 게임 일시정지
            Time.timeScale = isPaused ? 0f : 1f;

            Cursor.visible = isPaused;
            Cursor.lockState = isPaused ? CursorLockMode.None : CursorLockMode.Locked;

        }
    }

    // 계속하기 버튼
    public void OnClickResume()
    {
        isPaused = false;
        pausePanel.SetActive(false);
        Time.timeScale = 1f;
    }

    // 종료하기 버튼
    public void OnClickQuit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
        Debug.Log("게임 종료"); // 에디터 테스트용
    }
}
