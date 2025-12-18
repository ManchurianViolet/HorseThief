using UnityEngine;

public class TrainingEntrance : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject interactionUI;   // "[F] 입장하기" 안내 텍스트

    [Header("Training Managers (하나만 연결하세요)")]
    [SerializeField] private KeyBoardStage keyBoardStage; // 타자 훈련장일 경우 연결
    [SerializeField] private PaperPainter paperPainter;   // 그림 훈련장일 경우 연결
    [SerializeField] private RunningManager runningManager;

    // 내부 변수
    private GameObject playerObj;
    private bool isPlayerNear = false;

    private void Start()
    {
        if (interactionUI != null) interactionUI.SetActive(false);
    }

    private void Update()
    {
        // 플레이어가 근처에 있고 + F 키를 눌렀을 때
        if (isPlayerNear && Input.GetKeyDown(KeyCode.F))
        {
            StartGameDirectly();
        }
    }

    private void StartGameDirectly()
    {
        if (playerObj == null) return;

        bool isStarted = false;

        // 1. 타자 훈련장이 연결되어 있다면?
        if (keyBoardStage != null)
        {
            keyBoardStage.StartTraining(playerObj);
            isStarted = true;
            Debug.Log("타자 훈련 입장!");
        }
        // 2. 그림 훈련장이 연결되어 있다면?
        else if (paperPainter != null)
        {
            paperPainter.StartTraining(playerObj);
            isStarted = true;
            Debug.Log("그림 훈련 입장!");
        }
        else if (runningManager != null)
        {
            runningManager.StartTraining(playerObj);
            isStarted = true;
            Debug.Log("달리기 훈련 입장!");
        }

        // 훈련이 시작되었다면 안내 문구 끄기
        if (isStarted)
        {
            if (interactionUI != null) interactionUI.SetActive(false);
        }
    }

    // ★ [추가] OnTriggerStay: 트리거 안에 계속 있으면 매 프레임 체크
    private void OnTriggerStay(Collider other)
    {
        // 이미 플레이어가 인식되어 있으면 스킵 (중복 처리 방지)
        if (isPlayerNear && playerObj != null) return;

        // ★ [이중 체크] 1차: other 직접 태그 확인
        if (other.CompareTag("HorseChest"))
        {
            SetPlayerNear(other.gameObject);
            return;
        }

        // ★ [이중 체크] 2차: attachedRigidbody 확인
        Rigidbody targetRb = other.attachedRigidbody;
        if (targetRb != null && targetRb.CompareTag("HorseChest"))
        {
            SetPlayerNear(targetRb.gameObject);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // ★ [이중 체크] 1차: other 직접 태그 확인
        if (other.CompareTag("HorseChest"))
        {
            SetPlayerNear(other.gameObject);
            return;
        }

        // ★ [이중 체크] 2차: attachedRigidbody 확인
        Rigidbody targetRb = other.attachedRigidbody;
        if (targetRb != null && targetRb.CompareTag("HorseChest"))
        {
            SetPlayerNear(targetRb.gameObject);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // ★ [이중 체크] 1차: other 직접 태그 확인
        if (other.CompareTag("HorseChest"))
        {
            CheckPlayerExit(other.gameObject);
            return;
        }

        // ★ [이중 체크] 2차: attachedRigidbody 확인
        Rigidbody targetRb = other.attachedRigidbody;
        if (targetRb != null && targetRb.CompareTag("HorseChest"))
        {
            CheckPlayerExit(targetRb.gameObject);
        }
    }

    // ★ [새 함수] 플레이어 진입 처리
    private void SetPlayerNear(GameObject detectedPlayer)
    {
        playerObj = detectedPlayer;
        isPlayerNear = true;

        // 들어오면 "[F] 입장하기" 글씨 띄우기
        if (interactionUI != null)
        {
            interactionUI.SetActive(true);
            Debug.Log($"✅ [TrainingEntrance] UI 활성화 (감지된 오브젝트: {detectedPlayer.name})");
        }
    }

    // ★ [새 함수] 플레이어 퇴장 처리
    private void CheckPlayerExit(GameObject exitedPlayer)
    {
        // 저장된 플레이어와 같은 오브젝트일 때만 처리
        if (exitedPlayer == playerObj)
        {
            isPlayerNear = false;
            playerObj = null;

            // 나가면 안내 문구 끄기
            if (interactionUI != null)
            {
                interactionUI.SetActive(false);
                Debug.Log("❌ [TrainingEntrance] UI 비활성화");
            }
        }
    }
}