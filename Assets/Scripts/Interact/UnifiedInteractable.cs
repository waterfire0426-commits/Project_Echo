using UnityEngine;
using UnityEngine.Events;
using System.Collections;

public class UnifiedInteractable : MonoBehaviour, IInteractable
{
    public enum Kind { FuelPickup, Generator, PowerSwitch, HazmatEquip, Door, EggShell, None }

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

    // === Door 관련 ===
    [Header("Door Settings (Door 전용)")]
    public Transform doorHinge;           // 회전 중심
    public float openAngle = 90f;         // 열릴 각도
    public float openSpeed = 3f;          // 열리는 속도
    private bool isOpen = false;
    private Coroutine doorRoutine;

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

            // ✅ 오염도 트리거
            var contam = FindObjectOfType<Contamination>();
            if (contam != null)
            {
                contam.Add(10f); // 오염도 1단계 시작
                Debug.Log("[오염] 발전기 가동 → 오염도 상승 시작");
            }

            // ✅ 다음 목표 안내
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
    // 문 열기 (회전형)
    // -------------------------------
    void DoDoor(GameObject interactor)
{
    // doorHinge가 비어 있으면 자기 자신(Door 오브젝트)을 회전 대상으로 사용
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

        // 1) 식초 아이템이 선택되어 있는지 확인
        if (hotbar.SelectedIs("vinegar"))
        {
            // 2) 식초 1개 소모
            if (hotbar.RemoveFromSelected(1))
            {
                Debug.Log("[계란껍질] 식초 사용 → 제거됨");
                Quest.Notify("eggshell_removed");
                // 3) 오브젝트 제거
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
}
