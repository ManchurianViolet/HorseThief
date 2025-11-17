using UnityEngine;

public class BedBehaviour : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("HorseHead"))
        {
            FadeSystem.Instance.FadeInAndOut();
        }
    }
}
