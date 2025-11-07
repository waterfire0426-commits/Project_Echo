using UnityEngine;
using UnityEngine.UI;

public class CrosshairUI : MonoBehaviour
{
    [Header("Refs")]
    public Image dot;   // 중앙 점(UI Image)
    public Image ring;  // 속 빈 링(UI Image) - Source Image 비워둬도 됨(아래에서 생성)

    [Header("Sizes (px)")]
    public float dotSize = 6f;          // 평소 점 크기
    public float ringSizeIdle = 20f;    // 평소 링 지름
    public float ringSizeActive = 28f;  // 조준 시 링 지름(커짐)
    public float ringThickness = 2f;    // 링 두께(px)

    [Header("Lerp")]
    public float sizeLerp = 14f;        // 크기 보간 속도(높을수록 빠름)

    bool _active;
    Vector2 _ringSizeCurrent;

    void Awake()
    {
        // 링 스프라이트가 없으면 자동 생성(속 빈 원)
        if (ring && ring.sprite == null)
            ring.sprite = GenerateRingSprite(64, Mathf.Max(1, (int)ringThickness)); // 64x64 텍스처, 두께 적용

        // 시작 상태: 조준 아님
        SetActive(false, instant:true);

        // 점/링 초기 크기 세팅
        if (dot)  dot.rectTransform.sizeDelta  = Vector2.one * dotSize;
        _ringSizeCurrent = Vector2.one * ringSizeIdle;
        if (ring) ring.rectTransform.sizeDelta = _ringSizeCurrent;
    }

    void Update()
    {
        if (!ring) return;

        // 목표 크기: 조준 시 크다, 아닐 땐 작다
        float target = _active ? ringSizeActive : ringSizeIdle;
        _ringSizeCurrent = Vector2.Lerp(
            _ringSizeCurrent, 
            new Vector2(target, target),
            Time.unscaledDeltaTime * sizeLerp
        );
        ring.rectTransform.sizeDelta = _ringSizeCurrent;
    }

    /// <summary> PlayerInteractor에서 호출 </summary>
    public void SetActive(bool canInteract) => SetActive(canInteract, instant:false);

    void SetActive(bool canInteract, bool instant)
    {
        _active = canInteract;

        if (dot)  dot.enabled  = !canInteract; // 평소엔 점
        if (ring) ring.enabled =  canInteract; // 조준 시 링

        if (instant && ring)
        {
            float s = canInteract ? ringSizeActive : ringSizeIdle;
            _ringSizeCurrent = new Vector2(s, s);
            ring.rectTransform.sizeDelta = _ringSizeCurrent;
        }
    }

    // === 속 빈 원(도넛) 스프라이트 생성 ===
    Sprite GenerateRingSprite(int diameter, int thickness)
    {
        var tex = new Texture2D(diameter, diameter, TextureFormat.ARGB32, false);
        tex.wrapMode   = TextureWrapMode.Clamp;
        tex.filterMode = FilterMode.Bilinear;

        float rOuter = (diameter - 1) * 0.5f;
        float rInner = Mathf.Max(0, rOuter - thickness);

        for (int y = 0; y < diameter; y++)
        {
            for (int x = 0; x < diameter; x++)
            {
                float dx = x - rOuter;
                float dy = y - rOuter;
                float d  = Mathf.Sqrt(dx*dx + dy*dy);

                bool on = (d <= rOuter + 0.01f) && (d >= rInner - 0.01f);
                tex.SetPixel(x, y, on ? Color.white : new Color(0,0,0,0));
            }
        }
        tex.Apply(false, true);

        // 중앙 피벗으로 스프라이트 생성
        var sp = Sprite.Create(tex, new Rect(0,0,diameter,diameter), new Vector2(0.5f, 0.5f), 100f);
        sp.name = "GeneratedRing";
        return sp;
    }
}
