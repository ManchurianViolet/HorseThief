using UnityEngine;

public class RivalHorseMovement : MonoBehaviour
{
    public bool isCountdownEnd = false; // ī��Ʈ �ٿ� ���� �� true

    private Rigidbody rivalHorseRb;
    private Transform frontShinL, frontShinR, footL, footR;
    private Vector3 forwardVector = new Vector3(-1, 0, 0);

    // ���� ������
    [SerializeField] private float mainPushForce = 30f;

    // �ٸ� ȸ�� �ӵ� (�ʴ� ȸ�� ����)
    [SerializeField] private float rotationSpeed = 500f;

    // �ٸ��� �ٴ��� �� �� �߰����� �� (����)
    [SerializeField] private float legPushForce = 8000f;


    private void Awake()
    {
        // rigidbody ����
        rivalHorseRb = GetComponent<Rigidbody>();

        // �ٸ� ������Ʈ ����
        frontShinL = transform.Find("horse.001/Root/spine.005/spine.006/spine.007/front_shoulder.L/front_thigh.L/front_shin.L");
        frontShinR = transform.Find("horse.001/Root/spine.005/spine.006/spine.007/front_shoulder.R/front_thigh.R/front_shin.R");
        footL = transform.Find("horse.001/Root/spine.005/shoulder.L/thigh.L/shin.L/foot.L");
        footR = transform.Find("horse.001/Root/spine.005/shoulder.R/thigh.R/shin.R/foot.R");

        // ������Ʈ�� ����� �����Ǿ����� Ȯ��
        if (frontShinL == null || frontShinR == null || footL == null || footR == null)
        {
            Debug.LogError("�ٸ� ������Ʈ�� ã�� �� �����ϴ�. ��θ� Ȯ���ϼ���.");
        }
    }


    private void Update()
    {
        // isCountdownEnd�� false�� �Ʒ� ���� ��� ����
        if (!isCountdownEnd) return;

        // �ٸ� ����
        // ���̹��� �ڵ����� ȸ��
        if (frontShinL != null)
        {
            frontShinL.Rotate(-rotationSpeed * Time.deltaTime, 0, 0, Space.Self);
            ApplyPushForce(frontShinL);
        }
        if (frontShinR != null)
        {
            frontShinR.Rotate(-rotationSpeed * Time.deltaTime, 0, 0, Space.Self);
            ApplyPushForce(frontShinR);
        }
        if (footL != null)
        {
            footL.Rotate(-rotationSpeed * Time.deltaTime, 0, 0, Space.Self);
            ApplyPushForce(footL);
        }
        if (footR != null)
        {
            footR.Rotate(-rotationSpeed * Time.deltaTime, 0, 0, Space.Self);
            ApplyPushForce(footR);
        }
    }

    private void FixedUpdate()
    {
        // ���̹� ������ �̵�
        rivalHorseRb.AddForce(forwardVector * mainPushForce, ForceMode.Impulse);
    }


    // �ٸ��� �ٴ��� �� �� ���� �߰��ϴ� �Լ�
    private void ApplyPushForce(Transform leg)
    {
        Debug.DrawRay(leg.position, transform.forward * 2f, Color.blue, 1f);
        // �ٸ��� �ٴڿ� ��� �ִ��� Ȯ��
        RaycastHit hit;
        if (Physics.Raycast(leg.position, Vector3.down, out hit, 1f))
        {
            // �ٸ��� �ٴ��� �и鼭 ������ �� �߰�
            Vector3 forwardForce = -transform.forward * legPushForce * Time.deltaTime;
            rivalHorseRb.AddForceAtPosition(forwardForce, leg.position, ForceMode.Force);
        }
    }

    

}
