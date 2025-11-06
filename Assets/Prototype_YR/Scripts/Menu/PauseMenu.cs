using UnityEngine;

public class PauseMenu : MonoBehaviour
{
    public GameObject pausePanel;     // 서브 메뉴 패널
    public FPCamera cameraScript;     // FPCamera 스크립트 연결 (인스펙터에서 드래그)
    private bool isPaused = false;
    public GameObject crosshair; // 인스펙터에서 드래그
     public GameObject hudimage; // 인스펙터에서 드래그

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            isPaused = !isPaused;
            pausePanel.SetActive(isPaused);

            // 게임 일시정지
            Time.timeScale = isPaused ? 0f : 1f;

            // 카메라 스크립트에 일시정지 상태 전달
            if (cameraScript != null)
                cameraScript.isPaused = isPaused;

            // crosshair 보이기/숨기기
            if (crosshair != null)
                crosshair.SetActive(!isPaused);

            if (hudimage != null)
                hudimage.SetActive(!isPaused);
            
            // 커서 보이기/숨기기
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

        // 카메라 스크립트에 전달
        if (cameraScript != null)
            cameraScript.isPaused = false;

        if (crosshair != null)
            crosshair.SetActive(true);
            
        if (hudimage != null)
            hudimage.SetActive(true);

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
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
