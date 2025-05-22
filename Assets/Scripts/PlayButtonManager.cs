using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;
using System.Collections;

public class PlayButtonManager : NetworkBehaviour
{
    public GameObject playButtonManager;  // 带有NetworkAnimatorController的物体
    public GameObject playButton;         // 带有碰撞器的按钮
    public TimelineHostController timeline;  // Timeline控制器
    public Image progressLine;           // 进度条

    private Animator animator;
    private NetworkVariable<float> fillAmount = new NetworkVariable<float>(0f);
    private NetworkVariable<int> collisionCount = new NetworkVariable<int>(0);
    
    // 网络同步的状态变量
    public NetworkVariable<bool> isEntering = new NetworkVariable<bool>(false);
    public NetworkVariable<bool> isWaiting = new NetworkVariable<bool>(false);
    public NetworkVariable<bool> isAppearing = new NetworkVariable<bool>(false);

    private void Start()
    {
        animator = playButtonManager.GetComponent<Animator>();
        if (animator == null)
        {
            Debug.LogError("[PlayButtonManager] 未找到Animator组件！");
        }

        // 监听fillAmount变化
        fillAmount.OnValueChanged += (float previousValue, float newValue) =>
        {
            if (progressLine != null)
            {
                progressLine.fillAmount = newValue;
            }
        };
    }

    private void Update()
    {
        if (!IsHost) return;

        if (isAppearing.Value)
        {
            HandleAppearingState();
        }

        if (fillAmount.Value >= 1f && isAppearing.Value)
        {
            isAppearing.Value = false;
            PlayAnimationServerRpc("PlayButton_StartPlay");
            StartTimeLine();
        }
    }

    private void HandleAppearingState()
    {
        if (collisionCount.Value >= 2)
        {
            if (!isEntering.Value)
            {
                PlayAnimationServerRpc("PlayButton_Enter");
                isEntering.Value = true;
            }

            // 增加进度条
            float newFill = fillAmount.Value + (Time.deltaTime / 2f); // 2秒从0到1
            fillAmount.Value = Mathf.Clamp01(newFill);

        }
        else // collisionCount < 2
        {
            // 降低进度条
            float newFill = fillAmount.Value - (Time.deltaTime / 1f); // 1秒从1到0
            fillAmount.Value = Mathf.Clamp01(newFill);

            if (!isEntering.Value && !isWaiting.Value)
            {
                StartWait();
            }
            else if (!isEntering.Value && isWaiting.Value)
            {
                return;
            }
            else if (isEntering.Value)
            {
                isEntering.Value = false;
            }
        }
    }

    public void StartAppear()
    {
        if (!IsHost) return;
        StartAppearServerRpc();
    }

    public void StartEnter()
    {
        if (!IsHost) return;
        StartEnterServerRpc();
    }

    public void StartWait()
    {
        if (!IsHost) return;
        StartWaitServerRpc();
    }

    public void StartTimeLine()
    {
        if (!IsHost) return;
        if (timeline != null)
        {
            timeline.PlayTimeline();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void StartAppearServerRpc()
    {
        PlayAppearAnimationClientRpc();
    }

    [ClientRpc]
    private void PlayAppearAnimationClientRpc()
    {
        if (animator != null)
        {
            StartCoroutine(PlayAppearAnimation());
        }
    }

    private IEnumerator PlayAppearAnimation()
    {
        // 播放出现动画
        animator.Play("PlayButton_Appear");

        // 获取当前动画的信息
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        
        // 等待动画播放完成
        yield return new WaitForSeconds(stateInfo.length);

        // 如果是Host，则设置状态
        if (IsHost)
        {
            SetAppearingStateServerRpc();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetAppearingStateServerRpc()
    {
        isAppearing.Value = true;
    }

    [ServerRpc(RequireOwnership = false)]
    private void StartEnterServerRpc()
    {
        isWaiting.Value = false;
        isEntering.Value = true;
        PlayAnimationServerRpc("PlayButton_Entering");
    }

    [ServerRpc(RequireOwnership = false)]
    private void StartWaitServerRpc()
    {
        isWaiting.Value = true;
        PlayAnimationServerRpc("PlayButton_Waiting");
    }

    [ServerRpc(RequireOwnership = false)]
    private void PlayAnimationServerRpc(string animationName)
    {
        PlayAnimationClientRpc(animationName);
    }

    [ClientRpc]
    private void PlayAnimationClientRpc(string animationName)
    {
        if (animator != null)
        {
            animator.Play(animationName);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsHost) return;

        if (other.CompareTag("PlayerHand"))
        {
            collisionCount.Value = Mathf.Min(collisionCount.Value + 1, 3);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!IsHost) return;

        if (other.CompareTag("PlayerHand"))
        {
            collisionCount.Value = Mathf.Max(collisionCount.Value - 1, 0);
        }
    }
} 