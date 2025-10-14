using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

[DefaultExecutionOrder(-50)]
public class HotbarUI_Auto : MonoBehaviour
{
    public Hotbar hotbar;
    public Vector2 marginBR = new Vector2(24, 24);
    public float slotSize = 64f;
    public float spacing = 8f;

    public Color slotBg = new Color(0,0,0,0.4f);
    public Color selectTint = new Color(1,1,1,0.9f);
    public int countFontSize = 18;
    public int keyFontSize = 14;

    RectTransform root;
    HorizontalLayoutGroup hlg;
    readonly List<HotbarSlotUI> views = new();

    void Awake()
    {
        if (!hotbar)
        {
    #if UNITY_2023_1_OR_NEWER
            hotbar = FindFirstObjectByType<Hotbar>();   
    #else
            hotbar = FindObjectOfType<Hotbar>();       
    #endif
        }
        Build();
    }

    void OnEnable(){ if (hotbar) hotbar.OnChanged += Redraw; Redraw(); }
    void OnDisable(){ if (hotbar) hotbar.OnChanged -= Redraw; }

    void Build()
    {
        var go = new GameObject("HotbarPanel_Auto", typeof(RectTransform));
        go.transform.SetParent(transform, false);
        root = go.GetComponent<RectTransform>();
        root.anchorMin = new Vector2(1,0); root.anchorMax = new Vector2(1,0);
        root.pivot = new Vector2(1,0); root.anchoredPosition = new Vector2(-marginBR.x, marginBR.y);

        hlg = go.AddComponent<HorizontalLayoutGroup>();
        hlg.childAlignment = TextAnchor.MiddleRight;
        hlg.spacing = spacing;
        hlg.childControlWidth = hlg.childControlHeight = false;
        hlg.childForceExpandWidth = hlg.childForceExpandHeight = false;

        views.Clear();
        int n = hotbar ? hotbar.size : 8;
        for (int i = 0; i < n; i++)
        {
            views.Add(MakeSlot(root, i));
            views[i].SetKey((i+1).ToString());
        }
    }

   HotbarSlotUI MakeSlot(Transform parent, int index)
{
    // 배경
    var slotGO = new GameObject($"Slot_{index+1}", typeof(RectTransform), typeof(Image));
    slotGO.transform.SetParent(parent, false);
    var rt = (RectTransform)slotGO.transform;
    rt.sizeDelta = new Vector2(slotSize, slotSize);
    var bg = slotGO.GetComponent<Image>();
    bg.color = slotBg;

    // 슬롯 UI 본체
    var slotUI = slotGO.AddComponent<HotbarSlotUI>();

    // ── 테두리(Outline) 추가 ────────────────────────────────
    var ol = slotGO.AddComponent<Outline>();
    ol.effectColor = new Color(1f, 1f, 1f, 0.9f);   // 흰 테두리
    ol.effectDistance = new Vector2(2f, 2f);        // 두께 느낌
    ol.enabled = false;                             // 기본 Off
    slotUI.outline = ol;
    // ─────────────────────────────────────────────────────────────

    // Icon
    var iconGO = new GameObject("Icon", typeof(RectTransform), typeof(Image));
    iconGO.transform.SetParent(slotGO.transform, false);
    var iconRT = (RectTransform)iconGO.transform;
    iconRT.anchorMin = Vector2.zero; iconRT.anchorMax = Vector2.one;
    iconRT.offsetMin = new Vector2(6, 6); iconRT.offsetMax = new Vector2(-6, -6);
    var iconImg = iconGO.GetComponent<Image>();
    iconImg.preserveAspect = true;
    slotUI.icon = iconImg;

    // Count
    var countGO = new GameObject("Count", typeof(RectTransform), typeof(TextMeshProUGUI));
    countGO.transform.SetParent(slotGO.transform, false);
    var cRT = (RectTransform)countGO.transform;
    cRT.anchorMin = cRT.anchorMax = new Vector2(1, 1);
    cRT.pivot = new Vector2(1, 1);
    cRT.anchoredPosition = new Vector2(-6, -6);
    var countTMP = countGO.GetComponent<TextMeshProUGUI>();
    countTMP.fontSize = countFontSize;
    countTMP.alignment = TextAlignmentOptions.TopRight;
    countTMP.text = "";
    slotUI.countText = countTMP;

    // Key
    var keyGO = new GameObject("Key", typeof(RectTransform), typeof(TextMeshProUGUI));
    keyGO.transform.SetParent(slotGO.transform, false);
    var kRT = (RectTransform)keyGO.transform;
    kRT.anchorMin = kRT.anchorMax = new Vector2(0.5f, 0);
    kRT.pivot = new Vector2(0.5f, 0);
    kRT.anchoredPosition = new Vector2(0, 6);
    var keyTMP = keyGO.GetComponent<TextMeshProUGUI>();
    keyTMP.fontSize = keyFontSize;
    keyTMP.alignment = TextAlignmentOptions.Bottom;
    keyTMP.text = (index + 1).ToString();
    slotUI.keyText = keyTMP;

    return slotUI;
}


    void Redraw()
    {
        if (!hotbar || views.Count != hotbar.size) return;
        for (int i = 0; i < hotbar.size; i++)
        {
            var s = hotbar.slots[i];
            var spr = (s != null && s.def) ? s.def.icon : null;
            int cnt = (s != null) ? s.count : 0;
            views[i].Bind(spr, cnt, i == hotbar.selected);
        }
    }
}
