using UnityEngine;

public class Rotate : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [SerializeField] private float hoverAmplitude = 0.5f; // 위아래 이동 거리 (미터)
    [SerializeField] private float hoverSpeed = 2f;      // 호버링 속도

    private Vector3 startPos; // 오브젝트의 초기 위치
    void Start()
    {
        startPos = transform.position;
    }

    private void Update()
    {
        // 매 프레임마다 오브젝트를 회전시킴
        transform.Rotate(Vector3.up, 20 * Time.deltaTime);
        float hoverOffset = Mathf.Sin(Time.time * hoverSpeed) * hoverAmplitude;
        transform.position = startPos + new Vector3(0, hoverOffset, 0);
    }
}

