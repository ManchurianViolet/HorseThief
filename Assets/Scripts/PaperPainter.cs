using UnityEngine;
using TMPro; // UI 텍스트 사용을 위해 필수

public class PaperPainter : MonoBehaviour
{
    [Header("=== Hideout Integration Settings ===")]
    [SerializeField] private Transform playPosition;   // 붓 잡는 위치
    [SerializeField] private Transform getOutPosition; // 나갈 위치
    [SerializeField] private GameObject gameUIPanel;   // 미술 UI 부모 패널

    [Header("=== New UI Settings ===")]
    [SerializeField] private TextMeshProUGUI accuracyText; // ★ 점수 표시할 UI 텍스트 (UGUI)

    [Header("=== Reference Settings ===")]
    [SerializeField] private Renderer referenceRenderer; // ★ 완성된 그림을 보여줄 옆쪽 종이 (그리기 불가)
    [SerializeField] private Texture2D[] targetGallery;  // ★ 목표 그림들을 넣어두는 배열

    [Header("=== Painting Settings ===")]
    public Color paintColor = Color.blue;
    [SerializeField] private Texture2D paperTexture;     // 깨끗한 도화지 텍스처
    [SerializeField] private float brushSize = 20f;
    [SerializeField] private bool useDelayedCheck = true;
    [SerializeField] private float checkInterval = 1.0f;

    [Header("=== Paper Options ===")]
    [SerializeField] private Color paperBackgroundColor = new Color(0.9f, 0.85f, 0.7f, 1f);
    [SerializeField] private bool useTintedPaper = true;

    // 내부 상태 변수
    private GameObject currentPlayer;
    private bool isTrainingActive = false;

    private Texture2D modifiableTexture; // 실제 그려지는 텍스처
    private Renderer myRenderer;         // 내(도화지) 렌더러
    private Texture2D currentTarget;     // 현재 선택된 목표 그림

    private Color[] targetPixels;
    private bool[] pixelChecked;
    private int correctPixelsCount = 0;
    private int totalTargetAreaPixels = 0;
    private int totalWhiteAreaPixels = 0;

    private bool isPainting = false;
    private bool needsRecalculation = false;
    private float lastCheckTime = 0f;
    private float paintEndTime = 0f;

    private void Start()
    {
        myRenderer = GetComponent<Renderer>();

        // 시작 시 UI와 기능 꺼두기
        if (gameUIPanel != null) gameUIPanel.SetActive(false);

        // 초기 도화지 생성 (하얗게 만들기)
        CreateCleanCanvas();
    }

    private void Update()
    {
        if (!isTrainingActive) return;

        // Y키: 새로운 그림 랜덤 뽑기 & 초기화
        if (Input.GetKeyDown(KeyCode.Y))
        {
            SetNewRandomPainting();
        }

        // C키: 치트 (테스트용 - 현재 목표 그림으로 바로 완성)
        if (Input.GetKeyDown(KeyCode.C))
        {
            AutoCompleteTarget();
        }

        // ESC키: 훈련 종료
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            StopTraining();
        }

        // 붓 뗀 후 정확도 체크 (딜레이)
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

    // ====================================================
    // ★ 외부 호출 및 초기화 로직
    // ====================================================
    public void StartTraining(GameObject player)
    {
        isTrainingActive = true;
        currentPlayer = player;

        // 1. 플레이어 이동
        if (currentPlayer != null && playPosition != null)
        {
            Rigidbody rb = currentPlayer.GetComponent<Rigidbody>();
            if (rb != null) rb.isKinematic = true;
            currentPlayer.transform.position = playPosition.position;
            currentPlayer.transform.rotation = playPosition.rotation;
            if (rb != null) rb.isKinematic = false;
        }

        // 2. UI 켜기
        if (gameUIPanel != null) gameUIPanel.SetActive(true);

        // 3. ★ 입장하자마자 랜덤 그림 세팅
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
    }

    // ★ 랜덤 그림 뽑기 및 초기화 함수
    private void SetNewRandomPainting()
    {
        Debug.Log("새로운 그림을 세팅합니다.");

        // 1. 도화지 깨끗하게 밀기
        CreateCleanCanvas();

        // 2. 갤러리에서 랜덤 하나 뽑기
        if (targetGallery != null && targetGallery.Length > 0)
        {
            int randomIndex = Random.Range(0, targetGallery.Length);
            currentTarget = targetGallery[randomIndex];

            // 3. ★ 옆에 있는 '참고용 종이'에 목표 그림 띄우기 (그리는 곳 아님!)
            if (referenceRenderer != null)
            {
                referenceRenderer.material.mainTexture = currentTarget;
            }

            // 4. 목표 데이터 분석 (점수 계산용)
            PrepareTargetData(currentTarget);
        }
        else
        {
            Debug.LogError("Target Gallery가 비어있습니다! 인스펙터에서 그림을 추가해주세요.");
        }

        // 5. 점수 초기화 및 UI 갱신
        correctPixelsCount = 0;
        UpdateAccuracyUI(0f);
    }

    // ====================================================
    // ★ 그리기 로직 (참고용 종이 보호 포함)
    // ====================================================
    private void OnTriggerEnter(Collider _other)
    {
        if (!isTrainingActive) return;
        if (_other.gameObject.CompareTag("Brush"))
        {
            isPainting = true;
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
            isPainting = false;
            paintEndTime = Time.time;
            // 붓 뗄 때 즉시 체크 옵션이 꺼져있으면 여기서 체크 예약
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
        Vector3 direction = transform.up; // 도화지의 앞면 방향
        Ray ray1 = new Ray(brushPos, direction);
        Ray ray2 = new Ray(brushPos, -direction);
        RaycastHit hit;

        // ★ [중요] 레이캐스트로 붓 위치에서 도화지를 찾습니다.
        if (Physics.Raycast(ray1, out hit, 1.5f, LayerMask.GetMask("Paper")) ||
            Physics.Raycast(ray2, out hit, 1.5f, LayerMask.GetMask("Paper")))
        {
            // ★★★ [안전장치] ★★★
            // 부딪힌 놈(hit.collider.gameObject)이 '나(gameObject)' 일 때만 그립니다.
            // 옆에 있는 참고용 종이에 닿았다면 이 조건문이 false가 되어 무시합니다.
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

        // 월드 좌표 -> 텍스처 좌표 변환
        Vector3 localPos = transform.InverseTransformPoint(_worldPos);
        Vector2 uv = new Vector2((localPos.x / 10) + 0.5f, (localPos.z / 10) + 0.5f);

        if (uv.x < 0 || uv.x > 1 || uv.y < 0 || uv.y > 1) return;
        uv.x = 1f - uv.x;
        uv.y = 1f - uv.y;

        int x = (int)(uv.x * modifiableTexture.width);
        int y = (int)(uv.y * modifiableTexture.height);
        int brushSizeInt = Mathf.CeilToInt(brushSize);

        // 브러시 크기만큼 픽셀 칠하기
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
                                pixelChecked[paperIndex] = false; // 색이 바뀌면 다시 검사해야 함

                            modifiableTexture.SetPixel(pixelX, pixelY, paintColor);
                            needsRecalculation = true;
                        }
                    }
                }
            }
        }
        modifiableTexture.Apply();
    }

    // ====================================================
    // ★ 텍스처 처리 및 정확도 계산
    // ====================================================
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
                // 너무 밝은 부분(흰색)을 배경색으로 덮어씀
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
        if (!target.isReadable) { Debug.LogError($"'{target.name}' 텍스처의 Read/Write Enabled를 체크해주세요!"); return; }

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

        // 초기 상태(빈 도화지)의 점수 계산 (배경색이 정답과 같을 수도 있으니)
        RecalculateAccuracy();
    }

    private void RecalculateAccuracy()
    {
        if (targetPixels == null) return;

        correctPixelsCount = 0;
        for (int y = 0; y < modifiableTexture.height; y++)
        {
            for (int x = 0; x < modifiableTexture.width; x++)
            {
                int paperIndex = y * modifiableTexture.width + x;
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
                    if (colorDiff < 0.25f) { pixelChecked[paperIndex] = true; correctPixelsCount++; }
                    else { pixelChecked[paperIndex] = false; }
                }
                else
                {
                    if (targetBrightness < 0.8f) // 검은 선 부분
                    {
                        if (colorDiff < 0.2f) { pixelChecked[paperIndex] = true; correctPixelsCount++; }
                        else { pixelChecked[paperIndex] = false; }
                    }
                }
            }
        }

        // 점수 계산 및 표시
        float accuracy = 0f;
        if (useTintedPaper)
        {
            if (totalTargetAreaPixels > 0) accuracy = (float)correctPixelsCount / totalTargetAreaPixels * 100f;
        }
        else
        {
            int total = totalTargetAreaPixels + totalWhiteAreaPixels;
            if (total > 0) accuracy = (float)(correctPixelsCount + totalWhiteAreaPixels) / total * 100f;
        }

        UpdateAccuracyUI(accuracy);
    }

    private void UpdateAccuracyUI(float accuracy)
    {
        // 소수점 1자리까지 자르기
        float truncated = Mathf.Floor(accuracy * 10f) / 10f;

        if (accuracyText != null)
        {
            // 색상 효과: 75% 넘으면 초록색, 아니면 흰색
            if (truncated >= 75f)
            {
                accuracyText.text = $"<color=green>Accuracy: {truncated}%</color>";
                // 여기선 성공해도 멈추지 않음! (Endless)
            }
            else
            {
                accuracyText.text = $"Accuracy: {truncated}%";
            }
        }
    }

    private float ColorDifference(Color c1, Color c2)
    {
        return Mathf.Sqrt(Mathf.Pow(c1.r - c2.r, 2) + Mathf.Pow(c1.g - c2.g, 2) + Mathf.Pow(c1.b - c2.b, 2));
    }

    // 치트 기능: 현재 목표 그림을 도화지에 바로 덮어씀
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