using UnityEngine;

public class ChangePaintColor : MonoBehaviour
{
    [SerializeField] private PaperPainter paperPainter;
    [SerializeField] private Color thisPaintColor;
    [SerializeField] private ParticleSystem colorChangeParticle;
    [SerializeField] private GameObject playerBrush;

    private Material thisBrushMaterial;

    private void Start()
    {
        thisBrushMaterial = GetComponent<Renderer>().material;
    }

    private void OnTriggerEnter(Collider _other)
    {
        if (_other.gameObject.CompareTag("HorseChest") || _other.gameObject.CompareTag("HorseHead"))
        {
            Debug.Log("���� �� ����");

            // ���� ��ƼŬ ����
            Instantiate(colorChangeParticle, this.transform.position, this.transform.rotation);

            // ���� ���� ������ ����
            paperPainter.paintColor = thisPaintColor;

            // �÷��̾� �귯�ð� ��Ȱ��ȭ�� Ȱ��ȭ�ϱ�
            if (!playerBrush.activeInHierarchy)
            {
                playerBrush.SetActive(true);
            }

            // �÷��̾� �귯���� ��Ƽ���� ����
            if (playerBrush != null && thisBrushMaterial != null)
            {
                Renderer brushRenderer = playerBrush.GetComponent<Renderer>();
                if (brushRenderer != null)
                {
                    brushRenderer.material = thisBrushMaterial;
                    Debug.Log("�÷��̾� �귯�� ��Ƽ���� ���� �Ϸ�!");
                }
                else
                {
                    Debug.LogError("playerBrush�� Renderer�� �����ϴ�!");
                }
            }
        }
    }

}
