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

    // ★ [추가] 감지 설정
    [Header("Detection Settings")]
    [SerializeField] private LayerMask canvasLayer; // 캔버스 레이어
    [SerializeField] private float raycastDistance = 3f; // 감지 거리 (기존 1.5 -> 3)
    [SerializeField] private bool useDirectRaycast = false; // 대안 모드

    private Texture2D modifiableTexture;
    private Color[] targetPixels;

    private int correctPixelsCount = 0;
    private int totalTargetAreaPixels = 0;

    private void Start()
    {
        CreateCleanCanvas();
        UpdateAccuracyUI(0);

        if (GameManager.Instance != null && GameManager.Instance.currentMissionTarget != null)
        {
            Texture2D target = GameManager.Instance.currentMissionTarget.targetTexture;
            SetTargetPainting(target);
        }
        else
        {
            if (paperTexture != null) SetTargetPainting(paperTexture);
        }

        // ★ [추가] 레이어 마스크 자동 설정
        if (canvasLayer == 0)
        {
            canvasLayer = LayerMask.GetMask("Paper");
            if (canvasLayer == 0)
            {
                Debug.LogWarning("⚠️ 'Paper' 레이어를 찾을 수 없습니다! 모든 레이어를 감지합니다.");
                canvasLayer = ~0; // 모든 레이어
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
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
            TriggerCheck(other.gameObject);
        }
    }

    // ★ [개선] Raycast 로직 수정
    private void TriggerCheck(GameObject brush)
    {
        Vector3 brushPos = brush.transform.position;
        Vector3 direction = transform.up;

        RaycastHit hit;

        // ★ [수정 1] 레이어 마스크 추가
        if (Physics.Raycast(brushPos, direction, out hit, raycastDistance, canvasLayer) ||
            Physics.Raycast(brushPos, -direction, out hit, raycastDistance, canvasLayer))
        {
            if (hit.collider.gameObject == this.gameObject)
            {
                PaintAtPosition(hit.point);
                return;
            }
        }

        // ★ [추가] 대안 모드: Raycast 실패 시 직접 계산
        if (useDirectRaycast)
        {
            // 붓 위치를 캔버스의 로컬 좌표로 변환
            Vector3 localPos = transform.InverseTransformPoint(brushPos);

            // 캔버스 평면과의 거리 체크 (Y축 거리)
            if (Mathf.Abs(localPos.y) < raycastDistance)
            {
                // 캔버스 범위 내에 있는지 확인
                if (Mathf.Abs(localPos.x) < 5f && Mathf.Abs(localPos.z) < 5f)
                {
                    PaintAtPosition(brushPos);
                    Debug.Log("✅ [직접 모드] 그리기 성공!");
                }
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

        PaintAtUV(new Vector2((float)x / modifiableTexture.width,
                              (float)y / modifiableTexture.height));
    }

    private void PaintAtUV(Vector2 uv)
    {
        int x = (int)(uv.x * modifiableTexture.width);
        int y = (int)(uv.y * modifiableTexture.height);

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
            RecalculateAccuracy();
        }
    }

    private void CreateCleanCanvas()
    {
        if (paperTexture == null) return;
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