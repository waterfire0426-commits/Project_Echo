using UnityEngine;

public class MentosProjectile_YH : MonoBehaviour
{
    [Header("Life Settings")]
    public float lifeTime = 5f;
    public float damage = 50f;

    void Start()
    {
    }

    void OnCollisionEnter(Collision collision)
    {
        // 콜라 괴물 피격 처리
        var cola = collision.collider.GetComponent<ColaGrabber_YH>();
        if (cola)
        {
            Debug.Log("[ColaGrabber] 멘토스 피격 감지!");
            cola.OnHitByMentos(damage);
        }

        // 충돌 즉시 멘토스 삭제
        Destroy(gameObject);
    }
}
