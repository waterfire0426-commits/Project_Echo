using UnityEngine;

public class FuelPickupDebug : MonoBehaviour, IInteractable
{
    public void OnFocus()
    {
        // 필요하면 큐브 하이라이트 표시
    }

    public void OnUnfocus()
    {
        // 필요하면 하이라이트 해제
    }

    public void Interact(GameObject interactor)
    {
        Debug.Log("[PICKUP] 연료통 획득!");
        Destroy(gameObject); // 프로토타입: 파괴 처리
    }
}
