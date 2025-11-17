using UnityEngine;

public class BlinkerUIManager : MonoBehaviour
{
    [SerializeField] private Canvas _blinkerUI;
    [SerializeField] private GameObject _player;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //_blinkerUI = GetComponent<Canvas>();
    }

    // Update is called once per frame
    void Update()
    {
       
    }
    private void OnTriggerEnter(Collider other)
    {
        if (_player.gameObject.CompareTag("HorseChest"))
        {
            _blinkerUI.gameObject.SetActive(true);
        }
    }
            private void OnTriggerExit(Collider other)
    {
        if (_player.gameObject.CompareTag("HorseChest"))
        {
            _blinkerUI.gameObject.SetActive(false);
        }
    }
}

