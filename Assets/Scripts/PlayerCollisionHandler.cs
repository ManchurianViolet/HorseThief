using UnityEngine;
using System.Collections;
using UnityEngine.Rendering;
using Beautify.Universal;

public class PlayerCollisionHandler : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameOverUI gameOverUI;
    [SerializeField] private Rigidbody myRb;
    [SerializeField] private Volume globalVolume;

    // ★ 말의 애니메이터와 조작 스크립트를 끄기 위해 가져옴
    [SerializeField] private MonoBehaviour horseControlScript; // HorseControl 스크립트를 여기에 넣으세요

    [Header("Settings")]
    // ★ 질량 500을 날리려면 힘이 아주 커야 합니다! (기본값 50000으로 수정)
    [SerializeField] private float bounceForce = 300000f;

    private bool isDead = false;

    private void OnCollisionEnter(Collision collision)
    {
        if (isDead) return;

        if (collision.gameObject.CompareTag("PoliceCar"))
        {
            StartCoroutine(DieSequence(collision));
        }
    }

    private IEnumerator DieSequence(Collision collision)
    {
        isDead = true;
        Debug.Log("💀 WASTED sequence start");

        // 1. ★ [중요] Beautify Sepia 즉시 적용 (Yield 전에 실행!)
        if (globalVolume != null && globalVolume.profile.TryGet(out Beautify.Universal.Beautify beautify))
        {
            beautify.active = true;

            // Saturate 대신 Sepia를 1로!
            beautify.sepia.overrideState = true;
            beautify.sepia.value = 1f;

            Debug.Log("🎨 Sepia(흑백) 즉시 적용 완료!");
        }
        else
        {
            Debug.LogError("⚠️ Global Volume이 연결되지 않았거나 Beautify 프로필이 없습니다!");
        }

        // 2. ★ [중요] 애니메이터 끄기 (이게 켜져 있으면 절대 안 날아감)
        if (horseControlScript != null) horseControlScript.enabled = false;

        // 3. 물리 충돌 (뻥! 날리기)
        if (myRb != null)
        {
            myRb.isKinematic = false;
            myRb.constraints = RigidbodyConstraints.None; // 데굴데굴 구르게 잠금 해제

            Vector3 bounceDir = (transform.position - collision.contacts[0].point).normalized;
            bounceDir += Vector3.up * 1.0f; // 위로 솟구치게

            myRb.AddForce(bounceDir * bounceForce, ForceMode.Impulse);
            myRb.AddTorque(Random.insideUnitSphere * bounceForce, ForceMode.Impulse);
        }

        // 4. 슬로우 모션
        Time.timeScale = 0.3f;
        Time.fixedDeltaTime = 0.02f * Time.timeScale;

        // 5. 3초 대기 (흑백 화면 + 날아가는 모습 감상)
        yield return new WaitForSecondsRealtime(3.0f);

        // 6. UI 표시
        if (gameOverUI != null) gameOverUI.ShowWasted();
    }

    // 게임 재시작 시 색상 복구
    private void OnDestroy()
    {
        if (globalVolume != null && globalVolume.profile.TryGet(out Beautify.Universal.Beautify beautify))
        {
            beautify.sepia.value = 0f; // 원래대로 0
        }
    }
}