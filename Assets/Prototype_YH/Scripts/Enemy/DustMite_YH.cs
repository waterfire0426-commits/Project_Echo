using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Collider))]
public class DustMite_YH : MonoBehaviour
{
    [Header("이동 설정")]
    public float wanderRadius = 4f;
    public float speed = 2f;

    [Header("오염 설정")]
    public float contamOnTouch = 2f;
    public float touchCooldown = 1.5f;

    [Header("UV 반응 설정")]
    public float uvExposureTime = 0.5f; // UV 노출 지속 시간 후 제거
    public float uvCheckRadius = 3f;    // UV 탐지 반경
    public ParticleSystem vanishEffect; // 제거 이펙트 (선택)

    private float nextTouchTime;
    private Vector3 target;
    private bool isDying = false;
    private float uvTimer = 0f;

    private Transform player;
    private FlashlightController playerFlash; // 이름 수정

    void Start()
    {
        GetComponent<Collider>().isTrigger = true;
        PickNewTarget();

        var playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj)
        {
            player = playerObj.transform;
            playerFlash = playerObj.GetComponentInChildren<FlashlightController>(true); // ✅ 클래스명 변경
        }
    }

    void Update()
    {
        if (isDying) return;

        // 배회 로직
        Vector3 dir = (target - transform.position);
        dir.y = 0;
        if (dir.magnitude < 0.2f) PickNewTarget();
        else transform.position += dir.normalized * speed * Time.deltaTime;

        // UV 감지 (라이트 ON + 거리 내)
        if (playerFlash && playerFlash.IsUVOn) // 속성명 변경
        {
            float dist = Vector3.Distance(transform.position, playerFlash.transform.position);
            if (dist <= uvCheckRadius)
            {
                uvTimer += Time.deltaTime;
                if (uvTimer >= uvExposureTime)
                {
                    StartCoroutine(Vanish());
                }
            }
            else
            {
                uvTimer = 0f; // 범위 벗어나면 초기화
            }
        }
        else
        {
            uvTimer = 0f;
        }
    }

    void PickNewTarget()
    {
        Vector2 r = Random.insideUnitCircle * wanderRadius;
        target = new Vector3(transform.position.x + r.x, transform.position.y, transform.position.z + r.y);
    }

    void OnTriggerStay(Collider other)
    {
        if (!other.CompareTag("Player") || isDying) return;

        // UV 켜져 있을 경우 즉시 제거
        if (playerFlash && playerFlash.IsUVOn)
        {
            float dist = Vector3.Distance(transform.position, playerFlash.transform.position);
            if (dist <= uvCheckRadius)
            {
                StartCoroutine(Vanish());
                return;
            }
        }

        // 접촉으로 오염 증가
        if (Time.time < nextTouchTime) return;
        nextTouchTime = Time.time + touchCooldown;

        var contam = other.GetComponent<Contamination>();
        if (contam) contam.Add(contamOnTouch);
        Debug.Log("🦠 [먼지진드기] 접촉 → 오염도 상승");
    }

    IEnumerator Vanish()
    {
        if (isDying) yield break;
        isDying = true;

        Debug.Log("🦠 [먼지진드기] UV에 의해 제거됨");

        if (vanishEffect)
        {
            vanishEffect.Play();
            yield return new WaitForSeconds(0.3f);
        }

        Destroy(gameObject);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, uvCheckRadius);
    }
}
