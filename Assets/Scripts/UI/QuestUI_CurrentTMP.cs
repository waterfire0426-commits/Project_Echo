// File: QuestUI_CurrentTMP.cs
using UnityEngine;
using TMPro;

[RequireComponent(typeof(TMP_Text))]
public class QuestUI_CurrentTMP : MonoBehaviour
{
    public string title = "Objective";
    public bool autoAnchorTopLeft = true;
    public Vector2 margin = new Vector2(16f, -16f);
    public bool hideWhenAllDone = false;

    TMP_Text t;
    bool hooked;

    void Awake()
    {
        t = GetComponent<TMP_Text>();
        t.richText = true;

        if (autoAnchorTopLeft)
        {
            var rt = (RectTransform)transform;
            rt.anchorMin = new Vector2(0f, 1f);
            rt.anchorMax = new Vector2(0f, 1f);
            rt.pivot     = new Vector2(0f, 1f);
            rt.anchoredPosition = margin;
        }

        t.text = $"<b>{title}</b>\n(퀘스트 없음)";
    }

    void OnEnable()  { TryHook(); }
    void Update()    { if (!hooked) TryHook(); }

    void TryHook()
    {
        var qm = QuestManager_YH.Instance;
        if (qm == null) return;

        qm.OnChanged -= Redraw; // 중복 구독 방지
        qm.OnChanged += Redraw;
        hooked = true;
        Redraw();
    }

    void OnDisable()
    {
        var qm = QuestManager_YH.Instance;
        if (qm != null) qm.OnChanged -= Redraw;
        hooked = false;
    }

    void Redraw()
    {
        var qm = QuestManager_YH.Instance;
        if (!qm) { t.text = $"<b>{title}</b>\n(퀘스트 없음)"; t.enabled = true; return; }

        t.text = qm.BuildCurrentText(title);

        if (hideWhenAllDone && qm.currentIndex >= qm.steps.Count)
            t.enabled = false;
        else
            t.enabled = true;
    }
}
