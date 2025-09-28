using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using System.Collections;

public class ContaminationManager : MonoBehaviour
{
    [Header("UI")]
    public Slider contaminationSlider; // 오염 게이지
    [Range(0, 100)]
    public float contaminationValue = 0f; // 0~100

    [Header("Post-processing")]
    public Volume postProcessVolume;   // Global Volume
    private Vignette vignette;
    private FilmGrain filmGrain;

    private int previousLevel = -1; // 이전 단계 기억

    [Header("Flash Settings")]
    public float flashDuration = 0.2f; // 깜빡임 지속 시간
    public float flashIntensity = 0.3f; // 깜빡임 강도 (Vignette)

    // PlayerHealth 참조
    public PlayerHealth playerHealth;

    void Start()
    {
        // Slider 초기값
        contaminationSlider.minValue = 0;
        contaminationSlider.maxValue = 100;
        contaminationSlider.value = contaminationValue;

        // Post Processing 효과 가져오기
        postProcessVolume.profile.TryGet(out vignette);
        postProcessVolume.profile.TryGet(out filmGrain);

        UpdateUI();
    }

    void Update()
    {
        // 테스트용 키 입력
        if (Input.GetKeyDown(KeyCode.O)) IncreaseContamination(20f); // +1단계
        if (Input.GetKeyDown(KeyCode.P)) DecreaseContamination(20f); // -1단계
    }

    int GetLevelFromValue(float value)
    {
        if (value == 0) return 0;
        else if (value <= 20) return 1;
        else if (value <= 40) return 2;
        else if (value <= 60) return 3;
        else if (value <= 80) return 4;
        else return 5;
    }

    public void IncreaseContamination(float amount)
    {
        contaminationValue = Mathf.Clamp(contaminationValue + amount, 0f, 100f);
        UpdateUI();
    }

    public void DecreaseContamination(float amount)
    {
        contaminationValue = Mathf.Clamp(contaminationValue - amount, 0f, 100f);
        UpdateUI();
    }

    void UpdateUI()
    {
        contaminationSlider.value = contaminationValue;

        int level = GetLevelFromValue(contaminationValue);

        // 단계가 변경될 때만 효과 적용
        if (level != previousLevel)
        {
            ApplyEffects(level);
            previousLevel = level;
        }

        // 테스트용 로그
        Debug.Log("Current Level: " + level);

        // 오염 5단계 → PlayerHealth의 GameOver 호출
        if (level == 5 && playerHealth != null)
        {
            playerHealth.ForceGameOver();
        }
    }

    void ApplyEffects(int level)
    {
        switch (level)
        {
            case 0:
                vignette.intensity.value = 0f;
                filmGrain.intensity.value = 0f;
                break;
            case 1:
                vignette.intensity.value = 0.2f;
                filmGrain.intensity.value = 0.1f;
                StartCoroutine(FlashVignette());
                break;
            case 2:
                vignette.intensity.value = 0.3f;
                filmGrain.intensity.value = 0.2f;
                StartCoroutine(FlashVignette());
                break;
            case 3:
                vignette.intensity.value = 0.45f;
                filmGrain.intensity.value = 0.3f;
                StartCoroutine(FlashVignette());
                break;
            case 4:
                vignette.intensity.value = 0.6f;
                filmGrain.intensity.value = 0.5f;
                StartCoroutine(FlashVignette());
                break;
            case 5:
                vignette.intensity.value = 0.8f;
                filmGrain.intensity.value = 0.7f;
                // 사망 처리 여기에 넣으면 됨
                break;
        }
    }

    IEnumerator FlashVignette()
    {
        float original = vignette.intensity.value;
        vignette.intensity.value += flashIntensity; // 깜빡임
        yield return new WaitForSeconds(flashDuration);
        vignette.intensity.value = original; // 원래대로
    }
}
