using UnityEngine;

public class ContamHook_YH : MonoBehaviour
{
    Contamination contam;

    void Awake()
    {
        // 프로젝트에 하나 있는 Contamination을 찾아 둠
        contam = FindFirstObjectByType<Contamination>();
    }

    /// <summary>일시적으로 오염 수치를 증감(+/-)</summary>
    public void AddTemp(float amount)
    {
        if (contam) contam.Add(amount);
    }

    /// <summary>영구 오염 단계를 올림(예: +1 → 다음 단계)</summary>
    public void RaisePermanentStage(int delta)
    {
        if (!contam) return;
        contam.RaiseToStage(Mathf.Clamp(contam.Stage + delta, 0, 5));
    }
}
