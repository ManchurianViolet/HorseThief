using UnityEngine;
using UnityEngine.SceneManagement;

public class MainSoundManager : MonoBehaviour
{
    public static MainSoundManager Instance; // 싱글톤 패턴

    [Header("배경 오디오 소스")]
    public AudioSource sfxSource; // 효과음 출력용
    public AudioSource musicSource; // 배경음악 출력용

    [Header("효과음 사운드 클립")]
    public AudioClip walkSound; // 걷는 소리

    [Header("배경음악 사운드 클립")]
    public AudioClip titleMusic; // 타이틀 화면 음악
    public AudioClip gameplayMusic; // 게임 플레이 음악

    private void Awake()
    {
        // 싱글톤 설정
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // 씬 전환 시 파괴�지 않음
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnEnable()
    {
        // 씬 전환 이벤트 구독
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        // 이벤트 구독 해제 (메모리 누수 방지)
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Start()
    {
        // 첫 씬에서 타이틀 음악 재생
        PlayTitleMusic();
    }

    // 씬이 로드될 때 호출
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 씬 인덱스 기반으로 BGM 제어
        int sceneIndex = scene.buildIndex; // 현재 씬의 빌드 인덱스

        if (sceneIndex == 0 || sceneIndex == 1) // 씬 1, 2 (인덱스 0, 1)
        {
            PlayTitleMusic();
        }
        else if (sceneIndex >= 2 && sceneIndex <= 6) // 씬 3, 4, 5, 6 (인덱스 2, 3, 4, 5)
        {
            PlayGameplayMusic();
        }
        else if (sceneIndex == 7) // 씬 7 (인덱스 6)
        {
            StopGameplayMusic(2.0f); // 2초 동안 페이드 아웃
        }
    }

    // 공통 효과음 재생 함수
    private void PlaySFX(AudioClip clip)
    {
        if (sfxSource != null && clip != null)
        {
            sfxSource.PlayOneShot(clip);
        }
    }

    // 배경음악 재생 관련 함수
    public void PlayTitleMusic()
    {
        PlayMusic(titleMusic);
    }

    public void PlayGameplayMusic()
    {
        PlayMusic(gameplayMusic, 1.0f);
    }

    private void PlayMusic(AudioClip clip, float fadeDuration = 0f)
    {
        if (musicSource != null && clip != null)
        {
            if (musicSource.clip != clip || !musicSource.isPlaying)
            {
                StartCoroutine(ChangeMusicCoroutine(clip, fadeDuration));
            }
        }
    }

    private System.Collections.IEnumerator ChangeMusicCoroutine(AudioClip clip, float fadeDuration)
    {
        if (fadeDuration > 0 && musicSource.isPlaying)
        {
            yield return StartCoroutine(FadeOutCoroutine(fadeDuration));
        }

        musicSource.Stop();
        musicSource.clip = clip;
        musicSource.loop = true;
        musicSource.Play();

        if (fadeDuration > 0)
        {
            float targetVolume = musicSource.volume;
            musicSource.volume = 0;
            float time = 0;

            while (time < fadeDuration)
            {
                time += Time.deltaTime;
                musicSource.volume = Mathf.Lerp(0, targetVolume, time / fadeDuration);
                yield return null;
            }
        }
    }

    public void StopGameplayMusic(float fadeDuration = 0f)
    {
        if (musicSource != null)
        {
            if (fadeDuration > 0f)
            {
                StartCoroutine(FadeOutCoroutine(fadeDuration));
            }
            else
            {
                musicSource.Stop();
            }
        }
    }

    private System.Collections.IEnumerator FadeOutCoroutine(float duration)
    {
        float startVolume = musicSource.volume;
        float time = 0;

        while (time < duration)
        {
            time += Time.deltaTime;
            musicSource.volume = Mathf.Lerp(startVolume, 0, time / duration);
            yield return null;
        }

        musicSource.Stop();
        musicSource.volume = startVolume; // 다음 재생을 위해 볼륨 복원
    }

    public void PlayWalkSound()
    {
        PlaySFX(walkSound);
    }
}