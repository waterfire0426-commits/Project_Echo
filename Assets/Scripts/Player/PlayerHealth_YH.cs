// File: Assets/Scripts/Player/PlayerHealth_YH.cs
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

    public bool IsDead => Current <= 0f;

    void Awake()
    {
        Current = Mathf.Clamp(Current, 0f, Max);
    }

    public void TakeDamage(float amount)
    {
        if (IsDead) return;
        Current = Mathf.Clamp(Current - Mathf.Abs(amount), 0f, Max);
        onDamaged?.Invoke();
        if (IsDead) onDead?.Invoke();
        Debug.Log($"[플레이어HP] 피격: -{amount:0.##} → {Current:0.##}/{Max}");
    }

    public void Heal(float amount)
    {
        if (IsDead) return;
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
}
