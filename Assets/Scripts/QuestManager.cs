// 파일명: QuestManager.cs
using System.Collections.Generic;
using UnityEngine;

public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance { get; private set; }

    [Header("UI 연결")]
    public QuestUI uiManager;

    [Header("초기 활성 퀘스트 (선택 사항)")]
    public List<Quest> activeQuests = new();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        if (uiManager == null)
        {
            Debug.LogWarning("QuestUI가 연결되지 않았습니다!");
            return;
        }

        foreach (var q in activeQuests)
            uiManager.CreateQuestUI(q);
    }

    public void AddQuest(Quest quest)
    {
        if (quest == null || activeQuests.Contains(quest)) return;
        activeQuests.Add(quest);
        uiManager?.CreateQuestUI(quest);
    }

    public void CompleteQuest(Quest quest)
    {
        if (quest == null) return;
        if (!activeQuests.Contains(quest)) return;

        quest.isComplete = true;
        uiManager?.CompleteQuestUI(quest);
        activeQuests.Remove(quest);
    }
}
