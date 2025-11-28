using UnityEngine;

public class HorseControl : MonoBehaviour
{
    private Rigidbody rb;
    private Transform frontShinL, frontShinR, footL, footR;
    [SerializeField] private Transform head;
    [SerializeField] private Transform scull;
    [SerializeField] private Transform neck;

    [Header("Movement Settings (Base Values)")]
    [SerializeField] private float rotationSpeed = 90f;
    [SerializeField] private float pushForce = 200f;
    [SerializeField] private float turnTorque = 50f;
    [SerializeField] private float headRotationSpeed = 60f;
    [SerializeField] private float moveSpeed = 1f;

    [Header("Jump Settings")]
    [SerializeField] private float jumpForce = 500f;
    [SerializeField] private float chargeMultiplier = 2f;
    [SerializeField] private float maxChargeTime = 1f;

    // 내부 변수들
    private float chargeTime = 0f;
    private bool isCharging = false;
    private bool isGrounded = false;
    private float currentRotationZ = 0f;
    private float currentRotationX = 0f;
    private float minY = 0f;
    private float maxY = 5f;
    private bool isTurning = false;

    private LayerMask groundLayerMask;
    private RigidbodyConstraints defaultConstraints;

    [Header("Ground Check")]
    [SerializeField] private float groundCheckDistance = 0.2f;
    [SerializeField] private LayerMask obstacleLayer;

    // --- 초기 스탯 저장용 (업그레이드 계산을 위해) ---
    private float basePushForce;
    private float baseHeadRotSpeed;
    private float baseMoveSpeed;
    private float baseMaxChargeTime;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        groundLayerMask = LayerMask.GetMask("Ground");

        frontShinL = transform.Find("horse.001/Root/spine.005/spine.006/spine.007/front_shoulder.L/front_thigh.L/front_shin.L");
        frontShinR = transform.Find("horse.001/Root/spine.005/spine.006/spine.007/front_shoulder.R/front_thigh.R/front_shin.R");
        footL = transform.Find("horse.001/Root/spine.005/shoulder.L/thigh.L/shin.L/foot.L");
        footR = transform.Find("horse.001/Root/spine.005/shoulder.R/thigh.R/shin.R/foot.R");
        head = transform.Find("horse.001/Root/spine.005/spine.006/spine.007/spine.008");
        scull = transform.Find("horse.001/Root/spine.005/spine.006/spine.007/spine.008/spine.009/spine.010/scull");
        neck = transform.Find("horse.001/Root/spine.005/spine.006/spine.007/spine.008/spine.009");

        if (frontShinL == null || head == null) Debug.LogError("일부 관절을 찾을 수 없습니다!");

        rb.mass = 100f;
        rb.linearDamping = 0.05f;
        rb.angularDamping = 0.15f;
        defaultConstraints = RigidbodyConstraints.FreezeRotation;
        rb.constraints = defaultConstraints;

        // ★ [핵심] 초기 스탯 기억해두기
        basePushForce = pushForce;
        baseHeadRotSpeed = headRotationSpeed;
        baseMoveSpeed = moveSpeed;
        baseMaxChargeTime = maxChargeTime;

        // ★ [핵심] 업그레이드 적용 실행
        ApplyUpgrades();
    }

    // ====================================================
    // ★ 업그레이드 적용 함수
    // ====================================================
    public void ApplyUpgrades()
    {
        if (GameManager.Instance == null) return;

        int[] levels = GameManager.Instance.data.horseUpgradeLevels;

        // 1. 마력 (Lv당 20 증가)
        pushForce = basePushForce + (levels[0] * 20f);

        // 2. 목 회전 속도 (Lv당 5 증가)
        headRotationSpeed = baseHeadRotSpeed + (levels[1] * 5f);

        // 3. 목 길이 조절 속도 (Lv당 0.2 증가)
        moveSpeed = baseMoveSpeed + (levels[2] * 0.2f);

        // 4. 점프 충전 시간 (Lv당 0.05초 감소, 최소 0.1초)
        maxChargeTime = Mathf.Max(0.1f, baseMaxChargeTime - (levels[3] * 0.05f));

        Debug.Log($"[Horse Upgrade] 마력:{pushForce}, 목회전:{headRotationSpeed}, 목길이:{moveSpeed}, 점프충전:{maxChargeTime}");
    }

    void Update()
    {
        CheckGrounded();

        // ★ [수정] 바닥 뚫기 방지 로직 제거함 (그냥 원래대로 입력 처리만)
        HandleInput();
    }

    private void HandleInput()
    {
        // 점프
        if (Input.GetKey(KeyCode.Space) && isGrounded)
        {
            isCharging = true;
            chargeTime += Time.deltaTime;
            chargeTime = Mathf.Clamp(chargeTime, 0f, maxChargeTime);
        }
        if (Input.GetKeyUp(KeyCode.Space) && isCharging && isGrounded)
        {
            Jump();
            isCharging = false;
            chargeTime = 0f;
        }
        if (!isGrounded) chargeTime = 0f;

        // 다리 조종
        if (Input.GetKey(KeyCode.Q) && frontShinL != null) { frontShinL.Rotate(-rotationSpeed * Time.deltaTime, 0, 0, Space.Self); ApplyPushForce(frontShinL); }
        if (Input.GetKey(KeyCode.E) && frontShinR != null) { frontShinR.Rotate(-rotationSpeed * Time.deltaTime, 0, 0, Space.Self); ApplyPushForce(frontShinR); }
        if (Input.GetKey(KeyCode.O) && footL != null) { footL.Rotate(-rotationSpeed * Time.deltaTime, 0, 0, Space.Self); ApplyPushForce(footL); }
        if (Input.GetKey(KeyCode.P) && footR != null) { footR.Rotate(-rotationSpeed * Time.deltaTime, 0, 0, Space.Self); ApplyPushForce(footR); }

        // 머리 조종 (WASD)
        if (Input.GetKey(KeyCode.W) && head != null)
        {
            float rot = -headRotationSpeed * Time.deltaTime;
            if (currentRotationX + rot >= -60f) { head.Rotate(rot, 0, 0, Space.Self); currentRotationX += rot; }
        }
        if (Input.GetKey(KeyCode.S) && head != null)
        {
            float rot = headRotationSpeed * Time.deltaTime;
            if (currentRotationX + rot <= 60f) { head.Rotate(rot, 0, 0, Space.Self); currentRotationX += rot; }
        }
        if (Input.GetKey(KeyCode.A) && neck != null)
        {
            float rot = -headRotationSpeed * Time.deltaTime;
            if (currentRotationZ + rot >= -60f) { neck.Rotate(0, 0, rot, Space.Self); currentRotationZ += rot; }
        }
        if (Input.GetKey(KeyCode.D) && neck != null)
        {
            float rot = headRotationSpeed * Time.deltaTime;
            if (currentRotationZ + rot <= 60f) { neck.Rotate(0, 0, rot, Space.Self); currentRotationZ += rot; }
        }

        // 목 길이 (화살표)
        if (scull != null)
        {
            if (Input.GetKey(KeyCode.UpArrow))
            {
                float newY = scull.localPosition.y + moveSpeed * Time.deltaTime;
                scull.localPosition = new Vector3(scull.localPosition.x, Mathf.Clamp(newY, minY, maxY), scull.localPosition.z);
            }
            if (Input.GetKey(KeyCode.DownArrow))
            {
                float newY = scull.localPosition.y - moveSpeed * Time.deltaTime;
                scull.localPosition = new Vector3(scull.localPosition.x, Mathf.Clamp(newY, minY, maxY), scull.localPosition.z);
            }
        }

        ApplyTurnForce();
    }

    private void ApplyPushForce(Transform leg)
    {
        if (Physics.Raycast(leg.position, Vector3.down, 1f))
        {
            Vector3 forwardForce = -transform.forward * pushForce * Time.deltaTime;
            rb.AddForceAtPosition(forwardForce, leg.position, ForceMode.Force);
        }
    }

    private void ApplyTurnForce()
    {
        bool wasTurning = isTurning;
        isTurning = false;

        bool isTurningLeft = Input.GetKey(KeyCode.E) && Input.GetKey(KeyCode.P) && !Input.GetKey(KeyCode.Q) && !Input.GetKey(KeyCode.O);
        bool isTurningRight = Input.GetKey(KeyCode.Q) && Input.GetKey(KeyCode.O) && !Input.GetKey(KeyCode.E) && !Input.GetKey(KeyCode.P);

        if (isTurningLeft || isTurningRight)
        {
            isTurning = true;
            rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;

            Quaternion targetRotation = Quaternion.Euler(0f, rb.rotation.eulerAngles.y, 0f);
            rb.MoveRotation(targetRotation);

            if (isTurningLeft) rb.AddTorque(Vector3.up * turnTorque * Time.deltaTime, ForceMode.Force);
            else if (isTurningRight) rb.AddTorque(Vector3.up * -turnTorque * Time.deltaTime, ForceMode.Force);
        }

        if (wasTurning && !isTurning)
        {
            rb.constraints = defaultConstraints;
            rb.angularVelocity = Vector3.zero;
        }
        if (!isTurning) rb.constraints = defaultConstraints;
    }

    private void CheckGrounded()
    {
        int groundedLegs = 0;
        if (frontShinL != null && Physics.Raycast(frontShinL.position, Vector3.down, 1f, groundLayerMask)) groundedLegs++;
        if (frontShinR != null && Physics.Raycast(frontShinR.position, Vector3.down, 1f, groundLayerMask)) groundedLegs++;
        if (footL != null && Physics.Raycast(footL.position, Vector3.down, 1f, groundLayerMask)) groundedLegs++;
        if (footR != null && Physics.Raycast(footR.position, Vector3.down, 1f, groundLayerMask)) groundedLegs++;
        isGrounded = (groundedLegs >= 2);
    }

    private void Jump()
    {
        float chargedJumpForce = jumpForce * (1f + (chargeTime / maxChargeTime) * (chargeMultiplier - 1f));
        rb.AddForce(Vector3.up * chargedJumpForce, ForceMode.Impulse);
    }
}