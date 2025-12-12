using UnityEngine;
using TMPro;

public class MuseumPainter : MonoBehaviour
{
    [Header("UI Settings")]
    [SerializeField] private TextMeshProUGUI accuracyText;
    public float FinalAccuracy { get; private set; } = 0f;
    [Header("Painting Settings")]
    public Color paintColor = Color.blue;
    [SerializeField] private float brushSize = 20f;

    [Header("Canvas Settings")]
    [SerializeField] private Renderer myRenderer;
    [SerializeField] private Texture2D paperTexture;

    [Header("Target Settings")]
    [SerializeField] private Renderer referenceRenderer;

    private Texture2D modifiableTexture;
    private Color[] targetPixels;

    private int correctPixelsCount = 0;
    private int totalTargetAreaPixels = 0;

    private void Start()
    {
        CreateCleanCanvas();
        UpdateAccuracyUI(0); // 시작하자마자 0% 띄우기

        if (GameManager.Instance != null && GameManager.Instance.currentMissionTarget != null)
        {
            Texture2D target = GameManager.Instance.currentMissionTarget.targetTexture;
            SetTargetPainting(target);
        }
        else
        {
            // 테스트용 (타겟 없으면 종이 그대로 타겟 설정)
            if (paperTexture != null) SetTargetPainting(paperTexture);
        }
    }

    // 붓 감지
    private void OnTriggerEnter(Collider other)
    {
        // 태그 확인 로그
        if (other.CompareTag("Brush"))
        {
            Debug.Log("붓이 닿았습니다! (Tag 확인 OK)");
        }
        else
        {
            Debug.LogWarning($"무언가 닿았지만 태그가 Brush가 아닙니다. 닿은 것: {other.name}, 태그: {other.tag}");
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Brush"))
        {
            TriggerCheck(other.gameObject); // 이름 변경
        }
    }
    private void TriggerCheck(GameObject brush)
    {
        Vector3 brushPos = brush.transform.position;
        Vector3 direction = transform.up; // 캔버스의 up 방향
        Ray ray1 = new Ray(brushPos, direction);
        Ray ray2 = new Ray(brushPos, -direction);
        RaycastHit hit;

        // "Paper" 레이어 또는 적절한 레이어로 설정
        if (Physics.Raycast(ray1, out hit, 1.5f) ||
            Physics.Raycast(ray2, out hit, 1.5f))
        {
            if (hit.collider.gameObject == this.gameObject)
            {
                PaintAtPosition(hit.point); // UV 대신 월드 좌표 사용
            }
        }
    }
    private void PaintAtPosition(Vector3 worldPos)
    {
        Vector3 localPos = transform.InverseTransformPoint(worldPos);
        Vector2 uv = new Vector2((localPos.x / 10) + 0.5f, (localPos.z / 10) + 0.5f);

        // UV 범위 체크
        if (uv.x < 0 || uv.x > 1 || uv.y < 0 || uv.y > 1) return;

        // PaperPainter처럼 반전
        uv.x = 1f - uv.x;
        uv.y = 1f - uv.y;

        int x = (int)(uv.x * modifiableTexture.width);
        int y = (int)(uv.y * modifiableTexture.height);

        // 나머지는 기존 PaintAtUV 로직 사용
        PaintAtUV(new Vector2((float)x / modifiableTexture.width,
                              (float)y / modifiableTexture.height));
    }
    private void PaintAtUV(Vector2 uv)
    {
        int x = (int)(uv.x * modifiableTexture.width);
        int y = (int)(uv.y * modifiableTexture.height);

        // 캔버스 범위 밖인지 확인
        if (x < 0 || x >= modifiableTexture.width || y < 0 || y >= modifiableTexture.height) return;

        int brushSizeInt = (int)brushSize;
        bool painted = false;

        for (int i = -brushSizeInt; i <= brushSizeInt; i++)
        {
            for (int j = -brushSizeInt; j <= brushSizeInt; j++)
            {
                int pixelX = x + i;
                int pixelY = y + j;

                if (pixelX >= 0 && pixelX < modifiableTexture.width && pixelY >= 0 && pixelY < modifiableTexture.height)
                {
                    float dist = i * i + j * j;
                    if (dist < brushSize * brushSize)
                    {
                        modifiableTexture.SetPixel(pixelX, pixelY, paintColor);
                        painted = true;
                    }
                }
            }
        }

        if (painted)
        {
            modifiableTexture.Apply();
            RecalculateAccuracy(); // 칠할 때마다 점수 갱신
        }
    }

    private void CreateCleanCanvas()
    {
        if (paperTexture == null) return;
        // Read/Write 체크 안되어있으면 여기서 에러 날 수 있음
        try
        {
            modifiableTexture = new Texture2D(paperTexture.width, paperTexture.height, TextureFormat.RGBA32, false);
            modifiableTexture.SetPixels(paperTexture.GetPixels());
            modifiableTexture.Apply();
            if (myRenderer != null) myRenderer.material.mainTexture = modifiableTexture;
        }
        catch (UnityException e)
        {
            Debug.LogError($"텍스처 Read/Write 설정을 확인하세요! 에러: {e.Message}");
        }
    }

    public void SetTargetPainting(Texture2D target)
    {
        if (referenceRenderer != null) referenceRenderer.material.mainTexture = target;
        if (target.isReadable)
        {
            targetPixels = target.GetPixels();
            totalTargetAreaPixels = targetPixels.Length;
        }
    }

    private void RecalculateAccuracy()
    {
        if (targetPixels == null) return;
        correctPixelsCount = 0;

        // 성능을 위해 샘플링 (10칸씩 건너뛰며 검사)
        int step = 5;
        int sampledTotal = 0;

        for (int y = 0; y < modifiableTexture.height; y += step)
        {
            for (int x = 0; x < modifiableTexture.width; x += step)
            {
                float scaleX = (float)GameManager.Instance.currentMissionTarget.targetTexture.width / modifiableTexture.width;
                float scaleY = (float)GameManager.Instance.currentMissionTarget.targetTexture.height / modifiableTexture.height;
                int targetX = Mathf.Min((int)(x * scaleX), GameManager.Instance.currentMissionTarget.targetTexture.width - 1);
                int targetY = Mathf.Min((int)(y * scaleY), GameManager.Instance.currentMissionTarget.targetTexture.height - 1);
                int targetIndex = targetY * GameManager.Instance.currentMissionTarget.targetTexture.width + targetX;

                if (targetIndex < targetPixels.Length)
                {
                    Color targetColor = targetPixels[targetIndex];
                    Color currentColor = modifiableTexture.GetPixel(x, y);
                    if (ColorDifference(currentColor, targetColor) < 0.25f) correctPixelsCount++;
                    sampledTotal++;
                }
            }
        }

        float accuracy = 0f;
        if (sampledTotal > 0) accuracy = (float)correctPixelsCount / sampledTotal * 100f;
        FinalAccuracy = accuracy;
        UpdateAccuracyUI(accuracy);
    }

    private void UpdateAccuracyUI(float accuracy)
    {
        float truncated = Mathf.Floor(accuracy * 10f) / 10f;
        if (accuracyText != null) accuracyText.text = $"Accuracy: {truncated}%";
        else Debug.Log($"점수 갱신됨: {truncated}% (UI 연결 안됨)");
    }

    private float ColorDifference(Color c1, Color c2)
    {
        return Mathf.Sqrt(Mathf.Pow(c1.r - c2.r, 2) + Mathf.Pow(c1.g - c2.g, 2) + Mathf.Pow(c1.b - c2.b, 2));
    }

    public Texture2D GetFinalTexture() { return modifiableTexture; }
}