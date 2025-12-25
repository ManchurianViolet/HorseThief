using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class LaptopHeistTab : MonoBehaviour
{
    // ====================================================
    // 1. 데이터 정의 (인스펙터에서 설정)
    // ====================================================
    [System.Serializable]
    public class StageInfo
    {
        public string stageName;        // 미술관 이름
        public Sprite stageThumbnail;   // 사진
        [TextArea] public string description; // 설명 (팝업용)
        public int expectedProfit;      // 예상 수익 (팝업용)
        public List<ArtPieceData> artPool; // 훔칠 후보들
    }

    [Header("Stage Data")]
    public StageInfo[] stageList; // 6개 데이터 입력

    // ====================================================
    // 2. UI 연결 (슬롯 & 화살표)
    // ====================================================
    [Header("Carousel UI")]
    [SerializeField] private HeistStageSlot slotLeft;   // 왼쪽 슬롯 (들러리)
    [SerializeField] private HeistStageSlot slotCenter; // 가운데 슬롯 (주인공)
    [SerializeField] private HeistStageSlot slotRight;  // 오른쪽 슬롯 (들러리)

    [SerializeField] private Button btnPrev; // < 버튼
    [SerializeField] private Button btnNext; // > 버튼

    // ====================================================
    // 3. UI 연결 (팝업창)
    // ====================================================
    [Header("Popup UI")]
    [SerializeField] private GameObject popupPanel;      // 팝업 전체
    [SerializeField] private Image popupImage;           // 팝업 사진
    [SerializeField] private TextMeshProUGUI popupName;  // 팝업 이름
    [SerializeField] private TextMeshProUGUI popupDesc;  // 팝업 설명 ("설명 + 수익")
    [SerializeField] private Button btnPopupAction;      // "잠입" 버튼
    [SerializeField] private TextMeshProUGUI txtPopupAction; // 버튼 글씨

    // ====================================================
    // 내부 변수
    // ====================================================
    private int currentIndex = 0; // 현재 가운데에 떠있는 스테이지 번호 (0~5)

    private void OnEnable()
    {
        // 켜질 때, 내가 갈 수 있는 가장 높은 스테이지를 기본으로 보여줌
        if (GameManager.Instance != null)
        {
            currentIndex = Mathf.Clamp(GameManager.Instance.data.unlockedStageIndex, 0, stageList.Length - 1);
        }
        popupPanel.SetActive(false); // 팝업은 끄고 시작
        UpdateCarousel();
    }

    // ====================================================
    // 화살표 버튼 기능
    // ====================================================
    public void OnClickPrev()
    {
        if (currentIndex > 0)
        {
            currentIndex--;
            UpdateCarousel();
        }
    }

    public void OnClickNext()
    {
        if (currentIndex < stageList.Length - 1)
        {
            currentIndex++;
            UpdateCarousel();
        }
    }

    // ====================================================
    // 가운데 슬롯 클릭 -> 팝업 열기
    // ====================================================
    // HeistStageSlot 스크립트에서 이 함수를 호출하게 연결할 겁니다.
    public void OnClickCenterSlot(int stageIndex)
    {
        // 인자로 받은 index가 현재 보고 있는 index와 같은지 확인 (안전장치)
        if (stageIndex != currentIndex) return;

        OpenPopup(stageIndex);
    }

    // ====================================================
    // 팝업 내부 기능 (잠입 버튼 / 닫기 버튼)
    // ====================================================
    public void OnClickPopupAction() // "잠입" 버튼
    {
        // ★ [기존 코드 - 삭제하거나 주석 처리]
        // GameManager.Instance.GenerateMission(currentIndex);

        // ★ [새 코드 - 추가]
        HideoutDepartureCutscene cutscene = FindObjectOfType<HideoutDepartureCutscene>();

        if (cutscene != null)
        {
            cutscene.StartDeparture(currentIndex);
            Debug.Log($"🎬 {currentIndex + 1}탄 출발 연출 시작!");
        }
        else
        {
            // 연출 스크립트가 없으면 기존 방식으로 (안전장치)
            Debug.LogWarning("⚠️ HideoutDepartureCutscene을 찾을 수 없습니다. 바로 이동합니다.");
            GameManager.Instance.GenerateMission(currentIndex);
        }
    }

    public void OnClickPopupClose() // "X" 버튼
    {
        popupPanel.SetActive(false);
    }

    private void OpenPopup(int index) // ★ 여기 매개변수 이름이 'index'입니다.
    {
        StageInfo info = stageList[index];
        PlayerData data = GameManager.Instance.data;

        // 1. 정보 채우기
        popupImage.sprite = info.stageThumbnail;
        popupName.text = info.stageName;
        popupDesc.text = $"{info.description}\n\n예상 수익: ${info.expectedProfit}";

        // 2. 버튼 상태 결정 (잠금/완료/가능)

        // ★ [수정 1] dataIndex 대신 위에서 받은 'index'를 써야 합니다.
        int maxItems = GameManager.Instance.stageArtCounts[index];
        int stolenCount = data.GetStolenCount(index);
        bool isComplete = (stolenCount >= maxItems);

        // ★ [수정 2] 'isLocked' 변수를 먼저 만들어줘야 합니다.
        bool isLocked = false;

        // 잠금 체크 (1탄은 무조건 열림, 그 외는 이전 탄 완료 여부)
        if (index > 0) // ★ dataIndex -> index
        {
            // 이전 스테이지의 목표 개수 가져오기
            int prevMax = GameManager.Instance.stageArtCounts[index - 1]; // ★ dataIndex -> index

            // 이전 스테이지 훔친 개수가 목표보다 적으면 잠금!
            if (data.GetStolenCount(index - 1) < prevMax) isLocked = true; // ★ dataIndex -> index
        }

        // 3. UI 버튼 상태 반영
        if (isComplete)
        {
            txtPopupAction.text = "정복 완료";
            btnPopupAction.interactable = false;
            btnPopupAction.image.color = Color.gray;
        }
        else if (isLocked) // 이제 isLocked 변수가 있어서 에러 안 남!
        {
            txtPopupAction.text = "잠금됨 (이전 단계 필요)";
            btnPopupAction.interactable = false;
            btnPopupAction.image.color = Color.red;
        }
        else
        {
            txtPopupAction.text = "잠입 시작";
            btnPopupAction.interactable = true;
            btnPopupAction.image.color = Color.green;
        }

        popupPanel.SetActive(true);
    }

    // ====================================================
    // 화면 갱신 (슬라이드 처리)
    // ====================================================
    private void UpdateCarousel()
    {
        // 1. 화살표 상태
        btnPrev.gameObject.SetActive(currentIndex > 0);
        btnNext.gameObject.SetActive(currentIndex < stageList.Length - 1);

        // 2. 슬롯 데이터 업데이트 함수 (내부용)
        // index가 범위를 벗어나면(-1이거나 6이거나) 슬롯을 끕니다.
        UpdateSingleSlot(slotLeft, currentIndex - 1);
        UpdateSingleSlot(slotCenter, currentIndex);
        UpdateSingleSlot(slotRight, currentIndex + 1);
    }

    private void UpdateSingleSlot(HeistStageSlot slot, int dataIndex)
    {
        if (slot == null) return;

        // 데이터 범위 밖이면 슬롯 숨기기
        if (dataIndex < 0 || dataIndex >= stageList.Length)
        {
            slot.gameObject.SetActive(false);
            return;
        }

        slot.gameObject.SetActive(true);

        // 데이터 가져오기
        PlayerData data = GameManager.Instance.data;
        // ★ [수정] GameManager의 규칙표에서 개수 가져오기
        int maxItems = GameManager.Instance.stageArtCounts[dataIndex];

        int stolenCount = data.GetStolenCount(dataIndex);
        bool isComplete = (stolenCount >= maxItems);

        // ★ [수정] 잠금 체크 로직
        bool isLocked = false;
        if (dataIndex > 0)
        {
            // 이전 스테이지의 목표 개수 가져오기
            int prevMax = GameManager.Instance.stageArtCounts[dataIndex - 1];

            // 이전 스테이지 훔친 개수가 목표보다 적으면 잠금!
            if (data.GetStolenCount(dataIndex - 1) < prevMax) isLocked = true;
        }
        // 슬롯에게 정보 전달 (이미지, 상태 등)
        // *주의: 슬롯 스크립트도 약간 수정이 필요할 수 있습니다 (썸네일 변경 기능 추가 등)
        // 여기서는 기존 Initialize/UpdateState를 활용합니다.

        slot.Initialize(dataIndex, OnClickCenterSlot); // 클릭 시 내 함수 호출해달라고 연결
        slot.UpdateState(isLocked, isComplete, stolenCount, maxItems);

        // (추가) 썸네일이나 이름 바꾸는 기능이 슬롯에 있다면 여기서 호출
         slot.SetInfo(stageList[dataIndex].stageName, stageList[dataIndex].stageThumbnail);
    }
}