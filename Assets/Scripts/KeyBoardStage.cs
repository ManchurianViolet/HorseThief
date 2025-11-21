using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class KeyBoardStage : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField] private TextMeshPro playerTyping;      // 플레이어가 입력 중인 텍스트
    [SerializeField] private TextMeshPro successMessage;    // "Next Word in..." 메시지 표시용
    [SerializeField] private TextMeshPro title;             // 타이틀 (필요 없다면 비활성)
    [SerializeField] private TextMeshProUGUI question;      // 문제(PassWord) 표시
    [SerializeField] private TextMeshProUGUI timerDisplay;  // 경과 시간 표시 UI

    // [삭제됨] nextStagePortal, endStage (더 이상 사용 안 함)

    [Header("Input Settings")]
    public GameObject[] Keys; // 가상 키보드 버튼들
    [SerializeField] private AudioSource keyAudioSource;
    [SerializeField] private AudioClip keyPressSound;

    // 내부 상태 변수
    private bool isSuccess = false;     // 정답을 맞췄는지 (맞추면 대기 상태)
    private float currentTime = 0f;     // 경과 시간 (0초부터 시작)
    private Coroutine timerCoroutine;   // 타이머 코루틴 참조
    private Coroutine blinkCoroutine;   // 커서 코루틴 참조

    private string inputText = "";      // 입력된 텍스트
    private bool isCursorVisible = true;
    private string cursorChar = "|";
    private float blinkInterval = 0.5f;
    private bool isShiftPressed = false;

    // 단어 리스트
    private List<string> targetPhrases = new List<string>()
    {
        "a", "y", "b"
    };
    private string currentTargetPhrase;

    void Start()
    {
        // 오디오 소스 초기화
        if (keyAudioSource == null)
        {
            keyAudioSource = GetComponent<AudioSource>();
            if (keyAudioSource == null) keyAudioSource = gameObject.AddComponent<AudioSource>();
        }

        // 초기 메시지 숨김
        if (successMessage != null) successMessage.gameObject.SetActive(false);

        // 커서 시작
        blinkCoroutine = StartCoroutine(BlinkCursor());

        // ★ 첫 라운드 시작
        StartRound();
    }

    void Update()
    {
        // ★ R키로 강제 재시작 (디버깅용, 필요 시 유지)
        if (Input.GetKeyDown(KeyCode.R))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        // 정답 체크 (성공 상태가 아닐 때만)
        if (!isSuccess && inputText == currentTargetPhrase)
        {
            HandleSuccess(); // 정답 처리 함수 호출
        }
    }

    // ====================================================
    // ★ 핵심 로직: 라운드 시작 및 타이머 관리
    // ====================================================

    private void StartRound()
    {
        // 1. 변수 초기화
        isSuccess = false;          // 입력 가능 상태로 변경
        currentTime = 0f;           // 시간 0초로 리셋
        inputText = "";             // 입력 텍스트 초기화
        isShiftPressed = false;     // 쉬프트 초기화

        // 2. 새 단어 선택
        SetRandomTargetPhrase();

        // 3. UI 업데이트
        UpdateTextDisplay();
        UpdateTimerDisplay();
        if (successMessage != null) successMessage.gameObject.SetActive(false);

        // 4. 타이머 코루틴 시작 (스톱워치)
        if (timerCoroutine != null) StopCoroutine(timerCoroutine);
        timerCoroutine = StartCoroutine(RunStopwatch());
    }

    private void SetRandomTargetPhrase()
    {
        if (targetPhrases.Count > 0)
        {
            int randomIndex = Random.Range(0, targetPhrases.Count);
            currentTargetPhrase = targetPhrases[randomIndex];

            // 대소문자 구분을 명확히 하기 위해 표시는 그대로, 비교도 그대로
            if (question != null) question.text = $"PassWord: {currentTargetPhrase}";
        }
    }

    // ★ 스톱워치 코루틴 (0초부터 증가)
    private IEnumerator RunStopwatch()
    {
        while (!isSuccess) // 정답을 맞추기 전까지 계속 돔
        {
            currentTime += Time.deltaTime; // 시간 증가
            UpdateTimerDisplay();
            yield return null;
        }
        // 정답을 맞추면 while문 탈출 -> 타이머 자동 정지
    }

    private void UpdateTimerDisplay()
    {
        if (timerDisplay != null)
        {
            // 소수점 한 자리까지 표시 (예: Time: 12.5)
            timerDisplay.text = $"Time: {currentTime:F1}";
            timerDisplay.color = Color.white; // 색상은 항상 흰색 (경고 필요 없음)
        }
    }

    // ====================================================
    // ★ 정답 처리 및 다음 라운드 대기
    // ====================================================

    private void HandleSuccess()
    {
        isSuccess = true; // 이 시점부터 AddCharacter 입력이 막힘 & 타이머 while문 종료

        // 성공 효과음 (있다면)
        // if (keyAudioSource != null) keyAudioSource.PlayOneShot(successClip);

        // 다음 라운드 카운트다운 시작
        StartCoroutine(PrepareNextRound());
    }

    private IEnumerator PrepareNextRound()
    {
        // 메시지 활성화
        if (successMessage != null)
        {
            successMessage.gameObject.SetActive(true);
            successMessage.color = Color.green;
        }

        // 5초 카운트다운
        int countdown = 5;
        while (countdown > 0)
        {
            if (successMessage != null)
                successMessage.text = $"Next Word in {countdown}...";

            yield return new WaitForSeconds(1f);
            countdown--;
        }

        // 카운트다운 끝 -> 다음 라운드 즉시 시작
        StartRound();
    }

    // ====================================================
    // ★ 입력 처리 (기존 로직 유지)
    // ====================================================

    public void AddCharacter(string character)
    {
        // 성공 상태(대기 시간)에는 입력 무시
        if (isSuccess) return;

        if (playerTyping != null)
        {
            if (character == "Space")
            {
                inputText += " ";
            }
            else if (character == "BackSpace")
            {
                if (inputText.Length > 0)
                {
                    inputText = inputText.Substring(0, inputText.Length - 1);
                }
            }
            else
            {
                string charToAdd = character;
                if (isShiftPressed)
                {
                    charToAdd = character.ToUpper();
                    isShiftPressed = false; // 한 글자 쓰고 시프트 해제
                }
                inputText += charToAdd;
            }

            PlayKeyPressSound(); // 키 소리 재생
            UpdateTextDisplay();
        }
    }

    private void UpdateTextDisplay()
    {
        if (playerTyping != null)
        {
            playerTyping.text = inputText + (isCursorVisible ? cursorChar : "");
        }
    }

    private IEnumerator BlinkCursor()
    {
        while (true)
        {
            isCursorVisible = !isCursorVisible;
            UpdateTextDisplay();
            yield return new WaitForSeconds(blinkInterval);
        }
    }

    public void ToggleShift()
    {
        if (isSuccess) return;
        isShiftPressed = !isShiftPressed;
    }

    public void PlayKeyPressSound()
    {
        if (keyAudioSource != null && keyPressSound != null)
        {
            keyAudioSource.PlayOneShot(keyPressSound);
        }
    }
}