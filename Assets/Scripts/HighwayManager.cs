using UnityEngine;
using UnityEngine.UI;
using Unity.Cinemachine; // ★ 시네머신 네임스페이스 필수
using System.Collections;

public class HighwayManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image fadePanel;
    [SerializeField] private float fadeDuration = 1.0f;

    [Header("Game References")]
    [SerializeField] private Transform player;
    [SerializeField] private Transform highwaySpawnPoint;
    [SerializeField] private MuseumTimeManager timeManager;
    [SerializeField] private GameObject[] policeCars;

    [Header("Cinematic")]
    [SerializeField] private CinemachineCamera policeCamera; // ★ [추가] 경찰차 비추는 카메라

    [Header("Cleanup")]
    [SerializeField] private GameObject arrivalTruck;

    private bool isEscaping = false;

    void Start()
    {
        // 시작 시 페이드 패널 끄기
        if (fadePanel != null)
        {
            fadePanel.color = new Color(0, 0, 0, 0);
            fadePanel.gameObject.SetActive(false);
        }

        // 경찰 카메라는 꺼두고 시작
        if (policeCamera != null) policeCamera.Priority = 0;
    }

    public void StartEscapeSequence()
    {
        if (isEscaping) return;
        isEscaping = true;

        float score = 0f;
        MuseumPainter painter = FindObjectOfType<MuseumPainter>();
        if (painter != null) score = painter.FinalAccuracy;

        StartCoroutine(EscapeRoutine(score));
    }

    IEnumerator EscapeRoutine(float score)
    {
        Debug.Log("🎬 [연출] 탈출 시퀀스 시작!");

        if (arrivalTruck != null) arrivalTruck.SetActive(false);

        // 1. 페이드 아웃 (암전)
        if (fadePanel != null)
        {
            fadePanel.gameObject.SetActive(true);
            float timer = 0f;
            while (timer < fadeDuration)
            {
                timer += Time.deltaTime;
                float alpha = Mathf.Clamp01(timer / fadeDuration);
                fadePanel.color = new Color(0, 0, 0, alpha);
                yield return null;
            }
            fadePanel.color = Color.black;
        }

        yield return new WaitForSeconds(0.5f);

        // 2. 플레이어 이동 & 회전 (180도)
        if (player != null && highwaySpawnPoint != null)
        {
            player.transform.position = highwaySpawnPoint.position;
            player.transform.rotation = highwaySpawnPoint.rotation * Quaternion.Euler(0, 180f, 0);
        }

        // 3. ★ [핵심] 암전 중에 '경찰 카메라'로 전환!
        if (policeCamera != null)
        {
            policeCamera.Priority = 100; // 메인 카메라보다 높게 설정해서 화면 뺏기
            Debug.Log("📹 카메라 전환: 경찰차 시점");
        }

        // 시간 보너스 지급
        float bonus = (score > 50f) ? 60f : 30f;
        if (timeManager != null) timeManager.AddBonusTime(bonus);

        yield return new WaitForSeconds(0.5f); // 잠깐 대기

        // 4. 페이드 인 (화면 밝아짐 -> 경찰차가 보임!)
        if (fadePanel != null)
        {
            Debug.Log("☀️ 페이드 인 (경찰차 등장)");
            float timer = 0f;
            while (timer < fadeDuration)
            {
                timer += Time.deltaTime;
                float alpha = 1f - Mathf.Clamp01(timer / fadeDuration);
                fadePanel.color = new Color(0, 0, 0, alpha);
                yield return null;
            }
            fadePanel.gameObject.SetActive(false);
        }

        // 5. ★ 경찰차 출발!
        if (policeCars != null)
        {
            foreach (var car in policeCars)
            {
                if (car != null) car.SetActive(true);
            }
            Debug.Log("🚨 경찰차 출발!");
        }

        // 6. ★ 1초 동안 경찰차 보여주기
        yield return new WaitForSeconds(1.0f);

        // 7. ★ 다시 내 카메라(플레이어)로 복귀
        if (policeCamera != null)
        {
            policeCamera.Priority = 0; // 우선순위 낮춰서 메인 카메라에게 넘겨주기
            Debug.Log("📹 카메라 복귀: 플레이어 시점");
        }

        Debug.Log("🏁 [추격전 시작] 달려라!");
    }
}