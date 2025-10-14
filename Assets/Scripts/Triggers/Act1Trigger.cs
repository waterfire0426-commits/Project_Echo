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
        Quest.Notify("act1_start"); // 퀘스트 시스템에 알림 (있다면)

        // TODO: 이후 컷씬 or 씬 전환 연출 추가
        // 예시: SceneLoader.Load("Act1_Cutscene");
        // 또는 LightController.StartFlicker();
    }
}
