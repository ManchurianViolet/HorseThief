using UnityEngine;

public class HorseControl : MonoBehaviour
{
    private Rigidbody rb;
    private Transform frontShinL, frontShinR, footL, footR;
    [SerializeField] private Transform head;
    [SerializeField] private Transform scull;
    [SerializeField] private Transform neck;

    [SerializeField] private float rotationSpeed = 90f;
    private float currentRotationZ = 0f;
    private float currentRotationX = 0f;
    private LayerMask groundLayerMask;

    [SerializeField] private float pushForce = 200f;
    [SerializeField] private float turnTorque = 50f;
    [SerializeField] private float headRotationSpeed = 60f;

    [SerializeField] private float jumpForce = 500f;
    [SerializeField] private float chargeMultiplier = 2f;
    [SerializeField] private float maxChargeTime = 1f;
    [SerializeField] private float chargeTime = 0f;
    [SerializeField] private bool isCharging = false;
    [SerializeField] private bool isGrounded = false;
    [SerializeField] private float moveSpeed = 1f;
    [SerializeField] private float minY = 0f;
    [SerializeField] private float maxY = 5f;

    private bool isTurning = false;
    private RigidbodyConstraints defaultConstraints;
    [Header("Ground Check")]
    [SerializeField] private float groundCheckDistance = 0.2f; // 감지 거리 (너무 길면 안 내려감, 너무 짧으면 뚫림)
    [SerializeField] private LayerMask obstacleLayer; // 감지할 대상 (책상, 바닥 등)
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

        if (frontShinL == null || frontShinR == null || footL == null || footR == null || head == null)
        {
            Debug.LogError("다리 오브젝트를 찾을 수 없습니다. 경로를 확인하세요.");
        }
        rb.mass = 100f;
        rb.linearDamping = 0.05f;
        rb.angularDamping = 0.15f;

        // [수정됨] 평상시에는 모든 회전(X, Y, Z)을 다 고정합니다.
        defaultConstraints = RigidbodyConstraints.FreezeRotation;
        rb.constraints = defaultConstraints;
    }

    void Update()
    {
        CheckGrounded();

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
        if (isGrounded == false)
        {
            chargeTime = 0f;
        }


        // 다리 조종
        if (Input.GetKey(KeyCode.Q) && frontShinL != null)
        {
            frontShinL.Rotate(-rotationSpeed * Time.deltaTime, 0, 0, Space.Self);
            ApplyPushForce(frontShinL);
        }

        if (Input.GetKey(KeyCode.E) && frontShinR != null)
        {
            frontShinR.Rotate(-rotationSpeed * Time.deltaTime, 0, 0, Space.Self);
            ApplyPushForce(frontShinR);
        }

        if (Input.GetKey(KeyCode.O) && footL != null)
        {
            footL.Rotate(-rotationSpeed * Time.deltaTime, 0, 0, Space.Self);
            ApplyPushForce(footL);
        }

        if (Input.GetKey(KeyCode.P) && footR != null)
        {
            footR.Rotate(-rotationSpeed * Time.deltaTime, 0, 0, Space.Self);
            ApplyPushForce(footR);
        }

        // 머리 조종
        if (Input.GetKey(KeyCode.W) && head != null)
        {
            float rotationAmount = -headRotationSpeed * Time.deltaTime;
            float newRotation = currentRotationX + rotationAmount;
            if (newRotation >= -60f)
            {
                head.Rotate(rotationAmount, 0, 0, Space.Self);
                currentRotationX = Mathf.Clamp(newRotation, -60f, 60f);
            }
        }

        if (Input.GetKey(KeyCode.S) && head != null)
        {
            float rotationAmount = headRotationSpeed * Time.deltaTime;
            float newRotation = currentRotationX + rotationAmount;
            if (newRotation <= 60f)
            {
                head.Rotate(rotationAmount, 0, 0, Space.Self);
                currentRotationX = Mathf.Clamp(newRotation, -60f, 60f);
            }
        }

        if (Input.GetKey(KeyCode.A) && neck != null)
        {
            float rotationAmount = -headRotationSpeed * Time.deltaTime;
            float newRotation = currentRotationZ + rotationAmount;
            if (newRotation >= -60f)
            {
                neck.Rotate(0, 0, rotationAmount, Space.Self);
                currentRotationZ = Mathf.Clamp(newRotation, -60f, 60f);
            }
        }

        if (Input.GetKey(KeyCode.D) && neck != null)
        {
            float rotationAmount = headRotationSpeed * Time.deltaTime;
            float newRotation = currentRotationZ + rotationAmount;
            if (newRotation <= 60f)
            {
                neck.Rotate(0, 0, rotationAmount, Space.Self);
                currentRotationZ = Mathf.Clamp(newRotation, -60f, 60f);
            }
        }

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

        // [중요] 회전 함수 호출
        ApplyTurnForce();
    }

    private void ApplyPushForce(Transform leg)
    {
        Debug.DrawRay(leg.position, transform.forward * 2f, Color.blue, 1f);
        RaycastHit hit;
        if (Physics.Raycast(leg.position, Vector3.down, out hit, 1f))
        {
            Vector3 forwardForce = -transform.forward * pushForce * Time.deltaTime;
            rb.AddForceAtPosition(forwardForce, leg.position, ForceMode.Force);
        }
    }

    // [수정됨] 방향 전환 함수
    // 방향 전환 함수
    private void ApplyTurnForce()
    {
        bool wasTurning = isTurning;
        isTurning = false;

        bool isTurningLeft = Input.GetKey(KeyCode.E) && Input.GetKey(KeyCode.P) && !Input.GetKey(KeyCode.Q) && !Input.GetKey(KeyCode.O);
        bool isTurningRight = Input.GetKey(KeyCode.Q) && Input.GetKey(KeyCode.O) && !Input.GetKey(KeyCode.E) && !Input.GetKey(KeyCode.P);

        if (isTurningLeft || isTurningRight)
        {
            isTurning = true;

            // 1. Y축 회전 고정 해제 (X, Z는 고정)
            rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;

            // 2. [⭐핵심 수정⭐] X, Z 축 각도를 0으로 강제 보정 (기울임 방지)
            // 현재 Rigidbody의 Y축 회전은 유지하되, X와 Z는 0도로 설정합니다.
            Quaternion currentRotation = rb.rotation;
            float currentYAngle = currentRotation.eulerAngles.y;
            Quaternion targetRotation = Quaternion.Euler(0f, currentYAngle, 0f);

            // MoveRotation을 사용하여 물리적으로 안정적으로 각도를 보정합니다.
            rb.MoveRotation(targetRotation);

            // 3. 토크 적용 (기존 로직 유지)
            if (isTurningLeft)
            {
                Debug.DrawRay(transform.position, transform.right * 2f, Color.green, 1f);
                rb.AddTorque(Vector3.up * turnTorque * Time.deltaTime, ForceMode.Force);
            }
            else if (isTurningRight)
            {
                Debug.DrawRay(transform.position, -transform.right * 2f, Color.red, 1f);
                rb.AddTorque(Vector3.up * -turnTorque * Time.deltaTime, ForceMode.Force);
            }
        }

        // 4. 회전 키를 떼면 다시 모든 축(X, Y, Z)을 고정하고 관성 제거
        if (wasTurning && !isTurning)
        {
            rb.constraints = defaultConstraints; // XYZ 고정 복귀
            rb.angularVelocity = Vector3.zero;
        }

        // 회전 중이 아닐 때 강제로 고정
        if (!isTurning)
        {
            rb.constraints = defaultConstraints;
        }
    }

    private void CheckGrounded()
    {
        int groundedLegs = 0;
        float rayLength = 1f;

        if (frontShinL != null && Physics.Raycast(frontShinL.position, Vector3.down, rayLength, groundLayerMask))
        {
            groundedLegs++;
            Debug.DrawRay(frontShinL.position, Vector3.down * rayLength, Color.green, 0.1f);
        }

        if (frontShinR != null && Physics.Raycast(frontShinR.position, Vector3.down, rayLength, groundLayerMask))
        {
            groundedLegs++;
            Debug.DrawRay(frontShinR.position, Vector3.down * rayLength, Color.green, 0.1f);
        }

        if (footL != null && Physics.Raycast(footL.position, Vector3.down, rayLength, groundLayerMask))
        {
            groundedLegs++;
            Debug.DrawRay(footL.position, Vector3.down * rayLength, Color.green, 0.1f);
        }

        if (footR != null && Physics.Raycast(footR.position, Vector3.down, rayLength, groundLayerMask))
        {
            groundedLegs++;
            Debug.DrawRay(footR.position, Vector3.down * rayLength, Color.green, 0.1f);
        }

        isGrounded = (groundedLegs >= 2);
    }

    private void Jump()
    {
        float chargedJumpForce = jumpForce * (1f + (chargeTime / maxChargeTime) * (chargeMultiplier - 1f));
        rb.AddForce(Vector3.up * chargedJumpForce, ForceMode.Impulse);
    }
}