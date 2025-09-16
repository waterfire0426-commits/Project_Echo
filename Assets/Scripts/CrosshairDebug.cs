using UnityEngine;

public class CrosshairDebug : MonoBehaviour
{
    [Header("Sizes (px)")]
    public int dotDiameter  = 5;   // 평소 점 지름
    public int ringDiameter = 20;   // 상호작용 시 동그라미 지름
    public int ringThickness = 1;   // 링 테두리 두께(px)

    bool _active = false;           // 상호작용 가능 여부
    Texture2D _dotTex, _ringTex;

    public void SetActive(bool canInteract) => _active = canInteract;

    void OnGUI()
    {
        // 필요 시 텍스처 생성/갱신
        EnsureTextures();

        Texture2D tex = _active ? _ringTex : _dotTex;
        int w = tex.width, h = tex.height;

        float x = (Screen.width  - w) * 0.5f;
        float y = (Screen.height - h) * 0.5f;

        GUI.color = Color.white; // 흰색으로
        GUI.DrawTexture(new Rect(x, y, w, h), tex, ScaleMode.ScaleToFit, true); // true=알파 사용
    }

    void EnsureTextures()
    {
        if (_dotTex == null || _dotTex.width != dotDiameter || _dotTex.height != dotDiameter)
            _dotTex = MakeCircle(dotDiameter, filled:true,  thickness:0);

        if (_ringTex == null || _ringTex.width != ringDiameter || _ringTex.height != ringDiameter)
            _ringTex = MakeCircle(ringDiameter, filled:false, thickness:Mathf.Max(1, ringThickness));
    }

    // 지름/두께로 원 텍스처 생성(투명 배경)
    Texture2D MakeCircle(int diameter, bool filled, int thickness)
    {
        var tex = new Texture2D(diameter, diameter, TextureFormat.ARGB32, false);
        tex.wrapMode   = TextureWrapMode.Clamp;
        tex.filterMode = FilterMode.Point;

        float r = (diameter - 1) * 0.5f; // 픽셀센터 기준 반지름
        float inner = Mathf.Max(0, r - thickness); // 링 안쪽 경계

        for (int y = 0; y < diameter; y++)
        {
            for (int x = 0; x < diameter; x++)
            {
                float dx = x - r;
                float dy = y - r;
                float d  = Mathf.Sqrt(dx*dx + dy*dy);

                bool on;
                if (filled)
                    on = d <= r + 0.01f;                          // 꽉 찬 원
                else
                    on = (d <= r + 0.01f) && (d >= inner - 0.01f); // 테두리만

                tex.SetPixel(x, y, on ? Color.white : new Color(0,0,0,0));
            }
        }
        tex.Apply(false, true);
        return tex;
    }
}
