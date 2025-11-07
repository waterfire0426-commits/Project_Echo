using UnityEngine;
using System.Collections;

public class ContamHook_YH : MonoBehaviour
{
    [Header("Refs")]
    public Contamination contamination;

    // 오염 무적 상태 플래그
    private bool invincible = false;

    void Awake()
    {
        if (!contamination)
            contamination = FindFirstObjectByType<Contamination>();
    }

    /// <summary>
    /// 오염도 일시 상승 (외부에서 호출)
    /// </summary>
    public void AddTemp(float amount)
    {
        // 무적이면 무시
        if (invincible) return;

        if (contamination)
        {
            contamination.Add(amount);
            Debug.Log($"[오염도] 변화량: {amount}");
        }
    }

    /// <summary>
    /// 일정 시간 동안 오염 무적 (콜라괴물 탈출 등)
    /// </summary>
    public void SetInvincible(float duration)
    {
        StopAllCoroutines(); // 중복 방지
        StartCoroutine(InvincibleRoutine(duration));
    }

    private IEnumerator InvincibleRoutine(float dur)
    {
        invincible = true;
        Debug.Log($"[오염] {dur}초간 무적 진입");
        yield return new WaitForSeconds(dur);
        invincible = false;
        Debug.Log("[오염] 무적 해제");
    }

    /// <summary>
    /// 즉시 무적 ON/OFF 제어 (bool형)
    /// </summary>
    public void SetInvincible(bool enable)
    {
        invincible = enable;
        Debug.Log($"[오염] 무적 상태: {(enable ? "ON" : "OFF")}");
    }

    /// <summary>
    /// 현재 무적 상태 조회용
    /// </summary>
    public bool IsInvincible()
    {
        return invincible;
    }
}
