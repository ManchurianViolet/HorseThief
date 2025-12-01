using UnityEngine;

public class LaptopHeistTab : MonoBehaviour
{
    [Header("Slot References")]
    // 이제 버튼, 이미지, 텍스트 배열 3개를 관리할 필요 없이 이거 하나면 끝!
    [SerializeField] private HeistStageSlot[] stageSlots;

    private void Start()
    {
        // 시작할 때 각 슬롯에게 "너는 몇 번이고, 누르면 무슨 함수를 실행해라"고 알려줌
        for (int i = 0; i < stageSlots.Length; i++)
        {
            // OnClickStage 함수를 넘겨줘서 연결시킴
            stageSlots[i].Initialize(i, OnClickStage);
        }
    }

    private void OnEnable()
    {
        UpdateAllSlots();
    }

    // 버튼을 누르면 이 함수가 실행됨 (슬롯이 알아서 호출해줌)
    private void OnClickStage(int stageIndex)
    {
        GameManager.Instance.GenerateMission(stageIndex);
    }

    private void UpdateAllSlots()
    {
        if (GameManager.Instance == null) return;
        PlayerData data = GameManager.Instance.data;

        for (int i = 0; i < stageSlots.Length; i++)
        {
            // 1. 데이터 계산
            int maxItems = (i == 5) ? 1 : 5; // 마지막 탄은 1개
            int stolenCount = data.GetStolenCount(i);

            bool isComplete = (stolenCount >= maxItems);
            bool isLocked = false;

            if (i > 0) // 2탄부터는 이전 스테이지 확인
            {
                int prevMax = (i - 1 == 5) ? 1 : 5; // 이전 탄이 마지막 탄일 수도 있으니(그럴 리 없지만) 안전하게
                int prevCount = data.GetStolenCount(i - 1);
                // 이전 탄을 다 깼어야 해금 (5개)
                // (주의: 기획상 이전 탄은 무조건 5개 짜리임)
                isLocked = (prevCount < 5);
            }

            // 2. 슬롯에게 "너 상태 업데이트해!" 명령 (캡슐화)
            // 매니저는 UI가 어떻게 생겼는지 몰라도 됨. 그냥 상태만 던져주면 끝.
            if (i < stageSlots.Length)
            {
                stageSlots[i].UpdateState(isLocked, isComplete, stolenCount, maxItems);
            }
        }
    }
}