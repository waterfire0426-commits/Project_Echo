using UnityEngine;

[CreateAssetMenu(fileName = "NewQuest", menuName = "Quests/Quest")]
public class Quest : ScriptableObject
{
    [TextArea] public string description;
    public bool isComplete = false;

    [HideInInspector] public GameObject uiItem; // UI Text 연결
}
