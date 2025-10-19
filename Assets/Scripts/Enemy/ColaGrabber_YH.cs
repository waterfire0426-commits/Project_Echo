// File: Assets/Scripts/Enemy/ColaGrabber_YH.cs
using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Collider))]
public class ColaGrabber_YH : MonoBehaviour
{
    [Header("이동/탐지")]
    public float viewDistance = 18f;
    [Range(0,180)] public float viewAngle = 80f;
    public float speed = 4f;

    [Header("공격/잡기")]
    public float grabRange = 1.6f;
    public float contamOnFail = 10f;
    public string mentosItemId = "mentos";
    public KeyCode mashKey = KeyCode.E;
    public int mashCountToEscape = 12;
    public float grabDuration = 3f;

    Transform player;
    bool grabbing = false;

    void Start()
    {
        GetComponent<Collider>().isTrigger = false;
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
    }

    void Update()
    {
        if (!player || grabbing) return;

        if (CanSeePlayer())
        {
            Vector3 dir = (player.position - transform.position);
            dir.y = 0;
            float dist = dir.magnitude;

            if (dist > grabRange)
            {
                transform.position += dir.normalized * speed * Time.deltaTime;
                Face(player.position);
            }
            else
            {
                StartCoroutine(GrabRoutine());
            }
        }
    }

    bool CanSeePlayer()
    {
        Vector3 eye = transform.position + Vector3.up * 1.6f;
        Vector3 toP = player.position - eye;
        if (toP.magnitude > viewDistance) return false;
        if (Vector3.Angle(transform.forward, toP.normalized) > viewAngle * 0.5f) return false;

        if (Physics.Raycast(eye, toP.normalized, out RaycastHit hit, viewDistance, ~0, QueryTriggerInteraction.Ignore))
            return hit.transform.CompareTag("Player");
        return false;
    }

    void Face(Vector3 pos)
    {
        Vector3 dir = pos - transform.position; dir.y = 0;
        if (dir.sqrMagnitude < 0.001f) return;
        var look = Quaternion.LookRotation(dir);
        transform.rotation = Quaternion.Slerp(transform.rotation, look, Time.deltaTime * 8f);
    }

    IEnumerator GrabRoutine()
    {
        grabbing = true;
        Debug.Log("🥤 [콜라 인간] 플레이어를 붙잡았다! (E 연타로 탈출, 멘토스 사용 시 즉시 퇴치)");

        // 멘토스 즉시 퇴치 체크
        var hotbar = player.GetComponentInChildren<Hotbar>();
        if (hotbar && hotbar.SelectedIs(mentosItemId))
        {
            hotbar.RemoveFromSelected(1);
            Debug.Log("🥤 [콜라 인간] 멘토스 반응! 즉시 분해됨");
            Destroy(gameObject);
            yield break;
        }

        int count = 0;
        float t = 0f;
        while (t < grabDuration)
        {
            t += Time.deltaTime;
            if (Input.GetKeyDown(mashKey)) count++;

            if (count >= mashCountToEscape)
            {
                Debug.Log("🥤 [콜라 인간] 탈출 성공!");
                grabbing = false;
                yield break;
            }
            yield return null;
        }

        // 실패 → 오염/체력 처리(지금은 오염만)
        var contam = player.GetComponent<Contamination>();
        if (contam) contam.Add(contamOnFail);
        Debug.Log("🥤 [콜라 인간] 탈출 실패 → 오염 상승/피해");

        grabbing = false;
    }
}
