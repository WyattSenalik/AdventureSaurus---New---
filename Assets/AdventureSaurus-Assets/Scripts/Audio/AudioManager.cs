
using UnityEngine;
using System;
using UnityEngine.Audio;
using UnityEngine.SceneManagement; 
public class AudioManager : MonoBehaviour
{
    public Sound[] sounds;      // store all our sounds
    public Sound[] playlist;    // store all our music

    private int currentPlayingIndex = 999; // set high to signify no song playing
    public int LevelMusic;
    // a play music flag so we can stop playing music during cutscenes etc
    private bool shouldPlayMusic = false;

    public static AudioManager instance; // will hold a reference to the first AudioManager created

    private float mvol; // Global music volume
    private float evol; // Global effects volume

    private void Start()
    {
        //start the music
        CurrentSong();
        PlayMusic(LevelMusic);
    }
    

    private void Awake()
    {
        // get preferences
        mvol = PlayerPrefs.GetFloat("MusicVolume", 0.75f);
        evol = PlayerPrefs.GetFloat("EffectsVolume", 0.75f);

        createAudioSources(sounds, evol);     // create sources for effects
        createAudioSources(playlist, mvol);   // create sources for music
    }

    // create sources
    private void createAudioSources(Sound[] sounds, float volume)
    {
        foreach (Sound s in sounds)
        {   // loop through each music/effect
            s.source = gameObject.AddComponent<AudioSource>(); // create anew audio source(where the sound splays from in the world)
            s.source.clip = s.clip;     // the actual music/effect clip
            s.source.volume = s.volume * volume; // set volume based on parameter
            s.source.pitch = s.pitch;   // set the pitch
            s.source.loop = s.loop;     // should it loop
        }
    }

    public void PlaySound(string name)
    {
        // here we get the Sound from our array with the name passed in the methods parameters
        Sound s = Array.Find(sounds, sound => sound.name == name);
        if (s == null)
        {
            Debug.LogError("Unable to play sound " + name);
            return;
        }
        s.source.Play(); // play the sound
    }

    public void PlayMusic(int index)
    {
        if (shouldPlayMusic == false)
        {
            shouldPlayMusic = true;
            // pick a random song from our playlist
            currentPlayingIndex = index;
            playlist[currentPlayingIndex].source.volume = playlist[0].volume * mvol; // set the volume
            playlist[currentPlayingIndex].source.Play(); // play it
        }

    }

    // stop music
    public void StopMusic(int sindex)
    {
        if (shouldPlayMusic == true)
        {
            currentPlayingIndex = sindex;
            playlist[currentPlayingIndex].source.volume = playlist[0].volume * mvol; // set the volume
            playlist[currentPlayingIndex].source.Pause(); // play it
            shouldPlayMusic = false;
        }
    }

    //stop sound
    public void StopSound(string name)
    {
        // here we get the Sound from our array with the name passed in the methods parameters
        Sound s = Array.Find(sounds, sound => sound.name == name);
        if (s == null)
        {
            Debug.LogError("Unable to play sound " + name);
            return;
        }
        s.source.Pause(); // play the sound
    }

    void Update()
    {
        /*
        // if we are playing a track from the playlist && it has stopped playing
        if (currentPlayingIndex != 999 && !playlist[currentPlayingIndex].source.isPlaying)
        {
            currentPlayingIndex++; // set next index
            if (currentPlayingIndex >= playlist.Length)
            { //have we went too high
                currentPlayingIndex = 0; // reset list when max reached
            }
            playlist[currentPlayingIndex].source.Play(); // play that funky music
        }
        //checks scene for current level music
        CurrentSong();
        */
    }

    // get the song name
    public String getSongName()
    {
        return playlist[currentPlayingIndex].name;
    }

    // if the music volume change update all the audio sources
    public void musicVolumeChanged()
    {
        foreach (Sound m in playlist)
        {
            mvol = PlayerPrefs.GetFloat("MusicVolume", 0.75f);
            m.source.volume = playlist[0].volume * mvol;
        }
    }

    //if the effects volume changed update the audio sources
    public void effectVolumeChanged()
    {
        evol = PlayerPrefs.GetFloat("EffectsVolume", 0.75f);
        foreach (Sound s in sounds)
        {
            s.source.volume = s.volume * evol;
        }
        sounds[0].source.Play(); // play an effect so user can her effect volume
    }

    // this is kind of bad but i don't think I have enough time or know how to fix it right now
    //gets current song needed for the scene and pauses other music to play it
    public void CurrentSong()
    {
        Scene currentScene = SceneManager.GetActiveScene();
        string sceneName = currentScene.name;
            if (sceneName == "CampaignStart" && LevelMusic != 0)
            {
            StopMusic(LevelMusic);
            PlayMusic(0);
            LevelMusic = 0;
        }
            else if (sceneName == "InfiniteStart" && LevelMusic != 0)
            {
            StopMusic(LevelMusic);
            PlayMusic(0);
            LevelMusic = 0;
        }
            else if (sceneName == "GameOver"&&LevelMusic != 3)
            {
            StopMusic(LevelMusic);
            PlayMusic(3);
            LevelMusic = 3;
            }
            else if (sceneName == "WinScreen" && LevelMusic != 4)
            {
            StopMusic(LevelMusic);
            PlayMusic(4);
            LevelMusic = 4;
        }
            else if (sceneName == "KiranFight" && LevelMusic != 2)
            {
            StopMusic(LevelMusic);
            PlayMusic(2);
            LevelMusic = 2;
        }
            else if (sceneName == "ReloadScene" && LevelMusic != 0)
            {
                StopMusic(LevelMusic);
                PlayMusic(0);
                LevelMusic = 0;
            }
        else if (sceneName == "NextFloor" && _levelMusic != 0)
        {
            StopMusic(_levelMusic);
            PlayMusic(0);
            _levelMusic = 0;
        }
    }
}