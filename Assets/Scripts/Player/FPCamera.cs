using UnityEngine;

public class FPCamera : MonoBehaviour
{
    // 기준점(XZ) + 현재 시야 높이
    float baseX, baseZ;
    float viewY;

    [Header("References")]
    public Transform playerRoot;        // 플레이어 본체(좌우 회전)
    public CharacterController cc;      // 속도/지상 상태
    public PlayerMove motor;            // 스프린트 상태(선택)

    [Header("Mouse Look")]
    public float mouseXSens = 500f;
    public float mouseYSens = 500f;
    public float pitchMin = -80f;
    public float pitchMax = 80f;

    [Header("Crouch View")]
    public KeyCode crouchKey = KeyCode.LeftControl; // 누르는 동안 앉기
    public float standHeight = 1.6f;   // 카메라 로컬 y
    public float crouchHeight = 1.1f;  // 카메라 로컬 y
    public float crouchLerp = 12f;

    [Header("Head Bob (stride-based)")]
    public float walkBobAmp = 0.03f;
    public float sprintBobAmp = 0.05f;
    public float crouchBobAmp = 0.02f;
    public float bobLerp = 10f;        // 흔들림 보간 속도

    [Header("Cursor Lock")]
    public bool autoLockCursor = true;   // 항상 락 유지
    public bool isPaused = false;        // 일시적으로 UI 열면 true로

    void OnApplicationFocus(bool hasFocus) {
        if (hasFocus && autoLockCursor && !isPaused) {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible   = false;
        }
    }

    // 보폭 길이(미터) — 속도와 무관하게 자연스러운 템포를 위해 사용
    public float walkStride = 1.8f;
    public float sprintStride = 2.4f;
    public float crouchStride = 1.4f;

    float yaw, pitch;
    float stepAccum = 0f;              // 누적 이동거리(m)
    Vector3 bobOffset = Vector3.zero;

    void Awake()
    {
        // 참조 자동 바인딩(비워 두어도 동작)
        if (!playerRoot) playerRoot = transform.root;
        if (playerRoot && !cc)    cc    = playerRoot.GetComponent<CharacterController>();
        if (playerRoot && !motor) motor = playerRoot.GetComponent<PlayerMove>();

        // 기준 좌표 설정
        baseX = transform.localPosition.x;
        baseZ = transform.localPosition.z;

        if (Mathf.Approximately(standHeight, 0f))
            standHeight = transform.localPosition.y; // 에디터 값이 0이면 현재값 사용
        viewY = standHeight;

        if (playerRoot) yaw = playerRoot.eulerAngles.y;

        // 시작은 정면(pitch=0)
        pitch = 0f;
        if (playerRoot) playerRoot.rotation     = Quaternion.Euler(0f, yaw, 0f);
        transform.localRotation = Quaternion.Euler(pitch, 0f, 0f);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible   = false;
    }

    void Update()
    {
        if (!playerRoot || !cc) return;

        // ✅ 추가: 일시정지 상태면 회전/움직임 처리 안 함
        if (isPaused || Time.timeScale == 0f)
            return;

        // --- Mouse look ---
        float mx = Input.GetAxisRaw("Mouse X") * mouseXSens;
        float my = Input.GetAxisRaw("Mouse Y") * mouseYSens;

        yaw += mx;
        pitch = Mathf.Clamp(pitch - my, pitchMin, pitchMax);

        playerRoot.rotation = Quaternion.Euler(0f, yaw, 0f);   // 좌우(Yaw)는 본체
        transform.localRotation = Quaternion.Euler(pitch, 0f, 0f); // 상하(Pitch)는 카메라

        // --- Crouch view (hold) ---
        bool crouching = Input.GetKey(crouchKey);
        float targetY = crouching ? crouchHeight : standHeight;
        viewY = Mathf.Lerp(viewY, targetY, Time.deltaTime * crouchLerp);

        // --- Head bob (stride-based) ---
        Vector3 planarVel = cc.velocity; planarVel.y = 0f;
        float speed = planarVel.magnitude;
        bool grounded = cc.isGrounded;
        bool moving = speed > 0.05f && grounded;

        float amp, stride;
        if (crouching) { amp = crouchBobAmp; stride = crouchStride; }
        else if (motor && motor.isSprinting) { amp = sprintBobAmp; stride = sprintStride; }
        else { amp = walkBobAmp; stride = walkStride; }

        if (moving)
        {
            // 누적 거리 → 위상: (거리/보폭)*2π
            stepAccum += speed * Time.deltaTime;               // m
            float phase = (stepAccum / stride) * Mathf.PI * 2; // rad

            float s = Mathf.Sin(phase);
            float c = Mathf.Cos(phase * 0.5f);
            Vector3 targetBob = new Vector3(c * amp * 0.5f, s * amp, 0f); // Abs 안 씀(상하 균형)
            bobOffset = Vector3.Lerp(bobOffset, targetBob, Time.deltaTime * bobLerp);
        }
        else
        {
            bobOffset = Vector3.Lerp(bobOffset, Vector3.zero, Time.deltaTime * bobLerp);
            // 멈추면 보행거리 천천히 초기화(선택): stepAccum을 0으로 두면 리셋 템포가 일정함
            // stepAccum = 0f;
        }

        // --- 최종 위치(기준 XZ + 보간된 높이 + 보폭 오프셋) ---
        transform.localPosition = new Vector3(
            baseX + bobOffset.x,
            viewY + bobOffset.y,
            baseZ + bobOffset.z
        );

        // Update() 맨 아래나 LateUpdate()에 아래 블럭 추가
        if (autoLockCursor && !isPaused)
        {
            // 에디터/OS가 잠깐 락을 풀어도 즉시 되잡음
            if (Application.isFocused && Cursor.lockState != CursorLockMode.Locked)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }
    }
    // 이 부분 추가_YR 미니게임 입력 잠금/복구용용
    public void DisableInput()
    {
        isPaused = true;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Debug.Log("[FPCamera] 입력 잠금됨 (미니게임 중)");
    }

    public void EnableInput()
    {
        isPaused = false;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        Debug.Log("[FPCamera] 입력 복구됨 (미니게임 종료)");
    }

}
