using UnityEngine;
using UnityEngine.Splines;
using Unity.Mathematics;

public class PoliceCarSpline : MonoBehaviour
{
    [Header("Rail Settings")]
    public SplineContainer lane;

    [Header("Speed Settings")]
    public float minSpeed = 20f;
    public float maxSpeed = 30f;

    private float currentSpeed;
    private float distanceTraveled = 0f;
    private float splineLength;

    void Start()
    {
        if (lane != null)
        {
            splineLength = lane.CalculateLength();
        }

        currentSpeed = UnityEngine.Random.Range(minSpeed, maxSpeed);
        StartCoroutine(ChangeSpeedRoutine());

        // ★ [디버깅] Collider 확인
        Collider col = GetComponent<Collider>();
        if (col == null)
        {
            Debug.LogError($"❌ {gameObject.name}에 Collider가 없습니다!");
        }
        else if (!col.isTrigger)
        {
            Debug.LogError($"❌ {gameObject.name} Collider의 Is Trigger가 꺼져있습니다!");
        }
        else
        {
            Debug.Log($"✅ {gameObject.name} 충돌 설정 완료");
        }
    }

    System.Collections.IEnumerator ChangeSpeedRoutine()
    {
        yield return new WaitForSeconds(UnityEngine.Random.Range(0f, 1.0f));

        while (true)
        {
            yield return new WaitForSeconds(3.0f);
            currentSpeed = UnityEngine.Random.Range(minSpeed, maxSpeed);
        }
    }

    void Update()
    {
        if (lane == null) return;

        distanceTraveled += currentSpeed * Time.deltaTime;
        float t = distanceTraveled / splineLength;

        if (t >= 1f)
        {
            t = 1f;
        }

        Vector3 pos = lane.EvaluatePosition(t);
        Vector3 dir = lane.EvaluateTangent(t);

        transform.position = pos;

        if (dir != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(dir);
        }
    }

    // ★ [수정] OnCollisionEnter 사용 (물리 충돌)
    void OnCollisionEnter(Collision collision)
    {
        Collider other = collision.collider;

        // 일단 뭐든 닿으면 로그 출력
        Debug.Log($"🚨 [경찰차 {gameObject.name}] 충돌 감지! 이름: {other.name}, 태그: {other.tag}");

        // ★ [임시] 일단 말 이름이 포함되면 게임오버 (태그 무시)
        string otherName = other.name.ToLower();
        if (otherName.Contains("horse") || otherName.Contains("player"))
        {
            Debug.Log("🚨🚨🚨 [이름으로 감지] 경찰차 검거! GAME OVER 🚨🚨🚨");
            GameOver();
            return;
        }

        // Rigidbody 확인
        Rigidbody rb = other.attachedRigidbody;
        if (rb != null)
        {
            Debug.Log($"🔍 Rigidbody 발견! 오브젝트: {rb.name}, 태그: {rb.tag}");

            // Rigidbody 이름으로도 체크
            string rbName = rb.name.ToLower();
            if (rbName.Contains("horse") || rbName.Contains("player"))
            {
                Debug.Log("🚨🚨🚨 [Rigidbody 이름으로 감지] 경찰차 검거! GAME OVER 🚨🚨🚨");
                GameOver();
                return;
            }

            // 태그로도 체크
            if (rb.CompareTag("HorseChest") || rb.CompareTag("Player"))
            {
                Debug.Log("🚨🚨🚨 [태그로 감지] 경찰차 검거! GAME OVER 🚨🚨🚨");
                GameOver();
                return;
            }
        }

        // 직접 태그 확인
        if (other.CompareTag("HorseChest") || other.CompareTag("Player"))
        {
            Debug.Log("🚨🚨🚨 [직접 태그로 감지] 경찰차 검거! GAME OVER 🚨🚨🚨");
            GameOver();
        }
        else
        {
            Debug.LogWarning($"⚠️ 경찰차가 뭔가 건드렸지만 플레이어는 아님. 이름: {other.name}, 태그: {other.tag}");
        }
    }

    private void GameOver()
    {
        // ★ [나중에 구현] 게임오버 UI 표시
        // 일단 시간 멈춤
        Time.timeScale = 0f;
        Debug.Log("⏸️ 게임 정지 (Time.timeScale = 0)");
    }
}