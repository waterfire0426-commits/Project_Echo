using UnityEngine;
using UnityEngine.UI;

public class DarkOverlayTest : MonoBehaviour
{
    public Image img;
    void Start()
    {
        if (!img) img = GetComponent<Image>();
    }

    void Update()
    {
        // 1 키 누르면 점점 어두워짐
        if (Input.GetKey(KeyCode.Alpha1))
        {
            var c = img.color;
            c.a = Mathf.Clamp01(c.a + Time.deltaTime);
            img.color = c;
            Debug.Log($"DarkOverlay Alpha: {c.a:F2}");
        }

        // 2 키 누르면 다시 밝아짐
        if (Input.GetKey(KeyCode.Alpha2))
        {
            var c = img.color;
            c.a = Mathf.Clamp01(c.a - Time.deltaTime);
            img.color = c;
            Debug.Log($"DarkOverlay Alpha: {c.a:F2}");
        }
    }
}
