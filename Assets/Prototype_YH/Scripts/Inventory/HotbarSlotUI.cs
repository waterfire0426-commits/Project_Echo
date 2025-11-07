using UnityEngine;
using UnityEngine.UI;     // Image, Outline
using TMPro;              // TMP_Text

public class HotbarSlotUI : MonoBehaviour
{
    public Image icon;
    public TMP_Text countText;
    public TMP_Text keyText;

    // 채워진 사각형 대신 테두리만 쓰고 싶어서 Outline을 사용
    public Image selectFrame;   
    public Outline outline; 

    public void SetKey(string s) { if (keyText) keyText.text = s; }

    public void Bind(Sprite spr, int count, bool selected)
    {
        if (icon) { icon.sprite = spr; icon.enabled = spr != null; }
        if (countText) countText.text = (count > 1) ? count.ToString() : "";
        if (selectFrame) selectFrame.enabled = false; // 채우기 프레임은 끈다
        if (outline) outline.enabled = selected;      // 선택 시 테두리만 On
    }
}
