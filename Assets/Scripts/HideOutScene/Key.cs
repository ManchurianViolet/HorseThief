using UnityEngine;

public class Key : MonoBehaviour
{
    public string keyValue = "Q"; // 키 값 (인스펙터에서 "q"라고 적든 "Q"라고 적든 상관없음)

    // 두 매니저 참조
    private KeyBoardStage trainingManager;
    private MuseumHacking missionManager;

    private float keyCooldown = 0.2f;
    private float lastKeyTime;

    void Start()
    {
        trainingManager = FindAnyObjectByType<KeyBoardStage>();
        missionManager = FindAnyObjectByType<MuseumHacking>();

        // 콜리더 설정
        Collider collider = GetComponent<Collider>();
        if (collider == null)
        {
            collider = gameObject.AddComponent<BoxCollider>();
        }
        collider.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        // "Nose" 태그만 인식
        if (other.CompareTag("Nose") && Time.time - lastKeyTime > keyCooldown)
        {
            lastKeyTime = Time.time;
            PressKeyLogic();
        }
    }

    private void PressKeyLogic()
    {
        // ★ [변경 1] Shift 키는 아예 무시 (눌러도 아무 일 없음)
        if (keyValue == "Shift") return;

        // ★ [변경 2] 소문자(q)로 적혀있어도 무조건 대문자(Q)로 변환
        string upperKey = keyValue.ToUpper();

        // 1. 연습장 (Training)
        if (trainingManager != null)
        {
            // Shift 토글 기능 삭제 -> 그냥 글자 입력
            trainingManager.AddCharacter(upperKey);

            // 로그 확인용
            Debug.Log($"Training Input: {upperKey}");

            trainingManager.PlayKeyPressSound();
        }
        // 2. 미술관 (Museum)
        else if (missionManager != null)
        {
            missionManager.AddCharacter(upperKey);

            // 로그 확인용
            Debug.Log($"Museum Input: {upperKey}");
        }
    }
}