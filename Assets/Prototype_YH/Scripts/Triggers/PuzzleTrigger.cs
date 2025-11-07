using UnityEngine;

public class PuzzleTrigger : MonoBehaviour
{
    public int puzzleID = 1; // 퍼즐 번호 (1, 2, 3)
    public Act2Trigger act2Trigger;
    bool isDone = false;

    private void OnTriggerEnter(Collider other)
    {
        if (isDone) return;
        if (other.CompareTag("Player"))
        {
            Debug.Log($"[퍼즐] 퍼즐 {puzzleID} 트리거 진입");

            // 실제 퍼즐 완료 신호 전달
            if (act2Trigger != null)
            {
                act2Trigger.ActivatePuzzle(puzzleID);
                Debug.Log($"[퍼즐] 퍼즐 {puzzleID} 완료 전달 → Act2Trigger 호출");
            }

            isDone = true;
        }
    }
}
