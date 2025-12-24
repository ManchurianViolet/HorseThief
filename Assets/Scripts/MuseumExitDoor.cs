using UnityEngine;

public class MuseumExitDoor : MonoBehaviour
{
    public bool canExit = false;
    private bool hasTriggered = false; // ★ [추가] 중복 실행 방지

    // ★ [디버깅] 시작할 때 설정 확인
    private void Start()
    {
        Collider col = GetComponent<Collider>();
        if (col == null)
        {
            Debug.LogError("❌ MuseumExitDoor에 Collider가 없습니다!");
        }
        else if (!col.isTrigger)
        {
            Debug.LogError("❌ MuseumExitDoor Collider의 Is Trigger가 꺼져있습니다!");
        }
        else
        {
            Debug.Log("✅ MuseumExitDoor 설정 확인 완료");
        }
    }

    // ★ [디버깅] 뭐가 닿는지 전부 확인
    private void OnTriggerEnter(Collider other)
    {
        // ★ [핵심] 이미 탈출했으면 더 이상 실행 안 함!
        if (hasTriggered)
        {
            return;
        }

        // 일단 뭐든 닿으면 로그 출력
        Debug.Log($"🚪 [ExitDoor] 뭔가 닿았음! 이름: {other.name}, 태그: {other.tag}, canExit: {canExit}");

        if (!canExit)
        {
            Debug.Log("🔒 아직 그림을 훔치지 않았습니다!");
            return;
        }

        // Rigidbody 확인 (말은 Rigidbody가 달려있음)
        Rigidbody rb = other.attachedRigidbody;
        if (rb != null)
        {
            Debug.Log($"🔍 Rigidbody 발견! 태그: {rb.tag}");

            if (rb.CompareTag("HorseChest") || rb.CompareTag("Player"))
            {
                Debug.Log("✅ [태그 일치] 탈출 시퀀스 시작!");
                StartEscape();
                return;
            }
        }

        // 직접 태그 확인
        if (other.CompareTag("HorseChest") || other.CompareTag("Player"))
        {
            Debug.Log("✅ [직접 태그 일치] 탈출 시퀀스 시작!");
            StartEscape();
            return;
        }

        // ★ [추가] 이름으로도 체크 (태그가 안 먹힐 때 대비)
        string otherName = other.name.ToLower();
        if (otherName.Contains("horse") || otherName.Contains("player"))
        {
            Debug.Log("✅ [이름으로 감지] 탈출 시퀀스 시작!");
            StartEscape();
            return;
        }

        Debug.LogWarning($"⚠️ 뭔가 닿았지만 플레이어가 아님. 태그: {other.tag}");
    }

    private void StartEscape()
    {
        hasTriggered = true; // ★ [핵심] 플래그 켜기 (중복 방지)
        Debug.Log("🚪 탈출 조건 만족! 고속도로 씬으로 전환합니다.");

        HighwayManager highwayManager = FindObjectOfType<HighwayManager>();
        if (highwayManager != null)
        {
            highwayManager.StartEscapeSequence();
        }
        else
        {
            Debug.LogError("🚨 HighwayManager가 없습니다!");
        }
    }
}