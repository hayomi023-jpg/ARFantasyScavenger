using UnityEngine;

namespace ARFantasy.Core
{
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        [Header("Audio Sources")]
        [SerializeField] private AudioSource sfxSource;
        [SerializeField] private AudioSource musicSource;

        [Header("Sound Effects")]
        [SerializeField] private AudioClip collectSound;
        [SerializeField] private AudioClip spawnSound;
        [SerializeField] private AudioClip winSound;
        [SerializeField] private AudioClip uiClickSound;

        [Header("Settings")]
        [SerializeField] private bool soundEnabled = true;
        [SerializeField] private float sfxVolume = 1f;
        [SerializeField] private float musicVolume = 0.5f;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // Create audio sources if not assigned
            if (sfxSource == null)
            {
                sfxSource = gameObject.AddComponent<AudioSource>();
            }
            if (musicSource == null)
            {
                musicSource = gameObject.AddComponent<AudioSource>();
                musicSource.loop = true;
            }
        }

        public void PlayCollectSound()
        {
            PlaySFX(collectSound);
        }

        public void PlaySpawnSound()
        {
            PlaySFX(spawnSound);
        }

        public void PlayWinSound()
        {
            PlaySFX(winSound);
        }

        public void PlayUIClickSound()
        {
            PlaySFX(uiClickSound);
        }

        private void PlaySFX(AudioClip clip)
        {
            if (!soundEnabled || clip == null || sfxSource == null) return;
            sfxSource.PlayOneShot(clip, sfxVolume);
        }

        public void SetSoundEnabled(bool enabled)
        {
            soundEnabled = enabled;
            sfxSource.mute = !enabled;
            musicSource.mute = !enabled;
        }

        public void SetSFXVolume(float volume)
        {
            sfxVolume = Mathf.Clamp01(volume);
        }

        public void SetMusicVolume(float volume)
        {
            musicVolume = Mathf.Clamp01(volume);
            musicSource.volume = musicVolume;
        }
    }
}
