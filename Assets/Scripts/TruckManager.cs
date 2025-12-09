using UnityEngine;
using System.Collections;

public class TruckManager : MonoBehaviour
{
    [Header("Phase Settings")]
    [SerializeField] private MuseumPainter painter;
    [SerializeField] private GameObject player;
    [SerializeField] private GameObject museumSpawnPoint;
    [SerializeField] private GameObject playerBrush; // 트럭에서만 쓰는 붓

    [Header("Back Canvas")]
    [SerializeField] private GameObject horseBackCanvas;
    [SerializeField] private Renderer backCanvasRenderer;

    [Header("UI Control")]
    [SerializeField] private GameObject truckUI;   // ★ [추가] 트럭 UI (Accuracy 점수판 등)
    [SerializeField] private GameObject museumUI;  // ★ [추가] 미술관 UI (타이머, 비밀번호)
    [SerializeField] private MuseumTimeManager timeManager; // ★ [추가] 타이머 시작 명령용

    [Header("Effects")]
    [SerializeField] private GameObject truckLight;

    private bool isReady = false;

    void Update()
    {
        if (!isReady && Input.GetKeyDown(KeyCode.Y))
        {
            StartCoroutine(DepartToMuseum());
        }
    }

    IEnumerator DepartToMuseum()
    {
        isReady = true;
        Debug.Log("위조 완료! 미술관으로 출발합니다...");

        // 1. 그림 복사 & 붓 압수
        Texture2D forgery = painter.GetFinalTexture();
        if (horseBackCanvas != null && backCanvasRenderer != null)
        {
            horseBackCanvas.SetActive(true);
            backCanvasRenderer.material.mainTexture = forgery;
        }
        if (playerBrush != null) playerBrush.SetActive(false);

        // 2. ★ [추가] UI 교체 (바통 터치)
        if (truckUI != null) truckUI.SetActive(false); // 트럭 UI(점수) 끄기
        if (museumUI != null) museumUI.SetActive(true); // 미술관 UI(타이머) 켜기

        // 3. 연출 및 이동
        yield return new WaitForSeconds(1.0f);

        if (museumSpawnPoint != null)
        {
            player.transform.position = museumSpawnPoint.transform.position;
            player.transform.rotation = museumSpawnPoint.transform.rotation;
        }

        // 4. 조명 끄기 & ★ [추가] 타이머 시작
        if (truckLight != null) truckLight.SetActive(false);

        if (timeManager != null)
        {
            timeManager.StartTimer(); // 타이머야 돌아라!
        }
    }
}