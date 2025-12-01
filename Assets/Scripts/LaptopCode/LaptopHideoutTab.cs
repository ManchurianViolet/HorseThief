using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class LaptopHideoutTab : MonoBehaviour
{
    // === UI 연결 ===
    [Header("UI References")]
    [SerializeField] private Image hideoutImageDisplay; // 은신처 사진
    [SerializeField] private TextMeshProUGUI txtName;   // 은신처 이름
    [SerializeField] private TextMeshProUGUI txtEffect; // 효과 설명

    // [수정됨] 버튼 안의 텍스트 변수 삭제함
    [SerializeField] private Button btnAction;          // 메인 버튼 (이미지)

    [SerializeField] private Button btnPrev; // 왼쪽 화살표
    [SerializeField] private Button btnNext; // 오른쪽 화살표

    // === 데이터 세팅 ===
    [System.Serializable]
    public class HideoutInfo
    {
        public string hideoutName;
        public Sprite hideoutSprite;
        public int price;
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

        // 1. 정보 표시
        if (hideoutImageDisplay != null) hideoutImageDisplay.sprite = info.hideoutSprite;
        txtName.text = info.hideoutName;
        txtEffect.text = $"말 스텟 강화 최대치 {info.maxUpgradeLevel}";

        // 2. 화살표 활성/비활성
        btnPrev.interactable = (currentIndex > 0);
        btnNext.interactable = (currentIndex < hideoutList.Length - 1);

        // 3. 버튼 상태 (색깔로 구분)
        int targetLevel = currentIndex + 1;
        bool isUnlocked = GameManager.Instance.data.unlockedHideouts[currentIndex];
        bool isCurrent = (GameManager.Instance.data.currentHideoutLevel == targetLevel);

        if (isCurrent)
        {
            // 현재 살고 있는 집 -> 회색 (클릭 불가)
            btnAction.image.color = Color.gray;
            btnAction.interactable = false;
        }
        else if (isUnlocked)
        {
            // 이미 산 집 -> 흰색 (클릭 불가 - 이사 기능 뺐으므로)
            btnAction.image.color = Color.white;
            btnAction.interactable = false;
        }
        else
        {
            // 아직 안 산 집 -> 돈 있으면 노랑, 없으면 빨강
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