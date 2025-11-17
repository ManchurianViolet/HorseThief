using UnityEngine;

public class SoundManager : MonoBehaviour
{

    private void Awake()
    {
        //_instance = this;
    }

    [SerializeField] string dreamBGMName;
    [SerializeField] string realityBGMName;
    AudioSource BgmAudioSource;
    AudioSource SfxAudioSource;
    AudioClip audioClip;

    private void Start()
    {
        BgmAudioSource = GetComponent<AudioSource>();
        SfxAudioSource = transform.Find("SFX").GetComponent<AudioSource>();
        SetBGM(dreamBGMName, true, 1f);
    }

    public void SetBGM(string BGMName, bool isPlay, float delay)
    {
        BgmAudioSource.Stop();
        audioClip = (AudioClip)Resources.Load("KHW/BGM/" + BGMName);
        if (audioClip == null) { Debug.LogError("There is no BGM of such name!"); }
        else
        {
            BgmAudioSource.resource = audioClip;

            if(isPlay)
            {
                BgmAudioSource.PlayDelayed(delay);
            }
            else
            {
                //Do nothing.
            }
        }
    }

    public void PlayGlobalSFX(string sfxName, float delay)
    {
        AudioClip sfxToPlay = (AudioClip)Resources.Load("KHW/SFX/" + sfxName);
        if (sfxToPlay == null) { Debug.LogError("There is no SFX of such name!"); }
        else
        {
            SfxAudioSource.PlayOneShot(sfxToPlay);
        }
    }



}
