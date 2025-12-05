using UnityEngine;
using UnityEngine.UI;

public class MuseumGameplayManager : MonoBehaviour
{
    [Header("Art Frame Settings")]
    [SerializeField] private RawImage targetFrameImage; // 벽에 걸린 액자 (그림 뜰 곳)

    // (나중에 여기에 해킹, 도주 관련 변수도 추가될 예정)

    void Start()
    {
        SetupTargetArt();
    }

    private void SetupTargetArt()
    {
        // 1. 게임 매니저가 없거나, 타겟 정보가 없으면 패스 (에러 방지)
        if (GameManager.Instance == null || GameManager.Instance.currentMissionTarget == null)
        {
            Debug.LogWarning("타겟 정보가 없습니다! (테스트 중이라면 GameManager에서 세팅 필요)");
            return;
        }

        // 2. 데이터 가져오기
        ArtPieceData target = GameManager.Instance.currentMissionTarget;

        // 3. 액자에 그림 끼우기
        if (targetFrameImage != null)
        {
            targetFrameImage.texture = target.targetTexture;
            Debug.Log($"목표물 배치 완료: {target.artName}");
        }
    }
}