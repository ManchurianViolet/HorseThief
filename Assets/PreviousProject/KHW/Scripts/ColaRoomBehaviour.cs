using UnityEngine;

public class ColaRoomBehaviour : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("HorseHead"))
        {
            DreamManager.Instance.ColaEnding();
        }
    }
}
