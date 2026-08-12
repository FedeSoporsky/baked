using System;
using System.Collections.Generic;
using UnityEngine;
using static Assets.Scripts.Enums;

[CreateAssetMenu(fileName = "SO_GameResources")]
public class SO_GameResources : ScriptableObject
{
    [Header("Music Clips")]
    [Space(8)]
    [SerializeField]
    internal AudioClip corridorSong;

    [SerializeField]
    internal AudioClip clubSong;

    [SerializeField]
    internal AudioClip houseSong;

    [SerializeField]
    internal AudioClip gameOverSong;

    [SerializeField]
    internal AudioClip hiddenEndingSong;


    [Header("SFX Clips")]
    [Space(8)]
    [SerializeField]
    private List<CounterAudioEntry> entries = new List<CounterAudioEntry>();

    private Dictionary<Counter, CounterAudioEntry> lookup;

    private void OnEnable()
    {
        lookup = new Dictionary<Counter, CounterAudioEntry>();
        foreach (var entry in entries)
        {
            if (!lookup.ContainsKey(entry.counterType))
                lookup[entry.counterType] = entry;
        }
    }

    public AudioClip[] GetClickClips(Counter counterType)
    {
        if (lookup != null && lookup.TryGetValue(counterType, out var entry))
            return entry.clickClips;

        throw new Exception($"Missing audio entry for {counterType}");
    }

    public AudioClip GetTaskCompletedClip(Counter counterType)
    {
        if (lookup != null && lookup.TryGetValue(counterType, out var entry))
            return entry.taskCompletedClip;

        Debug.LogWarning($"Missing completed clip for {counterType}");
        return null;
    }

    [Serializable]
    public class CounterAudioEntry
    {
        public Counter counterType;
        public AudioClip[] clickClips;
        public AudioClip taskCompletedClip;
    }
}
