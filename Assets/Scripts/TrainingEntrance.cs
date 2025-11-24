using UnityEngine;
// Button, UI 관련 네임스페이스도 더 이상 필요 없지만 둬도 상관없습니다.

public class TrainingEntrance : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject interactionUI;   // "[F] 입장하기" 안내 텍스트
    [SerializeField] private KeyBoardStage keyBoardStage; // 훈련장 매니저

    // 내부 변수
    private GameObject playerObj;
    private bool isPlayerNear = false;

    private void Start()
    {
        // 시작 시 안내 문구 끄기
        if (interactionUI != null) interactionUI.SetActive(false);
    }

    private void Update()
    {
        // 플레이어가 근처에 있고 + F 키를 눌렀을 때 -> ★즉시 시작★
        if (isPlayerNear && Input.GetKeyDown(KeyCode.F))
        {
            StartGameDirectly();
        }
    }

    private void StartGameDirectly()
    {
        if (playerObj != null && keyBoardStage != null)
        {
            // 1. 안내 문구 끄기 (게임 시작하니까)
            if (interactionUI != null) interactionUI.SetActive(false);

            // 2. 훈련 시작 명령 바로 내리기
            keyBoardStage.StartTraining(playerObj);

            Debug.Log("훈련 즉시 입장!");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Rigidbody targetRb = other.attachedRigidbody;

        // 태그 확인 (HorseChest)
        if (targetRb != null && targetRb.CompareTag("HorseChest"))
        {
            playerObj = targetRb.gameObject;
            isPlayerNear = true;

            // 들어오면 "[F] 입장하기" 글씨 띄우기
            if (interactionUI != null) interactionUI.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        Rigidbody targetRb = other.attachedRigidbody;

        if (targetRb != null && targetRb.CompareTag("HorseChest"))
        {
            isPlayerNear = false;
            playerObj = null;

            // 나가면 안내 문구 끄기
            if (interactionUI != null) interactionUI.SetActive(false);
        }
    }
}