using UnityEngine;
using TMPro;

public class MuseumTimeManager : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float limitTime = 180f; // 3분
    [SerializeField] private TextMeshProUGUI timerText;

    private float currentTime;
    private bool isRunning = false;

    public void StartTimer()
    {
        currentTime = limitTime;
        isRunning = true;
        gameObject.SetActive(true);
    }

    // ★ [추가] 고속도로 진입 시 보너스 시간을 더해주는 함수
    public void AddBonusTime(float bonusSeconds)
    {
        currentTime += bonusSeconds;

        // (선택사항) 시간이 늘어났다는 걸 로그로 확인
        Debug.Log($"⏳ 보너스 시간 {bonusSeconds}초 획득! 현재 남은 시간: {currentTime:F1}초");
    }

    void Update()
    {
        if (!isRunning) return;

        currentTime -= Time.deltaTime;

        int min = Mathf.FloorToInt(currentTime / 60);
        int sec = Mathf.FloorToInt(currentTime % 60);

        if (timerText != null)
            timerText.text = $"{min:00}:{sec:00}";

        if (currentTime <= 0)
        {
            currentTime = 0;
            isRunning = false;
            Debug.Log("시간 초과! 경찰 출동!");
            // 나중에 HighwayManager에서 게임오버 처리 연결 가능
        }
    }
}