// File: Assets/Scripts/Enemy/SimpleSpawner_YH.cs
using UnityEngine;

public class SimpleSpawner_YH : MonoBehaviour
{
    public GameObject prefab;
    public float interval = 6f;
    public int maxAlive = 3;
    float t;
    int alive;

    void Update()
    {
        if (!prefab) return;
        t += Time.deltaTime;
        if (t >= interval && alive < maxAlive)
        {
            t = 0f;
            var go = Instantiate(prefab, transform.position, transform.rotation);
            alive++;
            go.AddComponent<AutoCount_YH>().onDestroyed = () => alive--;
        }
    }

    class AutoCount_YH : MonoBehaviour
    {
        public System.Action onDestroyed;
        void OnDestroy() { onDestroyed?.Invoke(); }
    }
}
