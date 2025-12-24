using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Splines;
using Unity.Cinemachine;
using System.Collections;

public class HighwayFinishLine : MonoBehaviour
{
    [Header("Cutscene References")]
    [SerializeField] private GameObject player;
    [SerializeField] private HorseControl horseControl;
    [SerializeField] private GameObject truck;
    [SerializeField] private CinemachineCamera successCamera;

    [Header("Spline Paths")]
    [SerializeField] private SplineContainer horseBoardingPath;
    [SerializeField] private SplineContainer truckEscapePath;

    [Header("Fade")]
    [SerializeField] private UnityEngine.UI.Image fadePanel;
    [SerializeField] private float fadeDuration = 1.5f;

    private bool hasFinished = false;

    // ★ 시작할 때 체크
    private void Start()
    {
        if (fadePanel != null) fadePanel.gameObject.SetActive(false);
        if (successCamera != null) successCamera.Priority = 0;

        // ★ [디버깅] Collider 확인
        Collider col = GetComponent<Collider>();
        if (col == null)
        {
            Debug.LogError("❌ FinishLine에 Collider가 없습니다!");
        }
        else if (!col.isTrigger)
        {
            Debug.LogError("❌ FinishLine Collider의 Is Trigger가 꺼져있습니다!");
        }
        else
        {
            Debug.Log("✅ FinishLine 설정 확인 완료");
        }
    }

    // ★ [핵심 수정] 디버깅 + Rigidbody 체크 추가
    private void OnTriggerEnter(Collider other)
    {
        // 일단 뭐가 닿는지 로그 출력
        Debug.Log($"🏁 [FinishLine] 뭔가 닿았음! 이름: {other.name}, 태그: {other.tag}");

        // 중복 실행 방지
        if (hasFinished)
        {
            Debug.Log("⚠️ 이미 완료됨 (중복 실행 방지)");
            return;
        }

        // ★ [핵심] Rigidbody 확인 (말은 Rigidbody가 달려있음)
        Rigidbody rb = other.attachedRigidbody;
        if (rb != null)
        {
            Debug.Log($"🔍 Rigidbody 발견! 오브젝트: {rb.name}, 태그: {rb.tag}");

            if (rb.CompareTag("HorseChest") || rb.CompareTag("Player"))
            {
                hasFinished = true;
                Debug.Log("🏁 [도착!] 결승선 통과! 탈출 연출 시작!");
                StartCoroutine(EscapeCutsceneRoutine());
                return;
            }
        }

        // 직접 태그 확인 (혹시 모를 경우 대비)
        if (other.CompareTag("HorseChest") || other.CompareTag("Player"))
        {
            hasFinished = true;
            Debug.Log("🏁 [도착!] 결승선 통과! 탈출 연출 시작!");
            StartCoroutine(EscapeCutsceneRoutine());
            return;
        }

        // ★ [추가] 이름으로도 체크 (최후의 수단)
        string otherName = other.name.ToLower();
        if (otherName.Contains("horse") || otherName.Contains("player"))
        {
            hasFinished = true;
            Debug.Log("🏁 [이름으로 감지] 결승선 통과!");
            StartCoroutine(EscapeCutsceneRoutine());
            return;
        }

        Debug.LogWarning($"⚠️ 태그가 안 맞음! 현재 태그: {other.tag}");
    }

    private IEnumerator EscapeCutsceneRoutine()
    {
        // 1. 게임 요소 정지
        StopGameplayElements();

        // 2. 조작 끄기 & 카메라 전환
        if (horseControl != null) horseControl.isControlEnabled = false;
        if (successCamera != null) successCamera.Priority = 200;

        // 3. 말 물리 끄기
        Rigidbody rb = player.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.linearVelocity = Vector3.zero;
        }

        // 4. 말이 트럭으로 점프
        Debug.Log("🐴 트럭 탑승 중...");
        yield return StartCoroutine(MoveAlongSpline(player.transform, horseBoardingPath, 15f));

        // 5. 말 숨기기
        player.SetActive(false);

        // 6. 트럭 출발
        Debug.Log("🚚 트럭 출발!");
        yield return StartCoroutine(MoveAlongSpline(truck.transform, truckEscapePath, 25f));

        // 7. 암전
        yield return StartCoroutine(FadeOut());

        // 8. 정산 및 저장 -> 은신처 복귀
        ProcessMissionSuccess();
    }

    private IEnumerator MoveAlongSpline(Transform target, SplineContainer path, float speed)
    {
        if (path == null) yield break;

        float len = path.CalculateLength();
        float dist = 0f;

        while (dist < len)
        {
            dist += speed * Time.deltaTime;
            float t = Mathf.Clamp01(dist / len);

            target.position = path.EvaluatePosition(t);
            Vector3 dir = path.EvaluateTangent(t);

            if (dir != Vector3.zero)
                target.rotation = Quaternion.Slerp(target.rotation, Quaternion.LookRotation(dir), Time.deltaTime * 10f);

            yield return null;
        }
    }

    private void StopGameplayElements()
    {
        // 타이머 끄기
        var tm = FindObjectOfType<MuseumTimeManager>();
        if (tm != null) tm.gameObject.SetActive(false);

        // 경찰차 모두 끄기
        var police = FindObjectsOfType<PoliceCarSpline>();
        foreach (var car in police) car.gameObject.SetActive(false);
    }

    private IEnumerator FadeOut()
    {
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
            fadePanel.color = Color.black;
        }
    }

    private void ProcessMissionSuccess()
    {
        if (GameManager.Instance != null && GameManager.Instance.currentMissionTarget != null)
        {
            // 돈 지급
            int reward = GameManager.Instance.currentMissionTarget.price;
            GameManager.Instance.AddMoney(reward);

            // 도감 채우기
            int tIndex = GameManager.Instance.currentTargetIndex;
            GameManager.Instance.data.collectedArts[tIndex] = true;

            // 스테이지 해금 로직
            int sIndex = GameManager.Instance.currentTargetStageIndex;
            int max = (sIndex == 5) ? 1 : 5;
            if (GameManager.Instance.data.GetStolenCount(sIndex) >= max && sIndex < 5)
            {
                GameManager.Instance.data.unlockedStageIndex = sIndex + 1;
            }

            GameManager.Instance.SaveGameData();

            // 은신처로 복귀
            SceneManager.LoadScene($"Hideout_Lv{GameManager.Instance.data.currentHideoutLevel}");
        }
    }
}