using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

/// <summary>
/// F키로 Off → White → UV 순환.
/// 방호복(슈트) 착용 상태(ISuitReceiver.IsSuited)가 아니면 입력 무시.
/// </summary>
public class FlashlightController : MonoBehaviour
{
    public enum Mode { Off = 0, White = 1, UV = 2 }

    [Header("References")]
    [Tooltip("화이트 라이트(Flashlight/WhiteLight)의 Light 컴포넌트")]
    public Light whiteLight;
    [Tooltip("UV 라이트(Flashlight/UVLight)의 Light 컴포넌트")]
    public Light uvLight;
    [Tooltip("토글 사운드(선택)")]
    public AudioSource sfx;

    [Header("Suit Requirement")]
    [Tooltip("방호복 착용 상태에서만 사용 가능하게 할지")]
    public bool requireSuit = true;
    [Tooltip("비워두면 부모에서 ISuitReceiver 자동 탐색 (보통 PlayerMove)")]
    public MonoBehaviour suitProvider;
    private ISuitReceiver suit;

    [Header("Input")]
    public KeyCode legacyToggleKey = KeyCode.F;
    public float debounceSeconds = 0.15f;

    [Header("Start")]
    [Tooltip("시작 모드(테스트용). 실제 게임에선 Off 권장")]
    public Mode startMode = Mode.Off;

    private Mode _mode = Mode.Off;
    private float _lastToggle;

    // 외부 참조용 읽기 전용 속성
    public bool IsUVOn => (_mode == Mode.UV);
    public bool IsWhiteOn => (_mode == Mode.White);
    public Mode CurrentMode => _mode;

    void Awake()
    {
        // ISuitReceiver 자동 바인딩
        if (suitProvider && suitProvider is ISuitReceiver sp)
            suit = sp;
        if (suit == null)
            suit = GetComponentInParent<ISuitReceiver>();

        // 라이트 자동 찾기
        if (!whiteLight)
        {
            var t = transform.Find("WhiteLight");
            if (t) whiteLight = t.GetComponent<Light>();
        }
        if (!uvLight)
        {
            var t = transform.Find("UVLight");
            if (t) uvLight = t.GetComponent<Light>();
        }

        // 시작 시 항상 모두 Off
        if (whiteLight) whiteLight.enabled = false;
        if (uvLight) uvLight.enabled = false;

        // 초기 상태 적용
        ApplyMode(startMode, playSound: false);
    }

    void OnEnable()
    {
        // 리로드나 재활성화 시 기존 모드 유지
        ApplyMode(_mode, playSound: false);
    }

    void Update()
    {
        // 구 Input System
        if (Input.GetKeyDown(legacyToggleKey))
            TryToggle();

        // 신 Input System
#if ENABLE_INPUT_SYSTEM
        if (Keyboard.current != null && Keyboard.current.fKey.wasPressedThisFrame)
            TryToggle();
#endif

        // 방호복 해제 시 강제 Off
        if (requireSuit && suit != null && !suit.IsSuited)
        {
            if (_mode != Mode.Off) ApplyMode(Mode.Off);
        }
    }

    private void TryToggle()
    {
        if (Time.unscaledTime - _lastToggle < debounceSeconds)
            return;

        // 방호복 조건
        if (requireSuit)
        {
            if (suit == null)
            {
                Debug.LogWarning("[Flashlight] ISuitReceiver를 찾지 못했습니다. requireSuit=true면 동작하지 않습니다.");
                return;
            }
            if (!suit.IsSuited)
                return;
        }

        Mode next = (Mode)(((int)_mode + 1) % 3); // Off→White→UV→Off
        ApplyMode(next);
        _lastToggle = Time.unscaledTime;
    }

    private void ApplyMode(Mode m, bool playSound = true)
    {
        _mode = m;

        bool onW = (m == Mode.White);
        bool onU = (m == Mode.UV);

        if (whiteLight) whiteLight.enabled = onW;
        if (uvLight) uvLight.enabled = onU;

        if (sfx && playSound && !sfx.isPlaying)
            sfx.Play();

        Debug.Log($"[Flashlight] 모드 변경: {_mode}");
    }

    public void ForceSetMode(Mode m, bool playSound = false)
    {
        ApplyMode(m, playSound);
    }
}
