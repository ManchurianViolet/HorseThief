using UnityEngine;

public class CameraFollowYRotation : MonoBehaviour
{
    public Transform target; // 따라갈 물체 (플레이어)
    public Vector3 offset = new Vector3(0f, 2f, -5f); // 카메라와 물체 간의 오프셋
    public float smoothSpeed = 0.125f; // 카메라 이동 부드러움 (0~1, 낮을수록 부드러움)
    public float rotationSmoothSpeed = 0.125f; // 회전 부드러움

    void LateUpdate()
    {
        if (target == null) return;

        // 목표 위치 계산 (물체 위치 + 오프셋)
        Vector3 desiredPosition = target.position + target.TransformDirection(offset);
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
        transform.position = smoothedPosition;

        // Y축 회전만 반영
        float targetYRotation = target.eulerAngles.y;
        float currentYRotation = transform.eulerAngles.y;
        float smoothedYRotation = Mathf.LerpAngle(currentYRotation, targetYRotation, rotationSmoothSpeed);

        // X, Z 회전은 고정, Y 회전만 업데이트
        transform.rotation = Quaternion.Euler(0f, smoothedYRotation, 0f);
    }
}