using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.UI;

public class GameOverUI : MonoBehaviour
{
    [Header("UI Panels")]
    [SerializeField] private GameObject wastedPanel; // Wasted 패널 (CanvasGroup 포함 필수)
    [SerializeField] private GameObject bustedPanel; // Busted 패널 (CanvasGroup 포함 필수)
    [SerializeField] private Image fadePanel;        // (이제 암전용으로는 사용하지 않음)

    private bool isJailEndingActive = false;

    private void Start()
    {
        // 시작할 때 패널들을 끄고 투명도를 0으로 초기화
        SetupInitialPanel(wastedPanel);
        SetupInitialPanel(bustedPanel);

        if (fadePanel) fadePanel.gameObject.SetActive(false);
    }

    private void SetupInitialPanel(GameObject panel)
    {
        if (panel != null)
        {
            panel.SetActive(false);
            CanvasGroup cg = panel.GetComponent<CanvasGroup>();
            if (cg != null) cg.alpha = 0f;
        }
    }

    public void ShowWasted()
    {
        isJailEndingActive = false;
        StartCoroutine(ShowPanelFadeInRoutine(wastedPanel));
    }

    public void ShowBusted()
    {
        isJailEndingActive = true;
        StartCoroutine(ShowPanelFadeInRoutine(bustedPanel));
    }

    private IEnumerator ShowPanelFadeInRoutine(GameObject panelToShow)
    {
        if (panelToShow == null) yield break;

        // 1. 패널 활성화 (아직 Alpha가 0이라 보이지 않음)
        panelToShow.SetActive(true);
        CanvasGroup cg = panelToShow.GetComponent<CanvasGroup>();

        // 2. 패널 자체의 투명도를 0에서 1로 올림 (Fade-in)
        if (cg != null)
        {
            float t = 0f;
            while (t < 1f)
            {
                // 슬로우 모션 중일 수 있으므로 unscaledDeltaTime 사용
                t += Time.unscaledDeltaTime;
                cg.alpha = Mathf.Lerp(0f, 1f, t);
                yield return null;
            }
            cg.alpha = 1f;
        }
        else
        {
            // CanvasGroup이 없으면 그냥 바로 보여줌
            panelToShow.SetActive(true);
        }

        // 3. 커서 설정
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        // 4. 시간 정지
        Time.timeScale = 0f;
    }

    public void OnClickReturnToHideout()
    {
        Time.timeScale = 1f;

        if (GameManager.Instance != null)
        {
            GameManager.Instance.ProcessMissionFail(isJailEndingActive);
        }

        int hideoutLevel = GameManager.Instance.data.currentHideoutLevel;
        SceneManager.LoadScene($"Hideout_Lv{hideoutLevel}");
    }
}