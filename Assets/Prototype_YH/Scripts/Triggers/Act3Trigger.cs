using UnityEngine;

public class Act3Trigger : MonoBehaviour
{
    [Header("References")]
    public Contamination contamination; // Player의 Contamination 스크립트 drag
    public GameObject mainCreaturePrefab; // 나중에 만들 메인 괴물 프리팹 drag

    private bool triggered = false;

    // 다른 스크립트(DataTerminal 등)에서 이 함수를 직접 호출해줄 거야.
    public void OnDownloadComplete()
    {
        if (triggered) return;
        triggered = true;

        Debug.Log("[Act3Trigger] 데이터 단말 다운로드 완료 → Act3 회상 발동");

        // 1) 회상 이벤트
        QuestManager.Notify("act3_start");
        Debug.Log("[Act3Trigger] 회상 이벤트 발동 (act3_start)");

        // 2) 영구 오염 4단계 진입
        if (contamination != null)
        {
            contamination.RaiseToStage(4);
            Debug.Log("[Act3Trigger] 영구 오염 4단계 진입");
        }
        else
        {
            Debug.LogWarning("[Act3Trigger] contamination 레퍼런스가 없습니다!");
        }

        // 3) 메인 괴물 스폰
        if (mainCreaturePrefab != null)
        {
            Instantiate(mainCreaturePrefab, transform.position + Vector3.forward * 5f, Quaternion.identity);
            Debug.Log("[Act3Trigger] 메인 괴물 등장!");
        }
        else
        {
            Debug.LogWarning("[Act3Trigger] mainCreaturePrefab이 비어 있습니다!");
        }
    }
}
