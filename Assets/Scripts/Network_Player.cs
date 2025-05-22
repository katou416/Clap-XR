using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class Network_Player : NetworkBehaviour
{
    public Transform root;
    public Transform head;
    public Transform leftHand;
    public Transform rightHand;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (IsOwner)
        {
            root.position = VRRigReferences.Singleton.root.position;
            root.rotation = VRRigReferences.Singleton.root.rotation;

            head.position = VRRigReferences.Singleton.head.position;
            head.rotation = VRRigReferences.Singleton.head.rotation;

            leftHand.position = VRRigReferences.Singleton.leftHandJoint0.position;
            leftHand.rotation = VRRigReferences.Singleton.leftHandJoint0.rotation;

            rightHand.position = VRRigReferences.Singleton.rightHandJoint0.position;
            rightHand.rotation = VRRigReferences.Singleton.rightHandJoint0.rotation;
        }
        
    }
}
