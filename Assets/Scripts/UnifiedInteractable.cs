using UnityEngine;
using UnityEngine.Events;

public class UnifiedInteractable : MonoBehaviour, IInteractable
{
    public enum Kind { FuelPickup, Generator, PowerSwitch, HazmatEquip, None }

    [Header("Object Type")]
    public Kind kind = Kind.None;

    [Header("Generator / PowerSwitch State")]
    public bool hasFuel;          // 발전기: 연료 주입 여부
    public bool isRunning;        // 발전기: 가동 여부
    public bool facilityPowerOn;  // 전력 스위치: 전력 ON 여부

    [Tooltip("If this is a PowerSwitch, assign the Generator object here")]
    public UnifiedInteractable generatorRef;

    [Header("Optional Events")]
    public UnityEvent onFueled;           // 발전기: 연료 주입 시
    public UnityEvent onGeneratorOn;      // 발전기: 가동 시
    public UnityEvent onFacilityPowerOn;  // 전력 스위치: ON 시

    // === IInteractable ===
    public void OnFocus()   { }
    public void OnUnfocus() { }

    public void Interact(GameObject interactor)
    {
        switch (kind)
        {
            case Kind.FuelPickup:   DoFuelPickup(interactor);  break;
            case Kind.Generator:    DoGenerator(interactor);    break;
            case Kind.PowerSwitch:  DoPowerSwitch(interactor);  break;
            case Kind.HazmatEquip:  DoHazmatEquip(interactor);  break;
            default:
                Debug.Log("[Interact] 타입이 설정되지 않음");
                break;
        }
    }

    void DoFuelPickup(GameObject interactor)
    {
        var inv = interactor.GetComponent<PlayerInventory>();
        if (!inv) { Debug.LogWarning("[연료통] PlayerInventory 없음"); return; }

        inv.hasFuel = true;
        Debug.Log("[목표] 연료통 획득 → 발전기에 주입하세요");
        Destroy(gameObject); // 연료통 제거
    }

    void DoGenerator(GameObject interactor)
    {
        // 1) 연료 주입
        if (!hasFuel)
        {
            var inv = interactor.GetComponent<PlayerInventory>();
            if (inv && inv.hasFuel)
            {
                inv.hasFuel = false;
                hasFuel = true;
                Debug.Log("[발전기] 연료 주입 완료");
                Debug.Log("[목표] 발전기를 가동하세요 (E)");
                onFueled?.Invoke();
            }
            else
            {
                Debug.Log("[발전기] 연료가 필요합니다 (연료통을 주워오세요)");
            }
            return;
        }

        // 2) 가동
        if (!isRunning)
        {
            isRunning = true;
            Debug.Log("[발전기] 가동 시작");
            Debug.Log("[목표] 비상 전력 스위치를 작동하세요");
            onGeneratorOn?.Invoke();
        }
        else
        {
            Debug.Log("[발전기] 이미 가동 중");
        }
    }

    void DoPowerSwitch(GameObject interactor)
    {
        if (facilityPowerOn) { Debug.Log("[전력] 이미 활성화됨"); return; }

        if (!generatorRef)
        {
            Debug.LogWarning("[전력] generatorRef 연결 안 됨 (인스펙터에서 설정 필요)");
            return;
        }
        if (generatorRef.kind != Kind.Generator)
        {
            Debug.LogWarning("[전력] generatorRef가 Generator 타입이 아님");
            return;
        }
        if (!generatorRef.isRunning)
        {
            Debug.Log("[전력] 발전기를 먼저 가동해야 합니다");
            return;
        }

        facilityPowerOn = true;
        Debug.Log("[전력] 비상 전력 ON");
        Debug.Log("[목표] 다음 단계로 진행 (배수/게이트 오픈 등)");
        onFacilityPowerOn?.Invoke();
    }

    void DoHazmatEquip(GameObject interactor)
    {
        // 플레이어가 ISuitReceiver를 구현해야 함
        var suit = interactor.GetComponentInChildren<ISuitReceiver>();
        if (suit == null)
        {
            Debug.LogWarning("[방호복] ISuitReceiver를 찾을 수 없음 (PlayerMove에 구현 필요)");
            return;
        }

        bool next = !suit.IsSuited;
        suit.ApplySuit(next);
        Debug.Log(next ? "[방호복] 착용 완료 → 이동이 묵직/느려짐 적용"
                       : "[방호복] 해제 → 기본 이동으로 복구");
    }
}
