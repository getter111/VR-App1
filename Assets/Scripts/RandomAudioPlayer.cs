using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Plays a random audio clip from a predefined list when triggered.
/// Attach this script to a GameObject with an AudioSource component.
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class RandomAudioPlayer : MonoBehaviour
{
    [Header("Audio Clips")]
    [Tooltip("List of audio clips to choose from.")]
    public AudioClip[] audioClips;

    [Header("Playback Settings")]
    [Tooltip("Should the audio source play on awake?")]
    public bool playOnAwake = false;

    [Tooltip("Should the audio clips loop?")]
    public bool loop = false;

    [Tooltip("Minimum volume for playback (0.0 to 1.0).")]
    [Range(0f, 1f)]
    public float minVolume = 0.5f;

    [Tooltip("Maximum volume for playback (0.0 to 1.0).")]
    [Range(0f, 1f)]
    public float maxVolume = 1f;

    // Reference to the AudioSource component
    private AudioSource audioSource;

    private List<AudioClip> clipPool = new List<AudioClip>();
    
    void Awake()
    {
        // Get the AudioSource component attached to the same GameObject
        audioSource = GetComponent<AudioSource>();

        // Configure AudioSource settings
        audioSource.loop = loop;
        audioSource.playOnAwake = false; // We'll handle play on awake manually
        
        // Initialize the clip pool
        if (audioClips != null)
        {
            clipPool.AddRange(audioClips);
        }
    }

    void Start()
    {
        if (playOnAwake)
        {
            PlayRandomClip();
        }
    }

    /// <summary>
    /// Plays a random audio clip from the audioClips array.
    /// </summary>
    public void PlayRandomClip()
    {
        if (audioClips == null || audioClips.Length == 0)
        {
            Debug.LogWarning("RandomAudioPlayer: No audio clips assigned.");
            return;
        }
        
        if (clipPool.Count == 0)
        {
            // Refill the pool when all clips have been played
            if (audioClips != null && audioClips.Length > 0)
            {
                clipPool.AddRange(audioClips);
            }
            else
            {
                Debug.LogWarning("RandomAudioPlayer: No audio clips assigned.");
                return;
            }
        }

        int randomIndex = Random.Range(0, clipPool.Count);
        AudioClip selectedClip = clipPool[randomIndex];
        clipPool.RemoveAt(randomIndex);

        if (selectedClip == null)
        {
            Debug.LogWarning($"RandomAudioPlayer: Audio clip at index {randomIndex} is null.");
            return;
        }

        // Optionally set a random volume within the specified range
        float randomVolume = Random.Range(minVolume, maxVolume);
        audioSource.volume = randomVolume;

        // Assign the selected clip to the AudioSource and play it
        audioSource.clip = selectedClip;
        audioSource.Play();
    }

    /// <summary>
    /// Stops the currently playing audio clip.
    /// </summary>
    public void StopClip()
    {
        if (audioSource.isPlaying)
        {
            audioSource.Stop();
        }
    }
}
