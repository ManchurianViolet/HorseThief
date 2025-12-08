using UnityEngine;

public class MuseumChangePaintColor : MonoBehaviour
{
    // ★ [변경] PaperPainter 대신 MuseumPainter를 연결하도록 수정
    [SerializeField] private MuseumPainter museumPainter;

    [SerializeField] private Color thisPaintColor;
    [SerializeField] private ParticleSystem colorChangeParticle;
    [SerializeField] private GameObject playerBrush; // 말 입에 있는 붓

    private Material thisBrushMaterial;

    private void Start()
    {
        thisBrushMaterial = GetComponent<Renderer>().material;
    }

    private void OnTriggerEnter(Collider _other)
    {
        // 말 머리나 가슴이 닿으면
        if (_other.gameObject.CompareTag("HorseChest") || _other.gameObject.CompareTag("HorseHead"))
        {
            Debug.Log("미술관용 물감 변경!");

            // 1. 파티클 재생
            if (colorChangeParticle != null)
                Instantiate(colorChangeParticle, this.transform.position, this.transform.rotation);

            // 2. ★ [변경] MuseumPainter에게 색상 전달
            if (museumPainter != null)
            {
                museumPainter.paintColor = thisPaintColor;
            }

            // 3. 말 입에 있는 붓 켜기 & 색깔 바꾸기 (기존 동일)
            if (playerBrush != null)
            {
                if (!playerBrush.activeInHierarchy) playerBrush.SetActive(true);

                Renderer brushRenderer = playerBrush.GetComponent<Renderer>();
                if (brushRenderer != null && thisBrushMaterial != null)
                {
                    brushRenderer.material = thisBrushMaterial;
                }
            }
        }
    }
}