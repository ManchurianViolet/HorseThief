using UnityEngine;
using TMPro;
using System.Collections;

public class RunningManager : MonoBehaviour
{
    [Header("=== Hideout Integration Settings ===")]
    [SerializeField] private Transform playPosition;
    [SerializeField] private Transform getOutPosition;
    [SerializeField] private GameObject gameUIPanel;

    [Header("=== UI System ===")]
    [SerializeField] private TextMeshProUGUI countdownText;
    [SerializeField] private TextMeshProUGUI speedText;
    [SerializeField] private TextMeshProUGUI distanceText;
    [SerializeField] private TextMeshProUGUI timeText;

    // ★ [변경] 개별 기록 텍스트 (5개)
    [Header("=== Milestone Record UI ===")]
    [SerializeField] private TextMeshProUGUI record100m; // 100m 기록
    [SerializeField] private TextMeshProUGUI record200m; // 200m 기록
    [SerializeField] private TextMeshProUGUI record300m; // 300m 기록
    [SerializeField] private TextMeshProUGUI record400m; // 400m 기록
    [SerializeField] private TextMeshProUGUI record500m; // 500m 기록

    // ★ [추가] 배경 이미지 (5개)
    [Header("=== Milestone Background Images ===")]
    [SerializeField] private GameObject background100m; // 100m 배경
    [SerializeField] private GameObject background200m; // 200m 배경
    [SerializeField] private GameObject background300m; // 300m 배경
    [SerializeField] private GameObject background400m; // 400m 배경
    [SerializeField] private GameObject background500m; // 500m 배경

    [Header("=== Audio ===")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip beepSound;
    [SerializeField] private AudioClip gunSound;

    private GameObject currentPlayer;
    private HorseControl playerControl;

    private bool isTrainingActive = false;
    private bool isRaceStarted = false;
    private bool isRaceFinished = false;

    private float raceTimer = 0f;
    private Vector3 startPosition;
    private float currentDistance = 0f;

    private int nextMilestoneIndex = 0;
    private readonly float[] milestones = { 100f, 200f, 300f, 400f, 500f };

    private Vector3 lastPosition;
    private float currentDisplaySpeed = 0f;

    void Start()
    {
        if (gameUIPanel != null) gameUIPanel.SetActive(false);
        InitializeUI();
    }

    void Update()
    {
        if (!isTrainingActive) return;

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            StopTraining();
            return;
        }

        if (!isRaceStarted || isRaceFinished || currentPlayer == null) return;

        raceTimer += Time.deltaTime;
        Vector3 currentPos = currentPlayer.transform.position;
        currentDistance = Vector3.Distance(startPosition, currentPos);

        float rawSpeedMPS = Vector3.Distance(lastPosition, currentPos) / Time.deltaTime;
        currentDisplaySpeed = Mathf.Lerp(currentDisplaySpeed, rawSpeedMPS, Time.deltaTime * 3f);
        float speedKPH = currentDisplaySpeed * 3.6f;
        if (speedKPH < 1f) speedKPH = 0f;

        lastPosition = currentPos;

        UpdateRealtimeUI(speedKPH);
        CheckMilestones();
    }

    public void StartTraining(GameObject player)
    {
        isTrainingActive = true;
        currentPlayer = player;
        playerControl = player.GetComponent<HorseControl>();

        if (currentPlayer != null && playPosition != null)
        {
            Rigidbody rb = currentPlayer.GetComponent<Rigidbody>();
            if (rb != null) rb.isKinematic = true;
            currentPlayer.transform.position = playPosition.position;
            currentPlayer.transform.rotation = playPosition.rotation;
            if (rb != null) rb.isKinematic = false;

            startPosition = playPosition.position;
            lastPosition = startPosition;
        }

        isRaceStarted = false;
        isRaceFinished = false;
        raceTimer = 0f;
        currentDistance = 0f;
        nextMilestoneIndex = 0;
        currentDisplaySpeed = 0f;

        InitializeUI();
        if (gameUIPanel != null) gameUIPanel.SetActive(true);

        if (playerControl != null) playerControl.enabled = false;

        StartCoroutine(CountdownRoutine());
    }

    public void StopTraining()
    {
        isTrainingActive = false;
        StopAllCoroutines();

        if (gameUIPanel != null) gameUIPanel.SetActive(false);
        if (countdownText != null) countdownText.gameObject.SetActive(false);

        if (currentPlayer != null && getOutPosition != null)
        {
            Rigidbody rb = currentPlayer.GetComponent<Rigidbody>();
            if (rb != null) rb.isKinematic = true;
            currentPlayer.transform.position = getOutPosition.position;
            Quaternion exitRotation = getOutPosition.rotation * Quaternion.Euler(0, 90, 0);
            currentPlayer.transform.rotation = exitRotation;
            if (rb != null) rb.isKinematic = false;
        }

        if (playerControl != null) playerControl.enabled = true;
        HorseControl horseControl = currentPlayer.GetComponent<HorseControl>();
        if (horseControl != null)
        {
            horseControl.ResetHeadPosition();
        }
        currentPlayer = null;
        playerControl = null;
    }

    private IEnumerator CountdownRoutine()
    {
        if (countdownText != null) countdownText.gameObject.SetActive(true);

        yield return ShowCount("3");
        yield return ShowCount("2");
        yield return ShowCount("1");

        if (countdownText != null) countdownText.text = "GO!";
        if (audioSource != null && gunSound != null) audioSource.PlayOneShot(gunSound);

        if (playerControl != null) playerControl.enabled = true;
        isRaceStarted = true;

        yield return new WaitForSeconds(1f);
        if (countdownText != null) countdownText.gameObject.SetActive(false);
    }

    // ★ [수정] 개별 텍스트에 기록 표시 + 배경 이미지 켜기
    private void CheckMilestones()
    {
        if (nextMilestoneIndex >= milestones.Length) return;

        if (currentDistance >= milestones[nextMilestoneIndex])
        {
            float splitTime = raceTimer;
            string timeString = FormatTime(splitTime);

            // ★ [핵심] 각 구간에 맞는 텍스트에 기록 넣기 + 배경 켜기
            switch (nextMilestoneIndex)
            {
                case 0: // 100m
                    if (record100m != null)
                        record100m.text = $"100m : {timeString}";
                    if (background100m != null) background100m.SetActive(true); // ★ 배경 켜기!
                    break;
                case 1:
                    record200m.text = $"200m : {timeString}";
                    background200m.SetActive(true);
                    break;
                case 2:
                    record300m.text = $"300m : {timeString}";
                    background300m.SetActive(true);
                    break;
                case 3:
                    record400m.text = $"400m : {timeString}";
                    background400m.SetActive(true);
                    break;
                case 4:
                    record500m.text = $"500m : {timeString}";
                    background500m.SetActive(true);
                    FinishRace();
                    break;
            }

            Debug.Log($"✅ {milestones[nextMilestoneIndex]}m 기록: {timeString}");
            nextMilestoneIndex++;
        }
    }

    private void FinishRace()
    {
        isRaceFinished = true;

        if (countdownText != null)
        {
            countdownText.gameObject.SetActive(true);
            countdownText.text = "FINISH!";
        }
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
        if (distanceText != null) distanceText.text = $"{currentDistance:F1} m";
        if (speedText != null) speedText.text = $"{speedKmh:F0} km/h";
    }

    // ★ [수정] 초기화 시 모든 기록 @@@로 표시 + 배경 끄기
    private void InitializeUI()
    {
        if (timeText != null) timeText.text = "00:00.0";
        if (distanceText != null) distanceText.text = "0.0 m";
        if (speedText != null) speedText.text = "0 km/h";

        // 기록 초기화 (@@@ 표시)
        if (record100m != null) record100m.text = "@@@";
        if (record200m != null) record200m.text = "@@@";
        if (record300m != null) record300m.text = "@@@";
        if (record400m != null) record400m.text = "@@@";
        if (record500m != null) record500m.text = "@@@";

        // ★ [추가] 배경 이미지 전부 끄기 (시작 시 숨김)
        if (background100m != null) background100m.SetActive(false);
        if (background200m != null) background200m.SetActive(false);
        if (background300m != null) background300m.SetActive(false);
        if (background400m != null) background400m.SetActive(false);
        if (background500m != null) background500m.SetActive(false);
    }

    private string FormatTime(float time)
    {
        return string.Format("{0:F1}s", time); // ← 이렇게 되어있어야 함!
    }
}