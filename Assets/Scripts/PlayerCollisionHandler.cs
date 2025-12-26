using UnityEngine;
using System.Collections;

public class PlayerCollisionHandler : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameOverUI gameOverUI;
    [SerializeField] private Rigidbody myRb;

    [Header("Settings")]
    [SerializeField] private float bounceForce = 20f; // 튕겨 나가는 힘

    private bool isDead = false;

    private void OnCollisionEnter(Collision collision)
    {
        if (isDead) return;

        // 경찰차 태그 확인 (경찰차에 'PoliceCar' 태그 꼭 붙이세요!)
        if (collision.gameObject.CompareTag("PoliceCar"))
        {
            StartCoroutine(DieSequence(collision));
        }
    }

    private IEnumerator DieSequence(Collision collision)
    {
        isDead = true;
        Debug.Log("💀 WASTED sequence start");

        // 1. 물리력으로 뻥! 날리기
        // (충돌 반대 방향 + 위쪽으로 힘을 가함)
        Vector3 bounceDir = (transform.position - collision.contacts[0].point).normalized;
        bounceDir += Vector3.up * 0.5f; // 약간 위로 뜨게
        myRb.AddForce(bounceDir * bounceForce, ForceMode.Impulse);

        // 말의 제어 스크립트 끄기 (HorseControl이 있다면)
        // GetComponent<HorseControl>().enabled = false; 
        // 대신 물리적인 회전(Ragdoll 느낌)을 위해 FreezeRotation 해제 추천
        myRb.constraints = RigidbodyConstraints.None;

        // 2. Beautify 회색조 처리 (Beautify 버전에 따라 API가 다를 수 있음)
        // 보통 Beautify.instance.saturation = 0; 방식을 씀
        // Beautify.instance.saturation = 0f; 
        // Beautify.instance.vignetting = true; 
        Debug.Log("🎨 화면 회색조 변경 (Beautify 적용)");

        // 3. 슬로우 모션 (0.3배속)
        Time.timeScale = 0.3f;
        Time.fixedDeltaTime = 0.02f * Time.timeScale; // 물리 연산도 같이 느려지게

        // 4. 3초 대기 (Realtime 기준 3초)
        yield return new WaitForSecondsRealtime(3.0f);

        // 5. UI 호출
        if (gameOverUI != null) gameOverUI.ShowWasted();
    }
}