using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransporterBehaviour : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("HorseHead"))
        {
            SceneManager.LoadScene("fff");
        }
    }
}
