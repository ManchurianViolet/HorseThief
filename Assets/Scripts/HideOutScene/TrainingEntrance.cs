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
            paperPainter.StartTraining(playerObj); // PaperPainter에도 StartTraining 함수가 있어야 함
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