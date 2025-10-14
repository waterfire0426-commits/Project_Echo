// File: FlashlightController.cs
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem; // New Input System (Keyboard.current)
#endif

/// <summary>
/// F키로 Off → White → UV 순환.
/// 방호복(슈트) 착용 상태(ISuitReceiver.IsSuited)가 아니면 입력 무시(옵션).
/// 새/구 Input System 둘 다 지원. (Project Settings → Active Input Handling가 무엇이든 동작)
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
    public MonoBehaviour suitProvider; // 반드시 ISuitReceiver 구현 컴포넌트여야 함
    private ISuitReceiver suit;

    [Header("Input")]
    public KeyCode legacyToggleKey = KeyCode.F; // 구 Input System
    public float debounceSeconds = 0.15f;

    [Header("Start")]
    [Tooltip("시작 모드(테스트용). 실제 게임에선 Off 권장")]
    public Mode startMode = Mode.Off;

    private Mode _mode = Mode.Off;
    private float _lastToggle;

    void Awake()
    {
        // ISuitReceiver 자동 바인딩
        if (suitProvider && suitProvider is ISuitReceiver sp) suit = sp;
        if (suit == null) suit = GetComponentInParent<ISuitReceiver>();

        // 라이트 자동 찾기(비어 있으면 자식 이름으로 스캔)
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

        // 시작 상태
        ApplyMode(Mode.Off, playSound:false);
        if (startMode != Mode.Off) ApplyMode(startMode, playSound:false);
    }

    void OnEnable()
    {
        // 씬 리로드/재활성 시 안전하게 Off
        ApplyMode(_mode, playSound:false);
    }

    void Update()
    {
        // ✅ 구 Input System (Input.GetKeyDown)
        if (Input.GetKeyDown(legacyToggleKey))
            TryToggle();

        // ✅ New Input System (Keyboard.current) — PlayerInput 없어도 동작
        #if ENABLE_INPUT_SYSTEM
        if (Keyboard.current != null && Keyboard.current.fKey.wasPressedThisFrame)
            TryToggle();
        #endif

        // 방호복 해제되면 강제 Off
        if (requireSuit && suit != null && !suit.IsSuited)
        {
            if (_mode != Mode.Off) ApplyMode(Mode.Off);
        }
    }

    /// <summary>새/구 입력 공통 토글 처리</summary>
    private void TryToggle()
    {
        if (Time.unscaledTime - _lastToggle < debounceSeconds) return;

        // 방호복 필요 조건 검사
        if (requireSuit)
        {
            if (suit == null)
            {
                // suitProvider를 지정하지 않았고 부모에서도 못 찾은 경우
                // 필요 시 경고만 남기고 막지 않으려면 아래 return 주석 처리
                Debug.LogWarning("[Flashlight] ISuitReceiver를 찾지 못했습니다. requireSuit=true면 동작하지 않습니다.");
                return;
            }
            if (!suit.IsSuited) return;
        }

        var next = (Mode)(((int)_mode + 1) % 3); // Off→White→UV→Off
        ApplyMode(next);
        _lastToggle = Time.unscaledTime;
    }

    /// <summary>실제 라이트 On/Off 적용</summary>
    private void ApplyMode(Mode m, bool playSound = true)
    {
        _mode = m;

        bool onW = (m == Mode.White);
        bool onU = (m == Mode.UV);

        if (whiteLight) whiteLight.enabled = onW;
        if (uvLight)    uvLight.enabled    = onU;

        if (sfx && playSound && !sfx.isPlaying) sfx.Play();
    }

    /// <summary>외부에서 강제로 모드 지정 (예: 슈트 착용 즉시 White 켜기)</summary>
    public void ForceSetMode(Mode m, bool playSound = false) => ApplyMode(m, playSound);
}
