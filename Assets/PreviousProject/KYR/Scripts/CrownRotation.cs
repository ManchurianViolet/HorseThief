using UnityEngine;

public class CrownRotation : MonoBehaviour
{
    [SerializeField] private float rotationSpeedY = 10f; // 회전속도
    [SerializeField] private Vector3 startPosition = new Vector3(0f, 10f, 0f); // 시작 위치 (하늘)
    [SerializeField] private Vector3 targetPosition = new Vector3(0f, 2f, 0f);  // 목표 위치
    [SerializeField] private float descendSpeed = 2f;                          // 내려오는 속도 (초 단위)
    [SerializeField] private float floatAmplitude = 0.5f;                      // 위아래 움직임 크기
    [SerializeField] private float floatFrequency = 1f;                        // 위아래 움직임 속도

    private bool isDescending = true; // 내려오는 중인지 체크
    private float descendProgress = 0f; // 내려오는 진행률

    void Start()
    {
        // 왕관의 초기 위치 설정
        transform.position = startPosition;
    }


    private void Update()
    {
        // 회전은 상시
        this.transform.Rotate(0, rotationSpeedY * Time.deltaTime, 0);

        if (isDescending)
        {
            // 내려오는 동작
            descendProgress += Time.deltaTime / descendSpeed;
            transform.position = Vector3.Lerp(startPosition, targetPosition, descendProgress);

            // 목표 위치에 도달했는지 체크
            if (descendProgress >= 1f)
            {
                isDescending = false;
                transform.position = targetPosition; // 정확히 목표 위치로
            }
        }
        else
        {
            // 목표 위치에서 위아래로 부드럽게 떠다니기
            float floatOffset = Mathf.Sin(Time.time * floatFrequency) * floatAmplitude;
            transform.position = targetPosition + new Vector3(0f, floatOffset, 0f);
        }
    }
}
