using UnityEngine;

public class ItemThrower_YH : MonoBehaviour
{
    public GameObject mentosPrefab; // 던질 멘토스 프리팹
    public float throwForce = 10f;

    public void ThrowMentos()
    {
        if (!mentosPrefab)
        {
            Debug.LogWarning("[멘토스] 던질 프리팹이 지정되지 않았습니다!");
            return;
        }

        // 플레이어 정면에서 던지기
        GameObject go = Instantiate(
            mentosPrefab,
            transform.position + transform.forward * 0.5f,
            Quaternion.identity
        );

        var rb = go.GetComponent<Rigidbody>();
        if (rb) rb.AddForce(transform.forward * throwForce, ForceMode.VelocityChange);

        Debug.Log("[멘토스] 투척 완료!");
    }
}
