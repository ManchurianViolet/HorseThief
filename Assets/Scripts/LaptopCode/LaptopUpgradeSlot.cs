using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems; // 마우스 감지용 필수!

public class LaptopUpgradeSlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("UI References")]
    [SerializeField] private Image[] levelIndicators; // 5개의 레벨 칸 (Lv1~Lv5)
    [SerializeField] private GameObject tooltipPanel; // 이 버튼의 설명창

    [Header("Colors")]
    [SerializeField] private Color activeColor = Color.green; // 켜진 색 (초록)
    [SerializeField] private Color inactiveColor = Color.gray; // 꺼진 색 (회색)

    // 마우스를 올렸을 때 -> 설명창 켜기
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (tooltipPanel != null) tooltipPanel.SetActive(true);
    }

    // 마우스를 뗐을 때 -> 설명창 끄기
    public void OnPointerExit(PointerEventData eventData)
    {
        if (tooltipPanel != null) tooltipPanel.SetActive(false);
    }

    // ★ 외부(매니저)에서 호출할 함수: "야, 지금 3레벨이니까 색칠해!"
    public void UpdateSlotVisual(int currentLevel)
    {
        for (int i = 0; i < levelIndicators.Length; i++)
        {
            // 배열은 0부터 시작하므로, (i < currentLevel)이면 켜야 함
            // 예: 3레벨이면 0, 1, 2번 인덱스(총 3개)가 켜짐
            if (i < currentLevel)
            {
                levelIndicators[i].color = activeColor;
            }
            else
            {
                levelIndicators[i].color = inactiveColor;
            }
        }
    }
}