using UnityEngine;
using UnityEngine.Events;

public class Contamination : MonoBehaviour
{
    [Header("Value (0~100) → Stage")]
    [Range(0,100)] public float value = 0f;
    public float[] stageThresholds = { 0, 10, 25, 50, 75, 100 }; // 0,1,2,3,4,5

    [Header("Natural Change (optional)")]
    public float passiveIncreasePerSec = 0f;   // 기본 증가량 (맵 전체 오염 등)
    public float passiveDecreasePerSec = 0f;   // 방호복/세이프존 회복 등

    [Header("Events")]
    public UnityEvent<int> onStageChanged;     // 파라미터: 새 단계
    public UnityEvent onDeath;                 // 5단계 도달 시

    public int Stage { get; private set; } = 0;

    void Update()
    {
        float delta = (passiveIncreasePerSec - passiveDecreasePerSec) * Time.deltaTime;
        if (Mathf.Abs(delta) > 0.0001f) Add(delta);
    }

    public void Add(float amount)
    {
        float prevStage = Stage;
        value = Mathf.Clamp(value + amount, 0f, 100f);
        UpdateStage();

        if (Stage != prevStage) onStageChanged?.Invoke(Stage);
        if (Stage >= 5) onDeath?.Invoke();
    }

    public void Set(float v)
    {
        value = Mathf.Clamp(v, 0f, 100f);
        UpdateStage();
    }

    void UpdateStage()
    {
        int s = 0;
        for (int i = 0; i < stageThresholds.Length; i++)
            if (value >= stageThresholds[i]) s = i;
        Stage = Mathf.Clamp(s, 0, 5);
    }

    // 편의 API
    public void AddPercent(float percent01) => Add(percent01 * 100f);
    public void ReducePercent(float percent01) => Add(-percent01 * 100f);
}
