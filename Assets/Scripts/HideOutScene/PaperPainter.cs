using UnityEngine;
using TMPro;

public class PaperPainter : MonoBehaviour
{
    [Header("=== Hideout Integration Settings ===")]
    [SerializeField] private Transform playPosition;
    [SerializeField] private Transform getOutPosition;
    [SerializeField] private GameObject gameUIPanel;

    [Header("=== New UI Settings ===")]
    [SerializeField] private TextMeshProUGUI accuracyText;

    [Header("=== Reference Settings ===")]
    [SerializeField] private Renderer referenceRenderer;
    [SerializeField] private Texture2D[] targetGallery;

    [Header("=== Player Settings ===")]
    [SerializeField] private GameObject playerBrush;

    [Header("=== Painting Settings ===")]
    public Color paintColor = Color.blue;
    [SerializeField] private Texture2D paperTexture;
    [SerializeField] private float brushSize = 20f;

    [Header("=== Paper Options ===")]
    [SerializeField] private Color paperBackgroundColor = new Color(0.9f, 0.85f, 0.7f, 1f);
    [SerializeField] private bool useTintedPaper = true;

    // 내부 상태 변수
    private GameObject currentPlayer;
    private bool isTrainingActive = false;

    private Texture2D modifiableTexture;
    private Renderer myRenderer;
    private Texture2D currentTarget;

    private Color[] targetPixels;
    private bool[] pixelChecked;
    private int correctPixelsCount = 0;
    private int totalTargetAreaPixels = 0;
    private int totalWhiteAreaPixels = 0;

    private void Start()
    {
        myRenderer = GetComponent<Renderer>();
        if (gameUIPanel != null) gameUIPanel.SetActive(false);
        CreateCleanCanvas();
    }

    private void Update()
    {
        if (!isTrainingActive) return;

        if (Input.GetKeyDown(KeyCode.Y)) SetNewRandomPainting();
        if (Input.GetKeyDown(KeyCode.C)) AutoCompleteTarget();
        if (Input.GetKeyDown(KeyCode.Escape)) StopTraining();
    }

    public void StartTraining(GameObject player)
    {
        isTrainingActive = true;
        currentPlayer = player;

        if (currentPlayer != null && playPosition != null)
        {
            Rigidbody rb = currentPlayer.GetComponent<Rigidbody>();
            if (rb != null) rb.isKinematic = true;
            currentPlayer.transform.position = playPosition.position;
            currentPlayer.transform.rotation = playPosition.rotation;
            if (rb != null) rb.isKinematic = false;
        }

        if (gameUIPanel != null) gameUIPanel.SetActive(true);
        SetNewRandomPainting();
    }

    public void StopTraining()
    {
        isTrainingActive = false;
        if (gameUIPanel != null) gameUIPanel.SetActive(false);

        if (currentPlayer != null && getOutPosition != null)
        {
            Rigidbody rb = currentPlayer.GetComponent<Rigidbody>();
            if (rb != null) rb.isKinematic = true;
            currentPlayer.transform.position = getOutPosition.position;
            currentPlayer.transform.rotation = getOutPosition.rotation;
            if (rb != null) rb.isKinematic = false;
            currentPlayer = null;
        }

        if (playerBrush != null)
        {
            playerBrush.SetActive(false);
        }
    }

    private void SetNewRandomPainting()
    {
        Debug.Log("새로운 그림을 세팅합니다.");
        CreateCleanCanvas();

        if (targetGallery != null && targetGallery.Length > 0)
        {
            int randomIndex = Random.Range(0, targetGallery.Length);
            currentTarget = targetGallery[randomIndex];
            if (referenceRenderer != null) referenceRenderer.material.mainTexture = currentTarget;
            PrepareTargetData(currentTarget);
        }
        else
        {
            Debug.LogError("Target Gallery가 비어있습니다!");
        }

        correctPixelsCount = 0;
        UpdateAccuracyUI(0f);
    }

    private void OnTriggerEnter(Collider _other)
    {
        if (!isTrainingActive) return;
        if (_other.gameObject.CompareTag("Brush"))
        {
            TriggerCheck(_other.gameObject);
        }
    }

    private void OnTriggerStay(Collider _other)
    {
        if (!isTrainingActive) return;
        if (_other.gameObject.CompareTag("Brush"))
        {
            TriggerCheck(_other.gameObject);
        }
    }

    private void OnTriggerExit(Collider _other)
    {
        if (!isTrainingActive) return;
        if (_other.gameObject.CompareTag("Brush"))
        {
            // 실시간 판정이므로 Exit에서 특별히 할 일 없음
        }
    }

    private void TriggerCheck(GameObject _checkObject)
    {
        Vector3 brushPos = _checkObject.transform.position;
        Vector3 direction = transform.up;
        Ray ray1 = new Ray(brushPos, direction);
        Ray ray2 = new Ray(brushPos, -direction);
        RaycastHit hit;

        if (Physics.Raycast(ray1, out hit, 1.5f, LayerMask.GetMask("Paper")) ||
            Physics.Raycast(ray2, out hit, 1.5f, LayerMask.GetMask("Paper")))
        {
            if (hit.collider.gameObject == this.gameObject)
            {
                PaintAtPosition(hit.point);
            }
        }
    }

    private void PaintAtPosition(Vector3 _worldPos)
    {
        if (!isTrainingActive) return;
        if (currentTarget == null) return;

        Vector3 localPos = transform.InverseTransformPoint(_worldPos);
        Vector2 uv = new Vector2((localPos.x / 10) + 0.5f, (localPos.z / 10) + 0.5f);

        if (uv.x < 0 || uv.x > 1 || uv.y < 0 || uv.y > 1) return;
        uv.x = 1f - uv.x;
        uv.y = 1f - uv.y;

        int x = (int)(uv.x * modifiableTexture.width);
        int y = (int)(uv.y * modifiableTexture.height);
        int brushSizeInt = Mathf.CeilToInt(brushSize);

        bool painted = false;

        for (int i = -brushSizeInt; i <= brushSizeInt; i++)
        {
            for (int j = -brushSizeInt; j <= brushSizeInt; j++)
            {
                int pixelX = x + i;
                int pixelY = y + j;

                if (pixelX >= 0 && pixelX < modifiableTexture.width &&
                    pixelY >= 0 && pixelY < modifiableTexture.height)
                {
                    float distance = Mathf.Sqrt(i * i + j * j);
                    if (distance <= brushSize)
                    {
                        int paperIndex = pixelY * modifiableTexture.width + pixelX;
                        Color oldColor = modifiableTexture.GetPixel(pixelX, pixelY);

                        if (ColorDifference(oldColor, paintColor) > 0.01f)
                        {
                            if (pixelChecked != null && pixelChecked.Length > paperIndex)
                                pixelChecked[paperIndex] = false;

                            modifiableTexture.SetPixel(pixelX, pixelY, paintColor);
                            painted = true;
                        }
                    }
                }
            }
        }

        if (painted)
        {
            modifiableTexture.Apply();
            RecalculateAccuracy(); // ★ 실시간 판정!
        }
    }

    private void CreateCleanCanvas()
    {
        if (paperTexture == null) return;

        bool useMipMap = paperTexture.mipmapCount > 1;
        modifiableTexture = new Texture2D(paperTexture.width, paperTexture.height, TextureFormat.RGBA32, useMipMap);

        if (useTintedPaper)
        {
            Color[] pixels = paperTexture.GetPixels();
            for (int i = 0; i < pixels.Length; i++)
            {
                if (pixels[i].r > 0.9f && pixels[i].g > 0.9f && pixels[i].b > 0.9f)
                    pixels[i] = paperBackgroundColor;
            }
            modifiableTexture.SetPixels(pixels);
        }
        else
        {
            modifiableTexture.SetPixels(paperTexture.GetPixels());
        }

        modifiableTexture.Apply();
        myRenderer.material.mainTexture = modifiableTexture;
    }

    private void PrepareTargetData(Texture2D target)
    {
        if (!target.isReadable)
        {
            Debug.LogError($"'{target.name}' 텍스처의 Read/Write Enabled를 체크해주세요!");
            return;
        }

        targetPixels = target.GetPixels();
        pixelChecked = new bool[modifiableTexture.width * modifiableTexture.height];
        totalTargetAreaPixels = 0;
        totalWhiteAreaPixels = 0;

        for (int y = 0; y < modifiableTexture.height; y++)
        {
            for (int x = 0; x < modifiableTexture.width; x++)
            {
                float scale = (float)target.width / modifiableTexture.width;
                int targetX = Mathf.Min((int)(x * scale), target.width - 1);
                int targetY = Mathf.Min((int)(y * scale), target.height - 1);
                int targetIndex = targetY * target.width + targetX;

                Color targetColor = targetPixels[targetIndex];
                float brightness = (targetColor.r + targetColor.g + targetColor.b) / 3f;

                if (useTintedPaper) totalTargetAreaPixels++;
                else { if (brightness < 0.8f) totalTargetAreaPixels++; else totalWhiteAreaPixels++; }
            }
        }
        RecalculateAccuracy();
    }

    private void RecalculateAccuracy()
    {
        if (targetPixels == null) return;

        correctPixelsCount = 0;

        // ★ 성능 최적화: 샘플링 방식 (5칸씩 건너뛰며 체크)
        int step = 5;
        int sampledTotal = 0;
        int sampledCorrect = 0;

        for (int y = 0; y < modifiableTexture.height; y += step)
        {
            for (int x = 0; x < modifiableTexture.width; x += step)
            {
                float scale = (float)currentTarget.width / modifiableTexture.width;
                int targetX = Mathf.Min((int)(x * scale), currentTarget.width - 1);
                int targetY = Mathf.Min((int)(y * scale), currentTarget.height - 1);
                int targetIndex = targetY * currentTarget.width + targetX;

                Color targetColor = targetPixels[targetIndex];
                Color currentColor = modifiableTexture.GetPixel(x, y);

                float colorDiff = ColorDifference(currentColor, targetColor);
                float targetBrightness = (targetColor.r + targetColor.g + targetColor.b) / 3f;

                if (useTintedPaper)
                {
                    sampledTotal++;
                    if (colorDiff < 0.25f) sampledCorrect++;
                }
                else
                {
                    if (targetBrightness < 0.8f)
                    {
                        sampledTotal++;
                        if (colorDiff < 0.2f) sampledCorrect++;
                    }
                }
            }
        }

        float accuracy = 0f;
        if (useTintedPaper)
        {
            if (sampledTotal > 0) accuracy = (float)sampledCorrect / sampledTotal * 100f;
        }
        else
        {
            // White area는 샘플링 비율로 추정
            int estimatedWhite = totalWhiteAreaPixels / (step * step);
            int estimatedTotal = sampledTotal + estimatedWhite;
            if (estimatedTotal > 0) accuracy = (float)(sampledCorrect + estimatedWhite) / estimatedTotal * 100f;
        }

        UpdateAccuracyUI(accuracy);
    }

    private void UpdateAccuracyUI(float accuracy)
    {
        float truncated = Mathf.Floor(accuracy * 10f) / 10f;

        if (accuracyText != null)
        {
            if (truncated >= 75f)
                accuracyText.text = $"{truncated}%";
            else
                accuracyText.text = $"{truncated}%";
        }
    }

    private float ColorDifference(Color c1, Color c2)
    {
        return Mathf.Sqrt(Mathf.Pow(c1.r - c2.r, 2) + Mathf.Pow(c1.g - c2.g, 2) + Mathf.Pow(c1.b - c2.b, 2));
    }

    private void AutoCompleteTarget()
    {
        if (currentTarget == null) return;

        for (int y = 0; y < modifiableTexture.height; y++)
        {
            for (int x = 0; x < modifiableTexture.width; x++)
            {
                float scale = (float)currentTarget.width / modifiableTexture.width;
                int targetX = Mathf.Min((int)(x * scale), currentTarget.width - 1);
                int targetY = Mathf.Min((int)(y * scale), currentTarget.height - 1);
                modifiableTexture.SetPixel(x, y, currentTarget.GetPixel(targetX, targetY));
            }
        }
        modifiableTexture.Apply();
        RecalculateAccuracy();
    }
}