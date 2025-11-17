using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic; // List를 사용하기 위해 추가
using UnityEngine.SceneManagement;

public class KeyBoardStage : MonoBehaviour
{
    [SerializeField] private TextMeshPro playerTyping;
    [SerializeField] private TextMeshPro successMessage;
    [SerializeField] private TextMeshPro title;
    [SerializeField] private TextMeshPro question;
    [SerializeField] private TextMeshPro endStage;
    [SerializeField] private GameObject nextStagePortal;
    public GameObject[] Keys; // A~Z + Space + Backspace
    [SerializeField] private bool isSuccess = false; // 성공 여부

    [Header("Sound Settings")]
    [SerializeField] private AudioSource keyAudioSource; // AudioSource 컴포넌트 (씬에서 연결)
    [SerializeField] private AudioClip keyPressSound;    // 재생할 소리 클립 (인스펙터에서 연결)

    [Header("Timer Settings")]
    public float timeLimit = 30f; // 인스펙터에서 설정 가능한 제한 시간
    [SerializeField] private TextMeshProUGUI timerDisplay; // 남은 시간을 표시할 UI
    private float currentTime;
    private Coroutine timerCoroutine;

    private string inputText = ""; // 실제 입력 텍스트 (커서 제외)
    private bool isCursorVisible = true; // 커서 표시 여부
    private string cursorChar = "|"; // 커서 문자
    private float blinkInterval = 0.5f; // 깜빡임 간격 (초)

    private Coroutine blinkCoroutine; // 커서 깜빡임 코루틴 참조
    private bool isShiftPressed = false;

    private List<string> targetPhrases = new List<string>()
    {
        "am gemini",
        "un",
        "ty",
        "he",
        "rand",
        "cs",
        "ga",
        "vi",
        "su",
        "p"
    };
    private string currentTargetPhrase; // 현재 스테이지의 정답 문장

    void Start()
    {
        // 1. 커서 깜빡임 시작
        blinkCoroutine = StartCoroutine(BlinkCursor());

        // ★★★ 타이머 초기화 및 시작
        currentTime = timeLimit;
        UpdateTimerDisplay();
        timerCoroutine = StartCoroutine(StartTimer());

        // 2. 랜덤 정답 문장 설정 및 UI 업데이트
        SetRandomTargetPhrase();

        if (keyAudioSource == null)
        {
            keyAudioSource = GetComponent<AudioSource>();
            if (keyAudioSource == null)
            {
                // 없으면 자동으로 추가
                keyAudioSource = gameObject.AddComponent<AudioSource>();
            }
        }

        successMessage.gameObject.SetActive(false);
        endStage.gameObject.SetActive(false);
        nextStagePortal.SetActive(false); // 포탈 비활성화
        UpdateTextDisplay();
    }

    private void SetRandomTargetPhrase()
    {
        // 리스트에서 무작위로 문장을 선택합니다.
        int randomIndex = Random.Range(0, targetPhrases.Count);
        currentTargetPhrase = targetPhrases[randomIndex];

        // question 텍스트를 업데이트합니다.
        question.text = $"Type: {currentTargetPhrase}";
    }

    private void Update()
    {
        // 성공 메시지 표시
        if (!isSuccess && inputText == currentTargetPhrase)
        {
            isSuccess = true;
            OnSuccess();
        }
        // 2. ★ R 키 입력 확인 (항상 가능)
        if (Input.GetKeyDown(KeyCode.R))
        {
            RestartLevel();
        }
    }
    // ★★★ 재시작 메서드 (Update()에서 호출)
    private void RestartLevel()
    {
        // 현재 활성화된 씬을 다시 로드합니다.
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    public void AddCharacter(string character)
    {
        // 성공 상태에서는 입력 무시
        if (isSuccess) return;

        if (playerTyping != null)
        {
            if (character == "Space")
            {
                inputText += " "; // 공백 추가
            }
            else if (character == "BackSpace")
            {
                if (inputText.Length > 0)
                {
                    inputText = inputText.Substring(0, inputText.Length - 1); // 마지막 글자 삭제
                }
            }
            else
            {
                string charToAdd = character;

                // Shift가 눌려 있다면 문자를 대문자로 변환합니다.
                if (isShiftPressed)
                {
                    charToAdd = character.ToUpper(); // 대문자로 변환
                    isShiftPressed = false;
                }

                inputText += charToAdd; // 변환된 글자 추가
            }
            UpdateTextDisplay(); // 텍스트와 커서 업데이트
        }
        else
        {
            Debug.LogError("playerTyping이 연결되지 않았습니다!");
        }
    }


    private void OnSuccess()
    {
        // 커서 깜빡임 중지
        if (blinkCoroutine != null)
        {
            StopCoroutine(blinkCoroutine);
            isCursorVisible = false;
            UpdateTextDisplay(); // 커서 제거된 최종 텍스트 표시
        }
        if (timerCoroutine != null)
        {
            StopCoroutine(timerCoroutine);
        }
        // 성공 메시지 표시
        successMessage.gameObject.SetActive(true);

        // 키보드 입력 비활성화 (예: 키 오브젝트 비활성화)
        foreach (GameObject key in Keys)
        {
            key.SetActive(false);
        }

        // 특별한 코루틴 실행
        StartCoroutine(SpecialCoroutine());
    }

    private void UpdateTextDisplay()
    {
        // 커서 포함한 텍스트 표시
        playerTyping.text = inputText + (isCursorVisible ? cursorChar : "");
    }

    private IEnumerator BlinkCursor()
    {
        while (true)
        {
            isCursorVisible = !isCursorVisible; // 커서 토글
            UpdateTextDisplay();
            yield return new WaitForSeconds(blinkInterval);
        }
    }
    public void ToggleShift()
    {
        // 성공 상태에서는 Shift 토글 무시
        if (isSuccess) return;

        isShiftPressed = !isShiftPressed;
        Debug.Log($"Shift Toggled. isShiftPressed: {isShiftPressed}");

        // 여기에 Shift 상태에 따라 키보드 비주얼을 업데이트하는 로직을 추가할 수 있습니다.
        // 예: 대문자/소문자 키 텍스트 변경 또는 Shift 키 색상 변경
    }
    private IEnumerator SpecialCoroutine()
    {
        Debug.Log("특별한 코루틴이 실행되었습니다! 스테이지 전환 대기 중...");

        yield return new WaitForSeconds(5f);

        successMessage.gameObject.SetActive(false); // 성공 메시지 숨기기
        playerTyping.gameObject.SetActive(false); // 입력 텍스트 숨기기
        title.gameObject.SetActive(false); // 타이틀 숨기기
        question.gameObject.SetActive(false); // 질문 숨기기
        endStage.gameObject.SetActive(true); // 종료 메시지 표시
        nextStagePortal.SetActive(true); // 포탈 활성화
    }
    public void PlayKeyPressSound()
    {
        if (keyAudioSource != null && keyPressSound != null)
        {
            // 한 번 재생되는 소리 (겹쳐서 재생 가능)
            keyAudioSource.PlayOneShot(keyPressSound);
        }
        else
        {
            // Debug.LogWarning("Key Audio Source 또는 Clip이 설정되지 않았습니다!");
        }
    }
    // ★★★ 타이머 UI 업데이트 메서드
    private void UpdateTimerDisplay()
    {
        // 남은 시간을 소수점 한 자리 초로 표시
        timerDisplay.text = $"Time: {currentTime:F1}";

        // 시간이 5초 이하로 남으면 UI 색상을 빨간색으로 변경
        if (currentTime <= 5f)
        {
            timerDisplay.color = Color.red;
        }
        else
        {
            timerDisplay.color = Color.white; // 기본 색상으로 유지
        }
    }

    // ★★★ 타이머 코루틴 (핵심)
    private IEnumerator StartTimer()
    {
        while (currentTime > 0 && !isSuccess)
        {
            currentTime -= Time.deltaTime; // 시간 감소
            UpdateTimerDisplay();
            yield return null; // 다음 프레임까지 대기
        }

        // 루프 종료 시점 확인
        if (currentTime <= 0 && !isSuccess)
        {
            // 시간 초과 실패
            currentTime = 0; // 시간 0으로 고정
            UpdateTimerDisplay();
            OnFailure(); // 실패 처리
        }
    }
    // ★★★ 시간 초과 시 실패 처리 메서드 (자동 재시작 제거)
    private void OnFailure()
    {
        // 1. 모든 코루틴 중지
        if (blinkCoroutine != null) StopCoroutine(blinkCoroutine);

        // 2. 키보드 입력 비활성화
        foreach (GameObject key in Keys)
        {
            if (key != null) key.SetActive(false);
        }

        // 3. 실패 메시지 표시
        if (endStage != null)
        {
            endStage.color = Color.red;
            endStage.text = "Test Failed"; // ★ 메시지 변경
            endStage.gameObject.SetActive(true);
        }

        // 4. 나머지 UI 숨기기
        playerTyping.gameObject.SetActive(false);
        title.gameObject.SetActive(false);
        question.gameObject.SetActive(false);

        if (timerDisplay != null)
        {
            timerDisplay.gameObject.SetActive(false);
        }

        // 5. ★ 자동 재시작 로직 제거 완료
        // 이제 R키를 누를 때까지 이 상태가 유지됩니다.
    }

    private IEnumerator RestartLevelAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        // 현재 씬을 다시 로드하여 레벨 재시작
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}