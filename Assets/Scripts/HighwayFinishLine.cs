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
    private Transform frontShinL, frontShinR, footL, footR;

    private void Start()
    {
        // 페이드 패널이 있다면 켜고 투명하게 시작 (혹은 꺼두기)
        if (fadePanel != null)
        {
            fadePanel.gameObject.SetActive(true);
            fadePanel.color = new Color(0, 0, 0, 0); // 투명
        }
        if (successCamera != null) successCamera.Priority = 0;

        if (player != null)
        {
            frontShinL = player.transform.Find("horse.001/Root/spine.005/spine.006/spine.007/front_shoulder.L/front_thigh.L/front_shin.L");
            frontShinR = player.transform.Find("horse.001/Root/spine.005/spine.006/spine.007/front_shoulder.R/front_thigh.R/front_shin.R");
            footL = player.transform.Find("horse.001/Root/spine.005/shoulder.L/thigh.L/shin.L/foot.L");
            footR = player.transform.Find("horse.001/Root/spine.005/shoulder.R/thigh.R/shin.R/foot.R");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (hasFinished) return;

        if (other.attachedRigidbody != null)
        {
            if (other.attachedRigidbody.CompareTag("HorseChest") || other.attachedRigidbody.CompareTag("Player"))
            {
                FinishLevel();
                return;
            }
        }
        if (other.CompareTag("HorseChest") || other.CompareTag("Player"))
        {
            FinishLevel();
            return;
        }
        if (other.name.ToLower().Contains("horse") || other.name.ToLower().Contains("player"))
        {
            FinishLevel();
            return;
        }
    }

    private void FinishLevel()
    {
        hasFinished = true;
        Debug.Log("🏁 [도착] 엔딩 시퀀스 시작!");
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

        // ★ [수정 1] 순간이동 감추기용 "깜빡(Blink)" 효과
        // 0.3초 동안 빠르게 암전
        yield return StartCoroutine(DoFade(0f, 1f, 0.5f));

        // (화면이 깜깜할 때) 위치 초기화 같은 게 필요하면 여기서 살짝 대기해도 됨
        // 말 위치를 스플라인 시작점으로 미리 보낸다거나... (MoveAlongSpline이 알아서 하긴 함)

        // 0.3초 동안 다시 밝아짐 (이제 말이 스플라인 위에 있는 것처럼 보임)
        yield return StartCoroutine(DoFade(1f, 0f, 0.5f));


        // 1. 말이 스플라인 타고 트럭 탑승 (풍차 돌리기)
        Debug.Log("🐴 트럭 탑승 중...");
        yield return StartCoroutine(MoveAlongSpline(player.transform, horseBoardingPath, horseBoardingSpeed, true));

        // ★ [수정 2] 말이 바로 사라지지 않음! 문부터 닫음.
        Debug.Log("🚪 문 닫는 중...");
        yield return StartCoroutine(CloseDoorsRoutine());

        // 문이 다 닫혔으니 이제 안전하게 삭제 (유저 눈에는 문 뒤에 있는 것처럼 보임)
        player.SetActive(false);


        // 2. 트럭 출발
        Debug.Log("🚚 트럭 출발!");
        yield return StartCoroutine(MoveAlongSpline(truck.transform, truckEscapePath, truckEscapeSpeed));

        // 3. 최종 암전 (천천히)
        yield return StartCoroutine(DoFade(0f, 1f, fadeDuration));

        ProcessMissionSuccess();
    }

    // ★ [범용] 페이드 효과 함수 (Start Alpha -> End Alpha)
    private IEnumerator DoFade(float startAlpha, float endAlpha, float duration)
    {
        if (fadePanel == null) yield break;

        fadePanel.gameObject.SetActive(true);
        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            float alpha = Mathf.Lerp(startAlpha, endAlpha, t);
            fadePanel.color = new Color(0, 0, 0, alpha);
            yield return null;
        }
        fadePanel.color = new Color(0, 0, 0, endAlpha);
    }

    // (기존 FadeOut 함수는 DoFade로 대체되었으므로 삭제해도 되지만, 호환성을 위해 둠)
    private IEnumerator FadeOut()
    {
        yield return StartCoroutine(DoFade(0f, 1f, fadeDuration));
    }

    private void AnimateLegsWindmill(float angle)
    {
        if (frontShinL != null) frontShinL.localRotation = Quaternion.Euler(angle, 0, 0);
        if (footR != null) footR.localRotation = Quaternion.Euler(angle, 0, 0);
        if (frontShinR != null) frontShinR.localRotation = Quaternion.Euler(angle + 180f, 0, 0);
        if (footL != null) footL.localRotation = Quaternion.Euler(angle + 180f, 0, 0);
    }

    private IEnumerator CloseDoorsRoutine()
    {
        if (leftDoor == null || rightDoor == null) yield break;

        float t = 0f;
        Quaternion startRotL = leftDoor.localRotation;
        Quaternion startRotR = rightDoor.localRotation;
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
        float legRotationValue = 0f;

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

            if (target == player.transform)
            {
                legRotationValue -= Time.deltaTime * 750f;
                AnimateLegsWindmill(legRotationValue);
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

    private void ProcessMissionSuccess()
    {
        if (GameManager.Instance != null && GameManager.Instance.currentMissionTarget != null)
        {
            int reward = GameManager.Instance.currentMissionTarget.price;
            GameManager.Instance.AddMoney(reward);

            int tIndex = GameManager.Instance.currentTargetIndex;
            GameManager.Instance.data.collectedArts[tIndex] = true;

            int sIndex = GameManager.Instance.currentTargetStageIndex;
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
    public void FailByTimeOver()
    {
        Debug.Log("⏰ 시간 초과! 트럭 출발!");
        StartCoroutine(TruckLeaveAloneRoutine());
    }

    private IEnumerator TruckLeaveAloneRoutine()
    {
        // 1. 조작 차단
        // if (horseControl != null) horseControl.isControlEnabled = false;

        // 2. 카메라 전환 (트럭 비추기)
        if (successCamera != null) successCamera.Priority = 200;

        // 3. 문 닫기
        yield return StartCoroutine(CloseDoorsRoutine());

        // 4. 트럭 출발 (말 없이 트럭만 이동)
        yield return StartCoroutine(MoveAlongSpline(truck.transform, truckEscapePath, truckEscapeSpeed));

        // 5. 트럭이 어느 정도 가면 (혹은 도착하면) UI 띄우기
        FindObjectOfType<GameOverUI>().ShowBusted();
    }
}