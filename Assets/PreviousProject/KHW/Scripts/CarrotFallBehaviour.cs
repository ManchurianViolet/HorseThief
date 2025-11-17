using UnityEngine;

public class CarrotFallBehaviour : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("HorseHead"))
        {
            DreamManager.Instance.CarrotEnding();
        }
    }
}
