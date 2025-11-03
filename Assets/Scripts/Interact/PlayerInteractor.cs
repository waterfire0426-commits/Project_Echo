using UnityEngine;
using System;
using System.Collections;

public class PlayerInteractor : MonoBehaviour
{
    [Header("Ray")]
    public float interactRange = 4.5f;
    public LayerMask interactMask = ~0;
    public bool includeTriggers = true;
    public bool debugLog = false;

    [Header("UI")]
    public CrosshairUI crosshair;

    private Camera mainCamera;
    public static event Action<IInteractable> OnFocusChanged;

    IInteractable currentInteractable;

    // 🔹 추가: 현재 상호작용 중 여부
    public static bool isInteracting = false;

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
            if (currentInteractable != null)
            {
                if (debugLog) Debug.Log("[Interactor] E → Interact() 호출");
                StartCoroutine(HandleInteract());
            }
            else if (debugLog)
            {
                Debug.LogWarning("[Interactor] 조준 대상 없음");
            }
        }
    }

    IEnumerator HandleInteract()
    {
        isInteracting = true;               // 🔹 플래그 켜기
        currentInteractable.Interact(gameObject);
        yield return null;                  // 한 프레임 기다리기
        isInteracting = false;              // 🔹 다음 프레임에 자동 해제
    }

    void CheckForInteractable()
    {
        var ray = new Ray(mainCamera.transform.position, mainCamera.transform.forward);
        var qti = includeTriggers ? QueryTriggerInteraction.Collide : QueryTriggerInteraction.Ignore;

        IInteractable newInteractable = null;

        if (Physics.Raycast(ray, out RaycastHit hit, interactRange, interactMask, qti))
        {
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

        if (newInteractable != currentInteractable)
        {
            if (currentInteractable != null) currentInteractable.OnUnfocus();
            if (newInteractable != null) newInteractable.OnFocus();

            currentInteractable = newInteractable;
            OnFocusChanged?.Invoke(newInteractable);
        }

        if (crosshair != null)
            crosshair.SetActive(currentInteractable != null);
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
