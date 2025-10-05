using UnityEngine;
using UnityEngine.UI;
using TMPro; // TextMeshPro를 쓰는 경우에 필요

public class DownloadUIManager : MonoBehaviour
{
    public GameObject downloadCanvas;   // 다운로드 UI 캔버스
    public Slider progressBar;          // 진행 바
    public TMP_Text percentageText;     // 진행률 텍스트 (Text 사용 시 Text로 바꿔도 됨)
    public float downloadTime = 5f;     // 다운로드 걸리는 시간 (초)

    private bool isDownloading = false;
    private float currentTime = 0f;

    void Start()
    {
        downloadCanvas.SetActive(false); // 시작할 땐 비활성화
    }

    public void StartDownload()
    {
        // 다운로드 시작
        downloadCanvas.SetActive(true);
        isDownloading = true;
        currentTime = 0f;
        progressBar.value = 0;
    }

    void Update()
    {
        if (isDownloading)
        {
            // 시간 기준으로 진행률 계산
            currentTime += Time.deltaTime;
            float progress = currentTime / downloadTime;
            progressBar.value = progress;
            percentageText.text = Mathf.RoundToInt(progress * 100f) + "%";

            // 완료 시 종료 처리
            if (progress >= 1f)
            {
                isDownloading = false;
                Invoke(nameof(EndDownload), 0.5f); // 살짝 지연 후 닫기
            }
        }
    }

    void EndDownload()
    {
        downloadCanvas.SetActive(false);
        Debug.Log("다운로드 완료!");
    }
}
