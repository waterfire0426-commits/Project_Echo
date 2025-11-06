using UnityEngine;

[CreateAssetMenu(fileName = "NewDialogueData", menuName = "Dialogue/Dialogue Data")]
public class DialogueData : ScriptableObject
{
    [System.Serializable]
    public struct DialogueLine
    {
        public string speakerName;
        [TextArea(2, 5)] public string text;
        public string voiceDebugID;
        public AudioClip voiceClip;
        public float displayTime;
    }

    public DialogueLine[] lines;
}
