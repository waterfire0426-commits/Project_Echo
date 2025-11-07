using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

[DefaultExecutionOrder(-500)]
public class QuestManager : MonoBehaviour
{
    // 싱글톤
    public static QuestManager Instance { get; private set; }

    [Serializable]
    public class Step
    {
        public string id;                 // 트리거 키
        [TextArea] public string text;    // 화면 문구
        [HideInInspector] public bool done;
    }

    public event Action OnChanged;

    [Header("Steps (위→아래 순서대로 진행)")]
    public List<Step> steps = new List<Step>();

    [Header("State (readonly)")]
    public int currentIndex = 0;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        OnChanged?.Invoke();
    }

    // 인스턴스 트리거 처리
    private void HandleNotify(string triggerId)
    {
        if (string.IsNullOrEmpty(triggerId)) return;

        if (currentIndex < steps.Count && steps[currentIndex].id == triggerId)
        {
            steps[currentIndex].done = true;
            currentIndex = Mathf.Min(currentIndex + 1, steps.Count);
            OnChanged?.Invoke();
        }
    }

    // ---- UI용 텍스트 ----
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

    // ---- 레거시 코드 호환용 얇은 API (필요 호출만 유지) ----
    public bool Completed => currentIndex >= steps.Count;

    public void AddQuest(Quest quest)
    {
        // 과거 AddQuest 호출과의 호환용: 지금은 단순 신호만 보냄
        OnChanged?.Invoke();
    }

    public void CompleteQuest(Quest quest)
    {
        if (quest != null) quest.isComplete = true; // SO 필드 반영
        OnChanged?.Invoke();
    }

    // 정적 래퍼 (외부에서 QuestManager.Notify("id")로 호출)
    public static void Notify(string triggerId) => Instance?.HandleNotify(triggerId);
}
