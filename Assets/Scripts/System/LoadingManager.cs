using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;

public class LoadingManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private RawImage targetArtImage; // 그림 (Texture2D용)
    [SerializeField] private TextMeshProUGUI targetNameText; // "목표: 모나리자"
    [SerializeField] private TextMeshProUGUI targetPriceText; // "가치: $10000"

    [Header("Settings")]
    [SerializeField] private float loadingTime = 3.0f; // 대기 시간 (3초)

    void Start()
    {
        // 1. GameManager에서 이번 미션 정보 가져오기
        if (GameManager.Instance != null && GameManager.Instance.currentMissionTarget != null)
        {
            ArtPieceData target = GameManager.Instance.currentMissionTarget;

            // 텍스트 표시
            if (targetNameText != null)
                targetNameText.text = $"TARGET: {target.artName}";

            if (targetPriceText != null)
                targetPriceText.text = $"VALUE: ${target.price}";

            // 그림 표시 (RawImage에 Texture2D 넣기)
            if (targetArtImage != null)
                targetArtImage.texture = target.targetTexture;
        }
        else
        {
            Debug.LogError("미션 타겟 정보가 없습니다! (GameManager 확인 필요)");
        }

        // 2. 잠시 후 미술관으로 이동
        StartCoroutine(LoadMuseumRoutine());
    }

    IEnumerator LoadMuseumRoutine()
    {
        // 3초 동안 대기 (연출용)
        yield return new WaitForSeconds(loadingTime);

        // 3. 진짜 미술관 씬으로 이동
        // 씬 이름 규칙: Museum_Stage1, Museum_Stage2...
        // (GameManager에 저장된 타겟 스테이지 번호를 이용)
        if (GameManager.Instance != null)
        {
            int stageIndex = GameManager.Instance.currentTargetStageIndex;
            string sceneName = $"Museum_Stage{stageIndex + 1}"; // 인덱스 0 -> Stage1

            Debug.Log($"{sceneName}으로 이동합니다!");
            SceneManager.LoadScene(sceneName);
        }
    }
}