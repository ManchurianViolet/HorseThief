using UnityEngine;
using TMPro;
using System.Collections;

public class CountdownManager : MonoBehaviour
{
    [Header("=== Player & Rivals ===")]
    [SerializeField] private HorseControl playerHorse;
    [SerializeField] private RivalHorseMovement rivalHorse1;
    [SerializeField] private RivalHorseMovement rivalHorse2;
    [SerializeField] private RivalHorseMovement rivalHorse3;

    [Header("=== UI System ===")]
    [SerializeField] private TextMeshProUGUI countdownText;
    [SerializeField] private TextMeshProUGUI speedText;
    [SerializeField] private TextMeshProUGUI distanceText;
    [SerializeField] private TextMeshProUGUI timeText;
    [SerializeField] private TextMeshProUGUI recordBoardText;

    [Header("=== Audio & Visuals ===")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioSource audioSourceFireworks;
    [SerializeField] private AudioClip beepSound;
    [SerializeField] private AudioClip gunSound;
    [SerializeField] private AudioClip finishSound;
    [SerializeField] private AudioClip fireworksSound;

    [SerializeField] private ParticleSystem startParticle;
    [SerializeField] private ParticleSystem endParticle;

    [Header("=== Lights ===")]
    [SerializeField] private GameObject light1;
    [SerializeField] private GameObject light2;
    [SerializeField] private GameObject light3;
    [SerializeField] private Material[] lightMat;

    // --- 내부 로직 변수 ---
    private bool isRaceStarted = false;
    private bool isRaceFinished = false;
    private float raceTimer = 0f;
    private Vector3 startPosition;
    private float currentDistance = 0f;

    // 구간 기록 체크용 변수
    private int nextMilestoneIndex = 0;
    private readonly float[] milestones = { 100f, 200f, 300f, 400f, 500f };

    // [수정] 속도 부드럽게 만들기 위한 변수
    private Vector3 lastPosition;
    private float currentDisplaySpeed = 0f; // UI에 표시할 속도 (보정됨)

    void Start()
    {
        if (playerHorse != null)
        {
            playerHorse.enabled = false;
            startPosition = playerHorse.transform.position;
            lastPosition = startPosition;
        }

        InitializeUI();
        StartCoroutine(CountdownRoutine());
    }

    void Update()
    {
        if (!isRaceStarted || isRaceFinished || playerHorse == null) return;

        // --- 1. 시간 및 거리 계산 ---
        raceTimer += Time.deltaTime;
        Vector3 currentPos = playerHorse.transform.position;

        currentDistance = Vector3.Distance(startPosition, currentPos);

        // --- [핵심 수정] 2. 속도 계산 (스무딩 적용) ---
        // 순간 속도 계산 (m/s)
        float rawSpeedMPS = Vector3.Distance(lastPosition, currentPos) / Time.deltaTime;

        // 순간 속도가 튀는 것을 방지하기 위해 Lerp(선형 보간) 사용
        // 현재 표시 속도가 목표 속도(rawSpeed)를 천천히(Time.deltaTime * 3f) 따라가게 함
        currentDisplaySpeed = Mathf.Lerp(currentDisplaySpeed, rawSpeedMPS, Time.deltaTime * 3f);

        float speedKPH = currentDisplaySpeed * 3.6f; // km/h 변환

        // 움직임이 거의 없으면 0으로 고정 (미세 떨림 방지)
        if (speedKPH < 1f) speedKPH = 0f;

        lastPosition = currentPos;

        // --- 3. 실시간 UI 갱신 ---
        UpdateRealtimeUI(speedKPH);

        // --- 4. 구간 기록 체크 ---
        CheckMilestones();
    }

    private IEnumerator CountdownRoutine()
    {
        countdownText.gameObject.SetActive(true);

        yield return ShowCount("3", light1);
        yield return ShowCount("2", light2);
        yield return ShowCount("1", light3);

        countdownText.text = "GO!";
        if (audioSource != null && gunSound != null) audioSource.PlayOneShot(gunSound);

        SetLightMaterial(light1, 2);
        SetLightMaterial(light2, 2);
        SetLightMaterial(light3, 2);

        if (startParticle != null) Instantiate(startParticle, new Vector3(14, 6, -3), Quaternion.Euler(-90, 0, 0));

        if (playerHorse != null) playerHorse.enabled = true;

        StartRivals();

        isRaceStarted = true;

        yield return new WaitForSeconds(1f);
        countdownText.gameObject.SetActive(false);
    }

    private void CheckMilestones()
    {
        if (nextMilestoneIndex >= milestones.Length) return;

        if (currentDistance >= milestones[nextMilestoneIndex])
        {
            float splitTime = raceTimer;
            float targetDistance = milestones[nextMilestoneIndex];

            // [수정] 기록에도 소수점 한 자리 포맷 적용
            string recordLine = $"{targetDistance:F0}m : {FormatTime(splitTime)}";
            recordBoardText.text += recordLine + "\n";

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

        if (audioSource != null && finishSound != null) audioSource.PlayOneShot(finishSound);
        if (audioSourceFireworks != null && fireworksSound != null) audioSourceFireworks.PlayOneShot(fireworksSound);

        SpawnFireworks();

        countdownText.gameObject.SetActive(true);
        countdownText.text = "FINISH!";
    }

    private IEnumerator ShowCount(string text, GameObject lightObj)
    {
        countdownText.text = text;
        if (audioSource != null && beepSound != null) audioSource.PlayOneShot(beepSound);
        SetLightMaterial(lightObj, 1);
        yield return new WaitForSeconds(1f);
    }

    private void UpdateRealtimeUI(float speedKmh)
    {
        // 시간 표시 (포맷 변경됨)
        timeText.text = FormatTime(raceTimer);

        // 거리 표시
        distanceText.text = $"Dist: {currentDistance:F1} m";

        // 속도 표시 (정수)
        speedText.text = $"Speed: {speedKmh:F0} km/h";
    }

    private void InitializeUI()
    {
        timeText.text = "00:00.0";
        distanceText.text = "Dist: 0.0 m";
        speedText.text = "Speed: 0 km/h";
        recordBoardText.text = "";
    }

    // [수정] 시간 포맷 변경 함수
    private string FormatTime(float time)
    {
        int minutes = (int)(time / 60);
        float seconds = time % 60;

        // {1:04.1f} 설명:
        // 04 -> 최소 4자리 확보 (앞에 0 채움, 예: 09.5)
        // .1f -> 소수점 첫째 자리까지만 표시
        return string.Format("{0:00}:{1:04.1f}", minutes, seconds);
    }

    private void SetLightMaterial(GameObject lightObj, int matIndex)
    {
        if (lightObj != null && lightMat.Length > matIndex)
            lightObj.GetComponent<Renderer>().material = lightMat[matIndex];
    }

    private void StartRivals()
    {
        if (rivalHorse1 != null) rivalHorse1.isCountdownEnd = true;
        if (rivalHorse2 != null) rivalHorse2.isCountdownEnd = true;
        if (rivalHorse3 != null) rivalHorse3.isCountdownEnd = true;
    }

    private void SpawnFireworks()
    {
        if (endParticle != null)
        {
            Instantiate(endParticle, new Vector3(-30, 7, -3), Quaternion.Euler(270, 0, 0));
            Instantiate(endParticle, new Vector3(-35, 7, -13), Quaternion.Euler(270, 0, 0));
            Instantiate(endParticle, new Vector3(-35, 7, 7), Quaternion.Euler(270, 0, 0));
        }
    }
}