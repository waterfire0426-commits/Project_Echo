using UnityEngine;
using TMPro;

public class ConveyorSpeedMiniGame : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI speedText;
    public TextMeshProUGUI timerText;
    public GameObject uiRoot;

    [Header("Game Settings")]
    public float lowSpeed = 1f;
    public float highSpeed = 2f;
    public float successHoldTime = 5f;

    [Header("Puzzle Link")]
    public int puzzleID = 3;               // 연결할 퍼즐 ID
    public PuzzleTrigger puzzleTrigger;     // 완료 신호 보낼 PuzzleTrigger

    private bool isHighSpeed = true;
    private bool isMiniGameActive = false;
    private float holdTimer = 0f;

    private MiniGameBridge bridge;

    void Start()
    {
        bridge = GetComponent<MiniGameBridge>();
        if (!uiRoot) Debug.LogWarning("[ConveyorSpeedMiniGame] uiRoot 미지정!");
        uiRoot.SetActive(false);

        UpdateSpeedUI();
        UpdateTimerUI();
    }

    void Update()
    {
        if (!isMiniGameActive) return;

        // R 키 입력 시 속도 토글
        if (Input.GetKeyDown(KeyCode.R))
        {
            isHighSpeed = !isHighSpeed;
            UpdateSpeedUI();

            if (isHighSpeed) holdTimer = 0f; // High로 돌아가면 타이머 초기화
        }

        // Low 상태 유지 시간 증가
        if (!isHighSpeed)
        {
            holdTimer += Time.deltaTime;
            if (holdTimer >= successHoldTime)
            {
                // 성공 처리
                bridge?.Finish();

                // 퍼즐 완료 처리
                if (puzzleTrigger != null)
                    puzzleTrigger.CompletePuzzle();

                EndMiniGame();
                Debug.Log("미니게임 성공!");
            }
        }

        UpdateTimerUI();

        // 진행률 브로드캐스트
        float progress = Mathf.Clamp01(holdTimer / successHoldTime);
        bridge?.UpdateProgress(progress);
    }

    void UpdateSpeedUI()
    {
        if (speedText != null)
            speedText.text = "Speed: " + (isHighSpeed ? "High" : "Low");
    }

    void UpdateTimerUI()
    {
        if (timerText != null)
            timerText.text = !isHighSpeed ? $"Hold Low: {Mathf.Ceil(successHoldTime - holdTimer)}s" : "";
    }

    public float GetCurrentSpeed()
    {
        return isHighSpeed ? highSpeed : lowSpeed;
    }

    public void StartMiniGame()
    {
        isMiniGameActive = true;
        isHighSpeed = true;
        holdTimer = 0f;
        uiRoot.SetActive(true);

        UpdateSpeedUI();
        UpdateTimerUI();

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        var fpCam = FindObjectOfType<FPCamera>();
        if (fpCam != null)
            fpCam.enabled = false;
    }

    public void EndMiniGame()
    {
        isMiniGameActive = false;
        holdTimer = 0f;
        uiRoot.SetActive(false);

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        var fpCam = FindObjectOfType<FPCamera>();
        if (fpCam != null)
            fpCam.enabled = true;
    }
}

