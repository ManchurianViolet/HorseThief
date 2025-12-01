using UnityEngine;
using Unity.Cinemachine;

public class LaptopInteraction : MonoBehaviour
{
    [Header("UI & Camera")]
    [SerializeField] private GameObject laptopUIPanel;   // ★ [추가됨] 노트북 화면 UI 패널
    [SerializeField] private CinemachineCamera laptopCamera;
    [SerializeField] private GameObject interactionText;

    [Header("Control Target")]
    [SerializeField] private HorseControl playerHorse;

    [SerializeField] private string playerLayerName = "HorseChest";
    [SerializeField] private LaptopUIManager uiManager;

    private bool isPlayerNear = false;
    private bool isLaptopOpen = false;
    private int originalCullingMask;

    void Start()
    {
        // 시작 시 UI들 끄기
        if (interactionText != null) interactionText.SetActive(false);
        if (laptopUIPanel != null) laptopUIPanel.SetActive(false); // ★ [추가됨] 시작할 때 노트북 화면 끄기

        if (laptopCamera != null) laptopCamera.Priority = 0;

        if (Camera.main != null) originalCullingMask = Camera.main.cullingMask;
    }

    void Update()
    {
        if (isPlayerNear && !isLaptopOpen && Input.GetKeyDown(KeyCode.F))
        {
            OpenLaptop();
        }
        else if (isLaptopOpen && Input.GetKeyDown(KeyCode.Escape))
        {
            // ★ [수정됨] 무조건 끄는 게 아니라, UI 매니저가 뒤로가기를 했는지 확인
            // uiManager가 없거나, TryGoBack이 false(메인화면임)를 반환하면 -> 노트북 끄기
            if (uiManager == null || !uiManager.TryGoBack())
            {
                CloseLaptop();
            }
        }
    }

    private void OpenLaptop()
    {
        isLaptopOpen = true;

        if (interactionText != null) interactionText.SetActive(false);

        // ★ [추가됨] 노트북 화면 UI 켜기
        if (laptopUIPanel != null) laptopUIPanel.SetActive(true);

        if (laptopCamera != null) laptopCamera.Priority = 20;
        if (playerHorse != null) playerHorse.isControlEnabled = false;

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        // 메인 카메라에서 'HorseChest' 레이어만 쏙 빼고 렌더링하기
        if (Camera.main != null)
        {
            int layerIndex = LayerMask.NameToLayer(playerLayerName);

            if (layerIndex != -1)
            {
                int playerLayerMask = 1 << layerIndex;
                Camera.main.cullingMask = originalCullingMask & ~playerLayerMask;
            }
            else
            {
                Debug.LogError($"Layer '{playerLayerName}'를 찾을 수 없습니다!");
            }
        }
    }

    public void CloseLaptop()
    {
        isLaptopOpen = false;

        if (isPlayerNear && interactionText != null) interactionText.SetActive(true);

        // ★ [추가됨] 노트북 화면 UI 끄기
        if (laptopUIPanel != null) laptopUIPanel.SetActive(false);

        if (laptopCamera != null) laptopCamera.Priority = 0;
        if (playerHorse != null) playerHorse.isControlEnabled = true;

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        if (Camera.main != null)
        {
            Camera.main.cullingMask = originalCullingMask;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.attachedRigidbody != null && other.attachedRigidbody.CompareTag("HorseChest"))
        {
            isPlayerNear = true;
            if (playerHorse == null)
                playerHorse = other.attachedRigidbody.GetComponent<HorseControl>();
            if (interactionText != null) interactionText.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.attachedRigidbody != null && other.attachedRigidbody.CompareTag("HorseChest"))
        {
            isPlayerNear = false;
            if (isLaptopOpen) CloseLaptop();
            if (interactionText != null) interactionText.SetActive(false);
        }
    }
}