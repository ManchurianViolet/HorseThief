using UnityEngine;

public class CarrotBehaviour : MonoBehaviour
{
    bool isEaten = false;
    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.CompareTag("HorseHead") ) //horse
        {
            if(!isEaten)
            {
                isEaten = true;
                Debug.Log("Carrot Obtained");
                DreamManager.Instance.UpdateCarrot(1);
                Destroy(gameObject);
            }

        }
    }
}
