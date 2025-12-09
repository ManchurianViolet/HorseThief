using UnityEngine;

public class Key : MonoBehaviour
{
    public string keyValue = "Q"; // "BackSpace", "Space", "Enter" 등 입력

    private KeyBoardStage trainingManager;
    private MuseumHacking missionManager;

    private float keyCooldown = 0.2f;
    private float lastKeyTime;

    void Start()
    {
        trainingManager = FindAnyObjectByType<KeyBoardStage>();
        missionManager = FindAnyObjectByType<MuseumHacking>();

        Collider collider = GetComponent<Collider>();
        if (collider == null) collider = gameObject.AddComponent<BoxCollider>();
        collider.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Nose") && Time.time - lastKeyTime > keyCooldown)
        {
            lastKeyTime = Time.time;
            PressKeyLogic();
        }
    }

    private void PressKeyLogic()
    {
        // Shift 무시
        if (keyValue == "Shift") return;

        // ★ [핵심] 무조건 대문자로 변환해서 보냄
        // BackSpace -> BACKSPACE
        // Space     -> SPACE
        // Enter     -> ENTER
        // q         -> Q
        string upperKey = keyValue.ToUpper();

        if (trainingManager != null)
        {
            trainingManager.AddCharacter(upperKey);
            // trainingManager.PlayKeyPressSound(); // 소리는 매니저에서
        }
        else if (missionManager != null)
        {
            missionManager.AddCharacter(upperKey);
        }
    }
}