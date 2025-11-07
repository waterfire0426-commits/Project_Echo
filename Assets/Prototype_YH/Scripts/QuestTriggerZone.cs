using UnityEngine;

[RequireComponent(typeof(Collider))]
public class QuestTriggerZone : MonoBehaviour
{
    public Quest quest;       // Inspector에서 연결할 퀘스트 데이터
    public bool singleUse = true;

    private bool questAccepted = false; // 이미 수락했는지 체크

    private void Reset()
    {
        var c = GetComponent<Collider>();
        c.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (quest == null)
        {
            Debug.LogWarning("Quest가 연결되지 않았습니다!");
            return;
        }

        // 이미 수락했으면 그냥 리턴
        if (singleUse && questAccepted) return;

        QuestManager.Instance.AddQuest(quest);
        Debug.Log("퀘스트 수락: " + quest.description);

        questAccepted = true; // 한 번 수락되면 다시 수락 못하도록 체크
    }
}
