using UnityEngine;

public class InteractiveArt : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float interactDistance = 2.0f; // 머리가 닿아야 하는 거리
    [SerializeField] private Renderer myRenderer; // 벽에 걸린 액자 렌더러

    // 참조 변수
    private Transform headTransform;
    private Renderer backCanvasRenderer;

    // 한 번만 훔치게 할 건지 여부 (true면 또 못 바꿈)
    private bool isSwapped = false;

    // 정답 데이터 (채점용)
    private Texture originalTexture; // 원래 여기 걸려있던 진품 텍스처

    void Awake()
    {
        if (myRenderer == null) myRenderer = GetComponent<Renderer>();
    }

    void Start()
    {
        // 시작할 때 원래 진품이 뭔지 기억해둠 (나중에 등 뒤로 보내기 위해)
        if (myRenderer != null)
        {
            originalTexture = myRenderer.material.mainTexture;
        }
    }

    void Update()
    {
        // 플레이어(말)를 못 찾았으면 계속 찾기
        if (headTransform == null || backCanvasRenderer == null)
        {
            FindPlayer();
            return;
        }

        // F키 입력 감지
        if (Input.GetKeyDown(KeyCode.F))
        {
            // 머리와의 거리 계산
            float dist = Vector3.Distance(transform.position, headTransform.position);

            // 거리가 가까우면 교체 시도
            if (dist <= interactDistance)
            {
                SwapArt();
            }
            else
            {
                // (선택사항) 너무 멀면 로그 띄우기
                Debug.Log($"❌ 조금 더 가까이 대세요! (현재: {dist:F1}m)");
            }
        }
    }

    // ---------------------------------------------------------
    // [핵심 기능] 그림 맞교환 (벽 <-> 등)
    // ---------------------------------------------------------
    void SwapArt()
    {
        // 안전장치
        if (backCanvasRenderer == null) return;

        Debug.Log("🔄 [교체] 벽의 그림과 등 뒤의 그림을 맞바꿉니다!");

        // 1. 텍스처 가져오기
        Texture wallArt = myRenderer.material.mainTexture;       // 현재 벽에 있는 거
        Texture backArt = backCanvasRenderer.material.mainTexture; // 현재 등에 있는 거

        // 2. 서로 바꾸기
        myRenderer.material.mainTexture = backArt;       // 벽에는 '내 그림'을 건다
        backCanvasRenderer.material.mainTexture = wallArt; // 등에는 '진품'을 멘다

        // 3. 등 뒤 캔버스 켜기 (혹시 꺼져있을까봐)
        backCanvasRenderer.gameObject.SetActive(true);

        // 4. 상태 변경
        isSwapped = true;

        // 5. 바로 채점 (위치가 아니라 '그림이 바뀌었는지' 확인)
        CalculateScore();

        // [기존 코드 아래에 추가] ---------------------------------

        Debug.Log("📞 HighwayManager에게 탈출 신호를 보냅니다.");

        // 임시 점수 (나중에 MuseumPainter에서 진짜 점수 가져오도록 수정 가능)
        float currentScore = 55f;

        // 씬에 있는 HighwayManager를 찾아서 실행
        HighwayManager manager = FindObjectOfType<HighwayManager>();
        if (manager != null)
        {
            manager.StartEscapeSequence();
        }
        else
        {
            Debug.LogError("🚨 씬에 HighwayManager가 없습니다! 생성해주세요.");
        }
    }

    // ---------------------------------------------------------
    // 플레이어 찾기 (말 머리 & 등 캔버스)
    // ---------------------------------------------------------
    void FindPlayer()
    {
        // 1. 말 본체 찾기 (태그: HorseChest)
        GameObject player = GameObject.FindGameObjectWithTag("HorseChest");
        if (player == null) return;

        // 2. 부품 찾기 (머리 & 등)
        Transform[] children = player.GetComponentsInChildren<Transform>(true);
        foreach (Transform t in children)
        {
            if (t.name == "Back_Canvas") backCanvasRenderer = t.GetComponent<Renderer>();
            if (t.CompareTag("HorseHead")) headTransform = t;
        }
    }

    // 매니저가 그림 세팅할 때 호출
    public void SetupArt(string name, Texture texture)
    {
        if (myRenderer != null)
        {
            myRenderer.material.mainTexture = texture;
            originalTexture = texture; // 진품 정보 갱신
        }
    }

    void CalculateScore()
    {
        Debug.Log("🏆 [완료] 위조품이 미술관에 성공적으로 걸렸습니다!");
        // 여기에 성공 이펙트(폭죽) 같은 거 넣으면 좋음
    }
}