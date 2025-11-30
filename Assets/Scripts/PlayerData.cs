[System.Serializable]
public class PlayerData
{
    public int money = 0;

    public int currentHideoutLevel = 1;
    public bool[] unlockedHideouts = new bool[3];

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
    }
}