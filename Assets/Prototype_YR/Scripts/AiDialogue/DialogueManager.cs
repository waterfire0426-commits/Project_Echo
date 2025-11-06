using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.Events;

public class DialogueManager : MonoBehaviour
{
    [Header("UI")]
    public GameObject dialoguePanel;         // 대화창
    public TextMeshProUGUI dialogueText;     // 대화 텍스트
    public float defaultLineTime = 4f;

    [Header("Audio")]
    public AudioSource voiceAudioSource;     // 대화 전용 AudioSource
    public bool logVoiceDebug = true;

    [Header("Events")]
    public UnityEvent onDialogueStart;
    public UnityEvent onDialogueEnd;

    // ✅ 이미 재생된 대화 저장 (런타임 캐시)
    private HashSet<DialogueData> playedDialogues = new HashSet<DialogueData>();

    private Coroutine running;

    // 대사 실행 (이미 재생된 건 무시)
    public void PlayDialogue(DialogueData data)
    {
        if (data == null)
        {
            Debug.LogWarning("DialogueData가 비어있습니다!");
            return;
        }

        if (playedDialogues.Contains(data))
        {
            Debug.Log($"[DialogueManager] '{data.name}'은(는) 이미 재생된 대사입니다.");
            return;
        }

        playedDialogues.Add(data); // 한 번만 재생되도록 등록

        if (running != null)
            StopCoroutine(running);

        running = StartCoroutine(PlayRoutine(data));
    }

    private IEnumerator PlayRoutine(DialogueData data)
    {
        onDialogueStart?.Invoke();
        dialoguePanel.SetActive(true);

        foreach (var line in data.lines)
        {
            string speaker = string.IsNullOrEmpty(line.speakerName) ? "" : $"<b>{line.speakerName}</b>: ";
            dialogueText.text = speaker + line.text;

            // 디버그 / 오디오 재생
            if (logVoiceDebug)
                Debug.Log($"[VoiceTrigger] ▶ {line.voiceDebugID ?? "NO_ID"} (Trigger here for audio).");

            if (line.voiceClip != null && voiceAudioSource != null)
            {
                voiceAudioSource.clip = line.voiceClip;
                voiceAudioSource.Play();
            }

            float waitTime = (line.displayTime > 0f) ? line.displayTime : defaultLineTime;
            yield return new WaitForSeconds(waitTime);
        }

        dialoguePanel.SetActive(false);
        onDialogueEnd?.Invoke();
        running = null;
    }

    // 빠른 취소 / 대사 건너뛰기 기능
    public void Skip()
    {
        if (running != null)
        {
            StopCoroutine(running);
            dialoguePanel.SetActive(false);
            running = null;
        }
    }

    // ✅ (선택) 모든 대사 초기화 - 예를 들어 Scene 재시작 시 호출
    public void ResetPlayedDialogues()
    {
        playedDialogues.Clear();
    }
}
