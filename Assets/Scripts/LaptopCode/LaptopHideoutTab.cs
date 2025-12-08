using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class LaptopHideoutTab : MonoBehaviour
{
    // === UI 연결 ===
    [Header("UI References")]
    [SerializeField] private Image hideoutImageDisplay; // 은신처 사진
    [SerializeField] private TextMeshProUGUI txtName;   // 은신처 이름

    [SerializeField] private TextMeshProUGUI txtDesc;   // ★ [수정됨] 설명글 (수동 입력 내용 표시)
    [SerializeField] private TextMeshProUGUI txtPrice;  // ★ [수정됨] 가격 표시용 별도 텍스트

    [SerializeField] private Button btnAction;          // 메인 버튼 (이미지)
    [SerializeField] private TextMeshProUGUI txtBtnAction; // 버튼 안의 글씨 ("구매하기" / "이동" 등)

    [SerializeField] private Button btnPrev; // 왼쪽 화살표
    [SerializeField] private Button btnNext; // 오른쪽 화살표

    // === 데이터 세팅 ===
    [System.Serializable]
    public class HideoutInfo
    {
        public string hideoutName;
        public Sprite hideoutSprite;
        [TextArea] public string description; // ★ [추가됨] 설명글 입력칸 (여러 줄 가능)
        public int price;
        // maxUpgradeLevel은 이제 UI 표시에 안 쓰지만, 로직용으로 남겨둬도 됨
        public int maxUpgradeLevel;
    }

    [Header("Hideout Data List")]
    public HideoutInfo[] hideoutList;

    private int currentIndex = 0;

    private void OnEnable()
    {
        if (GameManager.Instance != null)
        {
            currentIndex = Mathf.Clamp(GameManager.Instance.data.currentHideoutLevel - 1, 0, hideoutList.Length - 1);
        }
        UpdateUI();
    }

    public void OnClickPrev()
    {
        if (currentIndex > 0)
        {
            currentIndex--;
            UpdateUI();
        }
    }

    public void OnClickNext()
    {
        if (currentIndex < hideoutList.Length - 1)
        {
            currentIndex++;
            UpdateUI();
        }
    }

    public void OnClickAction()
    {
        HideoutInfo info = hideoutList[currentIndex];
        int targetLevel = currentIndex + 1;

        bool isUnlocked = GameManager.Instance.data.unlockedHideouts[currentIndex];

        if (isUnlocked) return; // 이미 샀으면 클릭 무시

        if (GameManager.Instance.SpendMoney(info.price))
        {
            Debug.Log($"은신처 {targetLevel} 구매 및 이사 완료!");
            GameManager.Instance.BuyAndMoveToHideout(targetLevel);
        }
        else
        {
            Debug.Log("돈이 부족합니다!");
        }
    }

    private void UpdateUI()
    {
        if (GameManager.Instance == null) return;

        HideoutInfo info = hideoutList[currentIndex];

        // 1. 정보 표시 (설명글 그대로 보여주기)
        if (hideoutImageDisplay != null) hideoutImageDisplay.sprite = info.hideoutSprite;
        if (txtName != null) txtName.text = info.hideoutName;
        if (txtDesc != null) txtDesc.text = info.description; // ★ 수동 입력한 설명 표시

        // 2. 화살표 활성/비활성
        btnPrev.interactable = (currentIndex > 0);
        btnNext.interactable = (currentIndex < hideoutList.Length - 1);

        // 3. 버튼 및 가격 표시
        int targetLevel = currentIndex + 1;
        bool isUnlocked = GameManager.Instance.data.unlockedHideouts[currentIndex];
        bool isCurrent = (GameManager.Instance.data.currentHideoutLevel == targetLevel);

        // ★ [수정됨] 우선순위 1등: 현재 살고 있는 집인가?
        if (isCurrent)
        {
            if (txtBtnAction != null) txtBtnAction.text = "Current Location";
            if (txtPrice != null) txtPrice.text = ""; // 가격 대신 보유함 표시
            btnAction.image.color = Color.gray;
            btnAction.interactable = false;
        }
        // ★ [수정됨] 우선순위 2등: 샀지만 지금 안 사는 집인가?
        else if (isUnlocked)
        {
            if (txtBtnAction != null) txtBtnAction.text = "Occupied";
            if (txtPrice != null) txtPrice.text = "-"; // 이미 샀으니 가격 표시 X
            btnAction.image.color = Color.white;
            btnAction.interactable = false;
        }
        // ★ 우선순위 3등: 아직 안 산 집인가?
        else
        {
            if (txtBtnAction != null) txtBtnAction.text = "Buy";
            if (txtPrice != null) txtPrice.text = $"{info.price} $"; // 가격 표시

            if (GameManager.Instance.data.money >= info.price)
            {
                btnAction.image.color = Color.yellow;
                btnAction.interactable = true;
            }
            else
            {
                btnAction.image.color = Color.red;
                btnAction.interactable = true;
            }
        }
    }
}