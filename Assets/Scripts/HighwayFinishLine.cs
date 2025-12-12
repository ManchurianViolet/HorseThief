using UnityEngine;
using UnityEngine.SceneManagement;

public class HighwayFinishLine : MonoBehaviour
{
    [Header("Success Settings")]
    [SerializeField] private float successDelay = 1.0f;

    private bool hasFinished = false;

    // ★ [디버깅] 시작할 때 설정 확인
    private void Start()
    {
        Collider col = GetComponent<Collider>();
        if (col == null)
        {
            Debug.LogError("❌ FinishLine에 Collider가 없습니다!");
        }
        else if (!col.isTrigger)
        {
            Debug.LogError("❌ FinishLine Collider의 Is Trigger가 꺼져있습니다!");
        }
        else
        {
            Debug.Log("✅ FinishLine 설정 확인 완료");
        }
    }

    // ★ [디버깅] 뭐가 닿는지 전부 확인
    private void OnTriggerEnter(Collider other)
    {
        // 일단 뭐든 닿으면 로그 출력
        Debug.Log($"🔔 [FinishLine] 뭔가 닿았음! 이름: {other.name}, 태그: {other.tag}");

        // 중복 실행 방지
        if (hasFinished)
        {
            Debug.Log("⚠️ 이미 완료됨 (중복 실행 방지)");
            return;
        }

        // Rigidbody 확인 (말은 Rigidbody가 달려있을 수 있음)
        Rigidbody rb = other.attachedRigidbody;
        if (rb != null)
        {
            Debug.Log($"🔍 Rigidbody 발견! 태그: {rb.tag}");

            if (rb.CompareTag("HorseChest") || rb.CompareTag("Player"))
            {
                hasFinished = true;
                Debug.Log("🏁 [도착!] 결승선 통과! 미션 성공!");
                OnMissionSuccess();
                return;
            }
        }

        // 직접 태그 확인
        if (other.CompareTag("HorseChest") || other.CompareTag("Player"))
        {
            hasFinished = true;
            Debug.Log("🏁 [도착!] 결승선 통과! 미션 성공!");
            OnMissionSuccess();
        }
        else
        {
            Debug.LogWarning($"⚠️ 태그가 안 맞음! 현재 태그: {other.tag}");
        }
    }

    private void OnMissionSuccess()
    {
        // 1. 타이머 멈추기
        MuseumTimeManager timeManager = FindObjectOfType<MuseumTimeManager>();
        if (timeManager != null)
        {
            timeManager.gameObject.SetActive(false);
            Debug.Log("⏱️ 타이머 정지");
        }

        // 2. 경찰차 멈추기
        PoliceCarSpline[] policeCars = FindObjectsOfType<PoliceCarSpline>();
        foreach (var car in policeCars)
        {
            car.gameObject.SetActive(false);
        }
        Debug.Log($"🚓 경찰차 {policeCars.Length}대 정지");

        // 3. 보상 지급 & 데이터 저장
        if (GameManager.Instance != null && GameManager.Instance.currentMissionTarget != null)
        {
            int reward = GameManager.Instance.currentMissionTarget.price;
            GameManager.Instance.AddMoney(reward);
            Debug.Log($"💰 보상 지급: ${reward}");

            int targetIndex = GameManager.Instance.currentTargetIndex;
            GameManager.Instance.data.collectedArts[targetIndex] = true;
            Debug.Log($"🎨 그림 수집 완료: Index {targetIndex}");

            int stageIndex = GameManager.Instance.currentTargetStageIndex;
            int maxItems = (stageIndex == 5) ? 1 : 5;
            int stolenCount = GameManager.Instance.data.GetStolenCount(stageIndex);

            if (stolenCount >= maxItems && stageIndex < 5)
            {
                GameManager.Instance.data.unlockedStageIndex = stageIndex + 1;
                Debug.Log($"🔓 다음 스테이지 해금: Stage {stageIndex + 2}");
            }

            GameManager.Instance.SaveGameData();
            Debug.Log("💾 데이터 저장 완료");
        }

        // 4. 은신처로 복귀
        Invoke(nameof(ReturnToHideout), successDelay);
    }

    private void ReturnToHideout()
    {
        if (GameManager.Instance != null)
        {
            int currentLevel = GameManager.Instance.data.currentHideoutLevel;
            string sceneName = $"Hideout_Lv{currentLevel}";

            Debug.Log($"🏠 은신처로 복귀: {sceneName}");
            SceneManager.LoadScene(sceneName);
        }
        else
        {
            Debug.LogError("🚨 GameManager를 찾을 수 없습니다!");
        }
    }
}