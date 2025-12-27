using UnityEngine;
using TMPro;

public class MuseumTimeManager : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float limitTime = 10f; // 3분
    [SerializeField] private TextMeshProUGUI timerText;

    // ★ [추가] 트럭 탈출 라인 스크립트를 연결할 변수
    [SerializeField] private HighwayFinishLine finishLine;

    private float currentTime;
    private bool isRunning = false;

    public void StartTimer()
    {
        currentTime = limitTime;
        isRunning = true;
        gameObject.SetActive(true);

        // ★ 만약 인스펙터에서 연결 안 했다면 자동으로 찾기 (안전장치)
        if (finishLine == null)
        {
            finishLine = FindObjectOfType<HighwayFinishLine>();
        }
    }

    public void AddBonusTime(float bonusSeconds)
    {
        currentTime += bonusSeconds;
        Debug.Log($"⏳ 보너스 시간 {bonusSeconds}초 획득! 현재 남은 시간: {currentTime:F1}초");
    }

    void Update()
    {
        if (!isRunning) return;

        currentTime -= Time.deltaTime;

        // 음수 방지 및 00:00 표시
        if (currentTime <= 0)
        {
            currentTime = 0;
            // 한 번만 실행되도록 isRunning을 먼저 끔
            isRunning = false;

            if (timerText != null) timerText.text = "00:00";

            Debug.Log("⏰ 시간 초과! 트럭이 떠납니다!");

            // ★ [핵심] 트럭 출발 함수 호출!
            if (finishLine != null)
            {
                finishLine.FailByTimeOver();
            }
            else
            {
                Debug.LogError("❌ HighwayFinishLine을 찾을 수 없습니다!");
            }
            return;
        }

        int min = Mathf.FloorToInt(currentTime / 60);
        int sec = Mathf.FloorToInt(currentTime % 60);

        if (timerText != null)
            timerText.text = $"{min:00}:{sec:00}";
    }
}