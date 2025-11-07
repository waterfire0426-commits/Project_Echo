using UnityEngine;

public class ItemThrower_YH : MonoBehaviour
{
    [Header("멘토스 투척 설정")]
    public float throwForce = 10f;    // 던지는 힘

    [Header("핫바 연결")]
    public Hotbar hotbar;             // 현재 선택된 아이템 정보 (핫바)

    // 🔹 줍고 바로 던지는 현상 방지용 플래그
    private bool recentlyPicked = false;

    void Awake()
    {
        // 🔹 Hotbar 자동 연결 (수동 연결 안 해도 됨)
        if (!hotbar)
        {
#if UNITY_2023_1_OR_NEWER
            hotbar = FindFirstObjectByType<Hotbar>();
#else
            hotbar = FindObjectOfType<Hotbar>();
#endif
        }
    }

    void Update()
    {
        // 🔹 상호작용 중이면 던지기 금지
        if (PlayerInteractor.isInteracting || recentlyPicked) return;

        if (Input.GetKeyDown(KeyCode.E))
        {
            ThrowMentos();
        }
    }


    // 🔹 멘토스 던지기
    public void ThrowMentos()
    {
        if (!hotbar)
        {
            Debug.LogWarning("[멘토스] Hotbar를 찾을 수 없습니다.");
            return;
        }

        var s = hotbar.slots[hotbar.selected];
        if (s == null || s.def == null)
        {
            Debug.Log("[던지기] 선택된 아이템이 없습니다.");
            return;
        }

        // 멘토스가 선택된 상태인지 확인
        if (s.def.id != "mentos")
        {
            Debug.Log("[던지기] 멘토스가 선택되어 있지 않습니다.");
            return;
        }

        // 🔹 ItemDef에서 던질 프리팹(usePrefab)을 가져옴
        GameObject prefab = s.def.usePrefab;
        if (!prefab)
        {
            Debug.LogWarning("[멘토스] ItemDef에 던질 프리팹(usePrefab)이 지정되지 않았습니다!");
            return;
        }

        // 🔹 카메라 기준으로 던지기
        var cam = Camera.main;
        if (!cam)
        {
            Debug.LogWarning("[멘토스] 메인 카메라를 찾지 못했습니다.");
            return;
        }

        Vector3 spawnPos = cam.transform.position + cam.transform.forward * 1.2f;
        Quaternion spawnRot = Quaternion.LookRotation(cam.transform.forward);
        GameObject go = Instantiate(prefab, spawnPos, spawnRot);

        // 🔹 Rigidbody가 있다면 물리 힘 적용
        if (go.TryGetComponent<Rigidbody>(out var rb))
            rb.AddForce(cam.transform.forward * throwForce, ForceMode.VelocityChange);

        // 🔹 아이템 1개 차감
        hotbar.RemoveFromSelected(1);

        Debug.Log("[멘토스] 투척 완료!");
    }

    // 🔹 줍기 직후 0.2초 동안 던지기 금지
    public void MarkRecentlyPicked()
    {
        recentlyPicked = true;
        Invoke(nameof(ResetPickFlag), 0.2f);
    }

    void ResetPickFlag()
    {
        recentlyPicked = false;
    }
}
