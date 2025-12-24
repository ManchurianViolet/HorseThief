using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public PlayerData data;
    public int upgradeCost = 1;
    [Header("=== Heist System ===")]
    // ★ 전체 미술품 30개 (인스펙터에서 순서대로 등록 필수!)
    public ArtPieceData[] allArtPieces = new ArtPieceData[30];

    // ★ 이번 판의 타겟 (로딩씬 & 미술관씬에서 이걸 갖다 씀)
    public ArtPieceData currentMissionTarget;
    // ★ [이 줄을 추가하세요!] 이번 판의 스테이지 번호 (로딩씬에서 씀)
    public int currentTargetStageIndex = 0;
    public int currentTargetIndex; // 타겟 번호 (0~29) - 나중에 저장할 때 씀
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
    public void GenerateMission(int stageIndex) // stageIndex: 0 ~ 5
    {
        // 1. 이 스테이지의 보물 구간 (예: 2탄이면 5~9번)
        int startIndex = stageIndex * 5;
        // ★ [추가] 목표 스테이지 번호 저장 (이게 없어서 에러 났음)
        currentTargetStageIndex = stageIndex;
        int maxItems = (stageIndex == 5) ? 1 : 5;
        // 2. 안 훔친 것들만 골라내기 (후보 리스트 작성)
        List<int> candidates = new List<int>();

        for (int i = 0; i < maxItems; i++)
        {
            int artIndex = startIndex + i;
            // "아직 안 훔쳤니(false)? 그럼 넌 후보다!"
            if (data.collectedArts[artIndex] == false)
            {
                candidates.Add(artIndex);
            }
        }

        // 3. 예외 처리 (이미 다 털었음)
        if (candidates.Count == 0)
        {
            Debug.Log("이 미술관은 이미 정복했습니다!");
            return;
        }

        // 4. 후보 중 하나를 랜덤으로 뽑기!
        int randomPick = Random.Range(0, candidates.Count);
        int finalTargetIndex = candidates[randomPick];

        // 5. 타겟 확정 및 저장
        currentTargetIndex = finalTargetIndex;
        currentMissionTarget = allArtPieces[finalTargetIndex];

        Debug.Log($"미션 확정! 목표: {currentMissionTarget.artName} (번호: {currentTargetIndex})");

    }
    [ContextMenu("Reset Data")]
    public void ResetData()
    {
        data = new PlayerData();
        SaveGameData();
        Debug.Log("데이터 리셋 완료");
    }
    // GameManager.cs 안에 추가/수정

    public bool HasSaveData()
    {
        string path = Application.persistentDataPath + "/savefile.json";
        return System.IO.File.Exists(path);
    }

    // "New Game" 눌렀을 때 실행할 함수
    public void StartNewGame()
    {
        // 1. 데이터 초기화
        data = new PlayerData();

        // 2. 초기화된 데이터를 즉시 파일에 덮어쓰기 (기존 파일 삭제 효과)
        SaveGameData();

        // 3. 1번 은신처(또는 튜토리얼)로 이동
        SceneManager.LoadScene("Hideout_Lv1");
    }

    // "Continue" 눌렀을 때 실행할 함수
    public void ContinueGame()
    {
        LoadGameData(); // 파일 불러오기

        // 저장된 은신처 레벨에 맞춰서 씬 이동
        SceneManager.LoadScene($"Hideout_Lv{data.currentHideoutLevel}");
    }
}