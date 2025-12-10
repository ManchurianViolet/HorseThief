using UnityEngine;
// using UnityEngine.UI; // 이제 UI는 아예 안 씁니다. 삭제!

public class InteractiveArt : MonoBehaviour
{
    [Header("Art Settings")]
    [SerializeField] private float attachDistance = 2.0f; // 벽에 붙일 때 최대 거리

    // ★ [중요] 내 몸체(Quad)의 렌더러를 연결하세요. (인스펙터에서 드래그)
    [SerializeField] private Renderer myRenderer;

    // 외부에서 주입될 이름 (SetupArt로 설정됨)
    private string artName;

    // ---------------------------------------------------------
    // 말(Player) 관련 참조 변수들
    // ---------------------------------------------------------
    private Transform playerTransform;
    private Transform headTransform;
    private Transform mouthPoint;       // 그림을 물 위치
    private Renderer backCanvasRenderer; // 등 뒤에 있는 그림(위조품)

    // ---------------------------------------------------------
    // 상태 변수
    // ---------------------------------------------------------
    private bool isStolen = false;       // 현재 입에 물고 있는가?
    private bool isPlaced = false;       // 다시 벽에 붙였는가? (미션 끝)

    // 정답 채점용 데이터
    private Vector3 originalLocalPos;
    private Quaternion originalLocalRot;
    private Transform originalParent;    // 원래 걸려있던 벽

    // 레이캐스트(투영)용
    private RaycastHit hitInfo;
    private bool canPlace = false;

    void Awake()
    {
        // 1. 시작하자마자 정답 위치(원래 위치)를 기억해둡니다.
        originalParent = transform.parent;
        originalLocalPos = transform.localPosition;
        originalLocalRot = transform.localRotation;

        // 혹시 인스펙터에서 연결 안 했으면 자동으로 찾기
        if (myRenderer == null) myRenderer = GetComponent<Renderer>();
        if (myRenderer == null) Debug.LogError("🚨 [ART] 내 Renderer가 연결 안 됐어요!");
        else Debug.Log($"✅ [ART] 시작됨! 현재 그림: {myRenderer.material.mainTexture?.name ?? "없음(Null)"}");
    }

    void Update()
    {
        // 1. 플레이어 찾기 시도
        if (playerTransform == null)
        {
            FindPlayer();
            return;
        }

        // 거리 계산 로그 (너무 자주 뜨면 주석 처리)
        // ★ [핵심 변경] 거리를 잴 때 '내 위치' vs '머리 위치(headTransform)'를 비교함
        float dist = Vector3.Distance(transform.position, headTransform.position);

        // F키 입력 감지
        if (Input.GetKeyDown(KeyCode.F))
        {
            Debug.Log($"👀 [거리 체크] 머리와의 거리: {dist:F2}m / 제한: {attachDistance}m");

            if (!isStolen && !isPlaced)
            {
                if (dist < attachDistance)
                {
                    StealArt();
                }
                else
                {
                    Debug.LogWarning("❌ 머리가 너무 멉니다! 더 가까이 대세요.");
                }
            }
        }

        // ... (UpdatePreviewPosition, PlaceArt 로직은 동일) ...
        if (isStolen && !isPlaced && Input.GetKeyDown(KeyCode.F))
        {
            if (canPlace) PlaceArt();
        }
        if (isStolen) UpdatePreviewPosition();
    }

    // ---------------------------------------------------------
    // [기능 1] 초기 세팅 (미술관 매니저가 호출)
    // ---------------------------------------------------------
    public void SetupArt(string name, Texture texture)
    {
        this.artName = name; // 이름표 붙이기

        // 3D Quad의 재질(그림)을 교체
        if (myRenderer != null)
        {
            myRenderer.material.mainTexture = texture;
        }
    }

    // ---------------------------------------------------------
    // [기능 2] 플레이어 찾기
    // ---------------------------------------------------------
    void FindPlayer()
    {
        // ★ [수정] "Player" 대신 "HorseChest" 태그로 찾습니다!
        GameObject player = GameObject.FindGameObjectWithTag("HorseChest");

        if (player == null)
        {
            // 여전히 못 찾으면 정말 없는 거니까 에러 띄우기
            Debug.LogError("🚨 [FindPlayer 실패] 씬 안에 'HorseChest' 태그를 가진 켜져있는 오브젝트가 없습니다!");
            return;
        }

        // 찾았으면 연결!
        playerTransform = player.transform;
        Debug.Log("✅ [FindPlayer] 말(HorseChest)을 찾았습니다! 이제 부품을 찾습니다...");

        // 자식 부품(입, 등) 찾기 (이건 그대로 두면 됩니다)
        Transform[] children = player.GetComponentsInChildren<Transform>(true);
        foreach (Transform t in children)
        {
            if (t.name == "MouthPoint") mouthPoint = t;
            if (t.name == "Back_Canvas") backCanvasRenderer = t.GetComponent<Renderer>();
            if (t.CompareTag("HorseHead"))
            {
                headTransform = t;
            }
        }

        // 확인 사살 로그
        if (mouthPoint == null) Debug.LogError("🚨 말 안에 'MouthPoint'가 없어요!");
        if (backCanvasRenderer == null) Debug.LogError("🚨 말 안에 'Back_Canvas'가 없어요!");
    }

    // ---------------------------------------------------------
    // [기능 3] 훔치기 (스왑 & 이동)
    // ---------------------------------------------------------
    void StealArt()
    {
        // 안전장치
        if (mouthPoint == null || backCanvasRenderer == null)
        {
            Debug.LogError("말의 'MouthPoint'나 'Back_Canvas'를 못 찾았습니다!");
            return;
        }

        isStolen = true;

        // ★ [핵심] 텍스처(그림) 서로 맞교환 (Renderer <-> Renderer)
        Texture realArt = myRenderer.material.mainTexture;       // 벽에 있는 진품
        Texture fakeArt = backCanvasRenderer.material.mainTexture; // 등 뒤에 있는 위조품
        Debug.Log($"🎨 [교체 전] 벽 그림: {realArt?.name ?? "Null"}, 등 그림: {fakeArt?.name ?? "Null"}");
        myRenderer.material.mainTexture = fakeArt;       // 이제 이 액자는 위조품이 됨
        backCanvasRenderer.material.mainTexture = realArt; // 등 뒤에는 진품을 멤

        // 등 뒤 캔버스가 혹시 꺼져있으면 켜주기
        backCanvasRenderer.gameObject.SetActive(true);

        // ★ [이동] 액자(위조품)를 입으로 가져옴
        // 1. 물리 충돌 끄기 (입에 물었을 때 덜렁거리거나 충돌 안 하게)
        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = false;

        // 2. 입의 자식으로 설정 및 위치 초기화
        transform.SetParent(mouthPoint);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
        Debug.Log("✨ [StealArt 완료] 그림이 입으로 이동했습니다.");
    }

    // ---------------------------------------------------------
    // [기능 4] 미리보기 (벽에 투영)
    // ---------------------------------------------------------
    void UpdatePreviewPosition()
    {
        // 입에서 정면으로 레이저 발사
        if (Physics.Raycast(mouthPoint.position, mouthPoint.forward, out hitInfo, attachDistance))
        {
            // 벽에 닿으면 그 위치로 그림 이동
            transform.position = hitInfo.point;

            // 회전은 말 머리 각도를 따라감 (플레이어가 돌려서 맞출 수 있게)
            transform.rotation = mouthPoint.rotation;

            // 벽에 파묻히지 않게 아주 살짝 띄워줌 (0.01m)
            transform.position += hitInfo.normal * 0.01f;

            canPlace = true;
        }
        else
        {
            // 허공을 보고 있으면 다시 입으로 돌아옴
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
            canPlace = false;
        }
    }

    // ---------------------------------------------------------
    // [기능 5] 부착 및 채점
    // ---------------------------------------------------------
    void PlaceArt()
    {
        isStolen = false;
        isPlaced = true;

        // 1. 현재 닿은 벽(또는 물체)에 고정
        transform.SetParent(hitInfo.transform);

        // 2. 물리 다시 켜기 (선택사항)
        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = true;

        // 3. 점수 계산
        CalculateScore();
    }

    void CalculateScore()
    {
        // 원래 정답 위치와 현재 위치 비교
        // (주의: 다른 벽에 붙였을 수도 있으니 World 좌표로 비교하거나, 정답 벽에 붙였다고 가정)

        float distError = Vector3.Distance(transform.localPosition, originalLocalPos);
        float angleError = Quaternion.Angle(transform.localRotation, originalLocalRot);

        Debug.Log($"[채점] 위치 오차: {distError * 100:F1}cm, 각도 오차: {angleError:F1}도");

        if (distError < 0.1f && angleError < 10f)
        {
            Debug.Log("🏆 PERFECT! 감쪽같습니다.");
            // 여기에 성공 이펙트나 사운드 추가
        }
        else
        {
            Debug.Log("😅 조금 삐뚤어졌네요.");
        }
    }
}