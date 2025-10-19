using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    void Start()
    {

    }

    void Update()
    {

    }

    public void OnClickNewGame()
    {
        Debug.Log("시작하기");
        SceneManager.LoadScene("MainScene_YR");
    }

    public void OnClickLoad()
    {
        Debug.Log("불러오기기");
    }

    public void OnClickCredits()
    {
        Debug.Log("제작진");
    }

    public void OnClickQuit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
