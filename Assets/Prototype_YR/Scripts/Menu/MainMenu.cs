// using UnityEngine;
// using UnityEngine.SceneManagement;

// public class MainMenu : MonoBehaviour
// {
//     void Start()
//     {

//     }

//     void Update()
//     {

//     }

//     public void OnClickNewGame()
//     {
//         Debug.Log("시작하기");
//         SceneManager.LoadScene("MainScene_YR");
//     }

//     public void OnClickLoad()
//     {
//         Debug.Log("불러오기");
//     }

//     public void OnClickQuit()
//     {
// #if UNITY_EDITOR
//         UnityEditor.EditorApplication.isPlaying = false;
// #else
//         Application.Quit();
// #endif
//     }
// }

using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void OnClickNewGame()
    {
        Debug.Log("시작하기");

        // 다음 씬 이름을 저장 (로딩씬에서 불러오기 위해)
        PlayerPrefs.SetString("NextScene", "MainScene_YR");

        // 로딩씬으로 이동
        SceneManager.LoadScene("LoadingScene_YR");
    }

    public void OnClickLoad()
    {
        Debug.Log("불러오기");
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
