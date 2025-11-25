using UnityEngine;

public class HorseControl_RacingStage : MonoBehaviour
{
    // 🔔 외부에서 카운트다운 시작/종료 상태를 제어하기 위한 변수
    [HideInInspector] public bool isCountdownEnd = false;

    // 🔔 CountdownManager가 이벤트를 구독하여 실격 여부를 알 수 있도록 합니다.
    public event System.Action OnFalseStart;
    private Rigidbody rb;
    private Transform frontShinL, frontShinR, footL, footR;
    [SerializeField] private Transform head; // 머리 오브젝트 참조
    [SerializeField] private Transform scull;
    [SerializeField] private Transform neck;
    // 다리 회전 속도 (초당 회전 각도)
    [SerializeField] private float rotationSpeed = 90f;
    private float currentRotationZ = 0f; // 현재 Z축 회전 각도
    private float currentRotationX = 0f; // 현재 X축 회전 각도
    private LayerMask groundLayerMask;

    // 다리가 바닥을 밀 때 추가적인 힘 (전방)
    [SerializeField] private float pushForce = 200f;

    // 방향 전환을 위한 회전 토크 (고정값)
    [SerializeField] private float turnTorque = 50f; // 회전 토크 크기

    // 머리 회전 속도 (초당 회전 각도)
    [SerializeField] private float headRotationSpeed = 60f;
    // 점프 관련 변수
    [SerializeField] private float jumpForce = 500f; // 기본 점프력
    [SerializeField] private float chargeMultiplier = 2f; // 충전 시 최대 배율
    [SerializeField] private float maxChargeTime = 1f; // 최대 충전 시간 (초)
    [SerializeField] private float chargeTime = 0f; // 현재 충전 시간
    [SerializeField] private bool isCharging = false; // 충전 중인지 체크
    [SerializeField] private bool isGrounded = false; // 바닥에 닿아 있는지 체크
    [SerializeField] private float moveSpeed = 1f; // 머리이동 속도 (초당 단위)
    [SerializeField] private float minY = 0f;     // 최소 Y 위치 (로컬 기준)
    [SerializeField] private float maxY = 5f;     // 최대 Y 위치 (로컬 기준)

    // 회전 중인지 여부를 추적하는 변수
    private bool isTurning = false;
    private RigidbodyConstraints defaultConstraints; // 기본 Constraints 저장
    void Start()
    {
        // Rigidbody 참조
        rb = GetComponent<Rigidbody>();
        groundLayerMask = LayerMask.GetMask("Ground");
        // 다리 오브젝트 참조
        frontShinL = transform.Find("horse.001/Root/spine.005/spine.006/spine.007/front_shoulder.L/front_thigh.L/front_shin.L");
        frontShinR = transform.Find("horse.001/Root/spine.005/spine.006/spine.007/front_shoulder.R/front_thigh.R/front_shin.R");
        footL = transform.Find("horse.001/Root/spine.005/shoulder.L/thigh.L/shin.L/foot.L");
        footR = transform.Find("horse.001/Root/spine.005/shoulder.R/thigh.R/shin.R/foot.R");
        head = transform.Find("horse.001/Root/spine.005/spine.006/spine.007/spine.008");
        scull = transform.Find("horse.001/Root/spine.005/spine.006/spine.007/spine.008/spine.009/spine.010/scull");
        neck = transform.Find("horse.001/Root/spine.005/spine.006/spine.007/spine.008/spine.009");
        // 오브젝트가 제대로 참조되었는지 확인
        if (frontShinL == null || frontShinR == null || footL == null || footR == null || head == null)
        {
            Debug.LogError("다리 오브젝트를 찾을 수 없습니다. 경로를 확인하세요.");
        }
        rb.mass = 100f; // 무게 조정
        rb.linearDamping = 0.05f; // 저항
        rb.angularDamping = 0.15f; // 회전 저항
        // Rigidbody Constraints 초기화 (Y축 회전 고정 포함)
        defaultConstraints = RigidbodyConstraints.FreezeRotationY;
        rb.constraints = defaultConstraints; // 초기 Constraints 설정
    }

    void Update()
    {
        // 🚨 실격 감지 로직 및 움직임 제어
        if (!isCountdownEnd)
        {
            // 카운트다운 중 (isCountdownEnd == false) 일 때,
            // Q, E, O, P 중 하나라도 눌리면 즉시 실격 이벤트를 발생시킵니다.
            CheckFalseStartInput();

            // 카운트다운 중에는 다른 모든 움직임 로직을 스킵합니다.
            return;
        }
        CheckGrounded(); // 바닥 체크

        // 스페이스바 입력 처리
        if (Input.GetKey(KeyCode.Space) && isGrounded)
        {
            isCharging = true;
            chargeTime += Time.deltaTime;
            chargeTime = Mathf.Clamp(chargeTime, 0f, maxChargeTime); // 최대 충전 시간 제한
        }

        if (Input.GetKeyUp(KeyCode.Space) && isCharging && isGrounded)
        {
            Jump(); // 스페이스바를 뗄 때 점프
            isCharging = false;
            chargeTime = 0f; // 충전 시간 초기화
        }
        if (isGrounded == false)
        {
            chargeTime = 0f; // 공중에 있을 때 충전 시간 초기화
        }


        // 다리 조종
        // Q: 왼쪽 앞다리 (front_shin_L) 회전
        if (Input.GetKey(KeyCode.Q) && frontShinL != null)
        {
            frontShinL.Rotate(-rotationSpeed * Time.deltaTime, 0, 0, Space.Self);
            ApplyPushForce(frontShinL);
        }

        // E: 오른쪽 앞다리 (front_shin_R) 회전
        if (Input.GetKey(KeyCode.E) && frontShinR != null)
        {
            frontShinR.Rotate(-rotationSpeed * Time.deltaTime, 0, 0, Space.Self);
            ApplyPushForce(frontShinR);
        }

        // O: 왼쪽 뒷다리 (foot_L) 회전
        if (Input.GetKey(KeyCode.O) && footL != null)
        {
            footL.Rotate(-rotationSpeed * Time.deltaTime, 0, 0, Space.Self);
            ApplyPushForce(footL);
        }

        // P: 오른쪽 뒷다리 (foot_R) 회전
        if (Input.GetKey(KeyCode.P) && footR != null)
        {
            footR.Rotate(-rotationSpeed * Time.deltaTime, 0, 0, Space.Self);
            ApplyPushForce(footR);
        }
        // 머리 조종
        if (Input.GetKey(KeyCode.W) && head != null)
        {
            float rotationAmount = -headRotationSpeed * Time.deltaTime; // 위로 회전 (음수)
            float newRotation = currentRotationX + rotationAmount;

            // 회전 각도를 60도로 제한
            if (newRotation >= -60f)
            {
                head.Rotate(rotationAmount, 0, 0, Space.Self);
                currentRotationX = Mathf.Clamp(newRotation, -60f, 60f);
            }
        }

        // S: 머리를 아래로 회전
        if (Input.GetKey(KeyCode.S) && head != null)
        {
            float rotationAmount = headRotationSpeed * Time.deltaTime; // 아래로 회전 (양수)
            float newRotation = currentRotationX + rotationAmount;

            // 회전 각도를 60도로 제한
            if (newRotation <= 60f)
            {
                head.Rotate(rotationAmount, 0, 0, Space.Self);
                currentRotationX = Mathf.Clamp(newRotation, -60f, 60f);
            }
        }
        // A: 머리를 왼쪽으로 회전
        if (Input.GetKey(KeyCode.A) && neck != null)
        {
            float rotationAmount = -headRotationSpeed * Time.deltaTime;
            float newRotation = currentRotationZ + rotationAmount;

            // 회전 각도를 -60 ~ 60도로 제한
            if (newRotation >= -60f)
            {
                neck.Rotate(0, 0, rotationAmount, Space.Self);
                currentRotationZ = Mathf.Clamp(newRotation, -60f, 60f);
            }
        }
        // D: 머리를 오른쪽으로 회전
        if (Input.GetKey(KeyCode.D) && neck != null)
        {
            float rotationAmount = headRotationSpeed * Time.deltaTime;
            float newRotation = currentRotationZ + rotationAmount;

            // 회전 각도를 -60 ~ 60도로 제한
            if (newRotation <= 60f)
            {
                neck.Rotate(0, 0, rotationAmount, Space.Self);
                currentRotationZ = Mathf.Clamp(newRotation, -60f, 60f);
            }
        }
        // 머리 위치 조정
        if (Input.GetKey(KeyCode.UpArrow) && scull != null)
        {
            Vector3 currentPosition = scull.localPosition;
            float newYPosition = currentPosition.y + moveSpeed * Time.deltaTime;
            newYPosition = Mathf.Clamp(newYPosition, minY, maxY);
            scull.localPosition = new Vector3(currentPosition.x, newYPosition, currentPosition.z);
        }
        if (Input.GetKey(KeyCode.DownArrow) && scull != null)
        {
            Vector3 currentPosition = scull.localPosition;
            float newYPosition = currentPosition.y - moveSpeed * Time.deltaTime;
            newYPosition = Mathf.Clamp(newYPosition, minY, maxY);
            scull.localPosition = new Vector3(currentPosition.x, newYPosition, currentPosition.z);
        }
        ApplyTurnForce();
    }

    // 다리가 바닥을 밀 때 힘을 추가하는 함수
    private void ApplyPushForce(Transform leg)
    {
        Debug.DrawRay(leg.position, transform.forward * 2f, Color.blue, 1f);
        // 다리가 바닥에 닿아 있는지 확인
        RaycastHit hit;
        if (Physics.Raycast(leg.position, Vector3.down, out hit, 1f))
        {
            // 다리가 바닥을 밀면서 앞으로 힘 추가
            Vector3 forwardForce = -transform.forward * pushForce * Time.deltaTime;
            rb.AddForceAtPosition(forwardForce, leg.position, ForceMode.Force);
        }
    }
    // 🔔 카운트다운 중에 Q, E, O, P 입력을 감지하는 함수
    private void CheckFalseStartInput()
    {
        // GetKeyDown은 키를 누른 '순간'에만 true를 반환하여 중복 실격을 방지합니다.
        if (Input.GetKeyDown(KeyCode.Q) || Input.GetKeyDown(KeyCode.E) ||
            Input.GetKeyDown(KeyCode.O) || Input.GetKeyDown(KeyCode.P))
        {
            // 실격 이벤트를 호출합니다.
            OnFalseStart?.Invoke();
        }
    }
    // 방향 전환을 위한 회전을 추가하는 함수
    private void ApplyTurnForce()
    {
        bool wasTurning = isTurning; // 이전 프레임에서 회전 중이었는지 저장
        isTurning = false; // 현재 프레임에서 회전 여부를 초기화

        // 왼쪽 회전 조건
        if (Input.GetKey(KeyCode.E) && Input.GetKey(KeyCode.P) && !Input.GetKey(KeyCode.Q) && !Input.GetKey(KeyCode.O))
        {
            // Y축 회전 고정 해제
            rb.constraints = defaultConstraints & ~RigidbodyConstraints.FreezeRotationY;

            Debug.DrawRay(transform.position, transform.right * 2f, Color.green, 1f); // 왼쪽 회전 방향 디버깅
            rb.AddTorque(Vector3.up * turnTorque * Time.deltaTime, ForceMode.Force); // 왼쪽 회전
            isTurning = true;
        }
        // 오른쪽 회전 조건
        else if (Input.GetKey(KeyCode.Q) && Input.GetKey(KeyCode.O) && !Input.GetKey(KeyCode.E) && !Input.GetKey(KeyCode.P))
        {
            // Y축 회전 고정 해제
            rb.constraints = defaultConstraints & ~RigidbodyConstraints.FreezeRotationY;

            Debug.DrawRay(transform.position, -transform.right * 2f, Color.red, 1f); // 오른쪽 회전 방향 디버깅
            rb.AddTorque(Vector3.up * -turnTorque * Time.deltaTime, ForceMode.Force); // 오른쪽 회전
            isTurning = true;
        }

        // 회전이 끝났다면 Y축 회전 고정 복원
        if (wasTurning && !isTurning)
        {
            rb.constraints = defaultConstraints | RigidbodyConstraints.FreezeRotationY;
        }
    }
    // 바닥에 닿아 있는지 확인하는 함수 (다리별로 체크)
    private void CheckGrounded()
    {
        int groundedLegs = 0; // 바닥에 닿은 다리 개수
        float rayLength = 1f; // 각 다리에서 바닥까지의 레이 거리 (ApplyPushForce와 동일하게 설정)

        // 왼쪽 앞다리 체크
        if (frontShinL != null && Physics.Raycast(frontShinL.position, Vector3.down, rayLength, groundLayerMask))
        {
            groundedLegs++;
            Debug.DrawRay(frontShinL.position, Vector3.down * rayLength, Color.green, 0.1f);
        }

        // 오른쪽 앞다리 체크
        if (frontShinR != null && Physics.Raycast(frontShinR.position, Vector3.down, rayLength, groundLayerMask))
        {
            groundedLegs++;
            Debug.DrawRay(frontShinR.position, Vector3.down * rayLength, Color.green, 0.1f);
        }

        // 왼쪽 뒷다리 체크
        if (footL != null && Physics.Raycast(footL.position, Vector3.down, rayLength, groundLayerMask))
        {
            groundedLegs++;
            Debug.DrawRay(footL.position, Vector3.down * rayLength, Color.green, 0.1f);
        }

        // 오른쪽 뒷다리 체크
        if (footR != null && Physics.Raycast(footR.position, Vector3.down, rayLength, groundLayerMask))
        {
            groundedLegs++;
            Debug.DrawRay(footR.position, Vector3.down * rayLength, Color.green, 0.1f);
        }

        // 다리 2개 이상이 바닥에 닿았으면 grounded
        isGrounded = (groundedLegs >= 2);
    }

    // 점프 함수
    private void Jump()
    {
        float chargedJumpForce = jumpForce * (1f + (chargeTime / maxChargeTime) * (chargeMultiplier - 1f));
        rb.AddForce(Vector3.up * chargedJumpForce, ForceMode.Impulse);
    }
}