using UnityEngine;
using Unity.Cinemachine;

public class CameraModeController : MonoBehaviour
{
    [Header("Cameras")]
    [SerializeField] private CinemachineCamera followCamera;
    [SerializeField] private CinemachineCamera freeLookCamera;

    [Header("References")]
    [SerializeField] private Camera mainUnityCamera;
    [SerializeField] private Transform playerTransform; // ★ 말의 위치 필수!

    private int lowPriority = 0;
    private int highPriority = 20;

    void Start()
    {
        if (mainUnityCamera == null) mainUnityCamera = Camera.main;

        if (followCamera != null) followCamera.Priority = 10;
        if (freeLookCamera != null) freeLookCamera.Priority = lowPriority;
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(1))
        {
            SyncCameraAngle();
            if (freeLookCamera != null) freeLookCamera.Priority = highPriority;
        }
        else if (Input.GetMouseButtonUp(1))
        {
            if (freeLookCamera != null) freeLookCamera.Priority = lowPriority;
        }
    }

    private void SyncCameraAngle()
    {
        if (mainUnityCamera == null || freeLookCamera == null || playerTransform == null) return;

        var orbital = freeLookCamera.GetComponent<CinemachineOrbitalFollow>();

        if (orbital != null)
        {
            // 1. [수평 각도 계산 수정]
            // "카메라 각도"에서 "말의 각도"를 뺍니다. (상대 각도 계산)
            // 예: 말이 90도(동)를 보고, 카메라도 90도(동)를 보면 -> 차이는 0 (정뒤)
            float relativeYaw = mainUnityCamera.transform.eulerAngles.y - playerTransform.eulerAngles.y;

            // 이 상대 각도를 넣어줘야 말이 회전해 있어도 엉뚱한 곳으로 안 튑니다.
            orbital.HorizontalAxis.Value = relativeYaw;


            // 2. [수직 각도(높이) 계산]
            float currentPitch = mainUnityCamera.transform.eulerAngles.x;
            if (currentPitch > 180) currentPitch -= 360;

            // 감도 조절 (100f가 너무 둔하면 80f 정도로 낮춰보세요)
            orbital.VerticalAxis.Value = 0.5f + (currentPitch / 100f);
        }
    }
}