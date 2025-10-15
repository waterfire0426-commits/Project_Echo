using UnityEngine;
using System; // Action

public class PlayerInteractor : MonoBehaviour
{
    [Header("Ray")]
    public float interactRange = 4.5f;
    public LayerMask interactMask = ~0;                 // Everything
    public bool includeTriggers = true;                 // 트리거도 맞추기
    public bool debugLog = false;

    [Header("UI")]
    public CrosshairUI crosshair;

    private Camera mainCamera;
    public static event Action<IInteractable> OnFocusChanged;

    IInteractable currentInteractable;

    void Start()
    {
        mainCamera = Camera.main;
    }

    void Update()
    {
        if (!mainCamera) { mainCamera = Camera.main; if (!mainCamera) return; }

        CheckForInteractable();

        if (Input.GetKeyDown(KeyCode.E))
        {
            if (currentInteractable != null) // ★ null 비교!
            {
                if (debugLog) Debug.Log("[Interactor] E → Interact() 호출");
                currentInteractable.Interact(gameObject);
            }
            else if (debugLog)
            {
                Debug.LogWarning("[Interactor] 조준 대상 없음");
            }
        }
    }

    void CheckForInteractable()
    {
        var ray = new Ray(mainCamera.transform.position, mainCamera.transform.forward);
        var qti = includeTriggers ? QueryTriggerInteraction.Collide : QueryTriggerInteraction.Ignore;

        IInteractable newInteractable = null;

        if (Physics.Raycast(ray, out RaycastHit hit, interactRange, interactMask, qti))
        {
            // 콜라이더가 자식이고 스크립트가 부모일 수도 있으니 부모까지 탐색
            newInteractable = hit.collider.GetComponentInParent<IInteractable>();

            #if UNITY_EDITOR
            if (debugLog)
                Debug.Log($"[Interactor] Hit: {hit.collider.name} (Layer={LayerMask.LayerToName(hit.collider.gameObject.layer)})"
                          + (newInteractable != null ? " -> IInteractable OK" : " -> IInteractable 없음"));
            Debug.DrawRay(ray.origin, ray.direction * hit.distance, Color.green);
            #endif
        }
        else
        {
            #if UNITY_EDITOR
            Debug.DrawRay(ray.origin, ray.direction * interactRange, Color.red);
            #endif
        }

        // 대상이 바뀌었으면 포커스 콜백 처리
        if (newInteractable != currentInteractable)
        {
            if (currentInteractable != null) currentInteractable.OnUnfocus();
            if (newInteractable   != null) newInteractable.OnFocus();

            currentInteractable = newInteractable;
            OnFocusChanged?.Invoke(newInteractable);
        }

        // 크로스헤어 갱신 (bool 요구하므로 null 비교!)
        if (crosshair != null) crosshair.SetActive(currentInteractable != null);
    }

    void OnDrawGizmosSelected()
    {
        if (!Application.isPlaying && Camera.main)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(Camera.main.transform.position,
                            Camera.main.transform.position + Camera.main.transform.forward * interactRange);
        }
    }
}
