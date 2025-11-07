// 파일명: DownloadUIManager.cs
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DownloadUIManager : MonoBehaviour
{
    public GameObject downloadCanvas;   // 다운로드 UI 패널
    public Slider progressBar;          // 게이지 바
    public TMP_Text percentageText;     // 퍼센트 표시 텍스트
    public float downloadTime = 5f;     // 다운로드 소요 시간

    [Header("퀘스트 연결")]
    public Quest linkedQuest; // 완료 시 완료 처리할 퀘스트

    private bool isDownloading = false;
    private float currentTime = 0f;

    void Start()
    {
        if (downloadCanvas != null) downloadCanvas.SetActive(false);
    }

    public void StartDownload()
    {
        if (isDownloading) return;

        if (downloadCanvas != null) downloadCanvas.SetActive(true);
        isDownloading = true;
        currentTime = 0f;
        if (progressBar != null) progressBar.value = 0f;
    }

    void Update()
    {
        if (!isDownloading) return;

        currentTime += Time.deltaTime;
        float progress = Mathf.Clamp01(currentTime / downloadTime);
        if (progressBar != null) progressBar.value = progress;
        if (percentageText != null)
            percentageText.text = Mathf.RoundToInt(progress * 100f) + "%";

        if (progress >= 1f)
        {
            isDownloading = false;
            Invoke(nameof(EndDownload), 0.5f);
        }
    }

    void EndDownload()
    {
        if (downloadCanvas != null) downloadCanvas.SetActive(false);
        Debug.Log("다운로드 완료!");

        if (linkedQuest != null)
            QuestManager.Instance.CompleteQuest(linkedQuest);
    }
}
