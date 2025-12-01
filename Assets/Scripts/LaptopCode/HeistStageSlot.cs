using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HeistStageSlot : MonoBehaviour
{
    [Header("내부 UI 컴포넌트 연결")]
    [SerializeField] private Button myButton;
    [SerializeField] private GameObject lockIcon;
    [SerializeField] private GameObject completeStamp;
    [SerializeField] private TextMeshProUGUI progressText;

    private int myStageIndex;

    // 초기화: 버튼에 클릭 기능을 심어주는 함수
    public void Initialize(int stageIndex, System.Action<int> onClickAction)
    {
        myStageIndex = stageIndex;
        // 람다식으로 클릭 이벤트 연결
        myButton.onClick.RemoveAllListeners();
        myButton.onClick.AddListener(() => onClickAction(myStageIndex));
    }

    // 상태 업데이트: 외부에서 "너 잠겼어", "너 깼어" 라고 알려주면 알아서 UI를 바꿈
    public void UpdateState(bool isLocked, bool isComplete, int currentCount, int maxCount)
    {
        // 1. 텍스트 갱신
        if (progressText != null)
            progressText.text = $"{currentCount}/{maxCount}";

        // 2. 상태에 따른 비주얼 변경
        if (isComplete)
        {
            // [완료]
            myButton.interactable = false;
            lockIcon.SetActive(false);
            completeStamp.SetActive(true);
        }
        else if (isLocked)
        {
            // [잠김]
            myButton.interactable = false;
            lockIcon.SetActive(true);
            completeStamp.SetActive(false);
        }
        else
        {
            // [해금 - 도전 가능]
            myButton.interactable = true;
            lockIcon.SetActive(false);
            completeStamp.SetActive(false);
        }
    }
}