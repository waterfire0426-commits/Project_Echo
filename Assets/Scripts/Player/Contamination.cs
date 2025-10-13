using UnityEngine;
using UnityEngine.Events;

public class Contamination : MonoBehaviour
{
    [Header("Value (0~100) → Stage")]
    [Range(0,100)] public float value = 0f;
    public float[] stageThresholds = { 0, 10, 25, 50, 75, 100 }; // 0,1,2,3,4,5

    [Header("Natural Change (optional)")]
    public float passiveIncreasePerSec = 0f;
    public float passiveDecreasePerSec = 0f;

    [Header("Events")]
    public UnityEvent<int> onStageChanged;
    public UnityEvent onDeath;

    public int Stage { get; private set; } = 0;

    void Update()
    {
        float delta = (passiveIncreasePerSec - passiveDecreasePerSec) * Time.deltaTime;
        if (Mathf.Abs(delta) > 0.0001f) Add(delta);
    }

    public void Add(float amount)
    {
        int prevStage = Stage;
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

    // ✅ 원하는 단계로 바로 끌어올리기(예: 4단계)
    public void RaiseToStage(int target)
    {
        target = Mathf.Clamp(target, 0, 5);
        int idx = Mathf.Clamp(target, 0, stageThresholds.Length - 1);
        float targetValue = stageThresholds[idx];

        if (value < targetValue)
        {
            value = targetValue;
            int prevStage = Stage;
            UpdateStage();
            if (Stage != prevStage) onStageChanged?.Invoke(Stage);
            if (Stage >= 5) onDeath?.Invoke();
        }
    }

    // 편의 API
    public void AddPercent(float percent01) => Add(percent01 * 100f);
    public void ReducePercent(float percent01) => Add(-percent01 * 100f);
}
