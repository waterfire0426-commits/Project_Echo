// File: QuestManager.cs
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

[DefaultExecutionOrder(-500)]
public class QuestManager : MonoBehaviour
{
    [Serializable]
    public class Step
    {
        public string id;                 // 트리거 키(Quest.Notify와 동일 문자열)
        [TextArea] public string text;    // 화면에 표시할 문구
        [HideInInspector] public bool done;
    }

    public static QuestManager Instance { get; private set; }

    [Header("Steps (위→아래 순서대로 진행)")]
    public List<Step> steps = new List<Step>();

    [Header("State (readonly)")]
    public int currentIndex = 0;

    public event Action OnChanged;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        OnChanged?.Invoke(); // UI가 켜져있어도 즉시 그릴 수 있게 신호
    }

    // 단계 완료 알림 (예: Quest.Notify("fuel_pickup"))
    public void Notify(string triggerId)
    {
        if (string.IsNullOrEmpty(triggerId)) return;

        if (currentIndex < steps.Count && steps[currentIndex].id == triggerId)
        {
            steps[currentIndex].done = true;
            currentIndex = Mathf.Min(currentIndex + 1, steps.Count);
            OnChanged?.Invoke();
        }
        // 필요하면 역순/무순서 처리 로직 추가 가능
    }

    // 전체 목록용(기존 UI에서 사용)
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

    // 🔹 지금 요구: 현재 단계만 표시
    public string BuildCurrentText(string title = "Objective")
    {
        if (currentIndex >= steps.Count)
            return $"<b>{title}</b>\n<color=#8CFF8C>✓ 모든 목표 완료</color>";

        var s = steps[currentIndex];
        return $"<b>{title}</b>\n• {s.text}";
    }
}

// 편의 정적 헬퍼(로직에서 호출)
public static class Quest
{
    public static void Notify(string triggerId)
        => QuestManager.Instance?.Notify(triggerId);
}
