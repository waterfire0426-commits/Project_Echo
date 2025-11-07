using UnityEngine;

public class MiniGameBridge : MonoBehaviour
{
    // 진행률 0~1 브로드캐스트(선택)
    public System.Action<float> OnProgress;

    // 게이지 100% 완료 시 호출(성공/실패 개념 없음)
    public System.Action OnFinished;

    // UI 닫힘(취소/종료) 알림
    public System.Action OnClosed;

    // ===== UI팀이 아래 메서드들을 적절한 타이밍에 호출 =====

    // 게이지 변화 시 0~1 값으로 호출
    public void UpdateProgress(float normalized01)
    {
        if (normalized01 < 0f) normalized01 = 0f;
        if (normalized01 > 1f) normalized01 = 1f;
        OnProgress?.Invoke(normalized01);
        if (Mathf.Approximately(normalized01, 1f))
        {
            // 100%면 자동으로 완료 처리하고 닫아도 됨(원하면 수동 Finish만 사용)
            Finish();
        }
    }

    // 게이지 100% 도달(완료 신호)
    public void Finish()
    {
        OnFinished?.Invoke();
        Destroy(gameObject);
    }

    // 창 닫기(취소 등)
    public void Close()
    {
        OnClosed?.Invoke();
        Destroy(gameObject);
    }
}
