using Beautify.Universal;
using System.Collections;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Rendering;

public class PlayerCollisionHandler : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameOverUI gameOverUI;
    [SerializeField] private Rigidbody myRb;
    [SerializeField] private Volume globalVolume;
    [SerializeField] private HorseControl horseControlScript; // ★ 타입 명시

    [Header("Settings")]
    [SerializeField] private float bounceForce = 5000f;    // ★ 충돌 반발력 (줄임)
    [SerializeField] private float additionalUpForce = 5000f; // ★ 추가 상승력 (대폭 줄임!)
    [SerializeField] private float torqueForce = 10000f;    // ★ 데굴데굴 구르는 힘

    private bool isDead = false;

    private void Start()
    {
        // ★ [디버깅] 시작할 때 세팅 확인
        if (myRb == null) myRb = GetComponent<Rigidbody>();

        Debug.Log($"🔍 [초기 세팅] Mass: {myRb.mass}, Drag: {myRb.linearDamping}, Angular Drag: {myRb.angularDamping}");
        Debug.Log($"🔍 [초기 세팅] isKinematic: {myRb.isKinematic}, Constraints: {myRb.constraints}");
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (isDead) return;

        // ★ [디버깅] 충돌 감지 로그
        Debug.Log($"🚗 충돌 감지! 상대: {collision.gameObject.name}, 태그: {collision.gameObject.tag}");
        Debug.Log($"🚗 충돌 지점: {collision.contacts[0].point}, 내 위치: {transform.position}");

        if (collision.gameObject.CompareTag("PoliceCar"))
        {
            StartCoroutine(DieSequence(collision));
        }
    }

    private IEnumerator DieSequence(Collision collision)
    {
        isDead = true;
        Debug.Log("💀💀💀 WASTED SEQUENCE START 💀💀💀");

        // ===== 0. 경찰차 카메라로 전환 (제일 먼저!) =====
        GameObject policeCar = collision.gameObject;
        CinemachineCamera policeCam = policeCar.GetComponentInChildren<CinemachineCamera>();

        if (policeCam != null)
        {
            var brain = Camera.main.GetComponent<CinemachineBrain>();

            // ★ v3 방식
            var prevBlend = brain.DefaultBlend;

            // ★ Cut 강제
            brain.DefaultBlend = new CinemachineBlendDefinition(
                CinemachineBlendDefinition.Styles.Cut,
                0f
                        );
            policeCam.Priority = 100; // 카메라 활성화!
            Debug.Log($"📹 경찰차 카메라로 전환: {policeCar.name}");
        }
        else
        {
            Debug.LogWarning($"⚠️ {policeCar.name}에 카메라가 없습니다!");
        }

        // ===== 1. 세피아 효과 (즉시 적용) =====
        if (globalVolume != null && globalVolume.profile.TryGet(out Beautify.Universal.Beautify beautify))
        {
            beautify.active = true;
            beautify.sepia.overrideState = true;
            beautify.sepia.value = 1f;
            Debug.Log("🎨 Sepia 적용 완료");
        }

        // ===== 2. 말 조작 스크립트 끄기 =====
        if (horseControlScript != null)
        {
            horseControlScript.enabled = false;
            Debug.Log("🔒 HorseControl 비활성화");
        }

        // ===== 3. 물리 엔진 완전 해제 =====
        if (myRb != null)
        {
            Debug.Log($"💥 [충돌 전] isKinematic: {myRb.isKinematic}, Constraints: {myRb.constraints}");

            // ★ [핵심] 모든 제약 해제
            myRb.isKinematic = false;
            myRb.constraints = RigidbodyConstraints.None;

            // ★ [핵심] 저항력 제거 (날아가는 걸 방해하는 요소들)
            myRb.linearDamping = 0f;  // 공기 저항 제거
            myRb.angularDamping = 0f; // 회전 저항 제거

            Debug.Log($"💥 [설정 변경 후] isKinematic: {myRb.isKinematic}, Constraints: {myRb.constraints}");
            Debug.Log($"💥 [저항 제거] Linear Damping: {myRb.linearDamping}, Angular Damping: {myRb.angularDamping}");

            // ★ 즉시 한 프레임 대기 (물리 엔진이 설정을 적용할 시간 주기)
            yield return new WaitForFixedUpdate();

            // ===== 4. 폭발적인 힘 적용 =====

            // ★ [충돌 지점에서 반대 방향으로 튕겨나가기]
            Vector3 hitPoint = collision.contacts[0].point;
            Vector3 bounceDirection = (transform.position - hitPoint).normalized;

            // 충돌 반발력 (자연스러운 튕겨나감)
            Vector3 bounceForceVector = bounceDirection * bounceForce;

            // ★ [추가] 위로 솟구치는 힘 (중력 거스르기!)
            Vector3 upForceVector = Vector3.up * additionalUpForce;

            // 최종 힘 = 튕겨나가는 힘 + 위로 솟구치는 힘
            Vector3 finalForce = bounceForceVector + upForceVector;

            Debug.Log($"🚀 [힘 분석]");
            Debug.Log($"   충돌 반발 방향: {bounceDirection}");
            Debug.Log($"   충돌 반발 힘: {bounceForce}");
            Debug.Log($"   추가 상승 힘: {additionalUpForce}");
            Debug.Log($"   최종 힘: {finalForce}");
            Debug.Log($"   예상 초기 속도: {finalForce.magnitude / myRb.mass} units/s");

            // 힘 적용! (Impulse = 순간적으로 강하게!)
            myRb.AddForce(finalForce, ForceMode.Impulse);

            // 회전도 추가 (공중에서 빙글빙글)
            Vector3 randomTorque = new Vector3(
                Random.Range(-torqueForce, torqueForce),
                Random.Range(-torqueForce, torqueForce),
                Random.Range(-torqueForce, torqueForce)
            );
            myRb.AddTorque(randomTorque, ForceMode.Impulse);

            Debug.Log($"💨 [즉시 확인] 속도: {myRb.linearVelocity}");
            Debug.Log($"💨 [즉시 확인] 회전: {myRb.angularVelocity}");
        }
        else
        {
            Debug.LogError("❌ Rigidbody가 없습니다!");
        }

        // ===== 5. 슬로우 모션 (감상 타임) =====
        Time.timeScale = 0.15f; // 더 느리게
        Time.fixedDeltaTime = 0.02f * Time.timeScale;
        Debug.Log("🎬 슬로우 모션 시작");

        // ===== 6. 3초 감상 =====
        float startTime = Time.realtimeSinceStartup;
        float maxHeight = transform.position.y;

        while (Time.realtimeSinceStartup - startTime < 3.0f)
        {
            // ★ [디버깅] 최고 높이 체크
            if (transform.position.y > maxHeight)
            {
                maxHeight = transform.position.y;
            }
            yield return null;
        }

        Debug.Log($"📊 [통계] 최고 도달 높이: {maxHeight:F1}");
        Debug.Log($"📊 [추천] 적정 높이는 50~150 정도입니다!");

        // ===== 7. UI 표시 =====
        if (gameOverUI != null)
        {
            gameOverUI.ShowWasted();
            Debug.Log("📺 게임오버 UI 표시");
        }
    }

    // 게임 재시작 시 정리
    private void OnDestroy()
    {
        Time.timeScale = 1f; // 시간 정상화

        if (globalVolume != null && globalVolume.profile.TryGet(out Beautify.Universal.Beautify beautify))
        {
            beautify.sepia.value = 0f;
        }
    }
}