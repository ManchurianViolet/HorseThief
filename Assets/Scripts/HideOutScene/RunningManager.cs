using UnityEngine;
using TMPro;
using System.Collections;

public class RunningManager : MonoBehaviour
{
    [Header("=== Hideout Integration Settings ===")]
    [SerializeField] private Transform playPosition;   // 출발선 위치
    [SerializeField] private Transform getOutPosition; // 나갈 위치
    [SerializeField] private GameObject gameUIPanel;   // 레이싱 UI 패널

    [Header("=== UI System ===")]
    [SerializeField] private TextMeshProUGUI countdownText;
    [SerializeField] private TextMeshProUGUI speedText;
    [SerializeField] private TextMeshProUGUI distanceText;
    [SerializeField] private TextMeshProUGUI timeText;
    [SerializeField] private TextMeshProUGUI recordBoardText;

    [Header("=== Audio ===")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip beepSound;
    [SerializeField] private AudioClip gunSound;
    // [삭제됨] 축포, 피니시 사운드, 파티클 관련 변수들 제거

    // --- 내부 로직 변수 ---
    private GameObject currentPlayer;     // 현재 훈련 중인 플레이어 오브젝트
    private HorseControl playerControl;   // 플레이어의 조작 스크립트 (카운트다운 때 멈추기 위해)

    private bool isTrainingActive = false;
    private bool isRaceStarted = false;
    private bool isRaceFinished = false;

    private float raceTimer = 0f;
    private Vector3 startPosition;
    private float currentDistance = 0f;

    // 구간 기록 체크용 변수
    private int nextMilestoneIndex = 0;
    private readonly float[] milestones = { 100f, 200f, 300f, 400f, 500f };

    // 속도 계산 변수
    private Vector3 lastPosition;
    private float currentDisplaySpeed = 0f;

    void Start()
    {
        // 시작 시 UI와 기능 꺼두기
        if (gameUIPanel != null) gameUIPanel.SetActive(false);
        InitializeUI();
    }

    void Update()
    {
        // 훈련 중이 아니면 실행 안 함
        if (!isTrainingActive) return;

        // ESC키: 훈련 강제 종료 (나가기)
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            StopTraining();
            return;
        }

        // 레이스 로직 (시작 전이나 끝난 후엔 계산 안 함)
        if (!isRaceStarted || isRaceFinished || currentPlayer == null) return;

        // --- 1. 시간 및 거리 계산 ---
        raceTimer += Time.deltaTime;
        Vector3 currentPos = currentPlayer.transform.position;

        // Y축(높이) 변화는 거리 계산에서 제외하고 수평 거리만 잴 수도 있지만, 
        // 여기선 단순하게 직선 거리로 계산 (필요시 수정 가능)
        currentDistance = Vector3.Distance(startPosition, currentPos);

        // --- 2. 속도 계산 (스무딩) ---
        float rawSpeedMPS = Vector3.Distance(lastPosition, currentPos) / Time.deltaTime;
        currentDisplaySpeed = Mathf.Lerp(currentDisplaySpeed, rawSpeedMPS, Time.deltaTime * 3f);
        float speedKPH = currentDisplaySpeed * 3.6f;
        if (speedKPH < 1f) speedKPH = 0f;

        lastPosition = currentPos;

        // --- 3. UI 갱신 ---
        UpdateRealtimeUI(speedKPH);

        // --- 4. 구간 기록 체크 ---
        CheckMilestones();
    }

    // ====================================================
    // ★ 외부(문 앞 트리거)에서 호출할 함수들
    // ====================================================
    public void StartTraining(GameObject player)
    {
        isTrainingActive = true;
        currentPlayer = player;
        playerControl = player.GetComponent<HorseControl>();

        // 1. 플레이어 이동 (출발선)
        if (currentPlayer != null && playPosition != null)
        {
            Rigidbody rb = currentPlayer.GetComponent<Rigidbody>();
            if (rb != null) rb.isKinematic = true; // 물리 끄고 이동
            currentPlayer.transform.position = playPosition.position;
            currentPlayer.transform.rotation = playPosition.rotation;
            if (rb != null) rb.isKinematic = false;

            // 기록용 시작 위치 저장
            startPosition = playPosition.position;
            lastPosition = startPosition;
        }

        // 2. 변수 초기화
        isRaceStarted = false;
        isRaceFinished = false;
        raceTimer = 0f;
        currentDistance = 0f;
        nextMilestoneIndex = 0;
        currentDisplaySpeed = 0f;

        // 3. UI 초기화 및 켜기
        InitializeUI();
        if (gameUIPanel != null) gameUIPanel.SetActive(true);

        // 4. 움직임 봉인 (카운트다운 동안)
        if (playerControl != null) playerControl.enabled = false;

        // 5. 카운트다운 시작
        StartCoroutine(CountdownRoutine());
    }

    public void StopTraining()
    {
        isTrainingActive = false;
        StopAllCoroutines(); // 카운트다운 중단

        // 1. UI 끄기
        if (gameUIPanel != null) gameUIPanel.SetActive(false);
        if (countdownText != null) countdownText.gameObject.SetActive(false);

        // 2. 플레이어 내보내기
        if (currentPlayer != null && getOutPosition != null)
        {
            Rigidbody rb = currentPlayer.GetComponent<Rigidbody>();
            if (rb != null) rb.isKinematic = true;
            currentPlayer.transform.position = getOutPosition.position;
            // ★ [수정 1] 시계방향 90도 회전 추가
            Quaternion exitRotation = getOutPosition.rotation * Quaternion.Euler(0, 90, 0);
            currentPlayer.transform.rotation = exitRotation;
            if (rb != null) rb.isKinematic = false;
        }

        // 3. 움직임 잠금 해제 (혹시 잠긴 채로 나갈까봐)
        if (playerControl != null) playerControl.enabled = true;
        HorseControl horseControl = currentPlayer.GetComponent<HorseControl>();
        if (horseControl != null)
        {
            horseControl.ResetHeadPosition();
        }
        currentPlayer = null;
        playerControl = null;
    }

    // ====================================================
    // 내부 로직
    // ====================================================

    private IEnumerator CountdownRoutine()
    {
        if (countdownText != null) countdownText.gameObject.SetActive(true);

        // 3, 2, 1 카운트
        yield return ShowCount("3");
        yield return ShowCount("2");
        yield return ShowCount("1");

        // GO!
        if (countdownText != null) countdownText.text = "GO!";
        if (audioSource != null && gunSound != null) audioSource.PlayOneShot(gunSound);



        // [삭제됨] 시작 파티클 제거 완료

        // 출발!
        if (playerControl != null) playerControl.enabled = true;
        isRaceStarted = true;

        yield return new WaitForSeconds(1f);
        if (countdownText != null) countdownText.gameObject.SetActive(false);
    }

    private void CheckMilestones()
    {
        if (nextMilestoneIndex >= milestones.Length) return;

        if (currentDistance >= milestones[nextMilestoneIndex])
        {
            float splitTime = raceTimer;
            float targetDistance = milestones[nextMilestoneIndex];

            // 기록판 갱신
            string recordLine = $"{targetDistance:F0}m : {FormatTime(splitTime)}";
            if (recordBoardText != null) recordBoardText.text += recordLine + "\n";

            // 마지막 500m 도달 시
            if (nextMilestoneIndex == milestones.Length - 1)
            {
                FinishRace();
            }

            nextMilestoneIndex++;
        }
    }

    private void FinishRace()
    {
        isRaceFinished = true;

        // [수정됨] 축포, 파티클, 요란한 사운드 제거
        // 그냥 텍스트만 띄움
        if (countdownText != null)
        {
            countdownText.gameObject.SetActive(true);
            countdownText.text = "FINISH!";
        }

        // 기록을 자세히 보고 싶을 수 있으니 움직임은 멈추지 않거나, 
        // 원한다면 여기서 playerControl.enabled = false; 해도 됨.
        // 현재는 자유롭게 뛰어놀게 둠.
    }

    private IEnumerator ShowCount(string text)
    {
        if (countdownText != null) countdownText.text = text;
        if (audioSource != null && beepSound != null) audioSource.PlayOneShot(beepSound);
        yield return new WaitForSeconds(1f);
    }

    private void UpdateRealtimeUI(float speedKmh)
    {
        if (timeText != null) timeText.text = FormatTime(raceTimer);
        if (distanceText != null) distanceText.text = $"Dist: {currentDistance:F1} m";
        if (speedText != null) speedText.text = $"Speed: {speedKmh:F0} km/h";
    }

    private void InitializeUI()
    {
        if (timeText != null) timeText.text = "00:00.0";
        if (distanceText != null) distanceText.text = "Dist: 0.0 m";
        if (speedText != null) speedText.text = "Speed: 0 km/h";
        if (recordBoardText != null) recordBoardText.text = "";
    }

    private string FormatTime(float time)
    {
        int minutes = (int)(time / 60);
        float seconds = time % 60;
        return string.Format("{0:00}:{1:04.1f}", minutes, seconds);
    }


}