using UnityEngine;
using System; // Action 사용 위해 추가


public class PlayerInteractor : MonoBehaviour
{
    public float interactRange = 3f;
    public CrosshairUI crosshair;
    private Camera mainCamera;

    // UI 파트에서 구독해서 사용할 수 있는 공개 이벤트
    public static event Action<IInteractable> OnFocusChanged;

    // 현재 바라보고 있는 오브젝트 추적
    IInteractable currentInteractable;

    void Start()
    {
        mainCamera = Camera.main; // 매번 검색하지 않도록 카메라 참조 미리 저장
    }

    void Update()
    {
        CheckForInteractable();

        if (Input.GetKeyDown(KeyCode.E) && currentInteractable != null)
        {
            currentInteractable.Interact(gameObject);
        }
    }

    void CheckForInteractable()
    {
        Ray ray = new Ray(mainCamera.transform.position, mainCamera.transform.forward);
        IInteractable newInteractable = null;

        if (Physics.Raycast(ray, out RaycastHit hit, interactRange))
        {
            newInteractable = hit.collider.GetComponent<IInteractable>();
        }

        // 바라보는 대상이 바뀌었는지 확인
        if (newInteractable != currentInteractable)
        {
            currentInteractable?.OnUnfocus(); // 이전 대상
            newInteractable?.OnFocus();     // 새로운 대상
            currentInteractable = newInteractable; // 현재 대상 업데이트

            // 이벤트 발생, 바라보는 대상이 바뀌었다고 모두에게 알림
            // newInteractable이 null일 수도 있음 (허공 볼 때)
            OnFocusChanged?.Invoke(newInteractable);
        }

        // 상호작용 가능한 대상이 있는지에 따라 크로스헤어 활성화
        crosshair?.SetActive(currentInteractable != null);
    }
}
