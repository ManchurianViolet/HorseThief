using UnityEngine;

public class Key : MonoBehaviour
{
    public string keyValue = "Q"; // 이 키의 글자 (인스펙터에서 설정)
    private KeyBoardStage keyboardStage; // KeyBoardStage 참조
    private float keyCooldown = 0.5f;
    private float lastKeyTime;

    // Start 메서드는 그대로 유지
    void Start()
    {
        // KeyBoardStage 찾기
        keyboardStage = FindAnyObjectByType<KeyBoardStage>();
        if (keyboardStage == null)
        {
            Debug.LogError("KeyBoardStage를 찾을 수 없습니다!");
        }

        // 콜리더 설정 확인
        Collider collider = GetComponent<Collider>();
        if (collider == null)
        {
            collider = gameObject.AddComponent<BoxCollider>();
        }
        collider.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("HorseHead") && keyboardStage != null && Time.time - lastKeyTime > keyCooldown)
        {
            lastKeyTime = Time.time; // 쿨다운 시간 갱신

            // ★★★ 수정된 핵심 로직
            if (keyValue == "Shift")
            {
                // keyValue가 "Shift"일 경우, 문자를 입력하는 대신 Shift 상태를 토글합니다.
                keyboardStage.ToggleShift();
                Debug.Log("Shift Key Toggled.");
                keyboardStage.PlayKeyPressSound();
            }
            else
            {
                // 그 외 일반 키들은 문자를 입력합니다.
                keyboardStage.AddCharacter(keyValue);
                Debug.Log("Key Pressed: " + keyValue);
                keyboardStage.PlayKeyPressSound();
            }
        }
    }
}