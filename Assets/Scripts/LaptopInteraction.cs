using UnityEngine;

public class LaptopInteraction : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject laptopUIPanel;   // 노트북 화면 UI (전체 패널)
    [SerializeField] private GameObject interactionText; // "[F] 노트북 사용" 안내 텍스트

    // 내부 변수
    private bool isPlayerNear = false;
    private bool isLaptopOpen = false;
    private HorseControl playerControl; // 플레이어 움직임 제어용

    void Start()
    {
        // 시작 시 UI들 끄기
        if (laptopUIPanel != null) laptopUIPanel.SetActive(false);
        if (interactionText != null) interactionText.SetActive(false);
    }

    void Update()
    {
        // 1. 플레이어가 근처에 있고 + 노트북이 꺼져있을 때 + F키 누름 -> 켜기
        if (isPlayerNear && !isLaptopOpen && Input.GetKeyDown(KeyCode.F))
        {
            OpenLaptop();
        }

        // 2. 노트북이 켜져있을 때 + ESC키 누름 -> 끄기
        if (isLaptopOpen && Input.GetKeyDown(KeyCode.Escape))
        {
            CloseLaptop();
        }
    }

    private void OpenLaptop()
    {
        isLaptopOpen = true;

        // UI 켜기 / 안내 문구 끄기
        if (laptopUIPanel != null) laptopUIPanel.SetActive(true);
        if (interactionText != null) interactionText.SetActive(false);

        // 마우스 커서 보이기 & 잠금 해제 (클릭해야 하니까)
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        // 플레이어 움직임 멈추기 (선택사항: 뒤에서 조작 스크립트 끄기)
        if (playerControl != null) playerControl.enabled = false;

        // (선택) 물리 엔진 멈추기? -> 보통 UI 띄울 땐 Time.timeScale = 0 할 수도 있지만, 
        // 배경이 움직이는 게 좋으면 놔둬도 됨. 여기선 플레이어 조작만 막음.
    }

    public void CloseLaptop()
    {
        isLaptopOpen = false;

        // UI 끄기 / 안내 문구 다시 켜기 (아직 근처에 있으니까)
        if (laptopUIPanel != null) laptopUIPanel.SetActive(false);
        if (interactionText != null) interactionText.SetActive(true);

        // 마우스 커서 숨기기 (게임 플레이 모드로 복귀)
        // 만약 게임이 원래 마우스를 쓰는 게임이라면 이 줄은 삭제하세요.
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        // 플레이어 움직임 다시 허용
        if (playerControl != null) playerControl.enabled = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        // 태그 확인 (HorseChest)
        if (other.attachedRigidbody != null && other.attachedRigidbody.CompareTag("HorseChest"))
        {
            isPlayerNear = true;
            playerControl = other.attachedRigidbody.GetComponent<HorseControl>();

            // "[F] 노트북 사용" 안내 띄우기
            if (interactionText != null) interactionText.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.attachedRigidbody != null && other.attachedRigidbody.CompareTag("HorseChest"))
        {
            isPlayerNear = false;
            playerControl = null;

            // 멀어지면 강제로 노트북 닫기
            if (isLaptopOpen) CloseLaptop();

            // 안내 문구 끄기
            if (interactionText != null) interactionText.SetActive(false);
        }
    }
}