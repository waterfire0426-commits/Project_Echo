using UnityEngine;
using UnityEngine.UI; // UI 요소를 사용하기 위해 꼭 필요합니다!
using System.Collections;

[RequireComponent(typeof(Image))] // 이 스크립트는 Image 컴포넌트가 꼭 필요함
public class UIFlicker : MonoBehaviour
{
    public float minAlpha = 0.1f;  // 최소 투명도 (0.0 ~ 1.0)
    public float maxAlpha = 1.0f;  // 최대 투명도 (0.0 ~ 1.0)
    public float flickerSpeed = 0.08f; // 깜빡이는 속도 (낮을수록 빠름)

    private Image targetImage;
    private Color originalColor;

    void Start()
    {
        targetImage = GetComponent<Image>();
        originalColor = targetImage.color; // 원래 색상 저장 (흰색이 기본)
        StartCoroutine(Flicker());
    }

    IEnumerator Flicker()
    {
        while (true) // 무한 반복
        {
            // 새로운 알파(투명도) 값을 랜덤하게 설정
            float newAlpha = Random.Range(minAlpha, maxAlpha);
            
            // 이미지의 색상 변경 (R, G, B는 유지하고 A(Alpha)만 변경)
            targetImage.color = new Color(originalColor.r, originalColor.g, originalColor.b, newAlpha);

            // 지정된 속도만큼 대기
            yield return new WaitForSeconds(flickerSpeed);
        }
    }
}