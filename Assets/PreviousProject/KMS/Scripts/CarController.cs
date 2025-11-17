using UnityEngine;
using System.Collections;

public class CarController : MonoBehaviour
{

    [SerializeField] private AudioClip explosionSound;
    [SerializeField] private AudioClip driveSound;

    private Rigidbody _car;
    private AudioSource explosionSourceSound;
    private AudioSource driveSourceSound;

    private bool isFadingOut = false;

    private float speed = 100.0f;
    private float fadeOutDuration = 1.0f;

    void Start()
    {
        _car = GetComponent<Rigidbody>();

        driveSourceSound = gameObject.AddComponent<AudioSource>();
        explosionSourceSound = gameObject.AddComponent<AudioSource>();

        driveSourceSound.clip = driveSound;
        driveSourceSound.loop = true;
        driveSourceSound.spatialBlend = 1.0f;
        driveSourceSound.Play();
    }

    void Update()
    {
            _car.AddForce(Vector3.forward * speed * Time.deltaTime, ForceMode.Impulse);

        if (_car.transform.position.z > 110.0f && !isFadingOut)
        {
            StartCoroutine(FadeOutAndDestroy());
        }
    }

    IEnumerator FadeOutAndDestroy()
    {
        isFadingOut = true;
        float startVolume = driveSourceSound.volume;
        float time = 0f;

        while (time < fadeOutDuration)
        {
            time += Time.deltaTime;
            driveSourceSound.volume = Mathf.Lerp(startVolume+1, 0f, time / fadeOutDuration);
            yield return null;
        }

        // 볼륨이 0이 된 후에 객체 제거
        Destroy(gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            Destroy(gameObject);
            //날라가는 모션 일단 삭제로 대체
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("HorseChest"))
        {
            explosionSourceSound.clip = explosionSound;
            explosionSourceSound.spatialBlend = 1.0f;
            explosionSourceSound.Play();
        }
    }
}
