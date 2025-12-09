using UnityEngine;
using System.Collections;

public class TruckManager : MonoBehaviour
{
    [Header("Phase Settings")]
    [SerializeField] private MuseumPainter painter;      // 그림 그리기 시스템
    [SerializeField] private GameObject player;         // 플레이어(말)
    [SerializeField] private GameObject museumSpawnPoint; // 미술관 입구 위치 (빈 오브젝트)
    [SerializeField] private GameObject playerBrush;
    [Header("Back Canvas")]
    [SerializeField] private GameObject horseBackCanvas; // 말 등 뒤에 있는 Quad
    [SerializeField] private Renderer backCanvasRenderer; // 그 Quad의 렌더러

    [Header("UI & Effects")]
    [SerializeField] private GameObject truckLight;     // 트럭 조명 (연출용)
    // 나중에 페이드 아웃 효과 UI 추가 가능

    private bool isReady = false;

    void Update()
    {
        // 그림 그리기 완료 (Y키)
        // (PaperPainter의 기존 Y키 리셋 기능과 겹치지 않게 주의! 
        //  PaperPainter의 Update에서 Y키 입력을 주석 처리하거나 여기서만 쓰도록 약속)
        if (!isReady && Input.GetKeyDown(KeyCode.Y))
        {
            StartCoroutine(DepartToMuseum());
        }
    }

    IEnumerator DepartToMuseum()
    {
        isReady = true;
        Debug.Log("위조 완료! 미술관으로 출발합니다...");

        // 1. 현재 그린 그림 가져오기
        Texture2D forgery = painter.GetFinalTexture();

        // 2. 말 등 뒤 캔버스에 그림 입히기
        if (horseBackCanvas != null && backCanvasRenderer != null)
        {
            horseBackCanvas.SetActive(true); // 등 뒤 캔버스 켜기
            backCanvasRenderer.material.mainTexture = forgery; // 텍스처 복사
        }
        if (playerBrush != null)
        {
            playerBrush.SetActive(false);
        }
        // 3. 연출 (잠시 대기 or 암전)
        // 여기에 UI Fade Out 넣으면 좋습니다.
        yield return new WaitForSeconds(1.0f);

        // 4. 미술관으로 텔레포트
        if (museumSpawnPoint != null)
        {
            player.transform.position = museumSpawnPoint.transform.position;
            player.transform.rotation = museumSpawnPoint.transform.rotation;
        }

        // 5. 트럭 조명 끄기 (최적화)
        if (truckLight != null) truckLight.SetActive(false);

        // 6. 타이머 시작 등 게임 매니저에게 알리기 (나중에 추가)
    }
}