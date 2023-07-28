using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [Header("Music Tracks")]
    [Tooltip("This track is the intro portion of the menu loop.")]
    public AudioClip mainMenuIntro;
    [Tooltip("This track will loop during the main menu screen.")]
    public AudioClip mainMenuLoop;
    [Tooltip("This track will loop when either the controlls or credits screens are active within the main menu scene.")]
    public AudioClip optionsLoop;
    [Tooltip("This track will loop when in the chair select screen.")]
    public AudioClip selectLoop;
    [Tooltip("This track is the intro portion of the race loop.")]
    public AudioClip raceIntro;
    [Tooltip("This track will loop while in the race.")]
    public AudioClip raceLoop;
    [Tooltip("This track will play while the game is paused.")]
    public AudioClip pausedLoop;
    [Space(10)]
    [Header("SFX")]
    [Tooltip("SFX played when the player cycles between menu options.")]
    public AudioClip menuCycle;
    [Tooltip("SFX played when the player selects a menu option.")] 
    public AudioClip menuSelect;
    [Tooltip("SFX played whenever a player pauses the game.")] 
    public AudioClip pauseGame;
    [Tooltip("SFX played at the start of the race during the countdown.")] 
    public AudioClip countdown;
    [Tooltip("SFX played when a player completes a lap.")] 
    public AudioClip lapComplete;
    [Tooltip("SFX played when a position change occurs.")] 
    public AudioClip positionChange;
    [Tooltip("Loop played when a chair is moving slowly.")] 
    public AudioClip slowChairLoop;
    [Tooltip("Loop played when a chair is moving quickly.")] 
    public AudioClip fastChairLoop;
    [Tooltip("SFX played when a chair's instability rises too high.")] 
    public AudioClip fallSound;
    [Tooltip("List of SFX that play when a player kicks.")] 
    public List<AudioClip> wallKick = new List<AudioClip>();
    [Tooltip("SFX that plays when a player wall kicks.")] 
    public AudioClip wallKickBoost;
    [Tooltip("List of SFX that play when a player breaks a pot.")] 
    public List<AudioClip> potBreak = new List<AudioClip>();
    [Tooltip("List of SFX that play when a water cooler jug bounces off the ground.")] 
    public List<AudioClip> jugBounce = new List<AudioClip>();
    [Tooltip("Sound that plays when the player starts pressing the charge button.")]
    public AudioClip chargeStarted;
    [Tooltip("Sound that plays once the charge meter is full.")]
    public AudioClip chargeFull;
    [Tooltip("Sound that plays when someone tries to start the game when not everyone is ready.")]
    public AudioClip notReady;
    [Tooltip("SFX played when a can is fired.")]
    public AudioClip canLaunched;
    [Tooltip("List of SFX that play when the fire extinguisher hits another object")]
    public List<AudioClip> fireExtinguisherHit = new List<AudioClip>();



    static public AudioManager instance;

    public AudioSource musicSource;
    public AudioSource sfxSource;

    void Awake()
    {
        //  singleton
        //  if the instance is null
        if (!instance)
        {
            //  assign the instance to this object
            instance = this;
        }

        //  otherwise, it isn't null
        else
        {
            //  print error message to console
            Debug.LogWarning("ERROR: Second Audio Manager Found in Scene");
            //  destroy the gameObject this component is attached to
            Destroy(gameObject);
        }
    }

    void Update()
    {
        if (GameManager.instance.RaceOngoing)
        {
            bool isMoving = false;

            foreach (ChairController chair in GameManager.instance.players)
            {
                if (chair.isMoving)
                {
                    isMoving = true;
                    return;
                }
            }

            if (isMoving)
            {
                bool atLeastOneRacing = false;

                foreach (ChairController chair in GameManager.instance.players)
                {
                    if (!chair.finishedRacing)
                    {
                        atLeastOneRacing = true;
                        return;
                    }
                }

                if (atLeastOneRacing)
                {
                    sfxSource.loop = true;
                    sfxSource.clip = slowChairLoop;
                    sfxSource.Play();
                }
            }

            else
            {
                sfxSource.loop = false;
                sfxSource.Stop();
            }
        }
    }

    public IEnumerator PlayMusicIntro(AudioClip intro, AudioClip loop)
    {
        musicSource.Stop();
        //  assign the loop clip to the audio source
        musicSource.clip = loop;

        //  play the intro as a one shot
        musicSource.PlayOneShot(intro);

        //  wait for the one shot to be over:
        yield return new WaitForSecondsRealtime(intro.length);

        //  play the audio source (loop track)
        musicSource.Play();
    }

    /// <summary>
    /// Swaps the current track playing on the musicSource to the specified loop keeping the playback position on the track.
    /// </summary>
    /// <param name="newLoop"></param>
    public void SwapTrack(AudioClip newLoop)
    {
        //  cache the current playback position
        float trackPos = musicSource.time;
        //  assign the new loop to the audioSource
        musicSource.clip = newLoop;
        //  stop the current audioSource clip
        musicSource.Stop();
        //  play the new audioSource
        musicSource.Play();
        //  assign the playback position to the same position as before
        musicSource.time = trackPos;
    }

    public IEnumerator RaceStart()
    {
        musicSource.Stop();
        musicSource.PlayOneShot(countdown);

        yield return new WaitForSecondsRealtime(countdown.length);
        StartCoroutine(PlayMusicIntro(raceIntro, raceLoop));
    }

    public void PlayMenuCycling()
    {
        sfxSource.PlayOneShot(menuCycle);
    }

    public void PlayMenuSelect()
    {
        sfxSource.PlayOneShot(menuSelect);
    }

    public void PositionChange()
    {
        sfxSource.PlayOneShot(positionChange);
    }

    public void PlayLapSound()
    {
        sfxSource.PlayOneShot(lapComplete);
    }

    public void PlayPauseSound()
    {
        sfxSource.PlayOneShot(pauseGame);
    }

    public void PlayNotReadySound()
    {
        sfxSource.PlayOneShot(notReady);
    }

    public void PlayRandomKick()
    {
        sfxSource.PlayOneShot(wallKick[Random.Range(0, wallKick.Count)]);
    }

    public AudioClip PlayRandomJugBounce()
    {
        return jugBounce[Random.Range(0, jugBounce.Count)];
    }

    public AudioClip PlayRandomExtinguisherHit()
    {
        return fireExtinguisherHit[Random.Range(0, fireExtinguisherHit.Count)];
    }

    public AudioClip PlayChairFall()
    {
        return fallSound;
    }

    public void PlayWallKickBoost()
    {
        sfxSource.PlayOneShot(wallKickBoost);
    }
}
