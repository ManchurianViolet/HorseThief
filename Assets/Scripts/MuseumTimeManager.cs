using UnityEngine;
using TMPro;

public class MuseumTimeManager : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float limitTime = 180f; // 3분 (초 단위)
    [SerializeField] private TextMeshProUGUI timerText;

    private float currentTime;
    private bool isRunning = false;

    // 외부(TruckManager)에서 이 함수를 부르면 타이머 시작!
    public void StartTimer()
    {
        currentTime = limitTime;
        isRunning = true;
        gameObject.SetActive(true); // 나 자신(UI)을 켬
    }

    void Update()
    {
        if (!isRunning) return;

        currentTime -= Time.deltaTime;

        // 시간 포맷 (00:00)
        int min = Mathf.FloorToInt(currentTime / 60);
        int sec = Mathf.FloorToInt(currentTime % 60);

        if (timerText != null)
            timerText.text = $"{min:00}:{sec:00}";

        // 시간 종료
        if (currentTime <= 0)
        {
            currentTime = 0;
            isRunning = false;
            Debug.Log("시간 초과! 경찰 출동!");
            // 나중에 여기에 게임 오버 로직 추가
        }
    }
}