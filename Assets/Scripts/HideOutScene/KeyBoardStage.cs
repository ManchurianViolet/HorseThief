using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;
// SceneManager는 더 이상 필요 없으므로 삭제하거나 유지해도 됨

public class KeyBoardStage : MonoBehaviour
{
    [Header("Training Settings")]
    [SerializeField] private Transform playPosition; // 방 안의 박스 위치 (순간이동 타겟)
    [SerializeField] private GameObject gameUIPanel; // 텍스트, 타이머 등을 묶은 부모 오브젝트 (평소엔 꺼둠)
    [SerializeField] private Transform getoutPosition; // 방 나갔을때 박스 위치
    private GameObject currentPlayer;

    [Header("UI Components")]
    [SerializeField] private TextMeshProUGUI playerTyping;
    [SerializeField] private TextMeshProUGUI successMessage;
    // title은 굳이 제어 안 해도 되지만, 훈련 시작 시 켜고 싶다면 추가
    [SerializeField] private TextMeshProUGUI question;
    [SerializeField] private TextMeshProUGUI timerDisplay;

    [Header("Input Settings")]
    public GameObject[] Keys;
    [SerializeField] private AudioSource keyAudioSource;
    [SerializeField] private AudioClip keyPressSound;

    // 내부 상태 변수
    private bool isTrainingActive = false; // ★ 현재 훈련 중인가?
    private bool isSuccess = false;
    private float currentTime = 0f;
    private Coroutine timerCoroutine;
    private Coroutine blinkCoroutine;

    private string inputText = "";
    private bool isCursorVisible = true;
    private string cursorChar = "|";
    private float blinkInterval = 0.5f;
    private bool isShiftPressed = false;

    // 단어 리스트
    private List<string> targetPhrases = new List<string>() { "PARIS123", "MONALISA", "LOUVRE", "GALLERY" };
    private string currentTargetPhrase;

    void Start()
    {
        // 오디오 초기화
        if (keyAudioSource == null)
        {
            keyAudioSource = GetComponent<AudioSource>();
            if (keyAudioSource == null) keyAudioSource = gameObject.AddComponent<AudioSource>();
        }

        // ★ [변경점] 시작하자마자 게임을 켜지 않음!
        // 대신 UI를 숨기고 대기 상태로 만듭니다.
        if (gameUIPanel != null) gameUIPanel.SetActive(false);
        if (successMessage != null) successMessage.gameObject.SetActive(false);

        // 커서는 미리 깜빡거려도 상관없지만, 훈련 때만 켜는 게 자연스러움
        // 여기서는 일단 멈춰둠
    }

    void Update()
    {
        // ★ 훈련 중이 아니면 아무것도 하지 않음 (키 입력도 안 받음)
        if (!isTrainingActive) return;

        // 정답 체크
        if (!isSuccess && inputText == currentTargetPhrase)
        {
            HandleSuccess();
        }

        // (선택사항) ESC 누르면 훈련 포기하고 나가는 기능 추가 가능
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            StopTraining();
        }
    }

    // ====================================================
    // ★ [핵심] 외부(트리거)에서 호출할 시작 함수
    // ====================================================
    public void StartTraining(GameObject player)
    {
        isTrainingActive = true;
        currentPlayer = player; // ★ 플레이어 정보를 저장해둡니다.
        // 1. 플레이어 순간이동 (방 안의 박스 위치로)
        if (player != null && playPosition != null)
        {
            // 리지드바디가 있다면 물리력을 잠시 꺼야 안전하게 이동됨
            Rigidbody rb = player.GetComponent<Rigidbody>();
            if (rb != null) rb.isKinematic = true;

            player.transform.position = playPosition.position;
            player.transform.rotation = playPosition.rotation; // 방향도 맞춰줌

            if (rb != null) rb.isKinematic = false;
        }

        // 2. UI 켜기
        if (gameUIPanel != null) gameUIPanel.SetActive(true);

        // 3. 커서 코루틴 시작
        if (blinkCoroutine != null) StopCoroutine(blinkCoroutine);
        blinkCoroutine = StartCoroutine(BlinkCursor());

        // 4. 게임 로직 시작 (기존 StartRound 호출)
        StartRound();
    }

    // 훈련 강제 종료 (나가기)
    public void StopTraining()
    {
        isTrainingActive = false;

        if (gameUIPanel != null) gameUIPanel.SetActive(false);
        if (timerCoroutine != null) StopCoroutine(timerCoroutine);

        // ★ 플레이어를 문 밖(getoutPosition)으로 이동시키는 로직
        if (currentPlayer != null && getoutPosition != null)
        {
            Rigidbody rb = currentPlayer.GetComponent<Rigidbody>();

            // 순간이동 시 물리 충돌 방지를 위해 Kinematic 켬
            if (rb != null) rb.isKinematic = true;

            // 위치와 회전 이동
            currentPlayer.transform.position = getoutPosition.position;
            Quaternion exitRotation = getoutPosition.rotation * Quaternion.Euler(0, 90, 0);
            currentPlayer.transform.rotation = exitRotation;
            // ★ [수정 2] 머리 초기화 호출
            HorseControl horseControl = currentPlayer.GetComponent<HorseControl>();
            if (horseControl != null)
            {
                horseControl.ResetHeadPosition();
            }
            // 물리 다시 원상복구
            if (rb != null) rb.isKinematic = false;

            // 훈련이 끝났으니 플레이어 참조 해제
            currentPlayer = null;
        }
    }

    // ====================================================
    // 기존 로직 (StartRound, Timer 등)
    // ====================================================

    private void StartRound()
    {
        if (!isTrainingActive) return; // 안전장치

        isSuccess = false;
        currentTime = 0f;
        inputText = "";
        isShiftPressed = false;

        SetRandomTargetPhrase();
        UpdateTextDisplay();
        UpdateTimerDisplay();
        if (successMessage != null) successMessage.gameObject.SetActive(false);

        if (timerCoroutine != null) StopCoroutine(timerCoroutine);
        timerCoroutine = StartCoroutine(RunStopwatch());
    }

    private void SetRandomTargetPhrase()
    {
        if (targetPhrases.Count > 0)
        {
            int randomIndex = Random.Range(0, targetPhrases.Count);
            currentTargetPhrase = targetPhrases[randomIndex];
            if (question != null) question.text = $"PassWord: {currentTargetPhrase}";
        }
    }

    private IEnumerator RunStopwatch()
    {
        while (!isSuccess && isTrainingActive) // 훈련 중일 때만
        {
            currentTime += Time.deltaTime;
            UpdateTimerDisplay();
            yield return null;
        }
    }

    private void UpdateTimerDisplay()
    {
        if (timerDisplay != null) timerDisplay.text = $"Time: {currentTime:F1}";
    }

    private void HandleSuccess()
    {
        isSuccess = true;
        StartCoroutine(PrepareNextRound());
    }

    private IEnumerator PrepareNextRound()
    {
        if (successMessage != null)
        {
            successMessage.gameObject.SetActive(true);
            successMessage.color = Color.green;
        }

        int countdown = 5;
        while (countdown > 0)
        {
            if (!isTrainingActive) yield break; // 훈련 종료 시 코루틴 중단

            if (successMessage != null)
                successMessage.text = $"Next Word in {countdown}...";

            yield return new WaitForSeconds(1f);
            countdown--;
        }

        StartRound();
    }

    // 입력 처리 (isTrainingActive 체크 추가)
    public void AddCharacter(string character)
    {
        if (!isTrainingActive || isSuccess) return;

        if (playerTyping != null)
        {
            // Key.cs에서 대문자로 보내므로 대문자로 검사
            if (character == "SPACE")
            {
                inputText += " ";
            }
            else if (character == "BACKSPACE")
            {
                if (inputText.Length > 0) inputText = inputText.Substring(0, inputText.Length - 1);
            }
            else
            {
                // 이미 대문자로 들어오므로 그대로 추가
                inputText += character;
            }

            PlayKeyPressSound();
            UpdateTextDisplay();
        }
    }

    private void UpdateTextDisplay()
    {
        if (playerTyping != null)
            playerTyping.text = inputText + (isCursorVisible ? cursorChar : "");
    }

    private IEnumerator BlinkCursor()
    {
        while (isTrainingActive)
        {
            isCursorVisible = !isCursorVisible;
            UpdateTextDisplay();
            yield return new WaitForSeconds(blinkInterval);
        }
    }

    public void ToggleShift()
    {
        if (!isTrainingActive || isSuccess) return;
        isShiftPressed = !isShiftPressed;
    }

    public void PlayKeyPressSound()
    {
        if (keyAudioSource != null && keyPressSound != null)
            keyAudioSource.PlayOneShot(keyPressSound);
    }
}