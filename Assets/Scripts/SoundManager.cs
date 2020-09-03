using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public AudioSource music;
    public AudioSource monsterSound;
    public AudioSource playerSound;
    public AudioSource loopedSound;
    public AudioSource lowVolumeSound;
    public static SoundManager instance = null;

    public AudioClip crashing;
    public AudioClip creak;

    public AudioClip nightMusic;
    public AudioClip dayMusic;
    private bool switchMusic = true;

    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }
        else if(instance != null)
        {
            Destroy(gameObject);
        }
        DontDestroyOnLoad(gameObject);
    }

    public void playPlayerSound(AudioClip sound)
    {
        playerSound.clip = sound;
        playerSound.Play();
    }
    
    public void playMonsterSound(AudioClip sound)
    {
        monsterSound.clip = sound;
        monsterSound.Play();
    }
    public void playLooped(AudioClip sound)
    {
        loopedSound.clip = sound;
        loopedSound.Play();
    }

    public void playLowSound(AudioClip sound)
    {
        lowVolumeSound.clip = sound;
        lowVolumeSound.Play();
    }

    public void playNightMusic()
    {
        if (switchMusic)
        {
            music.clip = nightMusic;
            switchMusic = false;
            music.Play();
        }

    }

    public void playDayMusic()
    {
        if (!switchMusic)
        {
            music.clip = dayMusic;
            switchMusic = true;
            music.Play();
        }

    }
}
