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
    // ★ [추가] 다리 풍차 돌리기용 변수
    [Header("Leg Animation (Windmill)")]
    [SerializeField] private Transform legFL; // 앞왼쪽 (Front Left)
    [SerializeField] private Transform legFR; // 앞오른쪽 (Front Right)
    [SerializeField] private Transform legBL; // 뒤왼쪽 (Back Left)
    [SerializeField] private Transform legBR; // 뒤오른쪽 (Back Right)
    [SerializeField] private float legSpinSpeed = 700f; // 회전 속도 (빠를수록 웃김)
    private bool isCutscenePlaying = false;
    private Rigidbody playerRb;

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

        // 6. 스플라인 따라 자동 이동
        yield return StartCoroutine(WalkAlongSpline());

        // 7. 페이드 아웃
        yield return StartCoroutine(FadeOut());

        // 8. 로딩 씬으로 이동
        Debug.Log("🚀 로딩 씬으로 이동!");
        SceneManager.LoadScene("LoadingScene");
    }

    private IEnumerator WalkAlongSpline()
    {
        if (departurePath == null) yield break;

        float splineLength = departurePath.CalculateLength();
        if (splineLength <= 0.01f) yield break;

        Debug.Log($"🚶 이동 시작! (총 거리: {splineLength:F1}m)");

        float distanceTraveled = 0f;

        while (distanceTraveled < splineLength)
        {
            distanceTraveled += walkSpeed * Time.deltaTime;
            float t = Mathf.Clamp01(distanceTraveled / splineLength);

            Vector3 targetPos = departurePath.EvaluatePosition(t);
            Vector3 targetDir = departurePath.EvaluateTangent(t);

            player.transform.position = targetPos;

            // 1. 몸통 회전 (저번에 수정한 -targetDir 유지)
            if (targetDir != Vector3.zero)
            {
                player.transform.rotation = Quaternion.Slerp(
                    player.transform.rotation,
                    Quaternion.LookRotation(-targetDir), // ★ 뒤로 가면 여기에 (-) 붙이기
                    Time.deltaTime * 5f
                );
            }

            // 2. ★ [추가] 다리 풍차 돌리기 (X축 기준으로 뱅글뱅글)
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

    // ★ [선택사항] 연출 도중 ESC 눌러서 취소하기
    private void Update()
    {
        if (isCutscenePlaying && Input.GetKeyDown(KeyCode.Escape))
        {
            Debug.Log("⏹️ 연출 스킵!");
            StopAllCoroutines();
            SceneManager.LoadScene("LoadingScene");
        }
    }
    // ★ [추가] 다리 회전 함수
    private void RotateLegs()
    {
        float rotAmount = legSpinSpeed * Time.deltaTime;

        // 다리가 있다면 X축(Right) 기준으로 회전시킴
        // (만약 다리가 이상한 방향으로 돌면 Vector3.forward나 up으로 바꿔보세요)
        if (legFL) legFL.Rotate(Vector3.right * rotAmount);
        if (legFR) legFR.Rotate(Vector3.right * rotAmount);
        if (legBL) legBL.Rotate(Vector3.right * rotAmount);
        if (legBR) legBR.Rotate(Vector3.right * rotAmount);
    }
}