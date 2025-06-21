using UnityEngine;
using System.Collections.Generic;

namespace TinyWalnutGames.Tools
{
    public class PlaylistManager : MonoBehaviour
    {
        [Header("Playlist")]
        [Tooltip("List of music keys to play in order (must match keys in AudioManager.musicClips).")]
        public List<string> playlist = new();

        [Tooltip("Shuffle playlist on start.")]
        public bool shuffle = false;

        [Tooltip("Automatically start playing on Awake.")]
        public bool autoPlay = true;

        private int currentTrackIndex = 0;

        private void Awake()
        {
            if (shuffle)
                ShufflePlaylist();
        }

        private void Start()
        {
            if (autoPlay && playlist.Count > 0)
                PlayCurrentTrack();
        }

        private void Update()
        {
            // If music finished, play next track
            if (playlist.Count > 0 && !AudioManager.Instance.IsMusicPlaying())
            {
                PlayNext();
            }
        }

        public void PlayCurrentTrack()
        {
            if (playlist.Count == 0) return;
            string key = playlist[currentTrackIndex];
            AudioManager.Instance.PlayMusic(key, loop: false);
        }

        public void PlayNext()
        {
            if (playlist.Count == 0) return;
            currentTrackIndex = (currentTrackIndex + 1) % playlist.Count;
            PlayCurrentTrack();
        }

        public void PlayPrevious()
        {
            if (playlist.Count == 0) return;
            currentTrackIndex = (currentTrackIndex - 1 + playlist.Count) % playlist.Count;
            PlayCurrentTrack();
        }

        public void PlayTrackByIndex(int index)
        {
            if (playlist.Count == 0) return;
            currentTrackIndex = Mathf.Clamp(index, 0, playlist.Count - 1);
            PlayCurrentTrack();
        }

        public void PlayTrackByKey(string key)
        {
            int idx = playlist.IndexOf(key);
            if (idx >= 0)
            {
                currentTrackIndex = idx;
                PlayCurrentTrack();
            }
        }

        public string GetCurrentTrackKey()
        {
            if (playlist.Count == 0) return null;
            return playlist[currentTrackIndex];
        }

        private void ShufflePlaylist()
        {
            for (int i = 0; i < playlist.Count; i++)
            {
                int j = Random.Range(i, playlist.Count);
                (playlist[j], playlist[i]) = (playlist[i], playlist[j]);
            }
        }
    }
}
