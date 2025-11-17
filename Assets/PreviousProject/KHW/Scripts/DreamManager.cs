using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DreamManager : MonoBehaviour
{
    public static DreamManager Instance => _instance;
    static DreamManager _instance;

    private void Awake()
    {
        _instance = this;
    }
    public bool endingSeen;

    public int maxCarrot = 2;
    public int currentCarrot;

    public Action OnObtainAllCarrotAction;

    private void Start()
    {
        currentCarrot = 0;
        soundManager = GameObject.FindAnyObjectByType<SoundManager>();
        horse = GameObject.Find("Horse");
    }

    private void Update()
    {

        if (Input.GetKeyDown(KeyCode.Y))
        {
            SceneManager.LoadScene("Dreamy");
        }

    }

    public void UpdateCarrot(int updateValue)
    {
        currentCarrot += updateValue;

        if(currentCarrot == maxCarrot)
        {
            FadeSystem.Instance.FadeInAndOut();


            horse.transform.position = onDoorOpenTransform.position;
            horse.transform.rotation = onDoorOpenTransform.rotation;
            horse.GetComponent<Rigidbody>().linearVelocity = Vector3.zero;
            horse.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;

            OnObtainAllCarrotAction?.Invoke();
        }
    }

    public GameObject realCarrot;
    public GameObject bed;
    public GameObject environment;
    public GameObject originalTerrain;
    public GameObject superTerrain;
    public GameObject nextStageTrigger;

    public GameObject horse;
    public Transform onDoorOpenTransform;

    public GameObject normalEndingText;
    public GameObject normalEndingDescription;
    public GameObject colaEndingDescription;
    public GameObject voidEndingText;
    public GameObject voidEndingDescription;

    public GameObject Crown;

    public SoundManager soundManager;

    public void CarrotEnding()
    {

        //bed.SetActive(false);
        realCarrot.SetActive(true);
        //nextStageTrigger.SetActive(true);
        //superTerrain.SetActive(true);
        Invoke("NormalText", 5f);
        soundManager.PlayGlobalSFX("Flash", 0f);
        soundManager.SetBGM("Surburban Evening", true, 3f);
        Crown.SetActive(false);
        RenderSettings.skybox = (Material)Resources.Load("KHW/SkyBox/Blue Sky");
    }

    public void ColaEnding()
    {
        endingSeen = true;
        bed.SetActive(false);
        //nextStageTrigger.SetActive(true);
        //superTerrain.SetActive(true);
        Invoke("ColaText", 5f);
        soundManager.PlayGlobalSFX("Flash", 0f);
    }

    public void VoidEnding()    
    {
        endingSeen = true;
        bed.SetActive(false);
        environment.SetActive(false);
        //nextStageTrigger.SetActive(true);
        superTerrain.SetActive(true);
        Invoke("VoidText", 5f);
        soundManager.PlayGlobalSFX("Flash", 0f);
    }

    void NormalText()
    {
        normalEndingText.SetActive(true);
        normalEndingDescription.SetActive(true);
    }

    void ColaText()
    {
        normalEndingText.SetActive(true);
        colaEndingDescription.SetActive(true);
    }
        

    void VoidText()
    {
        voidEndingText.SetActive(true);
        voidEndingDescription.SetActive(true);
    }
}
