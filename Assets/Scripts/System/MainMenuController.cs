using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject titlePanel; // [HorSteal] Press Any Button 화면
    [SerializeField] private GameObject menuPanel;  // [Continue / New Game] 화면

    [Header("Buttons")]
    [SerializeField] private Button continueButton;
    [SerializeField] private Button newGameButton;
    [SerializeField] private Button exitButton;

    private bool isMenuActive = false;

    void Start()
    {
        // 1. 시작하면 타이틀만 켜고 메뉴는 끄기
        titlePanel.SetActive(true);
        menuPanel.SetActive(false);
        isMenuActive = false;

        // 2. 저장된 게임이 없으면 'Continue' 버튼 비활성화 (회색 처리)
        if (GameManager.Instance != null)
        {
            bool hasSave = GameManager.Instance.HasSaveData();
            continueButton.interactable = hasSave; // 파일 없으면 클릭 불가!

            // (선택) 파일 없으면 색깔도 흐릿하게
            if (!hasSave)
            {
                var colors = continueButton.colors;
                colors.disabledColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);
                continueButton.colors = colors;
            }
        }
    }

    void Update()
    {
        // 타이틀 화면에서 아무 키나 누르면 -> 메뉴 화면으로 전환
        if (!isMenuActive && Input.anyKeyDown)
        {
            ShowMenu();
        }
    }

    private void ShowMenu()
    {
        isMenuActive = true;
        titlePanel.SetActive(false); // 타이틀 끄고
        menuPanel.SetActive(true);   // 메뉴 켜기!
    }

    // === 버튼 연결용 함수 ===
    public void OnClickContinue()
    {
        if (GameManager.Instance != null)
        {
            Debug.Log("📂 이어하기 시작!");
            GameManager.Instance.ContinueGame();
        }
    }

    public void OnClickNewGame()
    {
        // (나중에 여기에 "정말 삭제하시겠습니까?" 팝업 넣으면 좋음)
        if (GameManager.Instance != null)
        {
            Debug.Log("🆕 새 게임 시작! (데이터 초기화)");
            GameManager.Instance.StartNewGame();
        }
    }

    public void OnClickExit()
    {
        Debug.Log("👋 게임 종료");
        Application.Quit();
    }
}