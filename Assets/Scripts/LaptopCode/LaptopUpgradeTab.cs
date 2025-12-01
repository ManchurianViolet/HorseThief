using UnityEngine;

public class LaptopUpgradeTab : MonoBehaviour
{
    [Header("Slots (순서대로: 마력, 목회전, 목길이, 점프)")]
    [SerializeField] private LaptopUpgradeSlot slotPower;
    [SerializeField] private LaptopUpgradeSlot slotNeckRot;
    [SerializeField] private LaptopUpgradeSlot slotNeckLen;
    [SerializeField] private LaptopUpgradeSlot slotJump;

    private void OnEnable()
    {
        UpdateAllSlots(); // 켜질 때마다 최신 상태로 갱신
    }

    // === 버튼 연결용 함수 ===
    public void OnClickPower() { TryUpgrade(0); }
    public void OnClickNeckRot() { TryUpgrade(1); }
    public void OnClickNeckLen() { TryUpgrade(2); }
    public void OnClickJump() { TryUpgrade(3); }

    private void TryUpgrade(int typeIndex)
    {
        if (GameManager.Instance == null) return;

        // 1. 현재 레벨 확인
        int currentLv = GetCurrentLevel(typeIndex);

        // 2. ★ 은신처별 최대 레벨 제한 확인 (핵심 로직)
        int hideoutLv = GameManager.Instance.data.currentHideoutLevel;
        int maxAllowedLv = 0;

        if (hideoutLv == 1) maxAllowedLv = 2;      // 은신처1 -> 2렙까지
        else if (hideoutLv == 2) maxAllowedLv = 4; // 은신처2 -> 4렙까지
        else maxAllowedLv = 5;                     // 은신처3 -> 5렙(만렙)

        // 제한에 걸리면 업그레이드 거부
        if (currentLv >= maxAllowedLv)
        {
            Debug.Log($"현재 은신처(Lv.{hideoutLv})에서는 {maxAllowedLv}레벨까지만 강화 가능합니다!");
            return;
        }

        // 3. 만렙(5) 체크 (이미 5면 무시)
        if (currentLv >= 5) return;

        // 4. 게임 매니저에게 실제 업그레이드 요청 (돈 계산 등)
        bool success = GameManager.Instance.TryUpgradeStat(typeIndex);

        if (success)
        {
            UpdateAllSlots(); // 성공하면 색칠 다시 하기
            Debug.Log("강화 성공!");
        }
    }

    private void UpdateAllSlots()
    {
        if (GameManager.Instance == null) return;
        PlayerData data = GameManager.Instance.data;

        // 각 슬롯에게 "너 지금 몇 레벨이야, 색칠해!" 명령
        if (slotPower != null) slotPower.UpdateSlotVisual(data.powerLv);
        if (slotNeckRot != null) slotNeckRot.UpdateSlotVisual(data.neckRotLv);
        if (slotNeckLen != null) slotNeckLen.UpdateSlotVisual(data.neckLenLv);
        if (slotJump != null) slotJump.UpdateSlotVisual(data.jumpLv);
    }

    // 편의용 함수: 타입 번호로 현재 레벨 가져오기
    private int GetCurrentLevel(int typeIndex)
    {
        switch (typeIndex)
        {
            case 0: return GameManager.Instance.data.powerLv;
            case 1: return GameManager.Instance.data.neckRotLv;
            case 2: return GameManager.Instance.data.neckLenLv;
            case 3: return GameManager.Instance.data.jumpLv;
            default: return 0;
        }
    }
}