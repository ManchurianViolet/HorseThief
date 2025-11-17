using UnityEngine;
using UnityEngine.Events;

public class EndTrigger : MonoBehaviour
{
    public UnityEvent<Collider> onTriggerEnter; // CountdownManager�� ������ �̺�Ʈ

    private void OnTriggerEnter(Collider _other)
    {
        onTriggerEnter.Invoke(_other);
    }
}
