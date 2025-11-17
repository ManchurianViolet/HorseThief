using UnityEngine;

public class CarManager : MonoBehaviour
{
    // Inspector에서 할당할 carPrefab. 이 프리팹에 "Car" 태그가 설정되어 있어야 함.
    [SerializeField]
    private GameObject carPrefab;

    // 오브젝트가 생성될 위치를 지정합니다.
    [SerializeField]
    private Vector3 spawnPosition = Vector3.zero;

    // 몇 초마다 체크할지 지정(예를 들어, 1초)
    [SerializeField]
    private float checkInterval = 5.0f;

    private float timer = 0f;

    void Update()
    {
        timer += Time.deltaTime;

        // 지정한 시간 간격마다 확인
        if (timer >= checkInterval)
        {
            Instantiate(carPrefab, spawnPosition, Quaternion.identity);
            timer = 0f;
        }

    }
}
