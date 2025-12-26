using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.UI; // Image fade를 위해 필요

public class GameOverUI : MonoBehaviour
{
    [Header("UI Panels")]
    [SerializeField] private GameObject wastedPanel; // 구급차 이미지 있는 패널
    [SerializeField] private GameObject bustedPanel; // 철창 이미지 있는 패널
    [SerializeField] private Image fadePanel;        // 검은색 페이드 아웃용 이미지

    private void Start()
    {
        // 시작할 땐 다 꺼두기
        if (wastedPanel) wastedPanel.SetActive(false);
        if (bustedPanel) bustedPanel.SetActive(false);
        if (fadePanel) fadePanel.gameObject.SetActive(false);
    }

    // 부상 엔딩 (Wasted) 호출
    public void ShowWasted()
    {
        StartCoroutine(ShowPanelRoutine(wastedPanel));
    }

    // 체포 엔딩 (Busted) 호출
    public void ShowBusted()
    {
        StartCoroutine(ShowPanelRoutine(bustedPanel));
    }

    private IEnumerator ShowPanelRoutine(GameObject panelToShow)
    {
        // 1. 검은색 페이드 아웃 (1초 동안)
        if (fadePanel != null)
        {
            fadePanel.gameObject.SetActive(true);
            float t = 0f;
            while (t < 1f)
            {
                t += Time.unscaledDeltaTime; // TimeScale이 0일 수도 있어서 unscaled 사용
                fadePanel.color = new Color(0, 0, 0, t);
                yield return null;
            }
        }

        // 2. 패널 켜기
        panelToShow.SetActive(true);
        // ★ [요청하신 부분 수정] ★
        // UI가 떴으니, 앞을 가리고 있는 검은 페이드 이미지를 투명하게(또는 끄기) 만듭니다.
        if (fadePanel != null)
        {
            // 방법 A: 투명하게 만들기 (부드럽게 하려면 또 반복문 쓰면 됨)
            fadePanel.color = new Color(0, 0, 0, 0);

            // 방법 B: 아예 꺼버리기 (이게 더 확실함)
            fadePanel.gameObject.SetActive(false);
        }
        // 3. 커서 보이기
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        // 4. 시간 완전 정지 (필요하다면)
        Time.timeScale = 0f;
    }

    // 'Return to Hideout' 버튼에 연결할 함수
    public void OnClickReturnToHideout()
    {
        Time.timeScale = 1f; // 시간 정상화 필수!

        // ★ 여기서 "실패했다"는 정보를 GameManager에 저장하고 넘어가야 
        // 은신처에서 신문을 띄울 수 있음. (나중에 구현)
        // GameManager.Instance.SetLastResult(false, "Wasted"); 

        SceneManager.LoadScene("Hideout"); // 씬 이름 확인 필요
    }
}