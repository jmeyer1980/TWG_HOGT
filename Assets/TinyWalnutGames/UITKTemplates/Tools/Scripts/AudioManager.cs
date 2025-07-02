using UnityEngine;
using System.Collections.Generic;

namespace TinyWalnutGames.UITKTemplates.Tools
{
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        [Header("Audio Sources")]
        [SerializeField] private AudioSource sfxSource;
        [SerializeField] private AudioSource musicSource;

        [Header("Looping SFX Source")]
        [SerializeField] private AudioSource loopingSFXSource;

        [Header("SFX Clips")]
        [Tooltip("Assign SFX clips with a string key for lookup.")]
        public List<SFXEntry> sfxClips = new();

        [Header("Looping SFX Key")]
        [Tooltip("Key for the looping drag SFX in sfxClips.")]
        public string dragLoopSFXKey = "fade";

        [Header("Music Clips")]
        [Tooltip("Assign music clips with a string key for lookup.")]
        public List<MusicEntry> musicClips = new();

        private Dictionary<string, AudioClip> sfxDict;
        private Dictionary<string, AudioClip> musicDict;

        [Header("Volume Settings")]
        [Range(0f, 1f)] public float sfxVolume = 0.5f;
        [Range(0f, 1f)] public float musicVolume = 0.3f;

        public bool SFXMuted { get; private set; }
        public bool MusicMuted { get; private set; }

        [System.Serializable]
        public class SFXEntry
        {
            public string key;
            public AudioClip clip;
        }

        [System.Serializable]
        public class MusicEntry
        {
            public string key;
            public AudioClip clip;
        }

        private void Awake()
        {
            // Deparent to ensure this is a root object
            transform.SetParent(null);

            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            sfxDict = new Dictionary<string, AudioClip>();
            foreach (var entry in sfxClips)
                if (!string.IsNullOrEmpty(entry.key) && entry.clip != null)
                    sfxDict[entry.key] = entry.clip;

            musicDict = new Dictionary<string, AudioClip>();
            foreach (var entry in musicClips)
                if (!string.IsNullOrEmpty(entry.key) && entry.clip != null)
                    musicDict[entry.key] = entry.clip;

            if (sfxSource == null)
            {
                sfxSource = gameObject.AddComponent<AudioSource>();
                sfxSource.playOnAwake = false;
            }
            if (musicSource == null)
            {
                musicSource = gameObject.AddComponent<AudioSource>();
                musicSource.loop = true;
                musicSource.playOnAwake = false;
            }
            if (loopingSFXSource == null)
            {
                loopingSFXSource = gameObject.AddComponent<AudioSource>();
                loopingSFXSource.playOnAwake = false;
                loopingSFXSource.loop = true;
            }

            sfxSource.volume = sfxVolume;
            musicSource.volume = musicVolume;

            // Set initial mute states
            sfxSource.mute = SFXMuted;
            musicSource.mute = MusicMuted;
        }

        // Play a sound effect by key
        public void PlaySFX(string key, float? volume = null)
        {
            if (SFXMuted) return;
            if (sfxDict.TryGetValue(key, out var clip) && clip != null)
            {
                sfxSource.PlayOneShot(clip, volume ?? sfxVolume);
            }
        }

        /// <summary>
        /// Play a looping SFX by key (e.g., for slider drag).
        /// If already playing the same clip, does nothing.
        /// </summary>
        public void PlayLoopingSFX(string key, float? volume = null)
        {
            if (SFXMuted) return;
            if (sfxDict.TryGetValue(key, out var clip) && clip != null)
            {
                if (loopingSFXSource.isPlaying && loopingSFXSource.clip == clip)
                    return;
                loopingSFXSource.clip = clip;
                loopingSFXSource.volume = volume ?? sfxVolume;
                loopingSFXSource.loop = true;
                loopingSFXSource.mute = SFXMuted;
                loopingSFXSource.Play();
            }
        }

        /// <summary>
        /// Stop the currently playing looping SFX.
        /// </summary>
        public void StopLoopingSFX()
        {
            if (loopingSFXSource.isPlaying)
                loopingSFXSource.Stop();
        }

        // Play a music track by key
        public void PlayMusic(string key, bool loop = true)
        {
            if (musicDict.TryGetValue(key, out var clip) && clip != null)
            {
                musicSource.clip = clip;
                musicSource.loop = loop;
                if (!MusicMuted)
                    musicSource.Play();
            }
        }

        // Stop music
        public void StopMusic()
        {
            musicSource.Stop();
        }

        // Set SFX volume
        public void SetSFXVolume(float volume)
        {
            sfxVolume = Mathf.Clamp01(volume);
            sfxSource.volume = sfxVolume;
        }

        // Set music volume
        public void SetMusicVolume(float volume)
        {
            musicVolume = Mathf.Clamp01(volume);
            musicSource.volume = musicVolume;
        }

        // Mute/unmute SFX
        public void SetSFXMute(bool mute)
        {
            SFXMuted = mute;
            if (sfxSource != null)
                sfxSource.mute = mute;
        }

        // Mute/unmute music
        public void SetMusicMute(bool mute)
        {
            MusicMuted = mute;
            if (musicSource != null)
                musicSource.mute = mute;
        }

        // Check music and sfx muted states individually
        // Returns true if muted, false if enabled
        public bool IsSFXMuted()
        {
            return SFXMuted;
        }
        public bool IsMusicMuted()
        {
            return MusicMuted;
        }

        // Check if a music track is playing
        public bool IsMusicPlaying()
        {
            return musicSource.isPlaying;
        }

        // Get current music key
        public string GetCurrentMusicKey()
        {
            foreach (var kvp in musicDict)
            {
                if (musicSource.clip == kvp.Value)
                    return kvp.Key;
            }
            return null;
        }
    }
}
