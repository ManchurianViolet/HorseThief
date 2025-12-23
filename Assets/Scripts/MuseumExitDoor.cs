using UnityEngine;

public class MuseumExitDoor : MonoBehaviour
{
    // InteractiveArt에서 이 변수를 true로 바꿔줄 겁니다.
    public bool canExit = false;

    private void OnTriggerEnter(Collider other)
    {
        // 말(Player 또는 HorseChest)이 문에 닿았을 때
        if (other.CompareTag("Player") || other.CompareTag("HorseChest"))
        {
            if (canExit)
            {
                Debug.Log("🚪 탈출 조건 만족! 고속도로 씬으로 전환합니다.");

                // HighwayManager를 찾아서 탈출 시퀀스 시작
                HighwayManager highwayManager = FindObjectOfType<HighwayManager>();
                if (highwayManager != null)
                {
                    highwayManager.StartEscapeSequence();
                }
                else
                {
                    Debug.LogError("🚨 HighwayManager가 없습니다!");
                }
            }
            else
            {
                Debug.Log("🔒 아직 그림을 훔치지 않았습니다! (F키로 교체하세요)");
                // 팁: 여기에 "그림을 먼저 훔치세요!" 같은 UI 팝업을 띄워도 좋습니다.
            }
        }
    }
}