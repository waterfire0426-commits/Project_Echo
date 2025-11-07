using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(Light))]
public class FlashlightCookieGen : MonoBehaviour
{
    [Header("Cookie Texture")]
    public int size = 512;

    [Header("Hotspot Shape")]
    [Range(0f,1f)] public float inner   = 0.24f;
    [Range(0f,1f)] public float feather = 0.40f;
    [Range(0.1f,4f)] public float gamma = 1.8f;

    private Texture2D tex;
    private Light L;

    void OnEnable()
    {
        L = GetComponent<Light>();
        if (L.type != LightType.Spot)
            Debug.LogWarning("[FlashlightCookieGen] Spot Light에서 사용하세요.");

        Build(forceNew: true);
    }

    void OnDisable()
    {
        if (L) L.cookie = null;
        if (tex)
        {
            if (Application.isPlaying) Destroy(tex);
            else DestroyImmediate(tex);
            tex = null;
        }
    }

    void OnValidate()
    {
        if (!isActiveAndEnabled) return;
        Build(forceNew: false);
    }

    void Build(bool forceNew)
    {
        if (!L) L = GetComponent<Light>();

        // 필요 시 새 텍스처 생성
        if (forceNew || tex == null || tex.width != size || !tex.isReadable)
        {
            if (tex)
            {
                if (Application.isPlaying) Destroy(tex);
                else DestroyImmediate(tex);
            }
            tex = new Texture2D(size, size, TextureFormat.Alpha8, false, true)
            {
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Bilinear,
                name = "FlashlightCookie_Runtime",
                hideFlags = HideFlags.DontSave
            };
        }

        float half = (size - 1) * 0.5f;
        float rInner = Mathf.Clamp01(inner);
        float edgeEnd = Mathf.Clamp01(rInner + feather);

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dx = (x - half) / half;
                float dy = (y - half) / half;
                float d  = Mathf.Sqrt(dx * dx + dy * dy);

                float a = d <= rInner
                    ? 1f
                    : Mathf.Pow(Mathf.Clamp01(Mathf.InverseLerp(edgeEnd, rInner, d)), 1f / gamma);

                tex.SetPixel(x, y, new Color(1, 1, 1, a));
            }
        }

        tex.Apply(false, false);
        L.cookie = tex;
    }
}
