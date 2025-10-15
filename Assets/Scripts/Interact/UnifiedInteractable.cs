// File: Assets/Scripts/Interact/UnifiedInteractable.cs
using UnityEngine;
using UnityEngine.Events;
using System.Collections;

public class UnifiedInteractable : MonoBehaviour, IInteractable
{
    public enum Kind { FuelPickup, Generator, PowerSwitch, HazmatEquip, Door, EggShell, DataTerminal, None }

    [Header("Object Type")]
    public Kind kind = Kind.None;

    // 헤드리스(화면 없이 진행) 허용 여부
    [Header("Download (Headless)")]
    public bool allowHeadlessWhenNoUI = true;

    [Header("Generator / PowerSwitch State")]
    public bool hasFuel;
    public bool isRunning;
    public bool facilityPowerOn;

    [Header("Pickup (FuelPickup 전용)")]
    public ItemDef pickupItem;
    public int pickupAmount = 1;

    [Tooltip("If this is a PowerSwitch, assign the Generator object here")]
    public UnifiedInteractable generatorRef;

    [Header("Optional Events")]
    public UnityEvent onFueled;
    public UnityEvent onGeneratorOn;
    public UnityEvent onFacilityPowerOn;

    [Header("Integration")]
    public Transform uiRootOverride;           // (사용 안 해도 OK)
    public UnityEvent onMiniGameOpened;
    public UnityEvent onMiniGameClosed;

    // === Door ===
    [Header("Door Settings (Door 전용)")]
    public Transform doorHinge;
    public float openAngle = 90f;
    public float openSpeed = 3f;
    private bool isOpen = false;
    private Coroutine doorRoutine;

    // === DataTerminal ===
    [Header("Data Terminal (Download)")]
    public GameObject miniGamePrefab;                  // (씬에 DownloadUIManager가 있다면 비워도 됨)
    public UnityEvent<float> onMiniGameProgress;
    public UnityEvent onMiniGameFinished;
    private bool miniGameFinished = false;

    [Header("Download Modes")]
    public float slowSeconds = 20f;
    public float fastSeconds = 10f;
    [Range(0f, 2f)] public float slowNoisePerSec = 0.2f;
    [Range(0f, 2f)] public float fastNoisePerSec = 0.6f;
    public bool startFastByDefault = false;

    // === IInteractable ===
    public void OnFocus() { }
    public void OnUnfocus() { }

    public void Interact(GameObject interactor)
    {
        switch (kind)
        {
            case Kind.FuelPickup:   DoFuelPickup(interactor);  break;
            case Kind.Generator:    DoGenerator(interactor);    break;
            case Kind.PowerSwitch:  DoPowerSwitch(interactor);  break;
            case Kind.HazmatEquip:  DoHazmatEquip(interactor);  break;
            case Kind.Door:         DoDoor();                   break;
            case Kind.EggShell:     DoEggShell(interactor);     break;
            case Kind.DataTerminal: DoDataTerminal(interactor); break;
            default:
                Debug.LogWarning("[상호작용] 타입이 설정되지 않았습니다.");
                break;
        }
    }

    // -------------------------------
    // 연료통 줍기
    // -------------------------------
    void DoFuelPickup(GameObject interactor)
    {
        var inv = interactor.GetComponent<PlayerInventory>();
        if (!inv) { Debug.LogWarning("[연료통] PlayerInventory 컴포넌트가 없습니다."); return; }

        inv.hasFuel = true;

        var hotbar = interactor.GetComponentInChildren<Hotbar>();
        if (hotbar && pickupItem) hotbar.Add(pickupItem, pickupAmount);

        interactor.GetComponent<ContamHook_YH>()?.AddTemp(+10f); // (선택) 오염 상승

        QuestManager.Notify("fuel_pickup");
        Debug.Log("[목표] 연료통 획득 → 발전기에 주입하세요.");
        Destroy(gameObject);
    }

    // -------------------------------
    // 발전기
    // -------------------------------
    void DoGenerator(GameObject interactor)
    {
        if (!hasFuel)
        {
            var hotbar = interactor.GetComponentInChildren<Hotbar>();
            if (hotbar && hotbar.SelectedIs("fuel") && hotbar.RemoveFromSelected(1))
            {
                hasFuel = true;
                QuestManager.Notify("generator_fueled");
                Debug.Log("[발전기] 연료 주입 완료.");
                onFueled?.Invoke();
            }
            else
            {
                Debug.Log("[발전기] 연료가 필요합니다. 핫바에서 연료 선택 후 E키!");
            }
            return;
        }

        if (!isRunning)
        {
            isRunning = true;
            QuestManager.Notify("generator_on");
            Debug.Log("[발전기] 가동 시작.");
            onGeneratorOn?.Invoke();

            interactor.GetComponent<ContamHook_YH>()?.AddTemp(+5f); // (선택) 오염 상승

            QuestManager.Notify("next_task_power_switch");
            Debug.Log("[목표] 전력 스위치실로 이동하세요.");
        }
        else
        {
            Debug.Log("[발전기] 이미 가동 중입니다.");
        }
    }

    // -------------------------------
    // 전력 스위치
    // -------------------------------
    void DoPowerSwitch(GameObject interactor)
    {
        if (facilityPowerOn) { Debug.Log("[전력] 이미 활성화되어 있습니다."); return; }

        if (!generatorRef) { Debug.LogWarning("[전력] generatorRef가 연결되지 않았습니다."); return; }
        if (generatorRef.kind != Kind.Generator) { Debug.LogWarning("[전력] generatorRef 타입 오류입니다."); return; }
        if (!generatorRef.isRunning) { Debug.Log("[전력] 발전기를 먼저 가동해야 합니다."); return; }

        facilityPowerOn = true;
        QuestManager.Notify("power_on");
        Debug.Log("[전력] 비상 전력 ON.");
        onFacilityPowerOn?.Invoke();
    }

    // -------------------------------
    // 방호복 착용
    // -------------------------------
    void DoHazmatEquip(GameObject interactor)
    {
        var suit = interactor.GetComponentInChildren<ISuitReceiver>();
        if (suit == null)
        {
            Debug.LogWarning("[방호복] ISuitReceiver를 찾지 못했습니다. (PlayerMove에 구현 필요)");
            return;
        }

        bool next = !suit.IsSuited;
        suit.ApplySuit(next);
        Debug.Log(next ? "[방호복] 착용 완료." : "[방호복] 해제 완료.");

        if (next) QuestManager.Notify("suit_on");
    }

    // -------------------------------
    // 문 열기(회전형)
    // -------------------------------
    void DoDoor()
    {
        Transform target = doorHinge ? doorHinge : transform;

        if (doorRoutine != null) StopCoroutine(doorRoutine);
        doorRoutine = StartCoroutine(ToggleDoor(target));
    }

    IEnumerator ToggleDoor(Transform target)
    {
        float t = 0f;
        Quaternion startRot = target.localRotation;
        Quaternion targetRot = isOpen ?
            Quaternion.Euler(0f, 0f, 0f) :
            Quaternion.Euler(0f, openAngle, 0f);

        isOpen = !isOpen;

        while (t < 1f)
        {
            t += Time.deltaTime * openSpeed;
            target.localRotation = Quaternion.Slerp(startRot, targetRot, t);
            yield return null;
        }

        doorRoutine = null;
        Debug.Log(isOpen ? "[문] 열림." : "[문] 닫힘.");
    }

    // -------------------------------
    // 계란껍질 오브젝트
    // -------------------------------
    void DoEggShell(GameObject interactor)
    {
        var hotbar = interactor.GetComponentInChildren<Hotbar>();
        if (!hotbar)
        {
            Debug.LogWarning("[계란껍질] Hotbar가 없습니다.");
            return;
        }

        if (hotbar.SelectedIs("vinegar"))
        {
            if (hotbar.RemoveFromSelected(1))
            {
                Debug.Log("[계란껍질] 식초 사용 → 제거됨.");
                QuestManager.Notify("eggshell_removed");
                Destroy(gameObject);
            }
            else
            {
                Debug.Log("[계란껍질] 식초가 없습니다!");
            }
        }
        else
        {
            Debug.Log("[계란껍질] 식초를 선택한 상태에서 E키를 눌러야 합니다!");
        }
    }

    // -------------------------------
    // 데이터 터미널 (UI 있으면 UI, 없으면 헤드리스)
    // -------------------------------
    void DoDataTerminal(GameObject interactor)
    {
        Debug.Log("[단말] 상호작용 수신.");

        if (miniGameFinished) { Debug.Log("[단말] 이미 다운로드를 완료했습니다."); return; }

        // 씬에서 UI 브리지 찾기
        var bridge = FindFirstObjectByType<DownloadUIBridge_YH>();

        // UI가 있으면: UI 열고 모드 선택하여 진행
        if (bridge != null)
        {
            onMiniGameOpened?.Invoke(); // 🔒 입력/커서 잠금 등 외부 처리
            Debug.Log("[단말] UI 브리지 감지 → 다운로드 UI 오픈.");

            // Open은 '오픈만' 하고, 시작은 우리가 명시적으로 선택
            bridge.Open(
                onSlow:  null,
                onFast:  null,
                autoStartSlow: false
            );

            bool fast = startFastByDefault;
            if (fast)
            {
                Debug.Log($"[단말] 고속 모드 시작 ({fastSeconds:0.0}s).");
                QuestManager.Notify("download_start_fast");
                StartCoroutine(RunDownloadRoutine(interactor, bridge, fastSeconds, fastNoisePerSec));
            }
            else
            {
                Debug.Log($"[단말] 기본 모드 시작 ({slowSeconds:0.0}s).");
                QuestManager.Notify("download_start_slow");
                StartCoroutine(RunDownloadRoutine(interactor, bridge, slowSeconds, slowNoisePerSec));
            }
            return;
        }

        // UI가 없는데 헤드리스 허용이면: 로그만 찍고 타이머 진행
        if (allowHeadlessWhenNoUI)
        {
            bool fast = startFastByDefault;
            float sec   = fast ? fastSeconds     : slowSeconds;
            float noise = fast ? fastNoisePerSec : slowNoisePerSec;
            string startId = fast ? "download_start_fast" : "download_start_slow";

            Debug.Log($"[단말/헤드리스] UI 없음 → {(fast ? "고속" : "기본")} 모드 시작 ({sec:0.0}s).");
            QuestManager.Notify(startId);
            StartCoroutine(RunDownloadHeadless(interactor, sec, noise));
            return;
        }

        // UI도 없고 헤드리스도 금지면 안내
        Debug.LogWarning("[단말] UI(DownloadUIManager/Bridge)가 없고, 헤드리스도 비허용입니다.");
    }

    IEnumerator RunDownloadRoutine(GameObject interactor, DownloadUIBridge_YH bridge, float seconds, float noisePerSec)
    {
        var contam = interactor.GetComponent<ContamHook_YH>();
        float t = 0f;

        while (t < seconds)
        {
            t += Time.deltaTime;
            float p = Mathf.Clamp01(t / seconds);
            bridge.ShowProgress(p);
            onMiniGameProgress?.Invoke(p);

            if (contam) contam.AddTemp(noisePerSec * Time.deltaTime * 5f);
            yield return null;
        }

        QuestManager.Notify("download_done");
        onMiniGameFinished?.Invoke();
        onMiniGameClosed?.Invoke();
        bridge.Close();

        miniGameFinished = true;
        Debug.Log("[단말] 다운로드 완료 (UI 모드).");
    }

    IEnumerator RunDownloadHeadless(GameObject interactor, float seconds, float noisePerSec)
    {
        var contam = interactor.GetComponent<ContamHook_YH>();
        float t = 0f;

        while (t < seconds)
        {
            t += Time.deltaTime;
            if (contam) contam.AddTemp(noisePerSec * Time.deltaTime * 5f);
            yield return null;
        }

        QuestManager.Notify("download_done");
        miniGameFinished = true;
        Debug.Log("[단말/헤드리스] 다운로드 완료 → 트리거 발사.");
    }
}
