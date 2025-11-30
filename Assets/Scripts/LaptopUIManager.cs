using UnityEngine;
using UnityEngine.UI;

public class LaptopUIManager : MonoBehaviour
{
    [Header("Content Panels (내용물)")]
    [SerializeField] private GameObject panelUpgrade;
    [SerializeField] private GameObject panelHideout;
    [SerializeField] private GameObject panelHeist;

    [Header("Tab Buttons (선택된 탭 색깔 바꾸기용)")]
    [SerializeField] private Button btnUpgrade;
    [SerializeField] private Button btnHideout;
    [SerializeField] private Button btnHeist;

    private Color selectedColor = Color.gray;
    private Color normalColor = Color.white;

    // ★ [핵심 추가] 노트북이 켜질 때마다(SetActive true 될 때마다) 실행됨
    private void OnEnable()
    {
        CloseAllTabs(); // 켜지자마자 모든 탭을 닫아서 초기 상태로 만듦
    }

    // --- 초기화 함수 ---
    public void CloseAllTabs()
    {
        // 1. 패널 다 끄기
        if (panelUpgrade != null) panelUpgrade.SetActive(false);
        if (panelHideout != null) panelHideout.SetActive(false);
        if (panelHeist != null) panelHeist.SetActive(false);

        // 2. 버튼 색깔 초기화
        if (btnUpgrade != null) btnUpgrade.image.color = normalColor;
        if (btnHideout != null) btnHideout.image.color = normalColor;
        if (btnHeist != null) btnHeist.image.color = normalColor;
    }

    // --- 버튼 연결 함수들 ---

    public void ShowUpgradeTab()
    {
        CloseAllTabs();
        if (panelUpgrade != null) panelUpgrade.SetActive(true);
        if (btnUpgrade != null) btnUpgrade.image.color = selectedColor;
        Debug.Log("업그레이드 탭 열림");
    }

    public void ShowHideoutTab()
    {
        CloseAllTabs();
        if (panelHideout != null) panelHideout.SetActive(true);
        if (btnHideout != null) btnHideout.image.color = selectedColor;
        Debug.Log("은신처 탭 열림");
    }

    public void ShowHeistTab()
    {
        CloseAllTabs();
        if (panelHeist != null) panelHeist.SetActive(true);
        if (btnHeist != null) btnHeist.image.color = selectedColor;
        Debug.Log("미술관 탭 열림");
    }
}