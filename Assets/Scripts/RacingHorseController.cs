using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class RacingHorseController : MonoBehaviour
{
    public bool isCountdownEnd = false; // 카운트 다운 끝날 시 true

    [SerializeField] private float racingSpeed = 100f;
    [SerializeField] private float buttonWaitTime = 0.2f;
    [SerializeField] private Button buttonQ;
    [SerializeField] private Button buttonE;
    [SerializeField] private Button buttonO;
    [SerializeField] private Button buttonP;


    private Rigidbody playerRb;
    private Vector3 forwardVector = new Vector3(-1, 0, 0);




    private void Awake()
    {
        playerRb = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        // isCountdownEnd가 false면 아래 로직 모두 무시
        if (!isCountdownEnd) return;

        // Q 키 입력
        if (Input.GetKeyDown(KeyCode.Q))
        {
            MovePlayer();
            TriggerButton(buttonQ);
        }

        // E 키 입력
        if (Input.GetKeyDown(KeyCode.E))
        {
            MovePlayer();
            TriggerButton(buttonE);
        }

        // O 키 입력
        if (Input.GetKeyDown(KeyCode.O))
        {
            MovePlayer();
            TriggerButton(buttonO);
        }

        // P 키 입력
        if (Input.GetKeyDown(KeyCode.P))
        {
            MovePlayer();
            TriggerButton(buttonP);
        }
    }
    private void MovePlayer()
    {
        // 캐릭터 이동
        playerRb.AddForce(forwardVector * racingSpeed, ForceMode.Impulse);
    }

    private void TriggerButton(Button _button)
    {
        // 버튼 할당 여부 체크
        if (_button == null)
        {
            Debug.LogWarning("버튼이 할당되지 않았습니다!");
            return;
        }

        // 버튼 클릭 이벤트 실행
        _button.onClick.Invoke();
        Debug.Log($"{_button.name} 버튼 클릭 이벤트 실행");

        // Pressed Color로 색상 변경
        StartCoroutine(ChangeButtonColor(_button));
    }

    private IEnumerator ChangeButtonColor(Button _button)
    {
        // 버튼의 ColorBlock 가져오기
        ColorBlock colors = _button.colors;
        Color originalColor = colors.normalColor;
        Color pressedColor = colors.pressedColor;

        // 버튼 이미지 색상을 PressedColor로 변경
        Image buttonImage = _button.GetComponent<Image>();
        if (buttonImage != null)
        {
            buttonImage.color = pressedColor;
        }

        // 일정 시간 대기
        yield return new WaitForSeconds(buttonWaitTime);

        // 대기 후, 원래 색상으로 복구
        if (buttonImage != null)
        {
            buttonImage.color = originalColor;
        }

    }
}
