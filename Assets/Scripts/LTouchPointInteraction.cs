using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;
using System.Collections;

public class LTouchPointInteraction : NetworkBehaviour
{
    public GameObject LTP; // 父物体，挂载Animator
    public Image Line;     // 进度条
    public GameObject touchpoint; // 自身
    public float Goaltime = 2f;   // 得分动画总时长

    [HideInInspector]
    public bool GoalSuccess = false; // Goal成功标志

    private Animator animator;
    private SphereCollider sphereCollider;
    private NetworkVariable<int> collisionCount = new NetworkVariable<int>(0);
    private NetworkVariable<bool> isTouching = new NetworkVariable<bool>(false);

    private Coroutine goalCoroutine;
    private Coroutine missCoroutine;

    private void Start()
    {
        animator = LTP.GetComponent<Animator>();
        sphereCollider = touchpoint.GetComponent<SphereCollider>();
        if (animator == null)
            Debug.LogError("[LTouchPointInteraction] 未找到Animator组件！");
        if (sphereCollider == null)
            Debug.LogError("[LTouchPointInteraction] 未找到SphereCollider组件！");
    }

    private void Update()
    {
        if (!IsHost) return;

        if (collisionCount.Value >= 2 && !isTouching.Value)
        {
            isTouching.Value = true;
            GoalServerRpc();
        }
        else if (collisionCount.Value < 2 && isTouching.Value && !GoalSuccess)
        {
            if (sphereCollider != null)
                sphereCollider.isTrigger = false;
            MissServerRpc();
        }
    }

    // 播放Appear动画
    public void Appear()
    {
        if (!IsHost) return;
        AppearServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    private void AppearServerRpc()
    {
        AppearClientRpc();
    }

    [ClientRpc]
    private void AppearClientRpc()
    {
        if (animator != null)
            animator.Play("LTP_Appear");
    }

    // 开启碰撞
    public void Score()
    {
        if (!IsHost) return;
        ScoreServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    private void ScoreServerRpc()
    {
        if (sphereCollider != null)
            sphereCollider.isTrigger = true;
    }

    // Goal动画
    [ServerRpc(RequireOwnership = false)]
    private void GoalServerRpc()
    {
        GoalClientRpc();
    }

    [ClientRpc]
    private void GoalClientRpc()
    {
        if (goalCoroutine != null) StopCoroutine(goalCoroutine);
        goalCoroutine = StartCoroutine(GoalAnimation());
    }

    private IEnumerator GoalAnimation()
    {
        GoalSuccess = true; // 动画开始时设置为true
        if (animator != null)
        {
            // 设置LTP_GoalRemain动画播放速度
            float remainTime = Mathf.Max(Goaltime, 0.01f);
            animator.speed = animator.runtimeAnimatorController.animationClips != null ? 1f : 1f; // 默认速度
            // 获取LTP_GoalRemain动画长度
            AnimationClip remainClip = null;
            foreach (var clip in animator.runtimeAnimatorController.animationClips)
            {
                if (clip.name == "LTP_GoalRemain")
                {
                    remainClip = clip;
                    break;
                }
            }
            if (remainClip != null)
            {
                animator.speed = remainClip.length / remainTime;
            }
            animator.Play("LTP_GoalRemain");
            yield return new WaitForSeconds(remainTime);
            animator.speed = 1f; // 恢复默认速度
            animator.Play("LTP_GoalFinish");
            // 获取LTP_GoalFinish动画长度
            AnimationClip finishClip = null;
            foreach (var clip in animator.runtimeAnimatorController.animationClips)
            {
                if (clip.name == "LTP_GoalFinish")
                {
                    finishClip = clip;
                    break;
                }
            }
            float finishTime = finishClip != null ? finishClip.length : 0.5f;
            yield return new WaitForSeconds(finishTime);
        }
    }

    // Miss动画
    [ServerRpc(RequireOwnership = false)]
    private void MissServerRpc()
    {
        MissClientRpc();
    }

    [ClientRpc]
    private void MissClientRpc()
    {
        if (missCoroutine != null) StopCoroutine(missCoroutine);
        missCoroutine = StartCoroutine(MissAnimation());
    }

    private IEnumerator MissAnimation()
    {
        // 0.2秒缩放到0.2
        float t = 0f;
        Vector3 startScale = touchpoint.transform.localScale;
        Vector3 endScale = new Vector3(0.2f, 0.2f, startScale.z);
        while (t < 0.2f)
        {
            t += Time.deltaTime;
            touchpoint.transform.localScale = Vector3.Lerp(startScale, endScale, t / 0.2f);
            if (Line != null)
                Line.fillAmount = Mathf.Lerp(Line.fillAmount, 0, t / 0.2f);
            yield return null;
        }
        touchpoint.transform.localScale = endScale;
        if (Line != null)
            Line.fillAmount = 0f;
        // 播放消失动画
        if (animator != null)
            animator.Play("LTP_Disappear");
        isTouching.Value = false;
        collisionCount.Value = 0;
    }

    // CheckinEnd
    public void CheckinEnd()
    {
        if (!IsHost) return;
        GoalSuccess = false;
        collisionCount.Value = 0;
        CheckinEndServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    private void CheckinEndServerRpc()
    {
        if (isTouching.Value)
        {
            isTouching.Value = false;
            return;
        }
        else
        {
            MissServerRpc();
        }
    }

    // 碰撞检测
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