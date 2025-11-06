using UnityEngine;

public class DialogueTrigger : MonoBehaviour
{
    public DialogueData dialogue;
    public bool oneShot = true;
    private bool triggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (triggered && oneShot) return;
        if (!other.CompareTag("Player")) return;

        FindObjectOfType<DialogueManager>().PlayDialogue(dialogue);
        triggered = true;
    }

    // 필요 시 공개 메서드도 추가 (이벤트에서 호출)
    public void TriggerNow()
    {
        if (triggered && oneShot) return;
        FindObjectOfType<DialogueManager>().PlayDialogue(dialogue);
        triggered = true;
    }
}
