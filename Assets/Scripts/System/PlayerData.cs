[System.Serializable]
public class PlayerData
{
    public int money = 0;
    public int hospitalFailCount = 0;
    public int jailFailCount = 0;
    public int currentHideoutLevel = 1;
    public bool[] unlockedHideouts = new bool[3];
    public bool[] collectedArts = new bool[17];

    // 변수 선언부 (여기도 0, 아래도 0이어야 함)
    public int powerLv = 0;
    public int neckRotLv = 0;
    public int neckLenLv = 0;
    public int jumpLv = 0;

    public int unlockedStageIndex = 0;
    // public int[] stolenArtsCount = new int[6]; // 이건 이제 안 쓰는 변수라 지워도 됩니다 (체크리스트 쓰니까)

    // ★ [생성자] 게임 처음 시작할 때 딱 한 번 실행되는 곳
    public PlayerData()
    {
        money = 100;
        currentHideoutLevel = 1;

        unlockedHideouts[0] = true;
        unlockedHideouts[1] = false;
        unlockedHideouts[2] = false;


        // ★ [수정됨] 여기를 꼭 0으로 바꿔야 합니다!
        powerLv = 0;
        neckRotLv = 0;
        neckLenLv = 0;
        jumpLv = 0;

        unlockedStageIndex = 0;
        collectedArts = new bool[17];
    }

    // ★ 3. 개수 세는 함수 수정 (하드코딩 제거)
    public int GetStolenCount(int stageIndex)
    {
        // GameManager가 없으면 계산 불가 (안전장치)
        if (GameManager.Instance == null) return 0;

        // 시작 번호 계산
        int startIndex = 0;
        for (int i = 0; i < stageIndex; i++)
        {
            startIndex += GameManager.Instance.stageArtCounts[i];
        }

        // 끝 번호(개수) 계산
        int maxItems = GameManager.Instance.stageArtCounts[stageIndex];

        int count = 0;
        for (int i = 0; i < maxItems; i++)
        {
            // 배열 범위 체크 안전장치 추가
            if (startIndex + i < collectedArts.Length)
            {
                if (collectedArts[startIndex + i]) count++;
            }
        }
        return count;
    }
}