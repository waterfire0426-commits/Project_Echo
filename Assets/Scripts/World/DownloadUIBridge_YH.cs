// Assets/Scripts/World/DownloadUIBridge_YH.cs
using UnityEngine;
using System;

[RequireComponent(typeof(DownloadUIManager))]
public class DownloadUIBridge_YH : MonoBehaviour
{
    public DownloadUIManager ui;

    void Awake()
    {
        if (!ui) ui = GetComponent<DownloadUIManager>();
    }

    /// UI 열기 + 모드 선택 시작
    /// 버튼이 없다면 onSlow를 바로 실행하게 두면 됨.
    public void Open(Action onSlow, Action onFast, bool autoStartSlow = true)
    {
        if (!ui) return;
        ui.StartDownload();              // 캔버스 ON + 내부 초기화
        if (autoStartSlow) onSlow?.Invoke();
        else onSlow?.Invoke();           // 버튼이 생기면 여기서 onClick에 연결
    }

    public void ShowProgress(float p01)
    {
        if (!ui) return;
        if (ui.progressBar) ui.progressBar.value = p01;
        if (ui.percentageText) ui.percentageText.text = Mathf.RoundToInt(p01 * 100f) + "%";
    }

    /// UI 닫기 (EndDownload가 private이므로 직접 캔버스를 끔)
    public void Close()
    {
        if (ui && ui.downloadCanvas) ui.downloadCanvas.SetActive(false);
    }
}
