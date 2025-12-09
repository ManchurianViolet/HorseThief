using UnityEngine;
using TMPro;
using UnityEngine.Events; // 문 열기 이벤트용

public class MuseumHacking : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TMP_Text targetPwUI;     // 오른쪽 위 목표 비번
    [SerializeField] private TMP_Text laptopScreenUI; // 노트북 화면

    [Header("Settings")]
    public string password = "HORSE"; // 정답 (나중에 GameManager에서 랜덤 생성 가능)

    [Header("Door Event")]
    public UnityEvent onHackSuccess; // 성공하면 실행할 기능 (문 열기 등)

    private string currentInput = "";
    private bool isHacked = false;

    void Start()
    {
        // 시작할 때 오른쪽 위 UI에 목표 비번 띄우기
        if (targetPwUI != null) targetPwUI.text = $"PASSWORD: {password}";
        UpdateLaptopScreen();
    }

    // Key.cs에서 호출할 함수
    public void AddCharacter(string key)
    {
        if (isHacked) return;

        if (key == "BackSpace")
        {
            if (currentInput.Length > 0)
                currentInput = currentInput.Substring(0, currentInput.Length - 1);
        }
        else if (key == "Enter")
        {
            CheckPassword();
        }
        else // 일반 문자 입력
        {
            if (currentInput.Length < password.Length) // 정답 길이까지만 입력받음
            {
                currentInput += key;
            }
        }
        UpdateLaptopScreen();

        // (선택) 엔터 안 쳐도 다 맞으면 바로 열리게 하려면 아래 주석 해제
        // CheckPassword(); 
    }

    private void CheckPassword()
    {
        if (currentInput == password)
        {
            isHacked = true;
            Debug.Log("해킹 성공! 문이 열립니다.");
            if (laptopScreenUI != null) laptopScreenUI.text = "ACCESS GRANTED";

            // ★ 문 여는 기능 실행
            onHackSuccess.Invoke();
        }
        else
        {
            // 틀렸을 때 깜빡이거나 소리 나는 연출 추가 가능
        }
    }

    private void UpdateLaptopScreen()
    {
        if (laptopScreenUI != null) laptopScreenUI.text = currentInput;
    }
}