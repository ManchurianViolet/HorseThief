using UnityEngine;
using UnityEngine.Splines; // ★ 필수! 이게 있어야 스플라인을 다룸
using Unity.Mathematics;   // 수학 계산용

public class PoliceCarSpline : MonoBehaviour
{
    [Header("Rail Settings")]
    public SplineContainer lane; // 이 차가 달릴 레일 (인스펙터에서 연결)

    [Header("Speed Settings")]
    public float minSpeed = 20f; // 최소 속도 (km/h 느낌)
    public float maxSpeed = 30f; // 최대 속도

    private float currentSpeed;
    private float distanceTraveled = 0f; // 출발점으로부터 이동한 거리
    private float splineLength;          // 레일 전체 길이

    void Start()
    {
        // 1. 레일 길이 계산 (끝이 어딘지 알아야 함)
        if (lane != null)
        {
            splineLength = lane.CalculateLength();
        }

        // 2. 초기 속도 설정
        currentSpeed = UnityEngine.Random.Range(minSpeed, maxSpeed);

        // 3. 3초마다 속도 바꾸는 루틴 시작
        StartCoroutine(ChangeSpeedRoutine());
    }

    // 3초마다 랜덤 속도 변경 (기획 반영)
    System.Collections.IEnumerator ChangeSpeedRoutine()
    {
        // 차마다 박자가 똑같지 않게 시작 딜레이 약간 줌
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

        // 1. 거리 증가 (속도 * 시간)
        distanceTraveled += currentSpeed * Time.deltaTime;

        // 2. 레일 위의 위치 계산 (0 ~ 1 사이의 비율로 변환해야 함)
        // t = 현재간거리 / 전체길이
        float t = distanceTraveled / splineLength;

        // 레일 끝에 도달했으면 멈추거나 계속 유지
        if (t >= 1f)
        {
            t = 1f;
            // (선택) 여기서 도착 처리 로직 가능
        }

        // 3. ★ 핵심: 스플라인 위의 좌표와 방향 가져오기
        Vector3 pos = lane.EvaluatePosition(t); // 그 지점의 위치
        Vector3 dir = lane.EvaluateTangent(t);  // 그 지점의 앞방향(벡터)

        // 4. 적용 (본드처럼 붙이기)
        transform.position = pos;

        // 방향은 좀 더 부드럽게 회전 (선택사항)
        if (dir != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(dir);
        }
    }

    // 충돌 감지 (게임오버)
    void OnTriggerEnter(Collider other)
    {
        // 말(HorseChest)이나 플레이어 태그에 닿으면
        if (other.CompareTag("HorseChest") || other.CompareTag("Player"))
        {
            Debug.Log("🚨 경찰차 검거! GAME OVER");
            // 나중에 TimeManager.GameOver() 연결
        }
    }
}