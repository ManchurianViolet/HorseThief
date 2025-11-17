using UnityEngine;

public class VoidFallBehaviour : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("HorseHead"))
        {
            DreamManager.Instance.VoidEnding();
        }
    }
}
