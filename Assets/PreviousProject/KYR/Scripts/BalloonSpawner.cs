using System.Collections.Generic;
using UnityEngine;

public class BalloonSpawner : MonoBehaviour
{
    [SerializeField] private List<GameObject> balloons; // 물풍선 프리팹 인덱스
    [SerializeField] private GameObject paperBig; // 큰 종이 오브젝트(물풍선 프리팹 전달용)

    [SerializeField] private float spawnTime = 2f; // 스폰 시간 간격
    [SerializeField] private float spawnHeight = 10f; // 스폰 높이
    [SerializeField] private float SpawnWidthRange = 5f; // 스폰 X축 범위

    private int currentBalloonIndex = 0; // 현재 스폰할 물풍선 인덱스



    private void Start()
    {
        Invoke("SpawnBalloons", spawnTime);
    }

    private void SpawnBalloons()
    {
        // 리스트가 비어있거나 null이면 스폰 정지
        if (balloons == null || balloons.Count == 0)
        {
            Debug.LogWarning("물풍선 리스트가 비어있습니다! 프리팹을 추가해주세요.");
            return;
        }

        // 현재 인덱스 물풍선 선택
        GameObject balloonToSpawn = balloons[currentBalloonIndex];

        // 랜덤 X, Z 위치 계산
        float randomX = Random.Range(-SpawnWidthRange, SpawnWidthRange);
        float randomZ = Random.Range(-SpawnWidthRange, SpawnWidthRange);
        Vector3 spawnPos = new Vector3(this.transform.position.x + randomX, spawnHeight, this.transform.position.z + randomZ);

        // 물풍선 생성
        GameObject balloon = Instantiate(balloonToSpawn, spawnPos, Quaternion.identity);

        // 물풍선에 paparBig 코드 전달
        BalloonPaintColor balloonPaintColor = balloon.GetComponent<BalloonPaintColor>();
        if (balloonPaintColor != null && paperBig != null)
        {
            balloonPaintColor.SetPaperPainter(paperBig.GetComponent<PaperPainter>());
        }

        // 다음 물풍선으로 인덱스 업데이트
        currentBalloonIndex = (currentBalloonIndex + 1) % balloons.Count;
        // % 연산자는 인덱스가 리스트 크기를 넘지 않도록 순환시킴
        // 예: 0, 1, 2, 0, 1, 2, ...

        // 일정 시간 후 함수 재실행
        Invoke("SpawnBalloons", spawnTime);
    }


}
