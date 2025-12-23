using UnityEngine;

public class InteractiveArt : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float interactDistance = 2.0f;
    [SerializeField] private Renderer myRenderer;

    private Transform headTransform;
    private Renderer backCanvasRenderer;
    private Texture originalTexture;

    // ★ [추가] 문이 열렸는지 체크하기 위한 변수
    // (이 스크립트가 여러 개일 경우, 정적(static) 변수나 싱글톤을 쓰는 게 좋지만
    // 지금은 간단히 '마지막 훔친 그림' 기준으로 문을 열어주는 방식으로 갑니다.)

    void Awake()
    {
        if (myRenderer == null) myRenderer = GetComponent<Renderer>();
    }

    void Start()
    {
        if (myRenderer != null) originalTexture = myRenderer.material.mainTexture;
    }

    void Update()
    {
        if (headTransform == null || backCanvasRenderer == null)
        {
            FindPlayer();
            return;
        }

        if (Input.GetKeyDown(KeyCode.F))
        {
            float dist = Vector3.Distance(transform.position, headTransform.position);
            if (dist <= interactDistance)
            {
                SwapArt();
            }
        }
    }

    void SwapArt()
    {
        if (backCanvasRenderer == null) return;

        Debug.Log("🔄 [교체] 그림을 위조품과 맞바꿉니다!");

        Texture wallArt = myRenderer.material.mainTexture;
        Texture backArt = backCanvasRenderer.material.mainTexture;

        myRenderer.material.mainTexture = backArt;
        backCanvasRenderer.material.mainTexture = wallArt;
        backCanvasRenderer.gameObject.SetActive(true);

        // ★ [핵심 수정] 여기서 바로 탈출하지 않고, 문(Door)을 찾아 허락해줍니다.
        MuseumExitDoor exitDoor = FindObjectOfType<MuseumExitDoor>();
        if (exitDoor != null)
        {
            exitDoor.canExit = true; // 문아, 이제 열려라!
            Debug.Log("🔓 출입문 잠금 해제! 이제 나가세요.");
        }
        else
        {
            Debug.LogError("🚨 씬에 'MuseumExitDoor'가 없습니다! 문을 만들어주세요.");
        }
    }

    void FindPlayer()
    {
        GameObject player = GameObject.FindGameObjectWithTag("HorseChest");
        if (player == null) return;

        Transform[] children = player.GetComponentsInChildren<Transform>(true);
        foreach (Transform t in children)
        {
            if (t.name == "Back_Canvas") backCanvasRenderer = t.GetComponent<Renderer>();
            if (t.CompareTag("HorseHead")) headTransform = t;
        }
    }

    public void SetupArt(string name, Texture texture)
    {
        if (myRenderer != null)
        {
            myRenderer.material.mainTexture = texture;
            originalTexture = texture;
        }
    }
}