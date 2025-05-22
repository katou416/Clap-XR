using UnityEngine;
using Unity.Netcode;

public class TouchPointInteraction : NetworkBehaviour
{
    public GameObject TP;  // 引用动画控制器物体

    public int GoalID;
    private Animator animator;
    private SphereCollider sphereCollider;
    public NetworkVariable<int> collisionCount = new NetworkVariable<int>(0);

    // 网络同步的状态变量
    private NetworkVariable<bool> isScoring = new NetworkVariable<bool>(false);
    private NetworkVariable<bool> isTouching = new NetworkVariable<bool>(false);
    private NetworkVariable<bool> isMiss = new NetworkVariable<bool>(false);

    private void Start()
    {
        animator = TP.GetComponent<Animator>();
        sphereCollider = GetComponent<SphereCollider>();

        if (animator == null)
        {
            Debug.LogError("[TouchPointInteraction] 未找到Animator组件！");
        }
        if (sphereCollider == null)
        {
            Debug.LogError("[TouchPointInteraction] 未找到SphereCollider组件！");
        }
    }

    private void Update()
    {
        if (!IsHost) return;

        // 检查得分条件
        if (collisionCount.Value >= 2 && isScoring.Value)
        {
            Debug.Log("StartTouch!");
            StartTouch();
            isScoring.Value = false;
        }

        // 检查失误条件
        if (isTouching.Value && collisionCount.Value < 2)
        {
            PlayTPDisappearServerRpc();
            isMiss.Value = true;
        }
    }

    public void StartAppear()
    {
        if (!IsHost) return;
        PlayTPAppearClientRpc();
    }

    // 开始计分
    public void StartScore()
    {
        if (!IsHost) return;
        StartScoreServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    private void StartScoreServerRpc()
    {
        isScoring.Value = true;
        if (sphereCollider != null)
        {
            sphereCollider.isTrigger = true;
        }
    }

    // 开始触摸
    public void StartTouch()
    {
        if (!IsHost) return;
        PlayTPGoalServerRpc(GoalID);
        StartTouchServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    private void StartTouchServerRpc()
    {
        isTouching.Value = true;
    }

    // 结束触摸
    public void FinishTouch()
    {
        if (!IsHost) return;
        FinishTouchServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    private void FinishTouchServerRpc()
    {
        isTouching.Value = false;
        if (sphereCollider != null)
        {
            sphereCollider.isTrigger = false;
        }
        collisionCount.Value = 0;
    }

    // 检查结束
    public void CheckinEnd()
    {
        if (!IsHost) return;
        collisionCount.Value = 0;
        CheckinEndServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    private void CheckinEndServerRpc()
    {
        if (isMiss.Value)
        {
            isMiss.Value = false;
            return;
        }

        if (!isMiss.Value && isScoring.Value)
        {
            PlayTPDisappearServerRpc();
            isScoring.Value = false;
            return;
        }

        if (!isMiss.Value && !isScoring.Value)
        {
            return;
        }
    }

    // 触发器进入
    private void OnTriggerEnter(Collider other)
    {
        if (!IsHost) return;

        if (other.CompareTag("PlayerHand"))
        {
            collisionCount.Value = Mathf.Min(collisionCount.Value + 1, 3);
        }
    }

    // 触发器退出
    private void OnTriggerExit(Collider other)
    {
        if (!IsHost) return;

        if (other.CompareTag("PlayerHand"))
        {
            collisionCount.Value = Mathf.Max(collisionCount.Value - 1, 0);
        }
    }

    // 播放动画的RPC
    [ServerRpc(RequireOwnership = false)]
    private void PlayTPGoalServerRpc(int id)
    {
        PlayTPGoalClientRpc(id);
    }

    [ClientRpc]
    private void PlayTPGoalClientRpc(int id)
    {
        if (animator != null)
        {
            string stringid = id.ToString();
            animator.Play("TP_Goal" + stringid);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void PlayTPDisappearServerRpc()
    {
        PlayTPDisappearClientRpc();
    }

    [ClientRpc]
    private void PlayTPDisappearClientRpc()
    {
        if (animator != null)
        {
            animator.Play("TP_Disappear");
        }
    }

    [ClientRpc]
    private void PlayTPAppearClientRpc()
    {
        if (animator != null)
        {
            animator.Play("TP_Appear");
        }
    }

    
}