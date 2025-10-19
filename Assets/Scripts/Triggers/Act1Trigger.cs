using UnityEngine;

public class Act1Trigger : MonoBehaviour
{
    [Header("Trigger Options")]
    public bool oneTimeOnly = true;   // 한 번만 작동하게 할지
    private bool triggered = false;

    void OnTriggerEnter(Collider other)
    {
        if (triggered && oneTimeOnly) return;
        if (!other.CompareTag("Player")) return;

        triggered = true;

        Debug.Log("[1액트] 회상 트리거 발동!");
        QuestManager.Notify(TRG.ACT1_START); // 퀘스트/연출에 알림
        // TODO: 컷씬/씬전환 등은 팀 연동 포인트
    }
}
