#if false
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

[DefaultExecutionOrder(-500)]
public class QuestManagerLegacy : MonoBehaviour
{
    [Serializable]
    public class Step
    {
        public string id;
        [TextArea] public string text;
        [HideInInspector] public bool done;
    }

    // 타입을 올바르게 수정 (레거시 클래스 자신)
    public static QuestManagerLegacy Instance { get; private set; }

    [Header("Steps (위→아래 순서대로 진행)")]
    public List<Step> steps = new List<Step>();

    [Header("State (readonly)")]
    public int currentIndex = 0;

    public event Action OnChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        OnChanged?.Invoke();
    }

    public void Notify(string triggerId)
    {
        if (string.IsNullOrEmpty(triggerId)) return;

        if (currentIndex < steps.Count && steps[currentIndex].id == triggerId)
        {
            steps[currentIndex].done = true;
            currentIndex = Mathf.Min(currentIndex + 1, steps.Count);
            OnChanged?.Invoke();
        }
    }

    public string BuildDisplayText(string title = "Objectives")
    {
        var sb = new StringBuilder();
        sb.AppendLine($"<b>{title}</b>");
        for (int i = 0; i < steps.Count; i++)
        {
            var s = steps[i];
            string mark = s.done ? "<color=#8CFF8C>✓</color>" : (i == currentIndex ? "•" : "◻");
            sb.AppendLine($"{mark} {s.text}");
        }
        return sb.ToString();
    }

    public string BuildCurrentText(string title = "Objective")
    {
        if (currentIndex >= steps.Count)
            return $"<b>{title}</b>\n<color=#8CFF8C>✓ 모든 목표 완료</color>";

        var s = steps[currentIndex];
        return $"<b>{title}</b>\n• {s.text}";
    }
}
#endif

