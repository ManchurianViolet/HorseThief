using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class LaptopUpgradeTab : MonoBehaviour
{
    [Header("UI References (Text) - 레벨 표시용")]
    [SerializeField] private TextMeshProUGUI txtPowerLv;
    [SerializeField] private TextMeshProUGUI txtNeckRotLv;
    [SerializeField] private TextMeshProUGUI txtNeckLenLv;
    [SerializeField] private TextMeshProUGUI txtJumpLv;

    [Header("UI References (Buttons) - 클릭용")]
    [SerializeField] private Button btnPower;
    [SerializeField] private Button btnNeckRot;
    [SerializeField] private Button btnNeckLen;
    [SerializeField] private Button btnJump;

    // 탭이 켜질 때마다(SetActive true) 최신 정보로 갱신
    private void OnEnable()
    {
        UpdateAllUI();
    }

    // --- 버튼 클릭 연결 함수 ---
    // (인스펙터의 OnClick 이벤트에 연결하세요)
    // 0:마력, 1:목회전, 2:목길이, 3:점프충전

    public void OnClickPower() { TryUpgrade(0); }
    public void OnClickNeckRot() { TryUpgrade(1); }
    public void OnClickNeckLen() { TryUpgrade(2); }
    public void OnClickJump() { TryUpgrade(3); }

    private void TryUpgrade(int typeIndex)
    {
        // 게임 매니저에게 "돈 내고 업그레이드 해줘!" 요청
        // 성공하면 true, 실패하면(돈 부족/만렙) false 반환
        bool isSuccess = GameManager.Instance.TryUpgradeStat(typeIndex);

        if (isSuccess)
        {
            UpdateAllUI(); // 성공했으면 화면(레벨 숫자) 갱신
            Debug.Log("업그레이드 성공!");
        }
        else
        {
            Debug.Log("업그레이드 실패 (돈 부족 or 만렙)");
        }
    }

    private void UpdateAllUI()
    {
        if (GameManager.Instance == null) return;

        // ★ [수정됨] 변수명으로 가져오기
        PlayerData data = GameManager.Instance.data;
        int cost = GameManager.Instance.upgradeCost;

        if (txtPowerLv != null) txtPowerLv.text = $" Lv.{data.powerLv} ({cost}$)";
        if (txtNeckRotLv != null) txtNeckRotLv.text = $" Lv.{data.neckRotLv} ({cost}$)";
        if (txtNeckLenLv != null) txtNeckLenLv.text = $" Lv.{data.neckLenLv} ({cost}$)";
        if (txtJumpLv != null) txtJumpLv.text = $" Lv.{data.jumpLv} ({cost}$)";
    }
}