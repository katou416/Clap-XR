using UnityEngine;
using Unity.Netcode;

public class TPMapsController : NetworkBehaviour
{
    public GameObject TPMap;
    public GameObject LTPMap;
    private Animator animator;
    private Animator Lanimator;

    private void Start()
    {
        animator = TPMap.GetComponent<Animator>();
        Lanimator = LTPMap.GetComponent<Animator>();
    }

    public void PlayTPMove()
    {
        if (!IsHost) return;
        PlayTPMoveServerRpc();
    }


    [ServerRpc(RequireOwnership = false)]
    private void PlayTPMoveServerRpc()
    {
        PlayTPMoveClientRpc();
    }

    [ClientRpc]
    private void PlayTPMoveClientRpc()
    {
        if (animator != null)
        {
            animator.Play("TP_Move");
            Lanimator.Play("LTP_Move");
        }
    }
}