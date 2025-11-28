using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO; // 저장 기능을 위해 필요

public class GameManager : MonoBehaviour
{
    // 싱글톤 패턴 (어디서든 GameManager.Instance로 접근 가능)
    public static GameManager Instance;

    public PlayerData data; // 위에서 만든 데이터 가방

    // 업그레이드 비용 (일단 1달러로 고정)
    public int upgradeCost = 1;

    private void Awake()
    {
        // 싱글톤 유지 및 파괴 방지 로직
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // 씬 이동해도 나를 파괴하지 마라
            LoadGameData(); // 시작하자마자 저장된 데이터 불러오기
        }
        else
        {
            Destroy(gameObject); // 이미 매니저가 있으면 나는 가짜니까 사라짐
        }
    }

    // ====================================================
    // 💰 돈 관리 기능
    // ====================================================
    public void AddMoney(int amount)
    {
        data.money += amount;
        SaveGameData();
        Debug.Log($"돈 획득! 현재 잔고: {data.money}$");
    }

    public bool SpendMoney(int amount)
    {
        if (data.money >= amount)
        {
            data.money -= amount;
            SaveGameData();
            Debug.Log($"돈 사용! 남은 돈: {data.money}$");
            return true; // 구매 성공
        }
        else
        {
            Debug.Log("돈이 부족합니다!");
            return false; // 구매 실패
        }
    }

    // ====================================================
    // 🐎 업그레이드 기능
    // ====================================================
    // typeIndex -> 0:마력, 1:목회전, 2:목길이, 3:점프충전
    public bool TryUpgradeStat(int typeIndex)
    {
        // 1. 돈 체크
        if (data.money < upgradeCost) return false;

        // 2. 최대 레벨 제한 체크 (은신처 등급에 따라)
        int maxLevel = GetMaxLevelByHideout();
        if (data.horseUpgradeLevels[typeIndex] >= maxLevel)
        {
            Debug.Log("현재 은신처에서는 더 이상 업그레이드할 수 없습니다.");
            return false;
        }

        // 3. 업그레이드 실행
        SpendMoney(upgradeCost);
        data.horseUpgradeLevels[typeIndex]++;
        SaveGameData();
        return true;
    }

    public int GetMaxLevelByHideout()
    {
        switch (data.currentHideoutLevel)
        {
            case 1: return 5;
            case 2: return 10;
            case 3: return 20;
            default: return 5;
        }
    }

    // ====================================================
    // 🏠 은신처 구매 및 이동
    // ====================================================
    public void BuyAndMoveToHideout(int levelIndex) // levelIndex: 1, 2, 3
    {
        // 이미 샀거나, 돈 내고 살 수 있다면
        // (가격 로직은 나중에 구체화, 일단은 무료 처리 혹은 별도 로직)

        data.currentHideoutLevel = levelIndex;
        // 배열 인덱스는 0부터 시작하므로 -1
        data.unlockedHideouts[levelIndex - 1] = true;

        SaveGameData();

        // 씬 이동 (씬 이름 규칙: Hideout_Lv1, Hideout_Lv2...)
        SceneManager.LoadScene($"Hideout_Lv{levelIndex}");
    }

    // ====================================================
    // 💾 저장 및 불러오기 (JSON 방식)
    // ====================================================
    public void SaveGameData()
    {
        string json = JsonUtility.ToJson(data);
        File.WriteAllText(Application.persistentDataPath + "/savefile.json", json);
    }

    public void LoadGameData()
    {
        string path = Application.persistentDataPath + "/savefile.json";
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            data = JsonUtility.FromJson<PlayerData>(json);
        }
        else
        {
            data = new PlayerData(); // 저장된 게 없으면 새 데이터 생성
        }
    }

    // (개발용) 데이터 리셋 치트키
    [ContextMenu("Reset Data")]
    public void ResetData()
    {
        data = new PlayerData();
        SaveGameData();
        Debug.Log("데이터 리셋 완료");
    }
}