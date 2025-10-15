using UnityEngine;

public class WaterBlockTrigger : MonoBehaviour
{
    private bool triggered = false;

    void OnTriggerEnter(Collider other)
    {
        if (triggered) return; // 중복 방지
        if (other.CompareTag("Player"))
        {
            triggered = true;
            Debug.Log("💧물이 가득 차 있어 진입이 불가능하다. 다른 길을 찾아야 한다.");

            // 나중에 UI나 퀘스트 연결할 때 이 부분만 추가하면 됨
            // QuestManager.Notify("blocked_by_water");
        }
    }
}
