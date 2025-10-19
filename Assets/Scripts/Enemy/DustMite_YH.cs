// File: Assets/Scripts/Enemy/DustMite_YH.cs
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class DustMite_YH : MonoBehaviour
{
    [Header("이동")]
    public float wanderRadius = 4f;
    public float speed = 2f;

    [Header("오염")]
    public float contamOnTouch = 2f;
    public float touchCooldown = 1.5f;

    float nextTouchTime;
    Vector3 target;

    void Start()
    {
        GetComponent<Collider>().isTrigger = true;
        PickNewTarget();
    }

    void Update()
    {
        Vector3 dir = (target - transform.position);
        dir.y = 0;
        if (dir.magnitude < 0.2f) PickNewTarget();
        else transform.position += dir.normalized * speed * Time.deltaTime;
    }

    void PickNewTarget()
    {
        Vector2 r = Random.insideUnitCircle * wanderRadius;
        target = new Vector3(transform.position.x + r.x, transform.position.y, transform.position.z + r.y);
    }

    void OnTriggerStay(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        // UV 라이트가 켜져 있으면 즉시 제거
        if (PlayerHasActiveUV(other.gameObject))
        {
            Debug.Log("🦠 [먼지 진드기] UV에 의해 제거됨");
            Destroy(gameObject);
            return;
        }

        if (Time.time < nextTouchTime) return;
        nextTouchTime = Time.time + touchCooldown;

        var contam = other.GetComponent<Contamination>();
        if (contam) contam.Add(contamOnTouch);
        Debug.Log("🦠 [먼지 진드기] 접촉 → 오염도 소량 상승");
    }

    bool PlayerHasActiveUV(GameObject player)
    {
        // 방법 1) 태그로 찾은 Light가 켜져 있으면 UV 켠 것으로 간주
        var lights = GameObject.FindGameObjectsWithTag("UVEmitter");
        foreach (var l in lights)
        {
            if (!l.activeInHierarchy) continue;
            var li = l.GetComponentInChildren<Light>(true);
            if (li && li.enabled) return true;
        }
        return false;
    }
}
