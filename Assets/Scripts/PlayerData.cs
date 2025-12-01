[System.Serializable]
public class PlayerData
{
    public int money = 0;

    public int currentHideoutLevel = 1;
    public bool[] unlockedHideouts = new bool[3];
    public bool[] collectedArts = new bool[30];

    // ★ [수정됨] 배열을 없애고 이름을 붙였습니다!
    public int powerLv = 1;    // 마력
    public int neckRotLv = 1;  // 목 회전
    public int neckLenLv = 1;  // 목 길이
    public int jumpLv = 1;     // 점프 충전

    public int unlockedStageIndex = 0;
    public int[] stolenArtsCount = new int[6];

    public PlayerData()
    {
        money = 100;
        currentHideoutLevel = 1;

        unlockedHideouts[0] = true;
        unlockedHideouts[1] = false;
        unlockedHideouts[2] = false;

        // 초기화도 개별적으로
        powerLv = 1;
        neckRotLv = 1;
        neckLenLv = 1;
        jumpLv = 1;

        unlockedStageIndex = 0;
        collectedArts = new bool[30];
    }
    public int GetStolenCount(int stageIndex)
    {
        int count = 0;
        int startIndex = stageIndex * 5;

        // ★ 마지막 스테이지(5번)는 1개, 나머지는 5개
        int maxItems = (stageIndex == 5) ? 1 : 5;

        for (int i = 0; i < maxItems; i++)
        {
            if (collectedArts[startIndex + i]) count++;
        }
        return count;
    }
}