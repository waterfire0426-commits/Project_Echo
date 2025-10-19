// File: Assets/Scripts/Enemy/TentacleHazard_YH.cs
using UnityEngine;
using System.Collections;

[RequireComponent(typeof(SphereCollider))]
public class TentacleHazard_YH : MonoBehaviour, IInteractable
{
    [Header("공격")]
    public float attackRadius = 4f;
    public float attackInterval = 1.6f;
    public float contamOnHit = 6f;

    [Header("스턴(젓가락)")]
    public float stunDuration = 4f;
    public string chopsticksItemId = "chopsticks";

    SphereCollider col;
    bool stunned = false;
    float nextAtk;

    void Awake()
    {
        col = GetComponent<SphereCollider>();
        col.isTrigger = true;
        col.radius = attackRadius;
    }

    void Update()
    {
        if (stunned) return;

        var pl = GameObject.FindGameObjectWithTag("Player");
        if (!pl) return;

        float d = Vector3.Distance(transform.position, pl.transform.position);
        if (d <= attackRadius && Time.time >= nextAtk)
        {
            nextAtk = Time.time + attackInterval;
            var contam = pl.GetComponent<Contamination>();
            if (contam) contam.Add(contamOnHit);
            Debug.Log("🧶 [면발 촉수] 공격! 오염 상승");
        }
    }

    // IInteractable
    public void OnFocus() {}
    public void OnUnfocus() {}

    public void Interact(GameObject interactor)
    {
        if (stunned) { Debug.Log("🧶 [면발 촉수] 이미 기절 중"); return; }

        var hotbar = interactor.GetComponentInChildren<Hotbar>();
        if (hotbar && hotbar.SelectedIs(chopsticksItemId))
        {
            // 소비/비소비는 팀 룰에 맞춰 변경
            hotbar.RemoveFromSelected(1);
            StartCoroutine(StunRoutine());
            Debug.Log("🧶 [면발 촉수] 젓가락으로 스턴!");
        }
        else
        {
            Debug.Log("🧶 [면발 촉수] 젓가락 아이템이 필요합니다");
        }
    }

    IEnumerator StunRoutine()
    {
        stunned = true;
        yield return new WaitForSeconds(stunDuration);
        stunned = false;
        Debug.Log("🧶 [면발 촉수] 스턴 해제");
    }
}
