using UnityEngine;
using TMPro;
using System.Collections;

public class CountdownManager : MonoBehaviour
{
    [Header("Basic UI, Audio, Trigger")]
    [SerializeField] private TextMeshProUGUI countdownText; // 카운트다운 UI 텍스트
    [SerializeField] private AudioSource audioSource; // 메인 사운드 소스 (비프음, 총성 등)
    [SerializeField] private AudioSource audioSourceFireworks; // 불꽃놀이 사운드 소스
    [SerializeField] private AudioClip beepSound; // 카운트다운 경고음 클립
    [SerializeField] private AudioClip gunSound; // 레이스 시작 총성 클립
    [SerializeField] private AudioClip successSound; // 승리 사운드 클립
    [SerializeField] private AudioClip failureSound; // 패배/실격 사운드 클립
    [SerializeField] private AudioClip fireworksSound; // 불꽃놀이 사운드 클립
    [SerializeField] private EndTrigger endTrigger; // 종료 트리거 컴포넌트 참조
    [SerializeField] private ParticleSystem startParticle; // 시작 시 파티클
    [SerializeField] private ParticleSystem endParticle; // 종료 시 파티클

    [Header("Horse Objects")]
    [SerializeField] private HorseControl_RacingStage playerHorseMain; // 플레이어 말 컨트롤 스크립트
    [SerializeField] private RivalHorseMovement rivalHorse1; // 라이벌 말 1
    [SerializeField] private RivalHorseMovement rivalHorse2; // 라이벌 말 2
    [SerializeField] private RivalHorseMovement rivalHorse3; // 라이벌 말 3

    [Header("Countdown Light Objects")]
    [SerializeField] private GameObject light1; // 신호등 1
    [SerializeField] private GameObject light2; // 신호등 2
    [SerializeField] private GameObject light3; // 신호등 3
    [SerializeField] private Material[] lightMat; // 신호등 머티리얼 배열 (0:Off, 1:Ready, 2:Go)

    private bool isRaceFinished = false; // 레이스 종료 상태 플래그

    private void Start()
    {
        // 1. 종료 트리거 이벤트 리스너 등록
        if (endTrigger != null)
        {
            endTrigger.onTriggerEnter.AddListener(OnEndTriggerEnter);
        }

        // 2. 플레이어 말의 실격 이벤트 구독
        if (playerHorseMain != null)
        {
            playerHorseMain.OnFalseStart += HandleFalseStart;
        }

        // 3. 카운트다운 시작
        StartCoroutine(Countdown());
    }

    // 🔔 실격(False Start) 처리 함수
    private void HandleFalseStart()
    {
        if (isRaceFinished) return; // 이미 종료된 레이스는 무시

        StopAllCoroutines(); // 진행 중인 카운트다운 코루틴 정지

        if (countdownText != null) countdownText.gameObject.SetActive(false); // 카운트다운 텍스트 숨김

        isRaceFinished = true;

        if (audioSource != null && failureSound != null)
        {
            audioSource.PlayOneShot(failureSound); // 실패 사운드 재생
        }

        DisableAllHorseMovement(); // 모든 말의 움직임 정지
        // 실격 시 Failure 텍스트는 SuccessTextTyping과 유사한 스크립트를 통해 처리될 수 있습니다.
    }

    // 🔔 말이 실격/도착했을 때 모든 말의 움직임을 멈추는 함수
    private void DisableAllHorseMovement()
    {
        // 모든 말의 isCountdownEnd 플래그를 false로 설정하여 움직임 정지
        if (playerHorseMain != null) playerHorseMain.isCountdownEnd = false;
        if (rivalHorse1 != null) rivalHorse1.isCountdownEnd = false;
        if (rivalHorse2 != null) rivalHorse2.isCountdownEnd = false;
        if (rivalHorse3 != null) rivalHorse3.isCountdownEnd = false;
    }

    // 종료 트리거 진입 시 호출되는 함수
    private void OnEndTriggerEnter(Collider _other)
    {
        if (isRaceFinished) return; // 이미 레이스가 종료되었으면 무시

        // 태그 확인 (어떤 말이 결승선에 도착했는지)
        if (_other.CompareTag("HorseChest"))
        {
            // 플레이어 말이 먼저 통과 -> 승리 처리
            Debug.Log(">>> [WIN CHECK] Player HorseChest detected first! Calling HandleRaceResult(true).");
            HandleRaceResult(true);
        }
        else if (_other.CompareTag("RivalHorseChest"))
        {
            // 라이벌 말이 먼저 통과 -> 패배 처리
            HandleRaceResult(false);
        }
    }

    // 레이스 결과 최종 처리 함수
    private void HandleRaceResult(bool isSuccess)
    {
        isRaceFinished = true;

        // 모든 말 이동 정지 (HandleRaceResult 호출 직후에 호출하는 것이 안정적)
        DisableAllHorseMovement();

        // 승리 또는 패배 시 처리
        if (isSuccess) // 승리 시
        {
            // 승리 텍스트 활성화 (SuccessTextTyping 스크립트가 붙어있음)

            // 승리 파티클 생성
            Instantiate(endParticle, new Vector3(-30, 7, -3), Quaternion.Euler(new Vector3(270, 0, 0)));
            Instantiate(endParticle, new Vector3(-35, 7, -13), Quaternion.Euler(new Vector3(270, 0, 0)));
            Instantiate(endParticle, new Vector3(-35, 7, 7), Quaternion.Euler(new Vector3(270, 0, 0)));
            Instantiate(endParticle, new Vector3(-40, 7, -6), Quaternion.Euler(new Vector3(270, 0, 0)));
            Instantiate(endParticle, new Vector3(-40, 7, 0), Quaternion.Euler(new Vector3(270, 0, 0)));

            // 승리 사운드 재생
            audioSource.PlayOneShot(successSound);
            audioSourceFireworks.PlayOneShot(fireworksSound);
        }
        else // 패배 시 (라이벌 승리)
        {
            audioSource.PlayOneShot(failureSound); // 패배 사운드 재생
            // 패배 텍스트 활성화 로직이 추가될 수 있음
        }
    }


    private IEnumerator Countdown()
    {
        // 3초 카운트다운 시작
        // 3
        countdownText.text = "3";
        audioSource.PlayOneShot(beepSound);
        light1.GetComponent<Renderer>().material = lightMat[1]; // light1을 준비 상태(index 1)로 설정
        yield return new WaitForSeconds(1f);

        // 2
        countdownText.text = "2";
        audioSource.PlayOneShot(beepSound);
        light2.GetComponent<Renderer>().material = lightMat[1]; // light2를 준비 상태(index 1)로 설정
        yield return new WaitForSeconds(1f);

        // 1
        countdownText.text = "1";
        audioSource.PlayOneShot(beepSound);
        light3.GetComponent<Renderer>().material = lightMat[1]; // light3을 준비 상태(index 1)로 설정
        yield return new WaitForSeconds(1f);

        // 0 (GO!)
        countdownText.text = "GO!";
        if (audioSource != null && gunSound != null)
        {
            audioSource.PlayOneShot(gunSound); // 총성 사운드 재생
        }

        // 모든 말 움직임 시작 (isCountdownEnd를 true로 설정)
        playerHorseMain.isCountdownEnd = true;
        rivalHorse1.isCountdownEnd = true;
        rivalHorse2.isCountdownEnd = true;
        rivalHorse3.isCountdownEnd = true;

        // 모든 신호등을 GO 상태(index 2)로 설정
        light1.GetComponent<Renderer>().material = lightMat[2];
        light2.GetComponent<Renderer>().material = lightMat[2];
        light3.GetComponent<Renderer>().material = lightMat[2];

        // 시작 파티클 생성
        Instantiate(startParticle, new Vector3(14, 6, -3), Quaternion.Euler(new Vector3(-90, 0, 0)));
        yield return new WaitForSeconds(1f);

        // GO! 텍스트 숨김
        countdownText.gameObject.SetActive(false);
    }

    // 🔔 OnDestroy 시 이벤트 구독 해제
    private void OnDestroy()
    {
        if (playerHorseMain != null)
        {
            playerHorseMain.OnFalseStart -= HandleFalseStart;
        }
    }
}