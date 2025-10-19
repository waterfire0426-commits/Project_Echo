// File: GameDebugOverlay_YH.cs
using UnityEngine;

public class GameDebugOverlay_YH : MonoBehaviour
{
    public KeyCode toggleKey = KeyCode.F1;
    public bool visible = true;

    Contamination contam;
    PlayerHealth_YH hp;          // 없으면 null 그대로 OK
    Light uvLight;               // 태그 "UVEmitter" 달린 라이트 자동검색

    void Awake()
    {
        contam = FindFirstObjectByType<Contamination>();
        hp = FindFirstObjectByType<PlayerHealth_YH>(); // 없으면 표시 안 함
        var uvGo = GameObject.FindGameObjectWithTag("UVEmitter");
        if (uvGo) uvLight = uvGo.GetComponent<Light>();
    }

    void Update()
    {
        if (Input.GetKeyDown(toggleKey)) visible = !visible;
    }

    void OnGUI()
    {
        if (!visible) return;

        GUI.matrix = Matrix4x4.Scale(new Vector3(1.2f, 1.2f, 1));
        var rect = new Rect(10, 10, 600, 300);

        string uv = uvLight ? (uvLight.enabled ? "ON" : "OFF") : "N/A";
        string hpStr = hp ? $"{hp.Current}/{hp.Max}" : "N/A";
        string cStr = contam ? $"Value {contam.value:0.0} / Stage {contam.Stage}" : "N/A";

        GUI.color = new Color(0,0,0,0.6f);
        GUI.Box(rect, GUIContent.none);
        GUI.color = Color.white;

        GUILayout.BeginArea(rect);
        GUILayout.Label("=== DEBUG HUD ===");
        GUILayout.Label($"Contamination: {cStr}");
        GUILayout.Label($"Player HP: {hpStr}");
        GUILayout.Label($"UV Light: {uv} (tag=UVEmitter)");
        GUILayout.EndArea();
    }
}
