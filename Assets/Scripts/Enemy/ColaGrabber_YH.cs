using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Collider))]
public class ColaGrabber_YH : MonoBehaviour
{
    [Header("Detection")]
    public float detectRange = 10f;
    public float chaseSpeed = 3f;
    public float grabRange = 1.5f;

    [Header("QTE")]
    public float grabDuration = 3f;
    public KeyCode qteKey = KeyCode.Space;
    public int qtePressCount = 5;

    [Header("References")]
    public Transform player;
    public ContamHook_YH contamHook;
    public PlayerHealth_YH playerHealth;

    private bool isChasing = false;
    private bool isGrabbing = false;
    private bool defeated = false;
    private int pressProgress = 0;

    void Start()
    {
        if (!player)
            player = GameObject.FindGameObjectWithTag("Player")?.transform;
        if (player)
        {
            contamHook = player.GetComponent<ContamHook_YH>();
            playerHealth = player.GetComponent<PlayerHealth_YH>();
        }
    }

    void Update()
    {
        if (defeated || !player) return;

        float dist = Vector3.Distance(transform.position, player.position);

        // 감지 → 추적 시작
        if (!isChasing && dist <= detectRange)
        {
            isChasing = true;
            Debug.Log("[Cola] 추적 시작");
        }

        // 추적 로직
        if (isChasing && !isGrabbing)
        {
            transform.LookAt(player.position);
            transform.position = Vector3.MoveTowards(transform.position, player.position, chaseSpeed * Time.deltaTime);
        }

        // 붙잡기 조건
        if (isChasing && dist <= grabRange && !isGrabbing)
        {
            StartCoroutine(GrabSequence());
        }
    }

    IEnumerator GrabSequence()
    {
        isGrabbing = true;
        Debug.Log("[Cola] 붙잡음! QTE 시작 (스페이스 연타)");

        pressProgress = 0;
        float timer = grabDuration;

        while (timer > 0f)
        {
            if (Input.GetKeyDown(qteKey))
            {
                pressProgress++;
                Debug.Log($"[Cola] QTE 진행 {pressProgress}/{qtePressCount}");
                if (pressProgress >= qtePressCount)
                {
                    Debug.Log("[Cola] 탈출 성공!");
                    isGrabbing = false;
                    yield break;
                }
            }

            timer -= Time.deltaTime;
            yield return null;
        }

        // 실패 시
        Debug.Log("[Cola] 탈출 실패 → 오염+, 체력-");
        contamHook?.AddTemp(10f);
        playerHealth?.TakeDamage(10f);
        isGrabbing = false;
    }

    public void OnMentosUsed()
    {
        if (defeated) return;
        defeated = true;
        Debug.Log("[Cola] 멘토스 반응 → 퇴치!");
        Destroy(gameObject, 0.5f);
    }

    public void OnHitByMentos(float dmg)
    {
        Debug.Log($"[ColaGrabber] 멘토스에 의해 피해! {dmg} 데미지");
        Destroy(gameObject); // 임시로 즉시 제거 (이후 애니메이션/이펙트로 교체 가능)
    }

    void OnCollisionEnter(Collision col)
    {
        if (col.gameObject.CompareTag("Mentos"))
        {
            Debug.Log("[Cola] 멘토스에 맞음 → 퇴치!");
            Destroy(gameObject); // 콜라 괴물 제거
            Destroy(col.gameObject); // 멘토스도 제거
        }
    }


}
