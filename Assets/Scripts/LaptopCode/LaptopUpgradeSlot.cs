using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class LaptopUpgradeSlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("UI References")]
    [SerializeField] private Image[] levelIndicators; // 5개의 레벨 칸 (Image 컴포넌트들)
    [SerializeField] private GameObject tooltipPanel; // 이 버튼의 설명창

    // ★ [변경] 색상 변수 삭제 -> 스프라이트(이미지) 변수 추가
    [Header("Sprite Settings")]
    [SerializeField] private Sprite filledSprite; // 꽉 찬 초록 원 (활성)
    [SerializeField] private Sprite emptySprite;  // 테두리 초록 원 (비활성)

    // 마우스를 올렸을 때 -> 설명창 켜기 (기존 유지)
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (tooltipPanel != null) tooltipPanel.SetActive(true);
    }

    // 마우스를 뗐을 때 -> 설명창 끄기 (기존 유지)
    public void OnPointerExit(PointerEventData eventData)
    {
        if (tooltipPanel != null) tooltipPanel.SetActive(false);
    }

    // ★ 외부(매니저)에서 호출할 함수
    public void UpdateSlotVisual(int currentLevel)
    {
        for (int i = 0; i < levelIndicators.Length; i++)
        {
            // 배열은 0부터 시작하므로, (i < currentLevel)이면 켜야 함
            if (i < currentLevel)
            {
                // ★ [변경] 색깔 대신 이미지를 '꽉 찬 원'으로 교체
                levelIndicators[i].sprite = filledSprite;
            }
            else
            {
                // ★ [변경] 색깔 대신 이미지를 '테두리 원'으로 교체
                levelIndicators[i].sprite = emptySprite;
            }

            // ★ [중요] 이미지를 바꿨으므로, 색깔 간섭이 없도록 흰색으로 초기화
            // (만약 이전에 회색으로 설정된 게 남아있으면 이미지가 어둡게 보일 수 있음)
            levelIndicators[i].color = Color.white;
        }
    }
}