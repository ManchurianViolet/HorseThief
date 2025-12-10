using UnityEngine;
// using UnityEngine.UI; // UI 관련 코드는 이제 안 씀

public class MuseumGameplayManager : MonoBehaviour
{
    [Header("Art Frame Settings")]
    // ★ [수정] RawImage 대신 InteractiveArt 스크립트를 연결
    [SerializeField] private InteractiveArt targetFrame;

    void Start()
    {
        SetupTargetArt();
    }

    private void SetupTargetArt()
    {
        // 1. 안전장치 (기존 동일)
        if (GameManager.Instance == null || GameManager.Instance.currentMissionTarget == null)
        {
            Debug.LogWarning("타겟 정보가 없습니다! (테스트 중이라면 GameManager 확인)");
            return;
        }

        // 2. 데이터 가져오기
        var targetData = GameManager.Instance.currentMissionTarget;

        if (targetFrame != null)
        {
            // ★ [수정] targetData.missionName -> targetData.artName 으로 변경!
            // (ArtPieceData 안에 있는 변수 이름이 artName일 것입니다)
            targetFrame.SetupArt(targetData.artName, targetData.targetTexture);

            Debug.Log($"[미술관] 목표물 배치 완료: {targetData.artName}");
        }
        else
        {
            Debug.LogError("MuseumGameplayManager에 'Target Frame'이 연결되지 않았습니다!");
        }
    }
}