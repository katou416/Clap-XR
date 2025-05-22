using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VRRigReferences : MonoBehaviour
{
    public static VRRigReferences Singleton;

    public Transform root;
    public Transform head;
    public Transform leftHand;
    public Transform rightHand;

    public Transform leftHandJoint0;
    public Transform rightHandJoint0;

    private void Awake()
    {
        Singleton = this;

    }

    private void Start()
    {
        StartCoroutine(InitializeJoints());
    }

    private IEnumerator InitializeJoints()
    {
        yield return null;
        // 添加安全检查
        if (leftHand.childCount > 0)
        {
            leftHandJoint0 = leftHand.GetChild(0);
        }

        else
        {
            Debug.LogWarning("左手没有找到关节点!");
        }

        if (rightHand.childCount > 0)
        {
            rightHandJoint0 = rightHand.GetChild(0);
        }

        else
        {
            Debug.LogWarning("右手没有找到关节点!");
        }
    }

}
