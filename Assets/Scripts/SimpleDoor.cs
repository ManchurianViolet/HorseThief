using UnityEngine;

public class SimpleDoor : MonoBehaviour
{
    [SerializeField] private Vector3 openPositionOffset = new Vector3(0, 3, 0); // 위로 3미터 열림
    [SerializeField] private float speed = 2.0f;

    private bool isOpen = false;
    private Vector3 targetPos;

    void Start()
    {
        targetPos = transform.position; // 처음엔 제자리
    }

    void Update()
    {
        // 부드럽게 이동
        transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * speed);
    }

    // MuseumHacking에서 이 함수를 부를 겁니다!
    public void OpenDoor()
    {
        isOpen = true;
        // 현재 위치에서 위로(또는 설정한 방향으로) 이동 목표 설정
        targetPos = transform.position + openPositionOffset;
    }
}