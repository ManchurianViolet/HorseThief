using UnityEngine;
using UnityEngine.UI;
using TMPro; // ★ 필수

public class LaptopUIManager : MonoBehaviour
{
    [Header("Global UI (항상 떠있는 거)")]
    [SerializeField] private TextMeshProUGUI txtMoneyDisplay; // ★ 돈 표시 텍스트

    [Header("Content Panels")]
    [SerializeField] private GameObject panelUpgrade;
    [SerializeField] private GameObject panelHideout;
    [SerializeField] private GameObject panelHeist;

    [Header("Tab Buttons")]
    [SerializeField] private Button btnUpgrade;
    [SerializeField] private Button btnHideout;
    [SerializeField] private Button btnHeist;

    private Color selectedColor = Color.gray;
    private Color normalColor = Color.white;

    private void OnEnable()
    {
        CloseAllTabs();
    }

    private void Update()
    {
        // ★ [추가] 매 프레임 돈 표시 갱신 (가장 확실한 방법)
        if (GameManager.Instance != null && txtMoneyDisplay != null)
        {
            txtMoneyDisplay.text = $"보유 자산: {GameManager.Instance.data.money} $";
        }
    }

    // ★ [핵심 기능] 뒤로가기 처리를 담당하는 함수
    // 리턴값: true면 "내가 처리했음(노트북 끄지마)", false면 "난 할 거 없음(노트북 꺼도 됨)"
    public bool TryGoBack()
    {
        // 만약 어떤 패널이라도 켜져 있다면?
        if (panelUpgrade.activeSelf || panelHideout.activeSelf || panelHeist.activeSelf)
        {
            CloseAllTabs(); // 메인 화면으로 돌아감
            return true;    // "뒤로가기 성공했으니 노트북 끄지 마"
        }

        return false; // "메인 화면이니까 이제 노트북 꺼도 됨"
    }

    public void CloseAllTabs()
    {
        if (panelUpgrade != null) panelUpgrade.SetActive(false);
        if (panelHideout != null) panelHideout.SetActive(false);
        if (panelHeist != null) panelHeist.SetActive(false);

        if (btnUpgrade != null) btnUpgrade.image.color = normalColor;
        if (btnHideout != null) btnHideout.image.color = normalColor;
        if (btnHeist != null) btnHeist.image.color = normalColor;
    }

    // ... (버튼 연결 함수들은 기존과 동일) ...
    public void ShowUpgradeTab() { CloseAllTabs(); panelUpgrade.SetActive(true); btnUpgrade.image.color = selectedColor; }
    public void ShowHideoutTab() { CloseAllTabs(); panelHideout.SetActive(true); btnHideout.image.color = selectedColor; }
    public void ShowHeistTab() { CloseAllTabs(); panelHeist.SetActive(true); btnHeist.image.color = selectedColor; }
}