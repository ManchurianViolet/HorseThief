using UnityEngine;

public class BalloonPaintColor : MonoBehaviour
{
    [SerializeField] private Color thisPaintColor;
    [SerializeField] private ParticleSystem colorChangeParticle;

    [SerializeField] private float explosionForce = 100f;
    [SerializeField] private float explosionRange = 4f;
    [SerializeField] private float explosionUpward = 5f;

    private PaperPainter paperPainter;
    private Material balloonMat;

    private void Start()
    {
        // 현재 메터리얼을 가져오고, 색깔을 변경하기
        balloonMat = GetComponent<Renderer>().material;
        balloonMat.color = thisPaintColor;
    }

    private void OnTriggerEnter(Collider _other)
    {
        // 플레이어와 닿을 시 폭발 힘 추가
        if (_other.gameObject.CompareTag("HorseChest"))
        {
            _other.attachedRigidbody.AddExplosionForce(explosionForce, this.transform.position, explosionRange, explosionUpward);
        }

        // 종이와 닿을 시 페인팅 색깔 전환
        if (_other.gameObject.CompareTag("Paper"))
        {
            if (paperPainter != null)
            {
                paperPainter.paintColor = thisPaintColor;
                Debug.Log($"색상 변경 시도: {thisPaintColor}");
            }
            else
            {
                Debug.LogWarning("paperPainter가 null입니다!");
            }
        }

        // 생성 파티클 생성
        ParticleSystem thisParticle = Instantiate(colorChangeParticle, this.transform.position, this.transform.rotation);

        // 물풍선 자신 파괴
        // 이유는 모르겠지만 Destroy함수를 조금 지연시켰더니 작동 잘됨...
        Destroy(this.gameObject, 0.1f);
    }

    public void SetPaperPainter(PaperPainter _painter)
    {
        paperPainter = _painter;
        if (paperPainter == null)
        {
            Debug.LogError("PaperPainter가 설정되지 않았습니다.");
        } 
    }
}
