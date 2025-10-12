using UnityEngine;

// 이 스크립트는 CharacterController 컴포넌트가 반드시 필요합니다.
[RequireComponent(typeof(CharacterController))]
public class Map_PlayerController : MonoBehaviour
{
    [Header("플레이어 설정")]
    public float moveSpeed = 5.0f;          // 걷는 속도
    public float sprintSpeed = 8.0f;        // 뛰는 속도

    [Header("점프 및 중력 설정")]
    public float jumpHeight = 1.5f;         // 점프 높이
    public float gravity = -9.81f;          // 중력 값

    [Header("카메라 설정")]
    public Transform playerCamera;          // 플레이어 카메라의 Transform
    public float mouseSensitivity = 2.0f;   // 마우스 감도
    public float lookUpClamp = -70f;        // 카메라 위쪽 시야 제한 각도
    public float lookDownClamp = 80f;       // 카메라 아래쪽 시야 제한 각도

    private CharacterController characterController;
    private Vector3 playerVelocity;         // 플레이어의 현재 속도 (중력 포함)
    private bool isGrounded;                // 플레이어가 땅에 닿아있는지 여부
    private float xRotation = 0f;           // 카메라의 상하 회전을 저장할 변수

    void Start()
    {
        characterController = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        // 매 프레임마다 땅에 있는지 체크
        isGrounded = characterController.isGrounded;

        // 조작 처리 함수들을 순서대로 호출
        HandleMovement();
        HandleLook();
        HandleGravityAndJump();
    }

    private void HandleMovement()
    {
        // LeftShift 키를 누르고 있으면 뛰는 속도, 아니면 걷는 속도를 적용
        float currentSpeed = Input.GetKey(KeyCode.LeftShift) ? sprintSpeed : moveSpeed;

        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");

        // 이동 방향 계산
        Vector3 moveDirection = transform.forward * verticalInput + transform.right * horizontalInput;

        // CharacterController를 사용하여 플레이어 이동
        characterController.Move(moveDirection * currentSpeed * Time.deltaTime);
    }

    private void HandleLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        transform.Rotate(Vector3.up * mouseX);

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, lookUpClamp, lookDownClamp);

        playerCamera.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
    }

    // 중력 및 점프 처리
    private void HandleGravityAndJump()
    {
        // 땅에 닿아 있고, 수직 속도가 0보다 작으면 속도를 리셋 (미끄러짐 방지)
        if (isGrounded && playerVelocity.y < 0)
        {
            playerVelocity.y = -2f;
        }

        // 점프 키(Space)를 누르고 땅에 닿아 있으면 점프
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            // 물리 공식에 따라 점프에 필요한 수직 속도를 계산
            playerVelocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        // 매 프레임마다 중력을 수직 속도에 적용
        playerVelocity.y += gravity * Time.deltaTime;

        // 최종적으로 계산된 수직 속도를 플레이어에게 적용
        characterController.Move(playerVelocity * Time.deltaTime);
    }
}