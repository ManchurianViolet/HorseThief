using System.Collections.Generic;

[System.Serializable]
public class PlayerData
{
    // === 1. 자산 ===
    public int money = 0;

    // === 2. 은신처 상태 ===
    public int currentHideoutLevel = 1; // 현재 살고 있는 은신처 (1, 2, 3)
    public bool[] unlockedHideouts = new bool[3]; // 구매 여부 (0:Lv1, 1:Lv2, 2:Lv3)

    // === 3. 말 업그레이드 레벨 (현재 레벨) ===
    // [0]:마력, [1]:목회전, [2]:목길이, [3]:점프충전
    public int[] horseUpgradeLevels = new int[4];

    // === 4. 미술관 진행도 ===
    public int unlockedStageIndex = 0; // 현재 해금된 스테이지 (0 ~ 5)
    // 각 스테이지별 훔친 미술품 개수 (총 6개 스테이지)
    public int[] stolenArtsCount = new int[6];

    // 생성자 (초기화)
    public PlayerData()
    {
        money = 100; // 테스트용 초기 자금
        currentHideoutLevel = 1;

        // 기본 은신처(Lv1)는 해금 상태
        unlockedHideouts[0] = true;
        unlockedHideouts[1] = false;
        unlockedHideouts[2] = false;

        // 업그레이드는 모두 1레벨부터 시작
        for (int i = 0; i < 4; i++) horseUpgradeLevels[i] = 1;

        unlockedStageIndex = 0;
    }
}