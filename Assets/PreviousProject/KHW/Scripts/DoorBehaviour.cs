using UnityEngine;

public class DoorBehaviour : MonoBehaviour
{
    private void Start()
    {
        DreamManager.Instance.OnObtainAllCarrotAction += DoorOpen;
        animator = GetComponent<Animator>();
    }

    Animator animator;

    void DoorOpen()
    {
        animator.SetTrigger("DoorOpen");
    }

    private void OnDestroy()
    {
        DreamManager.Instance.OnObtainAllCarrotAction -= DoorOpen;
    }
}
