using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CamRotate : MonoBehaviour
{
    // 회전 속도 변수
    public float rotSpeed = 200f;

    // 회전 값 변수
    float my = 0;

    void Update()
    {
        // 마우스 입력 받기
        float mouse_Y = Input.GetAxis("Mouse Y");

        // 입력 값 누적 & 제한
        my += mouse_Y * rotSpeed * Time.deltaTime;
        my = Mathf.Clamp(my, -90f, 90f);

        // 회전에 카메라 pitch 반영
        transform.localEulerAngles = new Vector3(-my, 0f, 0f);
    }
}
