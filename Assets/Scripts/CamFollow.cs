using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CamFollow : MonoBehaviour
{
    // 목표
    public Transform target;

    // 미세 떨림 감소
    void LateUpdate()
    {
        // 카메라 위치 = 목표 트랜스폼 위치
        transform.position = target.position;
    }
}
