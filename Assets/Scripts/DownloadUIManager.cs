// // 파일명: DownloadUIManager.cs
// using UnityEngine;
// using UnityEngine.UI;
// using TMPro;

// public class DownloadUIManager : MonoBehaviour
// {
//     public GameObject downloadCanvas;   // 다운로드 UI 패널
//     public Slider progressBar;          // 게이지 바
//     public TMP_Text percentageText;     // 퍼센트 표시 텍스트
//     public float downloadTime = 5f;     // 다운로드 소요 시간

//     [Header("퀘스트 연결")]
//     public Quest linkedQuest; // 완료 시 완료 처리할 퀘스트

//     private bool isDownloading = false;
//     private float currentTime = 0f;
//     //추가
//     private MiniGameBridge bridge;

//     void Start()
//     {
//         if (downloadCanvas != null) downloadCanvas.SetActive(false);
//     }

//     public void StartDownload()
//     {
//         if (isDownloading) return;

//         if (downloadCanvas != null) downloadCanvas.SetActive(true);
//         isDownloading = true;
//         currentTime = 0f;
//         if (progressBar != null) progressBar.value = 0f;
//     }

//     void Update()
//     {
//         if (!isDownloading) return;

//         currentTime += Time.deltaTime;
//         float progress = Mathf.Clamp01(currentTime / downloadTime);
//         if (progressBar != null) progressBar.value = progress;
//         if (percentageText != null)
//             percentageText.text = Mathf.RoundToInt(progress * 100f) + "%";

//         if (progress >= 1f)
//         {
//             isDownloading = false;
//             Invoke(nameof(EndDownload), 0.5f);
//             //추가
//             bridge?.Finish();
//         }
//     }

//     void EndDownload()
//     {
//         if (downloadCanvas != null) downloadCanvas.SetActive(false);
//         Debug.Log("다운로드 완료!");

//          // 🔥 다운로드 완료 시 오염 4단계로
//         ContaminationManager contamManager = FindObjectOfType<ContaminationManager>();
//         if (contamManager != null)
//         {
//             float targetValue = 80f; // 레벨 4
//             float increaseAmount = targetValue - contamManager.contaminationValue;
//             if (increaseAmount > 0)
//                 contamManager.IncreaseContamination(increaseAmount);

//             Debug.Log("[오염] 다운로드 완료 → 오염 4단계 적용");
//         }
//         else
//         {
//             Debug.LogWarning("[오염] ContaminationManager를 찾을 수 없음!");
//         }


//         if (linkedQuest != null)
//             QuestManager.Instance.CompleteQuest(linkedQuest);
//     }
// }


using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DownloadUIManager : MonoBehaviour
{
    public GameObject downloadCanvas;
    public Slider progressBar;
    public TMP_Text percentageText;
    public float downloadTime = 5f;

    [Header("퀘스트 연결")]
    public Quest linkedQuest;

    private bool isDownloading = false;
    private bool isDownloaded = false; // ✅ 이미 다운로드 완료했는지 여부
    private float currentTime = 0f;

    private MiniGameBridge bridge;

    void Start()
    {
        if (downloadCanvas != null) downloadCanvas.SetActive(false);
    }

    public void StartDownload()
    {
        // ✅ 이미 완료된 경우 실행 안 함
        if (isDownloaded)
        {
            Debug.Log("[다운로드] 이미 완료된 파일입니다. 다시 다운로드할 수 없습니다.");
            return;
        }

        // ✅ 다운로드 중인 경우도 실행 안 함
        if (isDownloading)
        {
            Debug.Log("[다운로드] 이미 다운로드가 진행 중입니다.");
            return;
        }

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
            isDownloaded = true; // ✅ 다운로드 완료 상태로 변경
            Invoke(nameof(EndDownload), 0.5f);
            bridge?.Finish();
        }
    }

    void EndDownload()
    {
        if (downloadCanvas != null) downloadCanvas.SetActive(false);
        Debug.Log("다운로드 완료!");

        // 🔥 다운로드 완료 시 오염 4단계로
        ContaminationManager contamManager = FindObjectOfType<ContaminationManager>();
        if (contamManager != null)
        {
            float targetValue = 80f; // 레벨 4
            float increaseAmount = targetValue - contamManager.contaminationValue;
            if (increaseAmount > 0)
                contamManager.IncreaseContamination(increaseAmount);

            Debug.Log("[오염] 다운로드 완료 → 오염 4단계 적용");
        }
        else
        {
            Debug.LogWarning("[오염] ContaminationManager를 찾을 수 없음!");
        }

        if (linkedQuest != null)
            QuestManager.Instance.CompleteQuest(linkedQuest);
    }
}

