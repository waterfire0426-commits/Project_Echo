using UnityEngine;

public class QuestPanelToggle_YR : MonoBehaviour
{
    [Header("퀘스트 패널 (Canvas나 Panel 오브젝트)")]
    public GameObject questPanel;

    [Header("단축키 설정")]
    public KeyCode toggleKey = KeyCode.Q; // Q키로 열고 닫기

    void Update()
    {
        if (Input.GetKeyDown(toggleKey))
        {
            if (questPanel != null)
            {
                bool isActive = questPanel.activeSelf;
                questPanel.SetActive(!isActive);
            }
        }
    }
}
