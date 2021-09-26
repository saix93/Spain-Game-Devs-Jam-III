using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class SoundManager : MonoBehaviour
{
    [Header("Data")]
    public float FadeTime;
    public List<AudioClip> AmbientTrackList;

    private AudioSource ambientMusicSource;
    private AudioClip currentPlayingAmbientClip;

    private void Awake()
    {
        ambientMusicSource = GetComponent<AudioSource>();
    }

    public void PlayRandomAmbientTrack()
    {
        var tracks = AmbientTrackList.FindAll(t => t != currentPlayingAmbientClip);
        var track = tracks[Random.Range(0, tracks.Count)];

        StartCoroutine(PlayTrack(track));
    }
    private IEnumerator PlayTrack(AudioClip track)
    {
        currentPlayingAmbientClip = track;
        var origVolume = ambientMusicSource.volume;

        if (IsPlayingAmbientMusic())
        {
            do
            {
                ambientMusicSource.volume -= origVolume * Time.deltaTime / FadeTime;
                
                yield return null;
            } while (ambientMusicSource.volume > 0);
            ambientMusicSource.Stop();
        }

        ambientMusicSource.volume = 0;
        
        ambientMusicSource.PlayOneShot(track);
            
        do
        {
            ambientMusicSource.volume += origVolume * Time.deltaTime / FadeTime;
                
            yield return null;
        } while (ambientMusicSource.volume < origVolume);
    }
    public bool IsPlayingAmbientMusic()
    {
        return ambientMusicSource.isPlaying;
    }
}
