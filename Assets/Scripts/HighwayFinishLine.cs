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

    [Header("Speed Settings")]
    [SerializeField] private float horseBoardingSpeed = 5f;
    [SerializeField] private float truckEscapeSpeed = 8f;

    [Header("Truck Door Settings")]
    [SerializeField] private Transform leftDoor;
    [SerializeField] private Transform rightDoor;
    [SerializeField] private float doorCloseDuration = 1.0f;
    [SerializeField] private float doorCloseAngle = 120f;

    [Header("Fade")]
    [SerializeField] private UnityEngine.UI.Image fadePanel;
    [SerializeField] private float fadeDuration = 1.5f;

    private bool hasFinished = false;

    private void Start()
    {
        if (fadePanel != null) fadePanel.gameObject.SetActive(false);
        if (successCamera != null) successCamera.Priority = 0;
    }

    // ★ [수정됨] 다시 강력해진 감지 로직!
    private void OnTriggerEnter(Collider other)
    {
        if (hasFinished) return;

        // 1. 가장 확실한 방법: Rigidbody(몸통)를 찾아서 태그 확인
        if (other.attachedRigidbody != null)
        {
            if (other.attachedRigidbody.CompareTag("HorseChest") || other.attachedRigidbody.CompareTag("Player"))
            {
                FinishLevel(); // 성공 처리!
                return;
            }
        }

        // 2. 그냥 닿은 녀석의 태그 확인
        if (other.CompareTag("HorseChest") || other.CompareTag("Player"))
        {
            FinishLevel();
            return;
        }

        // 3. 최후의 수단: 이름으로 확인
        if (other.name.ToLower().Contains("horse") || other.name.ToLower().Contains("player"))
        {
            FinishLevel();
            return;
        }
    }

    // 성공 처리 분리 (코드 중복 방지)
    private void FinishLevel()
    {
        hasFinished = true;
        Debug.Log("🏁 [도착] 결승선 인식 성공! 엔딩 컷씬 시작!");
        StartCoroutine(EscapeCutsceneRoutine());
    }

    private IEnumerator EscapeCutsceneRoutine()
    {
        StopGameplayElements();

        if (horseControl != null) horseControl.isControlEnabled = false;
        if (successCamera != null) successCamera.Priority = 200;

        Rigidbody rb = player.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.linearVelocity = Vector3.zero;
        }

        // 1. 말이 트럭 탑승
        Debug.Log("🐴 트럭 탑승 중...");
        yield return StartCoroutine(MoveAlongSpline(player.transform, horseBoardingPath, horseBoardingSpeed, true));

        // 2. 말 숨기기
        player.SetActive(false);

        // 3. 문 닫기 연출
        Debug.Log("🚪 문 닫는 중...");
        yield return StartCoroutine(CloseDoorsRoutine());

        // 4. 트럭 출발
        Debug.Log("🚚 트럭 출발!");
        yield return StartCoroutine(MoveAlongSpline(truck.transform, truckEscapePath, truckEscapeSpeed));

        // 5. 암전
        yield return StartCoroutine(FadeOut());

        ProcessMissionSuccess();
    }

    private IEnumerator CloseDoorsRoutine()
    {
        if (leftDoor == null || rightDoor == null)
        {
            // 문 연결 안 했으면 그냥 패스 (에러 방지)
            yield break;
        }

        float t = 0f;
        Quaternion startRotL = leftDoor.localRotation;
        Quaternion startRotR = rightDoor.localRotation;

        // 목표 회전값 계산
        Quaternion endRotL = startRotL * Quaternion.Euler(0, doorCloseAngle, 0);
        Quaternion endRotR = startRotR * Quaternion.Euler(0, -doorCloseAngle, 0);

        while (t < 1f)
        {
            t += Time.deltaTime / doorCloseDuration;
            leftDoor.localRotation = Quaternion.Slerp(startRotL, endRotL, t);
            rightDoor.localRotation = Quaternion.Slerp(startRotR, endRotR, t);
            yield return null;
        }

        leftDoor.localRotation = endRotL;
        rightDoor.localRotation = endRotR;
        yield return new WaitForSeconds(0.5f);
    }

    private IEnumerator MoveAlongSpline(Transform target, SplineContainer path, float speed, bool isReverse = false)
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
            {
                Vector3 lookDir = isReverse ? -dir : dir;
                target.rotation = Quaternion.Slerp(target.rotation, Quaternion.LookRotation(lookDir), Time.deltaTime * 10f);
            }

            yield return null;
        }
    }

    private void StopGameplayElements()
    {
        var tm = FindObjectOfType<MuseumTimeManager>();
        if (tm != null) tm.gameObject.SetActive(false);

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
            int reward = GameManager.Instance.currentMissionTarget.price;
            GameManager.Instance.AddMoney(reward);

            int tIndex = GameManager.Instance.currentTargetIndex;
            GameManager.Instance.data.collectedArts[tIndex] = true;

            int sIndex = GameManager.Instance.currentTargetStageIndex;
            // 17개 패치 안전장치 적용
            int max = (GameManager.Instance.stageArtCounts != null && sIndex < GameManager.Instance.stageArtCounts.Length)
                      ? GameManager.Instance.stageArtCounts[sIndex] : 5;

            if (GameManager.Instance.data.GetStolenCount(sIndex) >= max)
            {
                GameManager.Instance.data.unlockedStageIndex = Mathf.Max(GameManager.Instance.data.unlockedStageIndex, sIndex + 1);
            }

            GameManager.Instance.SaveGameData();
            SceneManager.LoadScene($"Hideout_Lv{GameManager.Instance.data.currentHideoutLevel}");
        }
    }
}