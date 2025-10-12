using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ContamEffects : MonoBehaviour
{
    [Header("Refs")]
    public Contamination contamination;      // 자동 할당 가능
    public Image darkOverlay;                // Canvas Overlay의 검정 Image (알파만 조절)
    public AudioSource sfx;                  // OneShot용
    public AudioClip[] hallucinations;       // 환청 클립들

    [Header("Darkness")]
    [Range(0,1)] public float stage1Dark = 0.15f;
    [Range(0,1)] public float stage2Dark = 0.35f;
    [Range(0,1)] public float stage4Dark = 0.6f;
    public float flickerAmp = 0.15f;         // 4단계 깜빡임 강도
    public float flickerSpeed = 8f;

    [Header("Hallucination SFX")]
    public float sfxMinInterval = 6f;
    public float sfxMaxInterval = 12f;

    [Header("Spawns / Visibility")]
    public GameObject[] stage3Actors;        // 3단계부터 활성화
    public GameObject[] stage4Actors;        // 4단계부터 활성화

    int currentStage = 0;
    float sfxTimer;

    void Awake()
    {
        if (!contamination) contamination = FindObjectOfType<Contamination>();
        if (contamination) contamination.onStageChanged.AddListener(OnStageChanged);
        sfxTimer = Random.Range(sfxMinInterval, sfxMaxInterval);
    }

    void OnDestroy()
    {
        if (contamination) contamination.onStageChanged.RemoveListener(OnStageChanged);
    }

    void Update()
    {
        // 어둡기 기본값
        float baseDark = 0f;
        switch (currentStage)
        {
            case 1: baseDark = stage1Dark; break;
            case 2: baseDark = stage2Dark; break;
            case 3: baseDark = stage2Dark; break;
            case 4: baseDark = stage4Dark; break;
            default: baseDark = 0f; break;
        }

        // 4단계 깜빡임
        if (currentStage >= 4)
        {
            float flicker = (Mathf.PerlinNoise(Time.time * flickerSpeed, 0f) * 2f - 1f) * flickerAmp;
            baseDark = Mathf.Clamp01(baseDark + flicker);
        }

        if (darkOverlay) {
            var c = darkOverlay.color;
            c.a = Mathf.Lerp(c.a, baseDark, Time.deltaTime * 8f);
            darkOverlay.color = c;
        }

        // 환청
        if (currentStage >= 2 && sfx && hallucinations != null && hallucinations.Length > 0)
        {
            sfxTimer -= Time.deltaTime;
            if (sfxTimer <= 0f)
            {
                sfx.PlayOneShot(hallucinations[Random.Range(0, hallucinations.Length)]);
                sfxTimer = Random.Range(sfxMinInterval, sfxMaxInterval);
            }
        }
    }

    void OnStageChanged(int s)
    {
        currentStage = s;

        // 3단계: 고정형 괴물 활성화
        if (stage3Actors != null)
            foreach (var go in stage3Actors) if (go) go.SetActive(s >= 3);

        // 4단계: 더 많이 활성화
        if (stage4Actors != null)
            foreach (var go in stage4Actors) if (go) go.SetActive(s >= 4);

        // 5단계: 사망
        if (s >= 5) StartCoroutine(DeathRoutine());
    }

    IEnumerator DeathRoutine()
    {
        // 간단한 페이드 아웃
        float t = 0f;
        while (t < 0.8f)
        {
            t += Time.deltaTime;
            if (darkOverlay) {
                var c = darkOverlay.color; c.a = Mathf.Lerp(c.a, 1f, t/0.8f);
                darkOverlay.color = c;
            }
            yield return null;
        }
        // TODO: 실제 게임오버 처리 (씬 리로드/메뉴 호출/플레이어 체력 0 등)
        Debug.Log("[오염] 5단계 도달 → 사망");
    }
}
