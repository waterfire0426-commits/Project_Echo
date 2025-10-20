// File: Assets/Scripts/Enemy/ColaGrabberAI_YH.cs
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class ColaGrabberAI_YH : MonoBehaviour
{
    private NavMeshAgent agent;
    private Transform player;
    public float detectRange = 10f;
    public float chaseRange = 15f;
    public float attackDistance = 1.5f;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        player = GameObject.FindWithTag("Player")?.transform;
    }

    void Update()
    {
        if (!player) return;

        float dist = Vector3.Distance(transform.position, player.position);

        if (dist <= detectRange)
        {
            agent.isStopped = false;
            agent.SetDestination(player.position);
        }
        else if (dist > chaseRange)
        {
            agent.isStopped = true;
        }

        if (dist <= attackDistance)
        {
            Debug.Log("[ColaGrabber] 공격 시도!");
            // TODO: 공격 처리 or QTE 트리거
        }
    }
}
