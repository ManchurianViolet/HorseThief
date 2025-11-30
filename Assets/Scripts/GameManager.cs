using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public PlayerData data;
    public int upgradeCost = 1;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadGameData();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void AddMoney(int amount)
    {
        data.money += amount;
        SaveGameData();
    }

    public bool SpendMoney(int amount)
    {
        if (data.money >= amount)
        {
            data.money -= amount;
            SaveGameData();
            return true;
        }
        return false;
    }

    // ★ [수정됨] 배열 인덱스 대신 각 변수를 컨트롤하도록 변경
    public bool TryUpgradeStat(int typeIndex)
    {
        if (data.money < upgradeCost) return false;

        // 1. 현재 레벨이 몇인지 확인
        int currentLv = 0;
        switch (typeIndex)
        {
            case 0: currentLv = data.powerLv; break;
            case 1: currentLv = data.neckRotLv; break;
            case 2: currentLv = data.neckLenLv; break;
            case 3: currentLv = data.jumpLv; break;
        }

        // 2. 만렙 체크
        int maxLevel = GetMaxLevelByHideout();
        if (currentLv >= maxLevel) return false;

        // 3. 돈 쓰고 레벨업
        SpendMoney(upgradeCost);

        switch (typeIndex)
        {
            case 0: data.powerLv++; break;
            case 1: data.neckRotLv++; break;
            case 2: data.neckLenLv++; break;
            case 3: data.jumpLv++; break;
        }

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

    // ... (나머지 은신처 이동, 저장/불러오기 코드는 기존과 동일) ...
    public void BuyAndMoveToHideout(int levelIndex)
    {
        data.currentHideoutLevel = levelIndex;
        data.unlockedHideouts[levelIndex - 1] = true;
        SaveGameData();
        SceneManager.LoadScene($"Hideout_Lv{levelIndex}");
    }

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
            data = new PlayerData();
        }
    }

    [ContextMenu("Reset Data")]
    public void ResetData()
    {
        data = new PlayerData();
        SaveGameData();
        Debug.Log("데이터 리셋 완료");
    }
}