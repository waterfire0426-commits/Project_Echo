using UnityEngine;

public class MentosProjectile_YH : MonoBehaviour
{
    public float lifeTime = 5f;

    void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    void OnCollisionEnter(Collision col)
    {
        if (col.gameObject.CompareTag("ColaMonster"))
        {
            Debug.Log("[멘토스] 콜라 괴물 명중 → 제거!");
            Destroy(col.gameObject); // 괴물 삭제
        }

        Destroy(gameObject); // 멘토스 삭제
    }
}
