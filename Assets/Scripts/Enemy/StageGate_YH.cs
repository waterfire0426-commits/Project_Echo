// File: Assets/Scripts/Enemy/StageGate_YH.cs
using UnityEngine;

public class StageGate_YH : MonoBehaviour
{
    [Header("출현 단계")]
    [Tooltip("이 단계 이상일 때 활성화")]
    public int minStage = 0;
    [Tooltip("이 단계 초과 시 비활성화(-1이면 제한 없음)")]
    public int maxStage = -1;
    [Tooltip("스테이지 변화 시점에만 토글(시작 시 한 번 반영)")]
    public bool toggleActiveSelf = true;

    Contamination contam;

    void Start()
    {
        contam = FindFirstObjectByType<Contamination>();
        Apply();
        if (contam) contam.onStageChanged.AddListener(_ => Apply());
    }

    void OnDestroy()
    {
        if (contam) contam.onStageChanged.RemoveListener(_ => Apply());
    }

    void Apply()
    {
        if (!contam) return;
        int s = contam.Stage;
        bool on = (s >= minStage) && (maxStage < 0 || s <= maxStage);
        if (toggleActiveSelf) gameObject.SetActive(on);
        // 스포너일 경우 토글Active가 싫으면 Inspector에서 끄고, 여기서 Spawner 호출로 바꿔도 됨.
    }
}
