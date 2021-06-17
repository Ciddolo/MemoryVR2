using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum SoundClip
{
    Background,
    SwitchTurn,
    BeepUp,
    BeepDown,
    Beep
}

public class SoundManager : MonoBehaviour
{
    public AudioSource PlayerAudioSource;

    public List<AudioClip> AudioClips;

    private bool increaseVolume;
    private readonly float volumeTimer = 10.0f;
    private readonly float maxVolume = 0.5f;
    private float currentVolume;

    private void Start()
    {
        PlayerAudioSource.loop = true;
        PlayerAudioSource.clip = AudioClips[(int)SoundClip.Background];
        PlayerAudioSource.volume = 0.0f;
        PlayerAudioSource.Play();
        increaseVolume = true;
    }

    private void Update()
    {
        StartBackgroundMusic();
    }

    private void StartBackgroundMusic()
    {
        if (!increaseVolume) return;

        currentVolume += Time.deltaTime * (1.0f / (volumeTimer * 2.0f));
        currentVolume = Mathf.Clamp(currentVolume, 0.0f, maxVolume);
        PlayerAudioSource.volume = currentVolume;
        increaseVolume = currentVolume < maxVolume;
    }

    public void PlaySound(SoundClip soundClip)
    {
        PlayerAudioSource.PlayOneShot(AudioClips[(int)soundClip]);
    }
}
