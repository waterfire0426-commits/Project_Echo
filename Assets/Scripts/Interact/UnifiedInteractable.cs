using UnityEngine;
using UnityEngine.Events;
using System.Collections;

public class UnifiedInteractable : MonoBehaviour, IInteractable
{
    public enum Kind
    {
        FuelPickup,
        Generator,
        PowerSwitch,
        HazmatEquip,
        Door,
        EggShell,
        DataTerminal,
        MentosPickup, // 🟢 멘토스 줍기용
        Mentos,        // 🟢 멘토스 던지기용
        None
    }

    [Header("Object Type")]
    public Kind kind = Kind.None;

    // 🔒 자동 상호작용 방지 플래그
    private bool hasAutoTriggered = false;

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
    public Transform uiRootOverride;
    public UnityEvent onMiniGameOpened;
    public UnityEvent onMiniGameClosed;

    [Header("Door Settings (Door 전용)")]
    public Transform doorHinge;
    public float openAngle = 90f;
    public float openSpeed = 3f;
    private bool isOpen = false;
    private Coroutine doorRoutine;

    [Header("Data Terminal (Download)")]
    public GameObject miniGamePrefab;
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
        // 🔒 자동 호출 방지 (플레이어나 충돌로 인한 중복 실행 차단)
        if (hasAutoTriggered) return;
        hasAutoTriggered = true;

        switch (kind)
        {
            case Kind.FuelPickup:   DoFuelPickup(interactor);  break;
            case Kind.Generator:    DoGenerator(interactor);    break;
            case Kind.PowerSwitch:  DoPowerSwitch(interactor);  break;
            case Kind.HazmatEquip:  DoHazmatEquip(interactor);  break;
            case Kind.Door:         DoDoor();                   break;
            case Kind.EggShell:     DoEggShell(interactor);     break;
            case Kind.DataTerminal: DoDataTerminal(interactor); break;
            case Kind.Mentos:       DoMentos(interactor);       break;
            case Kind.MentosPickup: DoMentosPickup(interactor); break;
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

        interactor.GetComponent<ContamHook_YH>()?.AddTemp(+10f);
        QuestManager.Notify(TRG.FUEL_PICKUP);

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
                QuestManager.Notify(TRG.GENERATOR_FUELED);
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
            QuestManager.Notify(TRG.GENERATOR_ON);
            Debug.Log("[발전기] 가동 시작.");
            onGeneratorOn?.Invoke();

            interactor.GetComponent<ContamHook_YH>()?.AddTemp(+5f);

            QuestManager.Notify(TRG.NEXT_POWER_SWITCH);
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
        QuestManager.Notify(TRG.POWER_ON);
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
            Debug.LogWarning("[방호복] ISuitReceiver를 찾지 못했습니다.");
            return;
        }

        bool next = !suit.IsSuited;
        suit.ApplySuit(next);
        Debug.Log(next ? "[방호복] 착용 완료." : "[방호복] 해제 완료.");

        if (next) QuestManager.Notify(TRG.HAZMAT_ON);
    }

    // -------------------------------
    // 문 열기
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
                QuestManager.Notify(TRG.EGGSHELL_REMOVED);
                Destroy(gameObject);
            }
            else Debug.Log("[계란껍질] 식초가 없습니다!");
        }
        else Debug.Log("[계란껍질] 식초를 선택한 상태에서 E키를 눌러야 합니다!");
    }

    // -------------------------------
    // 데이터 터미널
    // -------------------------------
    void DoDataTerminal(GameObject interactor)
    {
        Debug.Log("[단말] 상호작용 수신.");

        if (miniGameFinished) { Debug.Log("[단말] 이미 다운로드를 완료했습니다."); return; }

        var bridge = FindFirstObjectByType<DownloadUIBridge_YH>();
        if (bridge != null)
        {
            onMiniGameOpened?.Invoke();
            Debug.Log("[단말] UI 브리지 감지 → 다운로드 UI 오픈.");

            bridge.Open(onSlow: null, onFast: null, autoStartSlow: false);

            bool fast = startFastByDefault;
            float sec = fast ? fastSeconds : slowSeconds;
            float noise = fast ? fastNoisePerSec : slowNoisePerSec;

            Debug.Log($"[단말] {(fast ? "고속" : "기본")} 모드 시작 ({sec:0.0}s).");
            QuestManager.Notify(fast ? TRG.DL_START_FAST : TRG.DL_START_SLOW);
            StartCoroutine(RunDownloadRoutine(interactor, bridge, sec, noise));
            return;
        }

        if (allowHeadlessWhenNoUI)
        {
            float sec = startFastByDefault ? fastSeconds : slowSeconds;
            float noise = startFastByDefault ? fastNoisePerSec : slowNoisePerSec;

            Debug.Log($"[단말/헤드리스] {(startFastByDefault ? "고속" : "기본")} 모드 시작 ({sec:0.0}s).");
            StartCoroutine(RunDownloadHeadless(interactor, sec, noise));
            return;
        }

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

        QuestManager.Notify(TRG.DL_DONE);
        onMiniGameFinished?.Invoke();
        onMiniGameClosed?.Invoke();
        bridge.Close();

        miniGameFinished = true;
        Debug.Log("[단말] 다운로드 완료 (UI 모드).");

        FindFirstObjectByType<Act3Trigger>()?.OnDownloadComplete();
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

        QuestManager.Notify(TRG.DL_DONE);
        miniGameFinished = true;
        Debug.Log("[단말/헤드리스] 다운로드 완료 → 트리거 발사.");
        FindFirstObjectByType<Act3Trigger>()?.OnDownloadComplete();
    }

    // -------------------------------
    // 멘토스 던지기
    // -------------------------------
    void DoMentos(GameObject interactor)
    {
        Debug.Log("[멘토스] 아이템 사용됨. 던질 준비 완료.");

        var thrower = interactor.GetComponentInChildren<ItemThrower_YH>();
        if (thrower)
        {
            thrower.ThrowMentos();
            Debug.Log("[멘토스] 투척 실행!");
        }
        else
        {
            Debug.LogWarning("[멘토스] 투척 컴포넌트를 찾을 수 없습니다.");
        }
    }

    // -------------------------------
    // 멘토스 줍기
    // -------------------------------
    void DoMentosPickup(GameObject interactor)
    {
        if (!interactor.CompareTag("Player")) return;

        var inv = interactor.GetComponent<PlayerInventory>();
        if (!inv)
        {
            Debug.LogWarning("[멘토스] PlayerInventory가 없습니다.");
            return;
        }

        var hotbar = interactor.GetComponentInChildren<Hotbar>();
        if (hotbar && pickupItem)
        {
            int addCount = Random.Range(1, 4);
            hotbar.Add(pickupItem, addCount);
            Debug.Log($"[멘토스] {addCount}개 획득!");
        }

        // 🟢 퀘스트 알림은 주석 처리 (자동 제거 방지)
        // QuestManager.Notify(TRG.MENTOS_PICKUP);

        Destroy(gameObject);
    }
}
