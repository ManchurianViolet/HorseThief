using UnityEngine;
using UnityEngine.UI; // 페이드 효과(Image) 제어용
using System.Collections;

public class HighwayManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image fadePanel; // 검은색 막 (Canvas -> Image)
    [SerializeField] private float fadeDuration = 1.0f; // 암전되는 시간

    [Header("Game References")]
    [SerializeField] private Transform player;        // 말 (Horse)
    [SerializeField] private Transform highwaySpawnPoint; // 고속도로 시작 위치
    [SerializeField] private MuseumTimeManager timeManager; // 시간 관리자
    [SerializeField] private GameObject[] policeCars; // 경찰차들 (5대)

    void Start()
    {
        // 시작할 땐 검은 막을 숨겨둠
        if (fadePanel != null)
        {
            fadePanel.gameObject.SetActive(false);
            fadePanel.color = new Color(0, 0, 0, 0); // 투명하게
        }
    }

    // InteractiveArt에서 이 함수를 호출할 겁니다!
    public void StartEscapeSequence()
    {
        // 1. 매니저가 직접 점수판(Painter)을 찾아서 점수 확인
        float score = 0f;
        MuseumPainter painter = FindObjectOfType<MuseumPainter>();

        if (painter != null)
        {
            score = painter.FinalAccuracy; // 아까 만든 그 변수에서 가져옴
            Debug.Log($"👮‍♂️ [HighwayManager] 점수 확인 완료: {score:F1}점");
        }
        else
        {
            Debug.LogError("🚨 MuseumPainter를 찾을 수 없습니다! 0점으로 진행.");
        }
        StartCoroutine(EscapeRoutine(score));
    }

    IEnumerator EscapeRoutine(float score)
    {
        Debug.Log("🎬 [연출] 탈출 시퀀스 시작!");

        // 1. 페이드 아웃 (점점 어두워짐)
        if (fadePanel != null)
        {
            fadePanel.gameObject.SetActive(true);
            float t = 0;
            while (t < 1)
            {
                t += Time.deltaTime / fadeDuration;
                fadePanel.color = new Color(0, 0, 0, t);
                yield return null;
            }
        }

        yield return new WaitForSeconds(0.5f); // 완전 깜깜해진 상태로 잠시 대기

        // 2. 텔레포트 (말 이동)
        if (player != null && highwaySpawnPoint != null)
        {
            // 물리 충돌 방지를 위해 잠시 끄거나 위치 강제 이동
            player.transform.position = highwaySpawnPoint.position;

            // 말의 회전도 도로 방향(앞)을 보게 맞춤
            player.transform.rotation = highwaySpawnPoint.rotation;

            Debug.Log("📍 [이동] 말이 고속도로로 이동했습니다.");
        }

        // 3. 시간 보너스 지급
        // 50점 넘으면 60초, 아니면 30초
        float bonus = (score > 50f) ? 60f : 30f;
        if (timeManager != null) timeManager.AddBonusTime(bonus);

        // 4. 경찰차 출발 (활성화)
        if (policeCars != null)
        {
            foreach (var car in policeCars)
            {
                if (car != null) car.SetActive(true);
            }
        }

        // 5. 페이드 인 (점점 밝아짐)
        if (fadePanel != null)
        {
            float t = 1;
            while (t > 0)
            {
                t -= Time.deltaTime / fadeDuration;
                fadePanel.color = new Color(0, 0, 0, t);
                yield return null;
            }
            fadePanel.gameObject.SetActive(false); // 다 밝아지면 끔
        }

        Debug.Log("🏁 [출발] 고속도로 추격전 시작!");
    }
}