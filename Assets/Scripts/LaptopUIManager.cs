using UnityEngine;
using UnityEngine.UI; // 버튼 색깔 바꾸려면 필요

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

    // 선택되었을 때 / 안 되었을 때 색상 설정
    private Color selectedColor = Color.gray; // 선택됨 (어둡게)
    private Color normalColor = Color.white;  // 평상시 (원래 색)

    void Start()
    {
        // ★ [수정] 시작할 때는 모든 탭을 닫아둡니다.
        CloseAllTabs();
    }

    // --- 초기화 함수 ---
    public void CloseAllTabs()
    {
        // 1. 패널 다 끄기
        panelUpgrade.SetActive(false);
        panelHideout.SetActive(false);
        panelHeist.SetActive(false);

        // 2. 버튼 색깔 다 원래대로(흰색) 돌리기
        // (버튼에 Image 컴포넌트가 있어야 작동합니다)
        if (btnUpgrade != null) btnUpgrade.image.color = normalColor;
        if (btnHideout != null) btnHideout.image.color = normalColor;
        if (btnHeist != null) btnHeist.image.color = normalColor;
    }

    // --- 버튼 연결 함수들 ---

    public void ShowUpgradeTab()
    {
        // 일단 다 끄고 시작 (초기화)
        CloseAllTabs();

        // 업그레이드만 켜기
        panelUpgrade.SetActive(true);

        // 업그레이드 버튼만 색깔 바꾸기 (선택된 느낌)
        if (btnUpgrade != null) btnUpgrade.image.color = selectedColor;

        Debug.Log("업그레이드 탭 열림");
    }

    public void ShowHideoutTab()
    {
        CloseAllTabs();
        panelHideout.SetActive(true);
        if (btnHideout != null) btnHideout.image.color = selectedColor;
        Debug.Log("은신처 탭 열림");
    }

    public void ShowHeistTab()
    {
        CloseAllTabs();
        panelHeist.SetActive(true);
        if (btnHeist != null) btnHeist.image.color = selectedColor;
        Debug.Log("미술관 탭 열림");
    }
}