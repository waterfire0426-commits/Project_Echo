using UnityEngine;
using UnityEngine.Events;

public class PlayerHealth_YH : MonoBehaviour
{
    [Header("HP")]
    public float Max = 100f;
    public float Current = 100f;

    [Header("Events")]
    public UnityEvent onDamaged;
    public UnityEvent onHealed;
    public UnityEvent onDead;

    private bool isDead = false;
    private bool invincible = false;   // 무적 여부 추가

    public bool IsDead => isDead;

    void Awake()
    {
        Current = Mathf.Clamp(Current, 0f, Max);
    }

    public void TakeDamage(float amount)
    {
        if (isDead || invincible) return;  // 무적이면 무시
        Current = Mathf.Clamp(Current - Mathf.Abs(amount), 0f, Max);
        onDamaged?.Invoke();
        if (Current <= 0f)
        {
            isDead = true;
            onDead?.Invoke();
        }
        Debug.Log($"[플레이어HP] 피격: -{amount:0.##} → {Current:0.##}/{Max}");
    }

    public void Heal(float amount)
    {
        if (isDead) return;
        Current = Mathf.Clamp(Current + Mathf.Abs(amount), 0f, Max);
        onHealed?.Invoke();
        Debug.Log($"[플레이어HP] 회복: +{amount:0.##} → {Current:0.##}/{Max}");
    }

    public void SetMax(float newMax, bool refill = true)
    {
        Max = Mathf.Max(1f, newMax);
        if (refill) Current = Max;
        else Current = Mathf.Clamp(Current, 0f, Max);
    }

    // 추가: 무적 상태 ON/OFF
    public void SetInvincible(bool enable)
    {
        invincible = enable;
        Debug.Log($"[플레이어HP] 무적 상태: {(enable ? "ON" : "OFF")}");
    }
}
