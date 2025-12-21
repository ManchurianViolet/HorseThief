using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Splines;
using Unity.Cinemachine;
using UnityEngine.SceneManagement;
using System.Collections;

public class HideoutDepartureCutscene : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject player; // 말
    [SerializeField] private HorseControl horseControl; // 말 조작 스크립트
    [SerializeField] private LaptopInteraction laptopInteraction; // 노트북

    [Header("Camera")]
    [SerializeField] private CinemachineCamera departureCamera; // 천장 카메라

    [Header("Spline Path")]
    [SerializeField] private SplineContainer departurePath; // 대문까지 가는 경로
    [SerializeField] private Transform departureStartPoint; // 시작 지점 (순간이동 위치)

    [Header("Fade")]
    [SerializeField] private Image fadePanel; // 검은 막
    [SerializeField] private float fadeDuration = 1.0f;

    [Header("Settings")]
    [SerializeField] private float walkSpeed = 2.0f; // 걷는 속도 (느리게)
    [SerializeField] private float arrivalDistance = 0.5f; // 도착 판정 거리

    // ★ [추가] 문 관련
    [Header("Door")]
    [SerializeField] private GameObject door; // 문
    [SerializeField] private float doorOpenAngle = 90f; // 문이 열리는 각도 (기본 90도)
    [SerializeField] private float doorOpenSpeed = 2f; // 문 여는 속도

    [Header("Leg Animation (Windmill)")]
    [SerializeField] private Transform legFL; // 앞왼쪽 (Front Left)
    [SerializeField] private Transform legFR; // 앞오른쪽 (Front Right)
    [SerializeField] private Transform legBL; // 뒤왼쪽 (Back Left)
    [SerializeField] private Transform legBR; // 뒤오른쪽 (Back Right)
    [SerializeField] private float legSpinSpeed = 700f; // 회전 속도 (빠를수록 웃김)

    private bool isCutscenePlaying = false;
    private Rigidbody playerRb;
    private float currentSpeedFL;
    private float currentSpeedFR;
    private float currentSpeedBL;
    private float currentSpeedBR;

    private void Start()
    {
        // 페이드 패널 초기화
        if (fadePanel != null)
        {
            fadePanel.gameObject.SetActive(false);
            fadePanel.color = new Color(0, 0, 0, 0);
        }

        // 천장 카메라 비활성화
        if (departureCamera != null)
        {
            departureCamera.Priority = 0;
        }

        // Rigidbody 찾기
        if (player != null)
        {
            playerRb = player.GetComponent<Rigidbody>();
        }
    }

    // ★ LaptopHeistTab에서 이 함수를 호출할 겁니다!
    public void StartDeparture(int stageIndex)
    {
        if (isCutscenePlaying)
        {
            Debug.LogWarning("⚠️ 이미 연출 중입니다!");
            return;
        }

        Debug.Log($"🎬 [연출 시작] 미션 {stageIndex + 1}탄 출발!");

        // GameManager에 미션 정보 저장
        if (GameManager.Instance != null)
        {
            GameManager.Instance.GenerateMission(stageIndex);
        }

        StartCoroutine(DepartureCutsceneRoutine());
    }

    private IEnumerator DepartureCutsceneRoutine()
    {
        isCutscenePlaying = true;

        // 1. 노트북 닫기
        if (laptopInteraction != null)
        {
            laptopInteraction.CloseLaptop();
            Debug.Log("💻 노트북 닫힘");
        }

        // 2. 플레이어 조작 끄기
        if (horseControl != null)
        {
            horseControl.isControlEnabled = false;
            Debug.Log("🔒 플레이어 조작 차단");
        }

        // 3. 카메라 전환 (천장 카메라 활성화)
        if (departureCamera != null)
        {
            departureCamera.Priority = 100; // 높은 우선순위로 전환
            Debug.Log("📷 천장 카메라로 전환");
        }

        // 4. 말 물리 차단 (Kinematic 켜기)
        if (playerRb != null)
        {
            playerRb.isKinematic = true;
            playerRb.linearVelocity = Vector3.zero;
            playerRb.angularVelocity = Vector3.zero;
            Debug.Log("🛡️ 말 물리 차단 (Kinematic ON)");
        }

        // 5. 순간이동 (스플라인 시작점)
        if (player != null && departureStartPoint != null)
        {
            player.transform.position = departureStartPoint.position;
            player.transform.rotation = departureStartPoint.rotation;
            Debug.Log("📍 말 순간이동 완료");
        }

        yield return new WaitForSeconds(0.5f); // 잠깐 정지 (숨 고르기)

        // ★ [추가] 6. 문 열기 (회전)
        yield return StartCoroutine(OpenDoor());

        // 7. 스플라인 따라 자동 이동
        yield return StartCoroutine(WalkAlongSpline());

        // 8. 페이드 아웃
        yield return StartCoroutine(FadeOut());

        // 9. 로딩 씬으로 이동
        Debug.Log("🚀 로딩 씬으로 이동!");
        SceneManager.LoadScene("LoadingScene");
    }

    // ★ [새 함수] 문 회전해서 열기
    private IEnumerator OpenDoor()
    {
        if (door == null)
        {
            Debug.LogWarning("⚠️ 문이 연결되지 않았습니다!");
            yield break;
        }

        Debug.Log("🚪 문 열기 시작...");

        // 시작 각도 저장
        Quaternion startRotation = door.transform.localRotation;

        // 목표 각도 계산 (Y축 기준으로 회전)
        Quaternion targetRotation = startRotation * Quaternion.Euler(0, doorOpenAngle, 0);

        float elapsed = 0f;
        float duration = 1f / doorOpenSpeed; // 속도를 시간으로 변환

        // 부드럽게 회전
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            // Ease-out 효과 (처음엔 빠르고 끝에선 느리게)
            t = Mathf.Sin(t * Mathf.PI * 0.5f);

            door.transform.localRotation = Quaternion.Slerp(startRotation, targetRotation, t);
            yield return null;
        }

        // 최종 위치 확정
        door.transform.localRotation = targetRotation;
        Debug.Log("🚪 문 열림 완료!");
    }

    private IEnumerator WalkAlongSpline()
    {
        if (departurePath == null) yield break;

        float splineLength = departurePath.CalculateLength();
        if (splineLength <= 0.01f) yield break;

        Debug.Log($"🚶 이동 시작! (총 거리: {splineLength:F1}m)");

        // 걷기 시작할 때, 다리마다 속도를 랜덤하게 정해줍니다!
        currentSpeedFL = legSpinSpeed * Random.Range(0.6f, 1.5f);
        currentSpeedFR = legSpinSpeed * Random.Range(0.6f, 1.5f);
        currentSpeedBL = legSpinSpeed * Random.Range(0.6f, 1.5f);
        currentSpeedBR = legSpinSpeed * Random.Range(0.6f, 1.5f);

        float distanceTraveled = 0f;

        while (distanceTraveled < splineLength)
        {
            distanceTraveled += walkSpeed * Time.deltaTime;
            float t = Mathf.Clamp01(distanceTraveled / splineLength);

            Vector3 targetPos = departurePath.EvaluatePosition(t);
            Vector3 targetDir = departurePath.EvaluateTangent(t);

            player.transform.position = targetPos;

            // 1. 몸통 회전
            if (targetDir != Vector3.zero)
            {
                player.transform.rotation = Quaternion.Slerp(
                    player.transform.rotation,
                    Quaternion.LookRotation(-targetDir),
                    Time.deltaTime * 5f
                );
            }

            // 2. 다리 풍차 돌리기
            RotateLegs();

            yield return null;
        }
    }

    private IEnumerator FadeOut()
    {
        if (fadePanel == null) yield break;

        fadePanel.gameObject.SetActive(true);
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Clamp01(elapsed / fadeDuration);
            fadePanel.color = new Color(0, 0, 0, alpha);
            yield return null;
        }

        fadePanel.color = new Color(0, 0, 0, 1);
        Debug.Log("🌑 페이드 아웃 완료");
    }

    private void Update()
    {
        if (isCutscenePlaying && Input.GetKeyDown(KeyCode.Escape))
        {
            Debug.Log("⏹️ 연출 스킵!");
            StopAllCoroutines();
            SceneManager.LoadScene("LoadingScene");
        }
    }

    private void RotateLegs()
    {
        // 다리 회전 (Vector3.left 방향으로)
        if (legFL) legFL.Rotate(Vector3.left * currentSpeedFL * Time.deltaTime);
        if (legFR) legFR.Rotate(Vector3.left * currentSpeedFR * Time.deltaTime);
        if (legBL) legBL.Rotate(Vector3.left * currentSpeedBL * Time.deltaTime);
        if (legBR) legBR.Rotate(Vector3.left * currentSpeedBR * Time.deltaTime);
    }
}