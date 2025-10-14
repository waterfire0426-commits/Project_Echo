using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMove : MonoBehaviour
{
    CharacterController cc;
    public Camera cam;                       // 비우면 자동 할당

    [Header("Speed")]
    public float walkSpeed = 7f;
    public float sprintSpeed = 10f;

    [Header("Jump & Gravity")]
    public float jumpPower = 10f;
    public float gravity = -18f;
    public float groundStick = -0.5f;       // 지면에 살짝 붙이는 값

    [Header("Stamina")]
    public float staminaMax = 100f;
    public float stamina = 100f;
    public float sprintCostPerSec = 20f;
    public float recoverPerSec = 12f;
    public float minToSprint = 10f;         // 이 이상 회복돼야 다시 달리기 허용 (나중에 바 만들때 다시 점검)

    [Header("State (read-only)")]
    public bool isSprinting { get; private set; }
    public bool isGrounded  { get; private set; }

    float yVel = 0f;

    void Awake()
    {
        cc = GetComponent<CharacterController>();
        if (!cam) cam = Camera.main;
        stamina = Mathf.Clamp(stamina, 0f, staminaMax);
    }

    void Update()
    {
        // --- 입력 ---
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        bool sprintHeld = Input.GetKey(KeyCode.LeftShift);

        // --- 이동 방향(카메라 기준) ---
        Vector3 f = cam.transform.forward; f.y = 0f; f.Normalize();
        Vector3 r = cam.transform.right;   r.y = 0f; r.Normalize();
        Vector3 moveXZ = (f * v + r * h);
        if (moveXZ.sqrMagnitude > 1f) moveXZ.Normalize();

        // --- 지상/중력/점프 ---
        isGrounded = cc.isGrounded;
        if (isGrounded && yVel < 0f) yVel = groundStick;
        if (isGrounded && Input.GetButtonDown("Jump")) yVel = jumpPower;
        yVel += gravity * Time.deltaTime;

        // --- 스프린트 조건 ---
        bool wantMove = moveXZ.sqrMagnitude > 0.0001f;
        bool canSprintNow = sprintHeld && wantMove && isGrounded && stamina > 0f && stamina >= (isSprinting ? 0f : minToSprint);
        isSprinting = canSprintNow;

        // --- 속도 선택 ---
        float speed = isSprinting ? sprintSpeed : walkSpeed;

        // --- 경사면 투영(한 줄 핵심) ---
        Vector3 groundNormal = Vector3.up;
        if (Physics.Raycast(transform.position + Vector3.up * 0.1f, Vector3.down, out RaycastHit gh, 1.2f))
            groundNormal = gh.normal;
        Vector3 planar = Vector3.ProjectOnPlane(moveXZ * speed, groundNormal);

        // --- 입력 없고 지상일 때 즉시 정지(미끄러짐 최소화) ---
        if (isGrounded && !wantMove) planar = Vector3.zero;

        // --- 최종 이동 ---
        Vector3 vel = new Vector3(planar.x, yVel, planar.z);
        cc.Move(vel * Time.deltaTime);

        // --- 스태미나 ---
        if (isSprinting)
        {
            stamina -= sprintCostPerSec * Time.deltaTime;
            if (stamina < 0f) stamina = 0f;
        }
        else
        {
            stamina += recoverPerSec * Time.deltaTime;
            if (stamina > staminaMax) stamina = staminaMax;
        }
    }
}
