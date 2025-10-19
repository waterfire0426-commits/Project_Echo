// using UnityEngine;

// public class FloatRemovalMiniGame : MonoBehaviour
// {
//     [Header("UI References")]
//     public RectTransform marker;       // 움직이는 마커
//     public RectTransform targetZone;   // 목표 영역 이미지
//     public RectTransform gauge;        // 게이지 배경
//     public GameObject uiRoot;          // UI 루트(Canvas나 Panel)

//     [Header("Game Settings")]
//     public float speed = 2f;           // 마커 이동 속도
//     public float successGoal = 5f;     // 목표 영역 유지 시간(초)

//     private float currentPos = 0.5f;   // 0~1 범위
//     private float successTime = 0f;
//     private float targetZoneMin;
//     private float targetZoneMax;
//     private bool isMiniGameActive = false;

//     private MiniGameBridge bridge;

//     void Start()
//     {
//         bridge = GetComponent<MiniGameBridge>();
//         if (!uiRoot) Debug.LogWarning("[FloatRemovalMiniGame] uiRoot 미지정!");
//         uiRoot.SetActive(false); // 게임 시작 시 UI 숨김

//         // TargetZone 위치를 0~1 비율로 변환
//         float gaugeHeight = gauge.rect.height;
//         float zoneYMin = targetZone.anchoredPosition.y - targetZone.rect.height / 2f;
//         float zoneYMax = targetZone.anchoredPosition.y + targetZone.rect.height / 2f;

//         targetZoneMin = Mathf.Clamp01((zoneYMin / gaugeHeight) + 0.5f);
//         targetZoneMax = Mathf.Clamp01((zoneYMax / gaugeHeight) + 0.5f);
//     }

//     void Update()
//     {
//         if (!isMiniGameActive) return;

//         // R키로 마커 이동
//         currentPos += Input.GetKey(KeyCode.R) ? Time.deltaTime * speed : -Time.deltaTime * speed;

//         // 마커가 게이지 밖으로 나가지 않도록 클램프
//         float gaugeHeight = gauge.rect.height;
//         float markerHalfHeight = marker.rect.height / 2f;
//         float minPos = markerHalfHeight / gaugeHeight;
//         float maxPos = 1f - minPos;
//         currentPos = Mathf.Clamp(currentPos, minPos, maxPos);

//         // 마커 위치 적용
//         marker.anchoredPosition = new Vector2(0, (currentPos - 0.5f) * gaugeHeight);

//         // 목표 영역 유지 시간 계산
//         if (currentPos >= targetZoneMin && currentPos <= targetZoneMax)
//             successTime += Time.deltaTime;
//         else
//             successTime = Mathf.Max(0, successTime - Time.deltaTime);

//         // 진행률 브로드캐스트
//         float progress = successTime / successGoal;
//         bridge?.UpdateProgress(progress);

//         // 성공 시 종료
//         if (progress >= 1f)
//         {
//             bridge?.Finish();
//             EndMiniGame();
//         }
//     }

//     // 미니게임 시작
//     public void StartMiniGame()
//     {
//         isMiniGameActive = true;
//         uiRoot.SetActive(true);
//         currentPos = 0.5f;
//         successTime = 0f;

//         // 🎯 마우스 커서 활성화 & 카메라 회전 잠금
//         Cursor.visible = true;
//         Cursor.lockState = CursorLockMode.None;

//         // 필요하다면 플레이어 카메라 비활성화
//         var fpCam = FindObjectOfType<FPCamera>();
//         if (fpCam != null)
//             fpCam.enabled = false;
//     }

//     //미니게임 종료
//     public void EndMiniGame()
//     {
//         isMiniGameActive = false;
//         uiRoot.SetActive(false);
//         currentPos = 0.5f;
//         successTime = 0f;

//         Cursor.visible = false;
//         Cursor.lockState = CursorLockMode.Locked;

//         // 카메라 다시 활성화
//         var fpCam = FindObjectOfType<FPCamera>();
//         if (fpCam != null)
//             fpCam.enabled = true;
//     }
// }

using UnityEngine;

public class FloatRemovalMiniGame : MonoBehaviour
{
    [Header("UI References")]
    public RectTransform marker;       // 움직이는 마커
    public RectTransform targetZone;   // 목표 영역 이미지
    public RectTransform gauge;        // 게이지 배경
    public GameObject uiRoot;          // UI 루트(Canvas나 Panel)

    [Header("Game Settings")]
    public float speed = 2f;           // 마커 이동 속도
    public float successGoal = 5f;     // 목표 영역 유지 시간(초)

    [Header("Puzzle Link")]
    public PuzzleTrigger puzzleTrigger; // 미니게임 성공 시 완료 처리할 퍼즐

    private float currentPos = 0.5f;   
    private float successTime = 0f;
    private float targetZoneMin;
    private float targetZoneMax;
    private bool isMiniGameActive = false;

    private MiniGameBridge bridge;

    void Start()
    {
        bridge = GetComponent<MiniGameBridge>();
        if (!uiRoot) Debug.LogWarning("[FloatRemovalMiniGame] uiRoot 미지정!");
        uiRoot.SetActive(false); // 게임 시작 시 UI 숨김

        // TargetZone 위치를 0~1 비율로 변환
        float gaugeHeight = gauge.rect.height;
        float zoneYMin = targetZone.anchoredPosition.y - targetZone.rect.height / 2f;
        float zoneYMax = targetZone.anchoredPosition.y + targetZone.rect.height / 2f;

        targetZoneMin = Mathf.Clamp01((zoneYMin / gaugeHeight) + 0.5f);
        targetZoneMax = Mathf.Clamp01((zoneYMax / gaugeHeight) + 0.5f);
    }

    void Update()
    {
        if (!isMiniGameActive) return;

        // R키로 마커 이동
        currentPos += Input.GetKey(KeyCode.R) ? Time.deltaTime * speed : -Time.deltaTime * speed;

        // 마커가 게이지 밖으로 나가지 않도록 클램프
        float gaugeHeight = gauge.rect.height;
        float markerHalfHeight = marker.rect.height / 2f;
        float minPos = markerHalfHeight / gaugeHeight;
        float maxPos = 1f - minPos;
        currentPos = Mathf.Clamp(currentPos, minPos, maxPos);

        // 마커 위치 적용
        marker.anchoredPosition = new Vector2(0, (currentPos - 0.5f) * gaugeHeight);

        // 목표 영역 유지 시간 계산
        if (currentPos >= targetZoneMin && currentPos <= targetZoneMax)
            successTime += Time.deltaTime;
        else
            successTime = Mathf.Max(0, successTime - Time.deltaTime);

        // 진행률 브로드캐스트
        float progress = successTime / successGoal;
        bridge?.UpdateProgress(progress);

        // 성공 시 종료 + 퍼즐 완료 처리
        if (progress >= 1f)
        {
            bridge?.Finish();

            if (puzzleTrigger != null)
                puzzleTrigger.CompletePuzzle();

            EndMiniGame();
            Debug.Log("미니게임 성공! 퍼즐 완료 처리됨");
        }
    }

    // 미니게임 시작
    public void StartMiniGame()
    {
        isMiniGameActive = true;
        uiRoot.SetActive(true);
        currentPos = 0.5f;
        successTime = 0f;

        // 🎯 마우스 커서 활성화 & 카메라 회전 잠금
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        // 필요하다면 플레이어 카메라 비활성화
        var fpCam = FindObjectOfType<FPCamera>();
        if (fpCam != null)
            fpCam.enabled = false;
    }

    // 미니게임 종료
    public void EndMiniGame()
    {
        isMiniGameActive = false;
        uiRoot.SetActive(false);
        currentPos = 0.5f;
        successTime = 0f;

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        // 카메라 다시 활성화
        var fpCam = FindObjectOfType<FPCamera>();
        if (fpCam != null)
            fpCam.enabled = true;
    }
}
