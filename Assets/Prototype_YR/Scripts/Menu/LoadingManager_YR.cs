using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

public class LoadingManager : MonoBehaviour
{
    [Header("UI References")]
    public Slider progressBar;
    public TMP_Text percentageText;
    public TMP_Text loadingText;

    private string nextScene;

    void Start()
    {
        nextScene = PlayerPrefs.GetString("NextScene", "MainScene_YR");
        StartCoroutine(LoadSceneAsync());
    }

    IEnumerator LoadSceneAsync()
    {
        yield return new WaitForSeconds(1f); // 시작 연출

        AsyncOperation op = SceneManager.LoadSceneAsync(nextScene);
        op.allowSceneActivation = false;

        float displayProgress = 0f;

        while (!op.isDone)
        {
            float targetProgress = Mathf.Clamp01(op.progress / 0.9f);

            // 실제 로딩 속도보다 느리게 채워줌
            displayProgress = Mathf.MoveTowards(displayProgress, targetProgress, Time.deltaTime * 0.15f);

            progressBar.value = displayProgress;
            percentageText.text = $"{(displayProgress * 100f):F0}%";
            loadingText.text = "Loading" + new string('.', Mathf.FloorToInt(Time.time % 3 + 1));

            // 100%에 도달했을 때 살짝 멈춘 후 전환
            if (displayProgress >= 1f && op.progress >= 0.9f)
            {
                yield return new WaitForSeconds(1.2f);
                op.allowSceneActivation = true;
            }

            yield return null;
        }
    }
}
