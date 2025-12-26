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

        // ★ [체크] 이제 물리 충돌을 해야 하므로 Collider가 Trigger가 아니어야 합니다.
        Collider col = GetComponent<Collider>();
        if (col != null && col.isTrigger)
        {
            Debug.LogWarning($"⚠️ {gameObject.name}의 Collider가 'Is Trigger'로 체크되어 있습니다! 말이 튕겨 나가려면 체크를 해제해야 합니다.");
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

        // 끝에 도달하면 다시 처음(0)은 아니고 1에서 멈추거나 루프 설정에 따라 다름.
        // 여기서는 그냥 진행하게 둠 (Spline이 Closed Loop라면 t가 1 넘어도 계속 돔)
        if (lane.Spline.Closed && t > 1f)
        {
            distanceTraveled -= splineLength; // 거리 초기화 (무한 루프)
        }
        else if (t >= 1f)
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

    // ★ 기존의 OnCollisionEnter와 GameOver 함수는 모두 삭제했습니다.
    // 이제 충돌 처리는 말(Player)에 붙은 'PlayerCollisionHandler'가 담당합니다!
}