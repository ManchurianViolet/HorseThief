using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class MainMenuController : MonoBehaviour
{
    [Header("=== Transition Overlay (검은 커튼) ===")]
    [SerializeField] private CanvasGroup blackCurtainGroup;

    [Header("=== Panels ===")]
    [SerializeField] private GameObject titlePanelObject;
    [SerializeField] private GameObject menuPanelObject;

    [Header("=== Intro Elements (타이틀) ===")]
    [SerializeField] private CanvasGroup titleLogoGroup;
    [SerializeField] private CanvasGroup horseSilhouetteGroup;
    [SerializeField] private GameObject introLightingObject;
    [SerializeField] private CanvasGroup pressKeyGroup;

    [Header("=== Menu Elements (메뉴) ===")]
    [SerializeField] private RectTransform[] menuButtons;
    [SerializeField] private GameObject menuLightingObject;
    [SerializeField] private Transform sparkleObject;

    [Header("=== Menu Buttons ===")]
    [SerializeField] private Button continueButton;

    private bool isIntroFinished = false;
    private bool isMenuActive = false;
    private List<Vector2> buttonOriginalPositions = new List<Vector2>();

    void Awake()
    {
        if (menuButtons != null)
        {
            foreach (var btn in menuButtons)
            {
                if (btn != null) buttonOriginalPositions.Add(btn.anchoredPosition);
            }
        }
    }

    void Start()
    {
        // 1. 커튼 초기화
        if (blackCurtainGroup != null)
        {
            blackCurtainGroup.gameObject.SetActive(true);
            blackCurtainGroup.alpha = 0f;
        }

        // 2. 패널 초기화
        if (titlePanelObject != null) titlePanelObject.SetActive(true);
        if (menuPanelObject != null) menuPanelObject.SetActive(false);

        // 3. 인트로 요소 숨김
        if (titleLogoGroup != null) titleLogoGroup.alpha = 0f;
        if (horseSilhouetteGroup != null) horseSilhouetteGroup.alpha = 0f;
        if (pressKeyGroup != null) pressKeyGroup.alpha = 0f;
        if (introLightingObject != null) introLightingObject.SetActive(false);

        CheckSaveFile();
        StartCoroutine(IntroSequence());
    }

    void Update()
    {
        if (isIntroFinished && !isMenuActive && Input.anyKeyDown)
        {
            StartCoroutine(SwitchToMenuSequence());
        }
    }

    // 🎬 [1] 오프닝
    IEnumerator IntroSequence()
    {
        yield return new WaitForSeconds(1f);
        yield return StartCoroutine(FadeCanvasGroup(titleLogoGroup, 0f, 1f, 1.0f));
        yield return StartCoroutine(FadeCanvasGroup(horseSilhouetteGroup, 0f, 1f, 1.0f));
        yield return new WaitForSeconds(0.2f);

        if (introLightingObject != null)
        {
            introLightingObject.SetActive(true);
            yield return new WaitForSeconds(0.05f);
            introLightingObject.SetActive(false);
            yield return new WaitForSeconds(0.1f);
            introLightingObject.SetActive(true);
        }

        yield return new WaitForSeconds(0.5f);
        yield return StartCoroutine(FadeCanvasGroup(pressKeyGroup, 0f, 1f, 1.0f));
        isIntroFinished = true;
    }

    // 🎬 [2] 메뉴 전환 (여기가 수정됨!)
    IEnumerator SwitchToMenuSequence()
    {
        isMenuActive = true;

        // 1. 검은 커튼 치기 (암전)
        yield return StartCoroutine(FadeCanvasGroup(blackCurtainGroup, 0f, 1f, 1.0f));

        // --- 암전 상태 ---

        // 2. 패널 교체 및 준비
        if (titlePanelObject != null) titlePanelObject.SetActive(false);
        if (menuPanelObject != null)
        {
            menuPanelObject.SetActive(true);

            // 버튼 숨기기 (화면 왼쪽으로 치워둠)
            for (int i = 0; i < menuButtons.Length; i++)
            {
                if (menuButtons[i] != null)
                {
                    Vector2 hiddenPos = buttonOriginalPositions[i];
                    hiddenPos.x -= 600f;
                    menuButtons[i].anchoredPosition = hiddenPos;
                }
            }
            if (menuLightingObject != null) menuLightingObject.SetActive(false);
            if (sparkleObject != null)
            {
                sparkleObject.gameObject.SetActive(true);
                StartCoroutine(SparkleAnimationLoop());
            }
        }

        yield return new WaitForSeconds(0.5f); // 암전 유지

        // 3. 커튼 걷기 (1초 동안 천천히 밝아짐)
        StartCoroutine(FadeCanvasGroup(blackCurtainGroup, 1f, 0f, 1.0f));

        // ★ [핵심] 커튼이 다 걷히고 + 조명이 켜질 때까지 충분히 기다림 (1.2초 대기)
        // 기존 0.5초 -> 1.2초로 대폭 늘림!
        yield return new WaitForSeconds(1.2f);

        // 4. 조명 켜기 & 버튼 등장 시작
        StartCoroutine(FlickerMenuLight());

        // 조명 켜지고 나서 살짝 더 뜸들이기 (0.3초) - "조명 딱! -> (잠시 후) -> 버튼 슉"
        yield return new WaitForSeconds(0.3f);

        // 5. 버튼 슬라이드 시작
        foreach (var btnRect in menuButtons)
        {
            if (btnRect != null)
            {
                int index = System.Array.IndexOf(menuButtons, btnRect);
                Vector2 targetPos = buttonOriginalPositions[index];

                StartCoroutine(SlideButtonIn(btnRect, targetPos));

                // 다음 버튼 나올 때까지 간격 (0.4초)
                yield return new WaitForSeconds(0.4f);
            }
        }
    }

    IEnumerator FlickerMenuLight()
    {
        if (menuLightingObject == null) yield break;
        menuLightingObject.SetActive(true);
        yield return new WaitForSeconds(0.05f);
        menuLightingObject.SetActive(false);
        yield return new WaitForSeconds(0.08f);
        menuLightingObject.SetActive(true);
        yield return new WaitForSeconds(0.05f);
        menuLightingObject.SetActive(false);
        yield return new WaitForSeconds(0.1f);
        menuLightingObject.SetActive(true);
    }

    IEnumerator SlideButtonIn(RectTransform btn, Vector2 targetPos)
    {
        float t = 0f;
        Vector2 startPos = btn.anchoredPosition;
        while (t < 1f)
        {
            t += Time.deltaTime * 2.0f;
            float smoothT = Mathf.Sin(t * Mathf.PI * 0.5f);
            btn.anchoredPosition = Vector2.Lerp(startPos, targetPos, smoothT);
            yield return null;
        }
        btn.anchoredPosition = targetPos;
    }

    IEnumerator SparkleAnimationLoop()
    {
        if (sparkleObject == null) yield break;
        float timer = 0f;
        Vector3 baseScale = sparkleObject.localScale;
        while (true)
        {
            timer += Time.deltaTime;
            sparkleObject.Rotate(0, 0, -100f * Time.deltaTime);
            float scaleFactor = 1f + (Mathf.Sin(timer * 5f) * 0.3f);
            sparkleObject.localScale = baseScale * scaleFactor;
            yield return null;
        }
    }

    IEnumerator FadeCanvasGroup(CanvasGroup cg, float start, float end, float duration)
    {
        if (cg == null) yield break;
        float t = 0f;
        cg.alpha = start;
        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            cg.alpha = Mathf.Lerp(start, end, t);
            yield return null;
        }
        cg.alpha = end;
    }

    private void CheckSaveFile()
    {
        if (GameManager.Instance != null && continueButton != null)
        {
            bool hasSave = GameManager.Instance.HasSaveData();
            continueButton.interactable = hasSave;
            if (!hasSave)
            {
                var colors = continueButton.colors;
                colors.disabledColor = new Color(0.3f, 0.3f, 0.3f, 0.5f);
                continueButton.colors = colors;
            }
        }
    }

    public void OnClickContinue() { if (GameManager.Instance != null) GameManager.Instance.ContinueGame(); }
    public void OnClickNewGame() { if (GameManager.Instance != null) GameManager.Instance.StartNewGame(); }
    public void OnClickExit() { Application.Quit(); }
}