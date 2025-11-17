using UnityEngine;

public class FallPointBehaviour : MonoBehaviour
{
    public Transform targetTransform;
    public GameObject playerHorse;
    public float delay;

    private void Start()
    {
        playerHorse = GameObject.Find("Horse");
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.CompareTag("HorseHead"))
        {
            Invoke("Fade", delay - 0.5f);
            Invoke("ChangePos", delay);

        }
    }

    void ChangePos()
    {
        playerHorse.transform.position = targetTransform.position;
        playerHorse.transform.rotation = targetTransform.rotation;
        playerHorse.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
        playerHorse.GetComponent<Rigidbody>().linearVelocity = Vector3.zero;
    }

    void Fade()
    {
        FadeSystem.Instance.FadeInAndOut();
    }
}
