using UnityEngine;

public class BlinkerController : MonoBehaviour
{

    private enum TrafficLight
    {
        Red,           
        YellowToGreen, 
        Green,         
        YellowToRed    
    }
    // first state is redlight
    private TrafficLight firstState = TrafficLight.Red;

    [SerializeField] private GameObject Player;
    [SerializeField] private GameObject policeCarPrefab;
    [SerializeField] private ParticleSystem policeStart;

    [SerializeField] private Canvas Blinker;
    [SerializeField] private Canvas Gameover;

    [SerializeField] private AudioClip policeSound;
    [SerializeField] private AudioClip explosionSound;
    [SerializeField] private AudioClip greenSound;


    [SerializeField] private float redDuration = 10.0f;
    [SerializeField] private float yellowDuration = 3.0f;
    [SerializeField] private float greenDuration = 10.0f;

    private float timer = 0f;
    //green sound boolean
    private bool hasPlayedGreenSound = false;

    private AudioSource policeAudioSource;
    private AudioSource explosionAudioSource;
    private AudioSource greenAudioSource;

    private GameObject _redLight;
    private GameObject _yellowLight;
    private GameObject _greenLight;

    public bool redLight = true;
    public bool yellowLight = false;
    public bool greenLight = false;
    public bool inPlayer = false;
    public bool isSpawned = false;

    // Start is called before the first frame update
    void Start()
    {
        //Find light color Baby~
        _redLight = Blinker.transform.Find("Red Light").gameObject;
        _yellowLight = Blinker.transform.Find("Yellow Light").gameObject;
        _greenLight = Blinker.transform.Find("Green Light").gameObject;

        // first traffic blinker state
        firstState = TrafficLight.Red;
        redLight = true;
        yellowLight = false;
        greenLight = false;

        policeAudioSource = gameObject.AddComponent<AudioSource>();
        explosionAudioSource = gameObject.AddComponent<AudioSource>();
        greenAudioSource = gameObject.AddComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        BlinkerSystem();
        TrafficOffense();
        GreenSound();
    }
    private void OnTriggerEnter(Collider other)
    {
        if (Player.gameObject.CompareTag("HorseChest"))
        {
            inPlayer = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (Player.gameObject.CompareTag("HorseChest"))
        {
            inPlayer = false;
        }
    }

    /// <summary>
    /// greenlight sound system
    /// </summary>
    void GreenSound()
    {
        if (greenLight && inPlayer)
        {
            if (!hasPlayedGreenSound)
            {
                greenAudioSource.PlayOneShot(greenSound);
                hasPlayedGreenSound = true;
            }
        }
        else
            hasPlayedGreenSound = false;
    }

    /// <summary>
    /// traffic offense game over!!
    /// </summary>
    void TrafficOffense()
    {
        if (redLight && inPlayer && Input.anyKeyDown && !isSpawned)
        {
            Debug.Log("Game Over");
            int numCars = 6;
            float radius = 5.0f;

            for (int i = 0; i < numCars; i++)
            {
                float angleDeg = i * (360f / numCars);
                float angleRad = angleDeg * Mathf.Deg2Rad;
                Vector3 spawnPosition = Player.transform.position + new Vector3(Mathf.Cos(angleRad) * radius, 0, Mathf.Sin(angleRad) * radius);

                Quaternion spawnRotation = Quaternion.LookRotation(Player.transform.position - spawnPosition);

                Instantiate(policeCarPrefab, spawnPosition, spawnRotation);
                Gameover.gameObject.SetActive(true);
            }
            isSpawned = true;
            Instantiate(policeStart, Player.transform.position, Quaternion.identity);

            policeAudioSource.clip = policeSound;
            explosionAudioSource.clip = explosionSound;
            policeAudioSource.Play();
            explosionAudioSource.Play();

        }
    }

    /// <summary>
    /// Blinker system(Light Controller)
    /// </summary>
    void BlinkerSystem()
    {
        timer += Time.deltaTime;

        switch (firstState)
        {
            case TrafficLight.Red:
                redLight = true;
                yellowLight = false;
                greenLight = false;
                if (timer >= redDuration)
                {
                    firstState = TrafficLight.YellowToGreen;
                    timer = 0f;
                }
                break;

            case TrafficLight.YellowToGreen:
                redLight = false;
                yellowLight = true;
                greenLight = false;
                if (timer >= yellowDuration)
                {
                    firstState = TrafficLight.Green;
                    timer = 0f;
                }
                break;

            case TrafficLight.Green:
                redLight = false;
                yellowLight = false;
                greenLight = true;
                if (timer >= greenDuration)
                {
                    firstState = TrafficLight.YellowToRed;
                    timer = 0f;
                }
                break;

            case TrafficLight.YellowToRed:
                redLight = false;
                yellowLight = true;
                greenLight = false;
                if (timer >= yellowDuration)
                {
                    firstState = TrafficLight.Red;
                    timer = 0f;
                }
                break;
        }

        if (redLight)
        {
            _redLight.SetActive(true);
            _yellowLight.SetActive(false);
            _greenLight.SetActive(false);

        }
        else if (yellowLight)
        {
            _redLight.SetActive(true);
            _yellowLight.SetActive(true);
            _greenLight.SetActive(false);
        }
        else if (greenLight)
        {
            _redLight.SetActive(false);
            _yellowLight.SetActive(false);
            _greenLight.SetActive(true);
        }
    }
}
