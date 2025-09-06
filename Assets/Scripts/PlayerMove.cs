using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMove : MonoBehaviour
{
    CharacterController cc;

    [Header("Gravity & Jump")]
    public float gravity = -20f;
    float yVelocity = 0f;
    public float jumpPower = 10f;
    public bool isJumping = false;

    [Header("Speeds")]
    public float walkSpeed = 7f;
    public float sprintSpeed = 10f;
    public float crouchSpeed = 3.5f;

    // 스프린트 상태 캐시
    bool isSprintingNow = false;

    [Header("Stamina")]
    public float staminaMax = 100f;
    public float stamina = 100f;
    public float sprintCostPerSec = 20f;
    public float recoverPerSec = 12f;
    public float minToSprint = 10f;   // 이 값 이상 회복돼야 다시 달리기 허용
    public bool canSprint = true;

    // 안 써도 문제 없음
    public System.Action<float, float> OnStaminaChanged; // (current, max)
    public System.Action<bool> OnSprintStateChanged;     // true=스프린트 시작, false=종료

    [Header("Crouch")]
    public KeyCode crouchKey = KeyCode.LeftControl;
    public bool crouchToggle = false; // true=토글, false=누르는 동안만
    bool isCrouching = false;

    [Header("Stealth Hooks")]
    public float noiseLevel { get; private set; } = 0f;
    public float walkNoise = 0.3f;
    public float sprintNoise = 0.8f;
    public float crouchNoise = 0.1f;

    [Header("Footstep")]
    public AudioSource footSrc;             
    public AudioClip stepWalk, stepSprint, stepCrouch;
    public float stepDistWalk = 2.0f;         // 발소리 간격(거리 기반)
    public float stepDistSprint = 1.3f;
    public float stepDistCrouch = 2.4f;
    float stepAccum = 0f;

    void Start()
    {
        cc = GetComponent<CharacterController>();
        stamina = Mathf.Clamp(stamina, 0f, staminaMax);
    }

    void Update()
    {
        // ---- 입력 ----
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        // 카메라 기준 '수평' 이동 벡터 (pitch 영향 제거)
        Vector3 camF = Camera.main.transform.forward; camF.y = 0f; camF.Normalize();
        Vector3 camR = Camera.main.transform.right;  camR.y = 0f; camR.Normalize();
        Vector3 moveXZ = (camF * v + camR * h).normalized;

        // 지상 체크
        bool grounded = cc.isGrounded;

        // 점프 (앉은 상태에선 점프 X)
        if (Input.GetButtonDown("Jump") && grounded && !isCrouching)
        {
            yVelocity = jumpPower;
            isJumping = true;
        }

        // 착지 처리
        if (grounded && yVelocity < 0f)
        {
            yVelocity = -2f;   // 지면에 살짝 붙여두기
            isJumping = false;
        }

        // 중력
        yVelocity += gravity * Time.deltaTime;

        // ---- 앉기 ----
        if (crouchToggle)
        {
            if (Input.GetKeyDown(crouchKey)) isCrouching = !isCrouching;
        }
        else
        {
            isCrouching = Input.GetKey(crouchKey);
        }

        // ---- 스프린트 ----
        bool sprintHeld = Input.GetKey(KeyCode.LeftShift);
        bool wantMove = moveXZ.sqrMagnitude > 0.0001f;

        // 입력 + 허용 + 스태미나 + 이동의지 + (앉기 중 X)
        bool canSprintNow = sprintHeld && canSprint && stamina > 0f && wantMove && !isCrouching;

        // 상태 변화시에만 이벤트
        bool prevSprint = isSprintingNow;
        isSprintingNow = canSprintNow;
        if (isSprintingNow != prevSprint)
            OnSprintStateChanged?.Invoke(isSprintingNow);

        // 최종 속도 선택(즉시 전환)
        float speed = isCrouching ? crouchSpeed : (isSprintingNow ? sprintSpeed : walkSpeed);

        // 이동
        Vector3 velocity = moveXZ * speed;
        velocity.y = yVelocity;
        cc.Move(velocity * Time.deltaTime);

        // ---- 스태미나 소모/회복 ----
        if (isSprintingNow)
        {
            stamina -= sprintCostPerSec * Time.deltaTime;
            if (stamina <= 0f)
            {
                stamina = 0f;
                canSprint = false;
                if (isSprintingNow) { isSprintingNow = false; OnSprintStateChanged?.Invoke(false); }
            }
        }
        else
        {
            stamina += recoverPerSec * Time.deltaTime;
            if (stamina >= staminaMax) stamina = staminaMax;
            if (!canSprint && stamina >= minToSprint) canSprint = true;
        }
        OnStaminaChanged?.Invoke(stamina, staminaMax);

        // ---- 소음 레벨 산출 ----
        Vector3 planarVel = cc.velocity; planarVel.y = 0f;
        float speedMag = planarVel.magnitude;
        bool moving = speedMag > 0.1f;

        if (!moving)                 noiseLevel = 0f;
        else if (isCrouching)        noiseLevel = crouchNoise;
        else if (isSprintingNow)     noiseLevel = sprintNoise;
        else                         noiseLevel = walkNoise;

        // ---- 발소리 ----
        stepAccum += speedMag * Time.deltaTime;
        float stepDist = isCrouching ? stepDistCrouch : (isSprintingNow ? stepDistSprint : stepDistWalk);

        if (moving && grounded && stepAccum >= stepDist)
        {
            stepAccum = 0f;
            var clip = isCrouching ? stepCrouch : (isSprintingNow ? stepSprint : stepWalk);
            if (clip && footSrc) footSrc.PlayOneShot(clip);
        }
    }
}
