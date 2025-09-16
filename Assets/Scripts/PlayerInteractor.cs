using UnityEngine;


public class PlayerInteractor : MonoBehaviour
{
    public float interactRange = 3f;
    public CrosshairDebug crosshair; // 인스펙터에 연결

    IInteractable current;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            Ray ray = new Ray(Camera.main.transform.position, Camera.main.transform.forward);
            if (Physics.Raycast(ray, out RaycastHit hit, interactRange))
            {
                var interactable = hit.collider.GetComponent<IInteractable>();
                if (interactable != null)
                {
                    interactable.Interact(gameObject);
                }
            }
        }

       // 중앙 레이로 대상 체크
        IInteractable next = null;
        if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward,
                            out RaycastHit checkHit, interactRange))
        {
            next = checkHit.collider.GetComponent<IInteractable>();
        }
        if (crosshair) crosshair.SetActive(next != null);


    }
}
