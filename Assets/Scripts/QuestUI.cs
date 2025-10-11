using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;  // Text 사용

public class QuestUI : MonoBehaviour
{
    public Transform questsParent;     // 퀘스트 표시 패널
    public GameObject questItemPrefab; // 퀘스트 한 줄 UI 프리팹

    private Dictionary<Quest, GameObject> questUIMap = new();

    // 퀘스트 UI 생성
    public void CreateQuestUI(Quest quest)
    {
        if (quest == null || questUIMap.ContainsKey(quest)) return;

        GameObject item = Instantiate(questItemPrefab, questsParent);
        var text = item.GetComponentInChildren<Text>(); 
        if (text != null)
            text.text = quest.description;

        var cg = item.GetComponent<CanvasGroup>();
        if (cg != null) cg.alpha = 1f;

        questUIMap.Add(quest, item);
    }

    // 퀘스트 완료 시 UI 제거
    public void CompleteQuestUI(Quest quest)
    {
        if (quest == null) return;
        if (questUIMap.TryGetValue(quest, out GameObject item))
        {
            StartCoroutine(FadeAndDestroy(item));
            questUIMap.Remove(quest);
        }
    }

    // 페이드 아웃 후 삭제
    private IEnumerator FadeAndDestroy(GameObject item)
    {
        CanvasGroup cg = item.GetComponent<CanvasGroup>();
        if (cg == null) cg = item.AddComponent<CanvasGroup>();

        float t = 0f, duration = 0.5f;
        Vector3 startScale = item.transform.localScale;

        while (t < duration)
        {
            t += Time.deltaTime;
            float r = t / duration;
            cg.alpha = 1f - r;
            item.transform.localScale = Vector3.Lerp(startScale, Vector3.zero, r);
            yield return null;
        }

        Destroy(item);
    }
}
