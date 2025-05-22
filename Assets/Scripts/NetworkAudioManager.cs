using UnityEngine;
using Unity.Netcode;

public class NetworkAudioManager : NetworkBehaviour
{
    private AudioSource audioSource;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            Debug.LogError("[NetworkAudioManager] 未找到AudioSource组件！");
            return;
        }
    }

    public void PlayAudio()
    {
        if (!IsHost) return;
        PlayAudioServerRpc();
    }

    public void StopAudio()
    {
        if (!IsHost) return;
        StopAudioServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    private void PlayAudioServerRpc()
    {
        PlayAudioClientRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    private void StopAudioServerRpc()
    {
        StopAudioClientRpc();
    }

    [ClientRpc]
    private void PlayAudioClientRpc()
    {
        if (audioSource != null)
        {
            audioSource.Play();
        }
    }

    [ClientRpc]
    private void StopAudioClientRpc()
    {
        if (audioSource != null)
        {
            audioSource.Stop();
        }
    }
} 