using UnityEngine;
using Unity.Netcode;

public class NetworkBGMPlayer : NetworkBehaviour
{
    private AudioSource audioSource;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            Debug.LogError("[NetworkBGMPlayer] 未找到AudioSource组件！");
        }
    }

    // 联机播放BGM
    public void PlayBGM()
    {
        if (!IsHost) return;
        PlayBGMServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    private void PlayBGMServerRpc()
    {
        PlayBGMClientRpc();
    }

    [ClientRpc]
    private void PlayBGMClientRpc()
    {
        if (audioSource != null && !audioSource.isPlaying)
        {
            audioSource.Play();
        }
    }
} 