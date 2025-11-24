using UnityEngine;
using TMPro;

public class PaintingStartTrigger : MonoBehaviour
{
    [SerializeField] TextMeshPro guideText;
    [SerializeField] TextMeshPro percentText;


    private void OnTriggerEnter(Collider _other)
    {
        if (_other.CompareTag("HorseChest"))
        {
            // 텍스트들 활성화
            guideText.gameObject.SetActive(true);
            percentText.gameObject.SetActive(true);
            guideText.text = "몬드리안 그림과 75% 이상 일치하도록 복사하세요!"; // 가이드 텍스트 변경
        }
    }

    public void ChangePercentText(float _percentRate)
    {
        // 소수점 1자리는 버리기 (예: 75.34% -> 75.3%)
        float truncatedFloat = Mathf.Floor(_percentRate * 10f) / 10f;
        int successGoal = 75; // 목표 정확도 75%

        // 출력
        if (percentText != null)
        {
            percentText.text = $"Accuracy: {truncatedFloat}% / {successGoal}%"; // 출력 형식 변경
        }

        // 75% 이상일시, 축하 메시지
        if (truncatedFloat >= successGoal)
        {
            percentText.text = "완벽해요! 몬드리안을 복사했습니다!";
        }
    }

}