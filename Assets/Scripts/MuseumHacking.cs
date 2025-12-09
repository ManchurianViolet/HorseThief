using UnityEngine;
using TMPro;
using UnityEngine.Events;
using System.Collections; // 코루틴용

public class MuseumHacking : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TMP_Text targetPwUI;
    [SerializeField] private TMP_Text laptopScreenUI;

    [Header("Settings")]
    public string password = "HORSE";

    [Header("Door Event")]
    public UnityEvent onHackSuccess;

    private string currentInput = "";
    private bool isHacked = false;

    // ★ 커서 깜빡임 관련 변수
    private bool isCursorVisible = true;
    private string cursorChar = "|";
    private float blinkInterval = 0.5f;
    private Coroutine blinkCoroutine;

    void Start()
    {
        if (targetPwUI != null) targetPwUI.text = $"PASSWORD: {password}";

        // 커서 깜빡임 시작
        if (blinkCoroutine != null) StopCoroutine(blinkCoroutine);
        blinkCoroutine = StartCoroutine(BlinkCursor());

        UpdateLaptopScreen();
    }

    // Key.cs에서 호출
    public void AddCharacter(string key)
    {
        if (isHacked) return;

        // ★ 대문자로 들어온 명령어 처리
        if (key == "BACKSPACE")
        {
            if (currentInput.Length > 0)
                currentInput = currentInput.Substring(0, currentInput.Length - 1);
        }
        else if (key == "ENTER")
        {
            CheckPassword();
        }
        else if (key == "SPACE") // ★ 스페이스바 추가
        {
            // 비밀번호에 공백이 포함될 수 있다면 추가 (보통은 막지만 요청하셔서 넣음)
            currentInput += " ";
        }
        else
        {
            // 일반 문자 (길이 1이고, 명령어 아님)
            if (key.Length == 1)
            {
                if (currentInput.Length < password.Length)
                {
                    currentInput += key;
                }
            }
        }
        UpdateLaptopScreen();
        CheckPassword();
    }

    private void CheckPassword()
    {
        if (currentInput == password)
        {
            isHacked = true;
            Debug.Log("해킹 성공! 문이 열립니다.");

            // 성공 시 커서 끄고 메시지 출력
            StopCoroutine(blinkCoroutine);
            if (laptopScreenUI != null) laptopScreenUI.text = "ACCESS GRANTED";

            onHackSuccess.Invoke();
        }
        else
        {
            // 틀렸을 때 로직 (필요시 추가)
        }
    }

    // 화면 갱신 (글자 + 커서)
    private void UpdateLaptopScreen()
    {
        if (laptopScreenUI == null) return;

        if (isHacked) return; // 해킹 성공했으면 갱신 안 함

        // 글자 뒤에 커서 붙이기
        laptopScreenUI.text = currentInput + (isCursorVisible ? cursorChar : "");
    }

    // ★ 깜빡이는 커서 코루틴
    private IEnumerator BlinkCursor()
    {
        while (!isHacked)
        {
            isCursorVisible = !isCursorVisible;
            UpdateLaptopScreen(); // 깜빡일 때마다 화면 다시 그리기
            yield return new WaitForSeconds(blinkInterval);
        }
    }
}