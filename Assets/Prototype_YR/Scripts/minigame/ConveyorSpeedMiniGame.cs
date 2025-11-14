// using UnityEngine;
// using TMPro;

// public class ConveyorSpeedMiniGame : MonoBehaviour
// {
//     [Header("UI References")]
//     public TextMeshProUGUI speedText;
//     public TextMeshProUGUI timerText;
//     public GameObject uiRoot;

//     [Header("Game Settings")]
//     public float lowSpeed = 1f;
//     public float highSpeed = 2f;
//     public float successHoldTime = 5f;

//     [Header("Puzzle Link")]
//     public int puzzleID = 3;               // 연결할 퍼즐 ID
//     public PuzzleTrigger puzzleTrigger;     // 완료 신호 보낼 PuzzleTrigger

//     private bool isHighSpeed = true;
//     private bool isMiniGameActive = false;
//     private float holdTimer = 0f;

//     private MiniGameBridge bridge;

//     void Start()
//     {
//         bridge = GetComponent<MiniGameBridge>();
//         if (!uiRoot) Debug.LogWarning("[ConveyorSpeedMiniGame] uiRoot 미지정!");
//         uiRoot.SetActive(false);

//         UpdateSpeedUI();
//         UpdateTimerUI();
//     }

//     void Update()
//     {
//         if (!isMiniGameActive) return;

//         // R 키 입력 시 속도 토글
//         if (Input.GetKeyDown(KeyCode.R))
//         {
//             isHighSpeed = !isHighSpeed;
//             UpdateSpeedUI();

//             if (isHighSpeed) holdTimer = 0f; // High로 돌아가면 타이머 초기화
//         }

//         // Low 상태 유지 시간 증가
//         if (!isHighSpeed)
//         {
//             holdTimer += Time.deltaTime;
//             if (holdTimer >= successHoldTime)
//             {
//                 // 성공 처리
//                 bridge?.Finish();

//                 // 퍼즐 완료 처리
//                 if (puzzleTrigger != null)
//                     puzzleTrigger.CompletePuzzle();

//                 EndMiniGame();
//                 Debug.Log("미니게임 성공!");
//             }
//         }

//         UpdateTimerUI();

//         // 진행률 브로드캐스트
//         float progress = Mathf.Clamp01(holdTimer / successHoldTime);
//         bridge?.UpdateProgress(progress);
//     }

//     void UpdateSpeedUI()
//     {
//         if (speedText != null)
//             speedText.text = "Speed: " + (isHighSpeed ? "High" : "Low");
//     }

//     void UpdateTimerUI()
//     {
//         if (timerText != null)
//             timerText.text = !isHighSpeed ? $"Hold Low: {Mathf.Ceil(successHoldTime - holdTimer)}s" : "";
//     }

//     public float GetCurrentSpeed()
//     {
//         return isHighSpeed ? highSpeed : lowSpeed;
//     }

//     public void StartMiniGame()
//     {
//         isMiniGameActive = true;
//         isHighSpeed = true;
//         holdTimer = 0f;
//         uiRoot.SetActive(true);

//         UpdateSpeedUI();
//         UpdateTimerUI();

//         Cursor.visible = true;
//         Cursor.lockState = CursorLockMode.None;

//         var fpCam = FindObjectOfType<FPCamera>();
//         if (fpCam != null)
//             fpCam.enabled = false;
//     }

//     public void EndMiniGame()
//     {
//         isMiniGameActive = false;
//         holdTimer = 0f;
//         uiRoot.SetActive(false);

//         Cursor.visible = false;
//         Cursor.lockState = CursorLockMode.Locked;

//         var fpCam = FindObjectOfType<FPCamera>();
//         if (fpCam != null)
//             fpCam.enabled = true;
//     }
// }


using UnityEngine;
using TMPro;

public class ConveyorSpeedMiniGame : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI speedText;       // 현재 속도 표시
    public TextMeshProUGUI targetText;      // 목표 속도 표시
    public TextMeshProUGUI timerText;       // 유지 시간 표시
    public TextMeshProUGUI modeText;        // High / Low 상태 표시
    public GameObject uiRoot;

    [Header("Game Settings")]
    public float minSpeed = 0f;             // 최소 속도
    public float maxSpeed = 5f;             // 최대 속도
    public float speedChangePerSecond = 0.25f; // 초당 변화량
    public float successHoldTime = 5f;      // 목표 근처 유지 시간
    public float allowedRange = 0.2f;       // 목표 근처 판정 범위

    [Header("Puzzle Link")]
    public PuzzleTrigger puzzleTrigger;

    private float currentSpeed = 0f;
    private float targetSpeed = 0f;
    private float holdTimer = 0f;
    private bool isMiniGameActive = false;
    private bool isHighSpeed = false;

    private MiniGameBridge bridge;

    void Start()
    {
        bridge = GetComponent<MiniGameBridge>();
        if (!uiRoot) Debug.LogWarning("[ConveyorSpeedMiniGame] uiRoot 미지정!");
        uiRoot.SetActive(false);
    }

    void Update()
    {
        if (!isMiniGameActive) return;

        // R키로 High / Low 전환
        if (Input.GetKeyDown(KeyCode.R))
        {
            isHighSpeed = !isHighSpeed;
        }

        // 속도 증가 / 감소
        float delta = (isHighSpeed ? speedChangePerSecond : -speedChangePerSecond) * Time.deltaTime;
        currentSpeed = Mathf.Clamp(currentSpeed + delta, minSpeed, maxSpeed);

        // 목표 속도와의 차이 계산
        float diff = Mathf.Abs(currentSpeed - targetSpeed);
        if (diff <= allowedRange)
            holdTimer += Time.deltaTime;
        else
            holdTimer = Mathf.Max(0, holdTimer - Time.deltaTime * 0.5f);

        // 진행률 계산
        float progress = Mathf.Clamp01(holdTimer / successHoldTime);
        bridge?.UpdateProgress(progress);

        // 성공 판정
        if (holdTimer >= successHoldTime)
        {
            bridge?.Finish();
            puzzleTrigger?.CompletePuzzle();
            EndMiniGame();
            Debug.Log("🎯 미니게임 성공! 목표 속도 유지 완료");
        }

        // UI 갱신
        UpdateUI();
    }

    void UpdateUI()
    {
        if (speedText != null)
            speedText.text = $"현재 속도: {currentSpeed:F2}";
        if (targetText != null)
            targetText.text = $"목표 속도: {targetSpeed:F2}";
        if (timerText != null)
            timerText.text = $"유지 시간: {holdTimer:F1} / {successHoldTime}초";
        if (modeText != null)
            modeText.text = isHighSpeed ? "High" : "Low";
    }

    public void StartMiniGame()
    {
        uiRoot.SetActive(true);
        isMiniGameActive = true;
        holdTimer = 0f;
        isHighSpeed = false;
        currentSpeed = Random.Range(minSpeed, maxSpeed);
        targetSpeed = Random.Range(minSpeed + 1f, maxSpeed - 1f); // 목표 속도는 범위 내 랜덤 설정

        UpdateUI();

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        var fpCam = FindObjectOfType<FPCamera>();
        if (fpCam != null)
            fpCam.enabled = false;
    }

    public void EndMiniGame()
    {
        isMiniGameActive = false;
        uiRoot.SetActive(false);
        holdTimer = 0f;

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        var fpCam = FindObjectOfType<FPCamera>();
        if (fpCam != null)
            fpCam.enabled = true;
    }
}
