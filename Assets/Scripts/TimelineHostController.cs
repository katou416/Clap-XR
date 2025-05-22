using UnityEngine;
using Unity.Netcode;
using UnityEngine.Playables;

public class TimelineHostController : NetworkBehaviour
{
    public PlayableDirector director;

    public void PlayTimeline()
    {
        if (!IsHost) return;
        if (director != null)
        {
            director.Play();
        }
    }
} 