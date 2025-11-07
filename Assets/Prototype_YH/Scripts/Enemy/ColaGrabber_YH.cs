using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Collider))]
public class ColaGrabber_YH : MonoBehaviour
{
    [Header("Detection")]
    public float detectRange = 10f;
    public float chaseSpeed = 3f;
    public float grabRange = 1.5f;

    [Header("QTE Settings")]
    public float grabDuration = 3f;
    public KeyCode qteKey = KeyCode.Space;
    public int qtePressCount = 5;
    public float stunAfterRelease = 2f;
    public float invincibleAfterRelease = 2f;
    public float regrabCooldown = 2f;
    public float pushForce = 1.2f; // 탈출 후 거리 (조정됨)

    [Header("References")]
    public Transform player;
    public ContamHook_YH contamHook;
    public PlayerHealth_YH playerHealth;
    public FPCamera playerCam;

    private bool isChasing = false;
    private bool isGrabbing = false;
    private bool defeated = false;
    private bool canGrab = true;
    private int pressProgress = 0;

    private PlayerMove move;
    private CharacterController cc;
    private bool cameraLocked = false;

    void Start()
    {
        if (!player)
            player = GameObject.FindGameObjectWithTag("Player")?.transform;

        if (player)
        {
            move = player.GetComponent<PlayerMove>();
            cc = player.GetComponent<CharacterController>();
            contamHook = player.GetComponent<ContamHook_YH>();
            playerHealth = player.GetComponent<PlayerHealth_YH>();
            playerCam = player.GetComponentInChildren<FPCamera>();
        }
    }

    void Update()
    {
        if (defeated || !player) return;

        Vector3 playerCenter = player.position + Vector3.up * 1.0f;
        float dist = Vector3.Distance(transform.position, playerCenter);

        // 추적 시작
        if (!isChasing && dist <= detectRange)
        {
            isChasing = true;
        }

        // 추적 중 (정면 유지)
        if (isChasing && !isGrabbing)
        {
            Vector3 targetPos = playerCenter;
            Vector3 dir = (targetPos - transform.position).normalized;

            Quaternion targetRot = Quaternion.LookRotation(new Vector3(dir.x, 0f, dir.z));
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * 5f);
            transform.position += transform.forward * chaseSpeed * Time.deltaTime;
        }

        // 붙잡기 조건
        if (isChasing && canGrab && dist <= grabRange && !isGrabbing)
        {
            Vector3 lookDir = (playerCenter - transform.position).normalized;
            lookDir.y = 0f;
            transform.rotation = Quaternion.LookRotation(lookDir);
            StartCoroutine(GrabSequence());
        }
    }

    IEnumerator GrabSequence()
    {
        isGrabbing = true;
        canGrab = false;
        float originalSpeed = chaseSpeed;
        chaseSpeed = 0f;

        pressProgress = 0;
        float timer = grabDuration;

        if (move) move.enabled = false;

        if (playerCam)
        {
            playerCam.enabled = false;
            Vector3 targetPoint = transform.position + Vector3.up * 0.8f;
            playerCam.transform.rotation = Quaternion.LookRotation(targetPoint - playerCam.transform.position);
            cameraLocked = true;
        }

        bool escaped = false;
        while (timer > 0f)
        {
            if (Input.GetKeyDown(qteKey))
            {
                pressProgress++;
                if (pressProgress >= qtePressCount)
                {
                    escaped = true;
                    break;
                }
            }

            timer -= Time.deltaTime;
            yield return null;
        }

        if (escaped)
        {
            yield return StartCoroutine(HandleEscapeSuccess());
        }
        else
        {
            contamHook?.AddTemp(10f);
            playerHealth?.TakeDamage(10f);
            yield return StartCoroutine(HandleEscapeFailure());
        }

        yield return new WaitForSeconds(regrabCooldown);

        chaseSpeed = originalSpeed;
        canGrab = true;
    }

    IEnumerator HandleEscapeSuccess()
    {
        contamHook?.SetInvincible(invincibleAfterRelease);

        if (move) move.enabled = true;
        if (playerCam && cameraLocked)
        {
            playerCam.enabled = true;
            cameraLocked = false;
        }

        if (cc)
        {
            Vector3 pushDir = (player.position - transform.position).normalized;
            cc.Move(pushDir * pushForce);
        }

        StartCoroutine(StunRoutine(stunAfterRelease));
        isGrabbing = false;
        yield break;
    }

    IEnumerator HandleEscapeFailure()
    {
        contamHook?.SetInvincible(invincibleAfterRelease);

        if (move) move.enabled = true;
        if (playerCam && cameraLocked)
        {
            playerCam.enabled = true;
            cameraLocked = false;
        }

        if (cc)
        {
            Vector3 pushDir = (player.position - transform.position).normalized;
            cc.Move(pushDir * pushForce * 0.5f);
        }

        StartCoroutine(StunRoutine(stunAfterRelease));
        isGrabbing = false;
        yield break;
    }

    IEnumerator StunRoutine(float time)
    {
        float oldSpeed = chaseSpeed;
        chaseSpeed = 0f;
        yield return new WaitForSeconds(time);
        chaseSpeed = oldSpeed;
    }

    public void OnMentosUsed()
    {
        if (defeated) return;
        defeated = true;
        Destroy(gameObject, 0.5f);
    }

    void OnCollisionEnter(Collision col)
    {
        if (col.gameObject.CompareTag("Mentos"))
        {
            Destroy(gameObject);
            Destroy(col.gameObject);
        }
    }
}
