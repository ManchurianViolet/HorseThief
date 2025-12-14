using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Splines;
using Unity.Cinemachine;
using System.Collections;

public class MuseumArrivalCutscene : MonoBehaviour
{
    [Header("Player")]
    [SerializeField] private GameObject player;
    [SerializeField] private HorseControl horseControl;

    [Header("Camera 1 - Truck Interior")]
    [SerializeField] private CinemachineCamera truckCamera;

    [Header("Camera 2 - Van Exterior")]
    [SerializeField] private CinemachineCamera vanCamera;

    [Header("Spline Path 1 - Truck Exit")]
    [SerializeField] private SplineContainer truckExitPath;

    [Header("Spline Path 2 - Van to Museum")]
    [SerializeField] private SplineContainer vanToMuseumPath;
    [SerializeField] private Transform vanSpawnPoint;

    [Header("Teleport - Interior")]
    [SerializeField] private Transform museumInteriorPoint;

    [Header("Door Animation")]
    [SerializeField] private Transform vanBackDoor;
    [SerializeField] private float doorOpenAngle = 90f;
    [SerializeField] private float doorOpenDuration = 1.0f;

    [Header("Fade")]
    [SerializeField] private Image fadePanel;
    [SerializeField] private float fadeDuration = 0.5f;

    [Header("Settings")]
    [SerializeField] private float walkSpeed = 2.0f;

    [Header("UI Control")]
    [SerializeField] private GameObject museumUI;  // ★ [추가] 미술관 UI (타이머, 비밀번호)
    [SerializeField] private MuseumTimeManager timeManager; // ★ [추가] 타이머 시작 명령용

    [Header("Leg Animation (Windmill)")]
    [SerializeField] private Transform legFL; // 앞왼쪽 (Front Left)
    [SerializeField] private Transform legFR; // 앞오른쪽 (Front Right)
    [SerializeField] private Transform legBL; // 뒤왼쪽 (Back Left)
    [SerializeField] private Transform legBR; // 뒤오른쪽 (Back Right)
    [SerializeField] private float legSpinSpeed = 700f; // 회전 속도 (빠를수록 웃김)
    private float currentSpeedFL;
    private float currentSpeedFR;
    private float currentSpeedBL;
    private float currentSpeedBR;

    private bool isCutscenePlaying = false;
    private Rigidbody playerRb;

    private void Start()
    {
        if (fadePanel != null)
        {
            fadePanel.gameObject.SetActive(false);
            fadePanel.color = new Color(0, 0, 0, 0);
        }
        if (truckCamera != null) truckCamera.Priority = 0;
        if (vanCamera != null) vanCamera.Priority = 0;
        if (player != null) playerRb = player.GetComponent<Rigidbody>();
    }

    public void StartArrivalCutscene()
    {
        if (isCutscenePlaying) return;
        Debug.Log("🎬 [미술관 도착 연출 시작]");
        StartCoroutine(ArrivalCutsceneRoutine());
    }

    private IEnumerator ArrivalCutsceneRoutine()
    {
        isCutscenePlaying = true;

        // 1. 트럭 탈출 (걷기)
        yield return StartCoroutine(Part1_ExitTruck());

        // 2. 암전 시작 (Fade Out)
        yield return StartCoroutine(FadeOut());

        // 3. ★ [수정] 암전된 상태에서 몰래 이동 & 카메라 세팅
        SetupPart2_WhileBlack();
        yield return new WaitForSeconds(0.5f); // 암전 상태로 잠깐 대기

        // 4. 화면 밝아짐 (Fade In)
        yield return StartCoroutine(FadeIn());

        // 5. 미술관 앞 연출 (문 열리고 걷기) - 이미 이동은 완료된 상태
        yield return StartCoroutine(Part2_AnimationOnly());

        // 6. 내부 진입 (순간이동 + 페이드 인)
        yield return StartCoroutine(Part3_TeleportToInterior());

        // 7. 게임 시작
        Debug.Log("🎬 [연출 완료] 게임 플레이 시작!");
        isCutscenePlaying = false;
        OnCutsceneComplete();
    }

    private IEnumerator Part1_ExitTruck()
    {
        Debug.Log("📹 [Part 1] 트럭 탈출");
        if (truckCamera != null) truckCamera.Priority = 100;

        if (horseControl != null) horseControl.isControlEnabled = false;
        if (playerRb != null)
        {
            playerRb.isKinematic = true;
            playerRb.linearVelocity = Vector3.zero;
            playerRb.angularVelocity = Vector3.zero;
        }

        yield return new WaitForSeconds(0.5f);
        yield return StartCoroutine(WalkAlongSpline(truckExitPath));
    }

    // ★ [신규] 암전 중에 실행할 세팅 함수 (이동, 카메라 전환)
    private void SetupPart2_WhileBlack()
    {
        // 작은 차 앞으로 순간이동
        if (player != null && vanSpawnPoint != null)
        {
            player.transform.position = vanSpawnPoint.position;
            // 차 뒤쪽을 바라보게(90도) 회전
            player.transform.rotation = vanSpawnPoint.rotation * Quaternion.Euler(0, 90, 0);
            Debug.Log("📍 [암전 중] 플레이어 위치 이동 완료");
        }

        // 카메라 전환 (트럭 끄고, 작은 차 켜고)
        if (truckCamera != null) truckCamera.Priority = 0;
        if (vanCamera != null) vanCamera.Priority = 100;
        Debug.Log("📹 [암전 중] 카메라 전환 완료");
    }

    // ★ [수정] Part2는 이제 이동 없이 애니메이션(문 열기, 걷기)만 담당
    private IEnumerator Part2_AnimationOnly()
    {
        // 문 열기
        yield return StartCoroutine(OpenVanDoor());

        Debug.Log("🚶 [Part 2] 미술관 입구로 이동");

        // 미술관으로 걸어가기
        yield return StartCoroutine(WalkAlongSpline(vanToMuseumPath));

        Debug.Log("🌑 [Part 2] 미술관 도착. 암전 시작.");
        yield return StartCoroutine(FadeOut());
    }

    private IEnumerator Part3_TeleportToInterior()
    {
        yield return new WaitForSeconds(0.5f); // 암전 대기

        // 1. 내부로 순간이동
        if (player != null && museumInteriorPoint != null)
        {
            player.transform.position = museumInteriorPoint.position;
            player.transform.rotation = museumInteriorPoint.rotation;
        }

        // 2. 외부 카메라 끄기
        if (vanCamera != null) vanCamera.Priority = 0;

        // 3. ★ [수정] 암전 상태일 때 미리 물리를 켭니다!
        if (playerRb != null)
        {
            playerRb.isKinematic = false; // 중력 작동 시작 -> 땅으로 툭 떨어짐 (안 보임)
            Debug.Log("⚖️ [암전 중] 물리 엔진 활성화 (착지 대기)");
        }

        yield return new WaitForSeconds(1f); // 땅에 착지할 시간 벌어주기

        // 4. 이제 눈을 뜨면 말이 바닥에 예쁘게 서 있음
        Debug.Log("☀️ [Part 3] 게임 시작 화면 밝힘");
        yield return StartCoroutine(FadeIn());
        if (timeManager != null)
        {
            timeManager.StartTimer(); // 타이머야 돌아라!
        }
        if (museumUI != null) museumUI.SetActive(true); // 미술관 UI(타이머) 켜기
    }

    // --- 보조 함수들 (기존 동일) ---

    private IEnumerator WalkAlongSpline(SplineContainer path)
    {
        currentSpeedFL = legSpinSpeed * Random.Range(0.6f, 1.5f);
        currentSpeedFR = legSpinSpeed * Random.Range(0.6f, 1.5f);
        currentSpeedBL = legSpinSpeed * Random.Range(0.6f, 1.5f);
        currentSpeedBR = legSpinSpeed * Random.Range(0.6f, 1.5f);
        if (path == null) yield break;
        float len = path.CalculateLength();
        if (len <= 0.01f) yield break;

        float dist = 0f;
        while (dist < len)
        {
            RotateLegs();
            dist += walkSpeed * Time.deltaTime;
            float t = Mathf.Clamp01(dist / len);
            Vector3 pos = path.EvaluatePosition(t);
            Vector3 dir = path.EvaluateTangent(t);

            player.transform.position = pos;
            if (dir != Vector3.zero)
                player.transform.rotation = Quaternion.Slerp(player.transform.rotation, Quaternion.LookRotation(-dir), Time.deltaTime * 5f);

            yield return null;
        }
    }

    private IEnumerator OpenVanDoor()
    {
        if (vanBackDoor == null) yield break;
        Vector3 start = vanBackDoor.localEulerAngles;
        Vector3 end = start + new Vector3(0, doorOpenAngle, 0);
        float t = 0;
        while (t < 1)
        {
            t += Time.deltaTime / doorOpenDuration;
            vanBackDoor.localEulerAngles = Vector3.Lerp(start, end, t);
            yield return null;
        }
    }

    private IEnumerator FadeOut()
    {
        if (fadePanel == null) yield break;
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

    private IEnumerator FadeIn()
    {
        if (fadePanel == null) yield break;
        float t = 1;
        while (t > 0)
        {
            t -= Time.deltaTime / fadeDuration;
            fadePanel.color = new Color(0, 0, 0, t);
            yield return null;
        }
        fadePanel.gameObject.SetActive(false);
    }

    private void OnCutsceneComplete()
    {
        if (horseControl != null) horseControl.isControlEnabled = true;
        if (playerRb != null) playerRb.isKinematic = false;

        MuseumTimeManager tm = FindObjectOfType<MuseumTimeManager>();
        if (tm != null) tm.StartTimer();
    }
    private void RotateLegs()
    {
        // ★ [수정 1] 방향 반대: Vector3.right -> Vector3.left (또는 -Vector3.right)
        // ★ [수정 2] 속도 랜덤: 위에서 정한 개별 속도(currentSpeedXX) 사용

        // 앞왼쪽
        if (legFL) legFL.Rotate(Vector3.left * currentSpeedFL * Time.deltaTime);

        // 앞오른쪽
        if (legFR) legFR.Rotate(Vector3.left * currentSpeedFR * Time.deltaTime);

        // 뒤왼쪽
        if (legBL) legBL.Rotate(Vector3.left * currentSpeedBL * Time.deltaTime);

        // 뒤오른쪽
        if (legBR) legBR.Rotate(Vector3.left * currentSpeedBR * Time.deltaTime);
    }
}