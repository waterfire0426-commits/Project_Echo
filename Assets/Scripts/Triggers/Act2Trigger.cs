using UnityEngine;

public class Act2Trigger : MonoBehaviour
{
    public bool puzzle1Done;
    public bool puzzle2Done;
    public bool puzzle3Done;

    void Update()
    {
        // 모든 퍼즐이 활성화되면 회상 트리거 실행
        if (puzzle1Done && puzzle2Done && puzzle3Done)
        {
            Debug.Log("⚡ 모든 장치가 작동! 2액트 회상 트리거 발동!");
            // Quest.Notify("act2_start");
            // 나중에 SceneManager.LoadScene("Act2_Scene"); 로 연결 가능
            enabled = false; // 중복 방지
        }
    }

    // 퍼즐 트리거에서 호출할 함수 (퍼즐 1,2,3 각각)
    public void ActivatePuzzle(int id)
    {
        if (id == 1) puzzle1Done = true;
        if (id == 2) puzzle2Done = true;
        if (id == 3) puzzle3Done = true;
        Debug.Log($"퍼즐 {id} 활성화됨");
    }
}
