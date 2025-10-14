using UnityEngine;
using UnityEngine.Events;
using System.Collections;

public class UnifiedInteractable : MonoBehaviour, IInteractable
{
    public enum Kind { FuelPickup, Generator, PowerSwitch, HazmatEquip, Door, EggShell, DataTerminal, None }

    [Header("Object Type")]
    public Kind kind = Kind.None;

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
    public Transform uiRootOverride;           // 미니게임 프리팹 붙일 캔버스(없으면 자동 탐색)
    public UnityEvent onMiniGameOpened;        // 미니게임 열릴 때(커서/입력 잠금 등)
    public UnityEvent onMiniGameClosed;        // 미니게임 닫힐 때(복구)


    // === Door 관련 ===
    [Header("Door Settings (Door 전용)")]
    public Transform doorHinge;
    public float openAngle = 90f;
    public float openSpeed = 3f;
    private bool isOpen = false;
    private Coroutine doorRoutine;

    // === DataTerminal(미니게임: 게이지 완료 시점 트리거) ===
    [Header("Data Terminal (MiniGame)")]
    public GameObject miniGamePrefab;          // UI팀 미니게임 프리팹
    [Tooltip("게이지 진행률 브로드캐스트(0~1). 필요 없으면 비워두기")]
    public UnityEvent<float> onMiniGameProgress; // 선택: 진행률 UI 등 연결
    [Tooltip("게이지 100% 완료 시 호출 (여기에 회상 재생 / 오염 4단계 연결)")]
    public UnityEvent onMiniGameFinished;        // ✅ 핵심: 완료 시점 기준
    private bool miniGameFinished = false;

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
            case Kind.Door:         DoDoor(interactor);         break;
            case Kind.EggShell:     DoEggShell(interactor);     break;
            case Kind.DataTerminal: DoDataTerminal(interactor); break;
            default:
                Debug.Log("[Interact] 타입이 설정되지 않음");
                break;
        }
    }

    // -------------------------------
    // 연료통 줍기
    // -------------------------------
    void DoFuelPickup(GameObject interactor)
    {
        var inv = interactor.GetComponent<PlayerInventory>();
        if (!inv) { Debug.LogWarning("[연료통] PlayerInventory 없음"); return; }

        inv.hasFuel = true;

        var hotbar = interactor.GetComponentInChildren<Hotbar>();
        if (hotbar && pickupItem) hotbar.Add(pickupItem, pickupAmount);

        Quest.Notify("fuel_pickup");
        Debug.Log("[목표] 연료통 획득 → 발전기에 주입하세요");
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
                Quest.Notify("generator_fueled");
                Debug.Log("[발전기] 연료 주입 완료");
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
            Quest.Notify("generator_on");
            Debug.Log("[발전기] 가동 시작");
            onGeneratorOn?.Invoke();

            var contam = FindFirstObjectByType<Contamination>();

            if (contam != null)
            {
                contam.Add(10f); // 오염도 1단계 시작
                Debug.Log("[오염] 발전기 가동 → 오염도 상승 시작");
            }

            Quest.Notify("next_task_power_switch");
            Debug.Log("[목표] 전력 스위치실로 이동하세요");
        }
        else
        {
            Debug.Log("[발전기] 이미 가동 중");
        }
    }

    // -------------------------------
    // 전력 스위치
    // -------------------------------
    void DoPowerSwitch(GameObject interactor)
    {
        if (facilityPowerOn) { Debug.Log("[전력] 이미 활성화됨"); return; }

        if (!generatorRef) { Debug.LogWarning("[전력] generatorRef 연결 안 됨"); return; }
        if (generatorRef.kind != Kind.Generator) { Debug.LogWarning("[전력] ref 타입 오류"); return; }
        if (!generatorRef.isRunning) { Debug.Log("[전력] 발전기를 먼저 가동해야 합니다"); return; }

        facilityPowerOn = true;
        Quest.Notify("power_on");
        Debug.Log("[전력] 비상 전력 ON");
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
            Debug.LogWarning("[방호복] ISuitReceiver를 찾을 수 없음 (PlayerMove에 구현 필요)");
            return;
        }

        bool next = !suit.IsSuited;
        suit.ApplySuit(next);
        Debug.Log(next ? "[방호복] 착용 완료" : "[방호복] 해제 완료");
    }

    // -------------------------------
    // 문 열기(회전형)
    // -------------------------------
    void DoDoor(GameObject interactor)
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
        Debug.Log(isOpen ? "[문] 열림" : "[문] 닫힘");
    }

    // -------------------------------
    // 계란껍질 오브젝트
    // -------------------------------
    void DoEggShell(GameObject interactor)
    {
        var hotbar = interactor.GetComponentInChildren<Hotbar>();
        if (!hotbar)
        {
            Debug.LogWarning("[계란껍질] Hotbar 없음");
            return;
        }

        if (hotbar.SelectedIs("vinegar"))
        {
            if (hotbar.RemoveFromSelected(1))
            {
                Debug.Log("[계란껍질] 식초 사용 → 제거됨");
                Quest.Notify("eggshell_removed");
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
    // 데이터 터미널(게이지 완료 기준)
    // -------------------------------
    void DoDataTerminal(GameObject interactor)
    {
        if (miniGameFinished) { Debug.Log("[단말] 이미 다운로드 완료"); return; }
        if (!miniGamePrefab) { Debug.LogWarning("[단말] miniGamePrefab 미지정"); return; }

        // 1) 프리팹 붙일 부모 찾기: 인스펙터 지정 > 'UIRoot' 태그 > 월드 루트
        Transform parent = uiRootOverride;
        if (!parent)
        {
            var uiGo = GameObject.FindWithTag("UIRoot"); // Canvas에 'UIRoot' 태그 달아두면 편함
            if (uiGo) parent = uiGo.transform;
        }

        GameObject inst = parent ? Instantiate(miniGamePrefab, parent) : Instantiate(miniGamePrefab);
        if (!parent) Debug.LogWarning("[단말] uiRootOverride/태그가 없어 월드에 생성했습니다.");

        var bridge = inst.GetComponent<MiniGameBridge>();
        if (!bridge) bridge = inst.AddComponent<MiniGameBridge>();

        // 🔒 입력/커서 잠금은 이벤트로 외부 처리(컨트롤러 disable 등 연결)
        onMiniGameOpened?.Invoke();

        // 2) 진행률 브로드캐스트(선택)
        bridge.OnProgress += p => { onMiniGameProgress?.Invoke(p); };

        // 3) 완료(게이지 100%) 시 후처리
        bridge.OnFinished += () =>
        {
            if (miniGameFinished) return;
            miniGameFinished = true;

            Quest.Notify("data_downloaded");
            onMiniGameFinished?.Invoke(); // 여기다 타임라인 Play, 오염 4단계 연결해 두면 됨
            onMiniGameClosed?.Invoke();   // 🔓 입력/커서 복구(외부에서 연결)
            Debug.Log("[단말] 데이터 다운로드 완료(게이지 100%)");
        };

        // 4) 창 닫힘(취소 등) 시 복구만
        bridge.OnClosed += () =>
        {
            onMiniGameClosed?.Invoke();   // 🔓 복구
            Debug.Log("[단말] 미니게임 종료");
        };
    }

}
