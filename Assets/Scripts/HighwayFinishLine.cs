using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Splines;
using Unity.Cinemachine;
using System.Collections;

public class HighwayFinishLine : MonoBehaviour
{
    [Header("Cutscene References")]
    [SerializeField] private GameObject player;           // 말
    [SerializeField] private HorseControl horseControl;   // 말 조작
    [SerializeField] private GameObject truck;            // 탈출용 트럭
    [SerializeField] private CinemachineCamera successCamera; // 연출용 카메라

    [Header("Spline Paths")]
    [SerializeField] private SplineContainer horseBoardingPath; // 말이 트럭 짐칸으로 점프하는 경로
    [SerializeField] private SplineContainer truckEscapePath;   // 트럭이 멀리 떠나는 경로

    [Header("Fade")]
    [SerializeField] private UnityEngine.UI.Image fadePanel;
    [SerializeField] private float fadeDuration = 1.5f;

    private bool hasFinished = false;

    // ★ 시작할 때 체크
    private void Start()
    {
        if (fadePanel != null) fadePanel.gameObject.SetActive(false);
        if (successCamera != null) successCamera.Priority = 0;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (hasFinished) return;

        // 플레이어가 결승선에 닿으면
        if (other.CompareTag("Player") || other.CompareTag("HorseChest"))
        {
            hasFinished = true;
            Debug.Log("🏁 결승선 통과! 탈출 연출 시작.");
            StartCoroutine(EscapeCutsceneRoutine());
        }
    }

    private IEnumerator EscapeCutsceneRoutine()
    {
        // 1. 게임 요소 정지 (타이머, 경찰차)
        StopGameplayElements();

        // 2. 조작 끄기 & 카메라 전환
        if (horseControl != null) horseControl.isControlEnabled = false;
        if (successCamera != null) successCamera.Priority = 200; // 카메라 뺏어오기

        // 3. 말 물리 끄기 (스플라인 이동 위해)
        Rigidbody rb = player.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.linearVelocity = Vector3.zero;
        }

        // 4. 말이 트럭으로 점프 (Boarding Path)
        Debug.Log("🐴 트럭 탑승 중...");
        yield return StartCoroutine(MoveAlongSpline(player.transform, horseBoardingPath, 15f)); // 속도 15

        // 5. 말 숨기기 (트럭 안에 탄 척)
        player.SetActive(false);

        // 6. 트럭 출발 (Escape Path)
        Debug.Log("🚚 트럭 출발!");
        yield return StartCoroutine(MoveAlongSpline(truck.transform, truckEscapePath, 25f)); // 속도 25

        // 7. 암전 (Fade Out)
        yield return StartCoroutine(FadeOut());

        // 8. 정산 및 저장 -> 은신처 복귀
        ProcessMissionSuccess();
    }

    // 스플라인 이동 도우미 함수
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