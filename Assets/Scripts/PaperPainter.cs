using UnityEngine;

public class PaperPainter : MonoBehaviour
{
    public Color paintColor = Color.blue;

    [SerializeField] private Texture2D paperTexture;
    [SerializeField] private Texture2D targetPicture;
    [SerializeField] private Texture2D happyMonkPicture;
    [SerializeField] private GameObject portal;
    [SerializeField] private PaintingStartTrigger paintingStartTrigger;
    [SerializeField] private float brushSize = 20f;
    [SerializeField] private int successAccuracyRatio = 75;
    [SerializeField] private bool isThisPaperCheckRatio = true;

    // 성능 관련 설정
    [SerializeField] private float checkInterval = 1.0f;
    [SerializeField] private bool useDelayedCheck = true;

    // 도화지 배경색 설정
    [SerializeField] private Color paperBackgroundColor = new Color(0.9f, 0.85f, 0.7f, 1f); // 구릿빛 베이지
    [SerializeField] private bool useTintedPaper = true; // 색깔 있는 도화지 사용 여부

    private Texture2D modifiableTexture;
    private Renderer paperRenderer;

    private Color[] targetPixels;
    private bool[] pixelChecked;
    private int correctPixelsCount = 0;
    private int totalTargetAreaPixels = 0;
    private int totalWhiteAreaPixels = 0; // 흰색 영역 픽셀 수

    private bool isSuccessTriggered = false;
    private bool isPainting = false;
    private bool needsRecalculation = false;
    private float lastCheckTime = 0f;
    private float paintEndTime = 0f;

    // 목표 그림 미리보기 관련
    private bool isShowingTarget = false;

    private void Start()
    {
        Debug.Log($"Paper 크기: {paperTexture.width}x{paperTexture.height}");
        Debug.Log($"Target 크기: {targetPicture.width}x{targetPicture.height}");

        SaveOriginalTexture();

        if (targetPicture != null)
        {
            PrepareTargetTexture();
        }
        else
        {
            Debug.LogError("targetPicture가 할당되지 않았습니다.");
        }
    }

    private void Update()
    {
        // T키: 목표 그림 보기/숨기기
        if (Input.GetKeyDown(KeyCode.T))
        {
            ToggleTargetPreview();
        }

        // C키: 정답 자동 완성 (치트)
        if (Input.GetKeyDown(KeyCode.C))
        {
            AutoCompleteTarget();
        }

        // 붓을 뗀 후 약간의 딜레이를 두고 체크
        if (useDelayedCheck && needsRecalculation && !isPainting)
        {
            if (Time.time - paintEndTime >= 0.2f && Time.time - lastCheckTime >= checkInterval)
            {
                RecalculateAccuracy();
                needsRecalculation = false;
                lastCheckTime = Time.time;
            }
        }
    }

    // T키: 목표 그림 토글
    private void ToggleTargetPreview()
    {
        isShowingTarget = !isShowingTarget;

        if (isShowingTarget)
        {
            paperRenderer.material.mainTexture = targetPicture;
            Debug.Log("목표 그림 표시 중 (T키로 다시 숨기기)");
        }
        else
        {
            paperRenderer.material.mainTexture = modifiableTexture;
            Debug.Log("현재 그린 그림으로 복귀");
        }
    }

    // C키: 정답 자동 완성
    private void AutoCompleteTarget()
    {
        Debug.Log("정답 자동 완성 중...");

        for (int y = 0; y < modifiableTexture.height; y++)
        {
            for (int x = 0; x < modifiableTexture.width; x++)
            {
                float scale = (float)targetPicture.width / modifiableTexture.width;
                int targetX = Mathf.Min((int)(x * scale), targetPicture.width - 1);
                int targetY = Mathf.Min((int)(y * scale), targetPicture.height - 1);

                Color targetColor = targetPicture.GetPixel(targetX, targetY);
                modifiableTexture.SetPixel(x, y, targetColor);
            }
        }

        modifiableTexture.Apply();
        paperRenderer.material.mainTexture = modifiableTexture;
        isShowingTarget = false;

        Debug.Log("정답 완성! 이게 목표 그림입니다.");

        // 정확도도 재계산
        needsRecalculation = true;
        RecalculateAccuracy();
    }

    private void PrepareTargetTexture()
    {
        Debug.Log("PrepareTargetTexture 시작!");

        if (!targetPicture.isReadable)
        {
            Debug.LogError("Target Picture의 Import Settings에서 'Read/Write Enabled'를 체크해야 합니다.");
            return;
        }

        targetPixels = targetPicture.GetPixels();
        pixelChecked = new bool[modifiableTexture.width * modifiableTexture.height];
        totalTargetAreaPixels = 0;
        totalWhiteAreaPixels = 0; // 초기화 추가

        for (int y = 0; y < modifiableTexture.height; y++)
        {
            for (int x = 0; x < modifiableTexture.width; x++)
            {
                float scale = (float)targetPicture.width / modifiableTexture.width;
                int targetX = Mathf.Min((int)(x * scale), targetPicture.width - 1);
                int targetY = Mathf.Min((int)(y * scale), targetPicture.height - 1);
                int targetIndex = targetY * targetPicture.width + targetX;

                Color targetColor = targetPixels[targetIndex];
                float brightness = (targetColor.r + targetColor.g + targetColor.b) / 3f;

                // 색깔 도화지 모드: 모든 영역을 칠해야 함
                if (useTintedPaper)
                {
                    totalTargetAreaPixels++; // 모든 픽셀 카운트

                    // 디버그: 처음 몇 개만 로그
                    if (totalTargetAreaPixels <= 5)
                    {
                        Debug.Log($"픽셀({x},{y}) 카운트 중... 현재 total: {totalTargetAreaPixels}");
                    }
                }
                else
                {
                    // 기존 방식: 어두운 영역만
                    if (brightness < 0.8f)
                    {
                        totalTargetAreaPixels++;
                    }
                    else
                    {
                        totalWhiteAreaPixels++;
                    }
                }
            }
        }

        Debug.Log($"칠해야 할 총 픽셀 수: {totalTargetAreaPixels}");
        Debug.Log($"흰색 영역 픽셀 수: {totalWhiteAreaPixels}");
        Debug.Log($"색깔 도화지 모드: {useTintedPaper}");
        Debug.Log($"전체 픽셀 수: {modifiableTexture.width * modifiableTexture.height}");

        CheckInitialWhiteAreas();
    }

    private void CheckInitialWhiteAreas()
    {
        correctPixelsCount = 0;

        // 색깔 있는 도화지를 사용하면 흰색 영역도 칠해야 함
        if (useTintedPaper)
        {
            Debug.Log("색깔 도화지 모드: 모든 영역을 칠해야 합니다 (흰색 포함)");
            return; // 초기 정확도 체크 건너뛰기
        }

        for (int y = 0; y < modifiableTexture.height; y++)
        {
            for (int x = 0; x < modifiableTexture.width; x++)
            {
                int paperIndex = y * modifiableTexture.width + x;

                float scale = (float)targetPicture.width / modifiableTexture.width;
                int targetX = Mathf.Min((int)(x * scale), targetPicture.width - 1);
                int targetY = Mathf.Min((int)(y * scale), targetPicture.height - 1);
                int targetIndex = targetY * targetPicture.width + targetX;

                Color targetColor = targetPixels[targetIndex];
                Color currentColor = modifiableTexture.GetPixel(x, y);

                float targetBrightness = (targetColor.r + targetColor.g + targetColor.b) / 3f;
                float currentBrightness = (currentColor.r + currentColor.g + currentColor.b) / 3f;

                if (targetBrightness >= 0.8f && currentBrightness >= 0.8f)
                {
                    pixelChecked[paperIndex] = true;
                }
            }
        }

        Debug.Log($"초기 정확도: {correctPixelsCount}/{totalTargetAreaPixels}");
        paintingStartTrigger.ChangePercentText(CalculateAccuracy());
    }

    private void OnTriggerEnter(Collider _other)
    {
        if (_other.gameObject.CompareTag("Brush"))
        {
            isPainting = true;
            TriggerCheck(_other.gameObject);
        }
    }

    private void OnTriggerStay(Collider _other)
    {
        if (_other.gameObject.CompareTag("Brush"))
        {
            TriggerCheck(_other.gameObject);
        }
    }

    private void OnTriggerExit(Collider _other)
    {
        if (_other.gameObject.CompareTag("Brush"))
        {
            isPainting = false;
            paintEndTime = Time.time;

            if (!useDelayedCheck && needsRecalculation)
            {
                if (Time.time - lastCheckTime >= checkInterval)
                {
                    RecalculateAccuracy();
                    needsRecalculation = false;
                    lastCheckTime = Time.time;
                }
            }
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
            PaintAtPosition(hit.point);
        }
    }

    private void SaveOriginalTexture()
    {
        paperRenderer = GetComponent<Renderer>();
        bool useMipMap = paperTexture.mipmapCount > 1;
        modifiableTexture = new Texture2D(paperTexture.width, paperTexture.height, TextureFormat.RGBA32, useMipMap);

        // 색깔 있는 도화지로 만들기
        if (useTintedPaper)
        {
            Color[] pixels = paperTexture.GetPixels();
            for (int i = 0; i < pixels.Length; i++)
            {
                // 흰색 픽셀을 구릿빛으로 변경
                if (pixels[i].r > 0.9f && pixels[i].g > 0.9f && pixels[i].b > 0.9f)
                {
                    pixels[i] = paperBackgroundColor;
                }
            }
            modifiableTexture.SetPixels(pixels);
        }
        else
        {
            modifiableTexture.SetPixels(paperTexture.GetPixels());
        }

        modifiableTexture.Apply();
        paperRenderer.material.mainTexture = modifiableTexture;
    }

    private void PaintAtPosition(Vector3 _worldPos)
    {
        if (targetPicture == null || targetPixels == null || totalTargetAreaPixels == 0) return;

        // 목표 그림을 보고 있을 때는 칠할 수 없음
        if (isShowingTarget)
        {
            Debug.Log("목표 그림을 보는 중에는 칠할 수 없어요! T키를 눌러 돌아가세요.");
            return;
        }

        Vector3 localPos = transform.InverseTransformPoint(_worldPos);
        Vector2 uv = new Vector2((localPos.x / 10) + 0.5f, (localPos.z / 10) + 0.5f);
        if (uv.x < 0 || uv.x > 1 || uv.y < 0 || uv.y > 1) return;
        uv.x = 1f - uv.x;
        uv.y = 1f - uv.y;

        int x = (int)(uv.x * modifiableTexture.width);
        int y = (int)(uv.y * modifiableTexture.height);
        int brushSizeInt = Mathf.CeilToInt(brushSize);

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
                            // 디버그: 브러시 중심점에서만 로그 출력
                            if (i == 0 && j == 0)
                            {
                                float scale = (float)targetPicture.width / modifiableTexture.width;
                                int targetX = Mathf.Min((int)(pixelX * scale), targetPicture.width - 1);
                                int targetY = Mathf.Min((int)(pixelY * scale), targetPicture.height - 1);
                                int targetIndex = targetY * targetPicture.width + targetX;
                                Color targetColor = targetPixels[targetIndex];

                                float colorDiff = ColorDifference(paintColor, targetColor);
                                string verdict = colorDiff < 0.25f ? "✓ 정답!" : "✗ 틀림";

                                float targetBrightness = (targetColor.r + targetColor.g + targetColor.b) / 3f;
                                bool isWhiteArea = targetBrightness >= 0.8f;

                                Debug.Log($"[칠하는 중] 픽셀({pixelX},{pixelY})\n" +
                                         $"내 붓: {ColorToString(paintColor)}\n" +
                                         $"목표: {ColorToString(targetColor)} {(isWhiteArea ? "(흰색 영역)" : "(색깔 영역)")}\n" +
                                         $"차이: {colorDiff:F3} {verdict}\n" +
                                         $"색깔 도화지 모드: {useTintedPaper}");
                            }

                            if (pixelChecked[paperIndex])
                            {
                                pixelChecked[paperIndex] = false;
                            }

                            modifiableTexture.SetPixel(pixelX, pixelY, paintColor);
                            needsRecalculation = true;
                        }
                    }
                }
            }
        }

        modifiableTexture.Apply();
    }

    private void RecalculateAccuracy()
    {
        float startTime = Time.realtimeSinceStartup;

        correctPixelsCount = 0; // 리셋

        for (int y = 0; y < modifiableTexture.height; y++)
        {
            for (int x = 0; x < modifiableTexture.width; x++)
            {
                int paperIndex = y * modifiableTexture.width + x;

                float scale = (float)targetPicture.width / modifiableTexture.width;
                int targetX = Mathf.Min((int)(x * scale), targetPicture.width - 1);
                int targetY = Mathf.Min((int)(y * scale), targetPicture.height - 1);
                int targetIndex = targetY * targetPicture.width + targetX;

                Color targetColor = targetPixels[targetIndex];
                Color currentColor = modifiableTexture.GetPixel(x, y);

                float targetBrightness = (targetColor.r + targetColor.g + targetColor.b) / 3f;
                float colorDiff = ColorDifference(currentColor, targetColor);

                // 색깔 도화지 모드: 모든 영역 체크
                if (useTintedPaper)
                {
                    if (colorDiff < 0.25f) // 흰색 허용 오차를 위해 0.2에서 0.25로 증가
                    {
                        pixelChecked[paperIndex] = true;
                        correctPixelsCount++;
                    }
                    else
                    {
                        pixelChecked[paperIndex] = false;
                    }
                }
                else
                {
                    // 기존 방식: 칠해야 하는 어두운 영역만 체크
                    if (targetBrightness < 0.8f)
                    {
                        if (colorDiff < 0.2f)
                        {
                            pixelChecked[paperIndex] = true;
                            correctPixelsCount++;
                        }
                        else
                        {
                            pixelChecked[paperIndex] = false;
                        }
                    }
                }
            }
        }

        float elapsedTime = (Time.realtimeSinceStartup - startTime) * 1000f;
        Debug.Log($"정확도 재계산 완료: {elapsedTime:F2}ms (정확도: {CalculateAccuracy():F1}%)");

        CheckSuccess();
    }

    private float ColorDifference(Color c1, Color c2)
    {
        return Mathf.Sqrt(
            Mathf.Pow(c1.r - c2.r, 2) +
            Mathf.Pow(c1.g - c2.g, 2) +
            Mathf.Pow(c1.b - c2.b, 2)
        );
    }

    // 색상을 읽기 쉬운 문자열로 변환
    private string ColorToString(Color c)
    {
        string colorName = "";

        // 색상 이름 추정
        if (c.r > 0.9f && c.g < 0.2f && c.b < 0.2f) colorName = "빨강";
        else if (c.r < 0.2f && c.g < 0.2f && c.b > 0.9f) colorName = "파랑";
        else if (c.r > 0.9f && c.g > 0.9f && c.b < 0.2f) colorName = "노랑";
        else if (c.r < 0.2f && c.g < 0.2f && c.b < 0.2f) colorName = "검정";
        else if (c.r > 0.8f && c.g > 0.8f && c.b > 0.8f) colorName = "흰색";
        else colorName = "기타";

        return $"{colorName} RGB({c.r:F2},{c.g:F2},{c.b:F2})";
    }

    private float CalculateAccuracy()
    {
        if (useTintedPaper)
        {
            // 색깔 도화지: 모든 픽셀이 목표
            if (totalTargetAreaPixels == 0) return 0f;
            return (float)correctPixelsCount / totalTargetAreaPixels * 100f;
        }
        else
        {
            // 흰 도화지: 흰색 영역은 보너스
            int totalPixels = totalTargetAreaPixels + totalWhiteAreaPixels;
            if (totalPixels == 0) return 0f;

            int totalCorrect = correctPixelsCount + totalWhiteAreaPixels;
            return (float)totalCorrect / totalPixels * 100f;
        }
    }

    private void CheckSuccess()
    {
        if (isSuccessTriggered || totalTargetAreaPixels == 0) return;

        float accuracyRatio = CalculateAccuracy();
        paintingStartTrigger.ChangePercentText(accuracyRatio);

        if (accuracyRatio >= successAccuracyRatio)
        {
            isSuccessTriggered = true;

            if (happyMonkPicture != null)
            {
                paperRenderer.material.mainTexture = happyMonkPicture;
            }

            portal.SetActive(true);
        }
    }
}