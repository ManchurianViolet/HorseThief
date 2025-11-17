using UnityEngine;

public class ThirdPersonCamera : MonoBehaviour
{
    // 따라갈 대상 (말의 루트 오브젝트)
    public Transform target;

    // 카메라와 대상 사이의 거리
    [SerializeField] private float distance = 4.76f;

    // 카메라의 높이 (대상 기준)
    [SerializeField] private float height = 2f;

    // 카메라의 오른쪽 오프셋 (오른쪽 뒤에서 보기 위해)
    [SerializeField] private float rightOffset = -1.03f;

    // 카메라 이동의 부드러움 (값이 작을수록 빠르게 따라감)
    [SerializeField] private float positionSmoothness = 0.91f;

    void Start()
    {
        // 대상이 설정되지 않았다면 경고 출력
        if (target == null)
        {
            Debug.LogError("카메라의 타겟(말)이 설정되지 않았습니다. 타겟을 설정해주세요.");
            return;
        }
    }

    void LateUpdate()
    {
        if (target == null) return;

        // 카메라의 목표 위치 계산
        Vector3 desiredPosition = CalculateCameraPosition();

        // 카메라 위치를 부드럽게 이동
        transform.position = Vector3.Lerp(transform.position, desiredPosition, positionSmoothness);

        // 카메라가 대상을 바라보도록 설정
        transform.LookAt(target.position + Vector3.up * height);

        // 디버깅: 카메라 방향 확인
        Debug.DrawRay(transform.position, transform.forward * 5f, Color.blue, 0.1f);
    }
    // 카메라의 목표 위치를 계산하는 함수
    private Vector3 CalculateCameraPosition()
    {
        // 캐릭터의 방향 (target.forward)
        Vector3 direction = -target.forward;

        // 카메라의 위치: 대상 위치에서 방향 반대쪽으로 거리만큼 떨어짐
        Vector3 offset = -direction * distance;

        // 오른쪽 오프셋 추가 (오른쪽 뒤에서 보기 위해)
        offset += target.right * rightOffset;

        // 높이 오프셋 추가
        offset += Vector3.up * height;

        // 최종 위치: 대상 위치 + 오프셋
        Vector3 desiredPosition = target.position + offset;

        return desiredPosition;
    }

}