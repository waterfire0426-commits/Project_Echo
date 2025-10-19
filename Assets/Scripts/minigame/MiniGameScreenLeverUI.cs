// using UnityEngine;
// using UnityEngine.UI;
// using TMPro;

// public class MiniGameScreenLeverUI : MonoBehaviour
// {
//     [Header("UI References")]
//     public RectTransform screenImage;   // 회전할 스크린 이미지
//     public Slider angleSlider;          // 마우스로 조절할 슬라이더
//     public TMP_Text angleText;          // 각도 표시 텍스트
//     public GameObject uiRoot;           // UI 루트 (Canvas/Panel)

//     [Header("Angle Settings")]
//     public float minAngle = 0f;
//     public float maxAngle = 90f;
//     public float idealMin = 30f;
//     public float idealMax = 45f;

//     [Header("Game Settings")]
//     public float successGoal = 5f;      // 이상 각도 유지 시간
//     private float successTime = 0f;
//     private bool isMiniGameActive = false;
//     private bool isFinished = false;

//     private MiniGameBridge bridge;

//     void Start()
//     {
//         bridge = GetComponent<MiniGameBridge>();
//         if (!uiRoot) Debug.LogWarning("[ScreenAngleMiniGame] uiRoot 미지정!");
//         uiRoot.SetActive(false);

//         angleSlider.minValue = minAngle;
//         angleSlider.maxValue = maxAngle;
//         angleSlider.value = (minAngle + maxAngle) / 2f;
//         angleSlider.onValueChanged.AddListener(OnSliderChanged);
//     }

//     void Update()
//     {
//         if (!isMiniGameActive) return;

//         float currentAngle = angleSlider.value;
//         float efficiency = CalculateAngleEfficiency(currentAngle);

//         // 이상 각도 범위 유지 중이면 시간 누적
//         if (efficiency >= 0.45f)
//             successTime += Time.deltaTime;
//         else
//             successTime = Mathf.Max(0, successTime - Time.deltaTime);

//         // 진행률 계산 및 브로드캐스트
//         float progress = successTime / successGoal;
//         bridge?.UpdateProgress(progress);

//         if (progress >= 1f && !isFinished)
//         {
//             isFinished = true;
//             bridge?.Finish();
//             EndMiniGame();
//         }
//     }

//     void OnSliderChanged(float value)
//     {
//         screenImage.localRotation = Quaternion.Euler(0, 0, -value);
//         angleText.text = $"각도: {Mathf.Round(value)}°";
//     }

//     float CalculateAngleEfficiency(float angle)
//     {
//         if (angle < idealMin)
//             return (angle / idealMin) * 0.5f;
//         if (angle > idealMax)
//             return ((maxAngle - angle) / (maxAngle - idealMax)) * 0.5f;
//         return 0.5f; // 이상 범위 내
//     }

//     // 🎮 미니게임 시작
//     public void StartMiniGame()
//     {
//         // if (isMiniGameActive || isFinished) return;

//         uiRoot.SetActive(true);
//         isMiniGameActive = true;
//         successTime = 0f;

//         // 마우스 커서 활성화 & 카메라 잠금
//         Cursor.visible = true;
//         Cursor.lockState = CursorLockMode.None;

//         var fpCam = FindObjectOfType<FPCamera>();
//         if (fpCam != null)
//             fpCam.enabled = false;
//     }

//     // 🎮 미니게임 종료
//     public void EndMiniGame()
//     {
//         isMiniGameActive = false;
//         uiRoot.SetActive(false);

//         Cursor.visible = false;
//         Cursor.lockState = CursorLockMode.Locked;

//         var fpCam = FindObjectOfType<FPCamera>();
//         if (fpCam != null)
//             fpCam.enabled = true;
//     }
// }


using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MiniGameScreenLeverUI : MonoBehaviour
{
    [Header("UI References")]
    public RectTransform screenImage;   // 회전할 스크린 이미지
    public Slider angleSlider;          // 마우스로 조절할 슬라이더
    public TMP_Text angleText;          // 각도 표시 텍스트
    public GameObject uiRoot;           // UI 루트 (Canvas/Panel)

    [Header("Angle Settings")]
    public float minAngle = 0f;
    public float maxAngle = 90f;
    public float idealMin = 30f;
    public float idealMax = 45f;

    [Header("Game Settings")]
    public float successGoal = 5f;      // 이상 각도 유지 시간

    [Header("Puzzle Link")]
    public PuzzleTrigger puzzleTrigger; // 미니게임 성공 시 완료 처리할 퍼즐

    private float successTime = 0f;
    private bool isMiniGameActive = false;
    private bool isFinished = false;

    private MiniGameBridge bridge;

    void Start()
    {
        bridge = GetComponent<MiniGameBridge>();
        if (!uiRoot) Debug.LogWarning("[ScreenAngleMiniGame] uiRoot 미지정!");
        uiRoot.SetActive(false);

        angleSlider.minValue = minAngle;
        angleSlider.maxValue = maxAngle;
        angleSlider.value = (minAngle + maxAngle) / 2f;
        angleSlider.onValueChanged.AddListener(OnSliderChanged);
    }

    void Update()
    {
        if (!isMiniGameActive) return;

        float currentAngle = angleSlider.value;
        float efficiency = CalculateAngleEfficiency(currentAngle);

        // 이상 각도 범위 유지 중이면 시간 누적
        if (efficiency >= 0.45f)
            successTime += Time.deltaTime;
        else
            successTime = Mathf.Max(0, successTime - Time.deltaTime);

        // 진행률 계산 및 브로드캐스트
        float progress = successTime / successGoal;
        bridge?.UpdateProgress(progress);

        if (progress >= 1f && !isFinished)
        {
            isFinished = true;

            // MiniGameBridge 종료
            bridge?.Finish();

            // 퍼즐 완료 처리
            if (puzzleTrigger != null)
                puzzleTrigger.CompletePuzzle();

            EndMiniGame();
            Debug.Log("미니게임 성공! 퍼즐 완료 처리됨");
        }
    }

    void OnSliderChanged(float value)
    {
        screenImage.localRotation = Quaternion.Euler(0, 0, -value);
        angleText.text = $"각도: {Mathf.Round(value)}°";
    }

    float CalculateAngleEfficiency(float angle)
    {
        if (angle < idealMin)
            return (angle / idealMin) * 0.5f;
        if (angle > idealMax)
            return ((maxAngle - angle) / (maxAngle - idealMax)) * 0.5f;
        return 0.5f; // 이상 범위 내
    }

    // 🎮 미니게임 시작
    public void StartMiniGame()
    {
        uiRoot.SetActive(true);
        isMiniGameActive = true;
        successTime = 0f;
        isFinished = false;

        // 마우스 커서 활성화 & 카메라 잠금
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        var fpCam = FindObjectOfType<FPCamera>();
        if (fpCam != null)
            fpCam.enabled = false;
    }

    // 🎮 미니게임 종료
    public void EndMiniGame()
    {
        isMiniGameActive = false;
        uiRoot.SetActive(false);

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        var fpCam = FindObjectOfType<FPCamera>();
        if (fpCam != null)
            fpCam.enabled = true;
    }
}

