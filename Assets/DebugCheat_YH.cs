using UnityEngine;

public class DebugCheat_YH : MonoBehaviour
{
    void Update()
    {
        // 숫자키 3 누르면 오염도 3단계로 상승
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            var contam = FindAnyObjectByType<Contamination>();
            if (contam)
            {
                contam.RaiseToStage(3);
                Debug.Log("[디버그] 오염도 강제 3단계 진입");
            }
        }
    }
}
