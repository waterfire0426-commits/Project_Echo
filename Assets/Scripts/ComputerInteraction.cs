using UnityEngine;

public class ComputerInteraction : MonoBehaviour
{
    public DownloadUIManager downloadUI; // 다운로드 매니저 연결
    private bool isPlayerNear = false;   // 플레이어 근처인지 확인

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNear = true;
            Debug.Log("컴퓨터 접근 - E키로 다운로드 시작 가능");
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNear = false;
        }
    }

    void Update()
    {
        if (isPlayerNear && Input.GetKeyDown(KeyCode.E))
        {
            downloadUI.StartDownload();
        }
    }
}
