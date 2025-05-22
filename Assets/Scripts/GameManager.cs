using UnityEngine;
using Unity.Netcode;
using TMPro;
using System.Collections;
using static UnityEngine.InputSystem.OnScreen.OnScreenStick;
using System.Drawing;
using System.Runtime.InteropServices;
using System;

public class GameManager : NetworkBehaviour
{
    public GameObject targetPrefab;        // 要生成的公共物体预制体
    public Transform wallTransform;        // 墙的Transform
    public TextMeshProUGUI scoreText;     // 分数文本
    
    private NetworkVariable<int> score = new NetworkVariable<int>(0);
    private GameObject currentTarget;
    private Bounds spawnBounds;
    private bool isPlaying = false;
    private bool isRepositioning = false;  // 防止重复转移位置
    private bool hasScored = false;       // 防止重复计分

    private void Start()
    {

        // 获取墙的边界
        if (wallTransform != null)
        {
            Collider wallCollider = wallTransform.GetComponent<Collider>();
            if (wallCollider != null)
            {
                spawnBounds = wallCollider.bounds;
            }
        }

        // 监听分数变化
        score.OnValueChanged += (int previousValue, int newValue) =>
        {
            UpdateScoreUI();
        };

        UpdateScoreUI();
    }

    // 更新UI显示
    private void UpdateScoreUI()
    {
        if (scoreText != null)
        {
            scoreText.text = $"Score: {score.Value}";
        }
    }

    // 开始游戏
    [ServerRpc(RequireOwnership = false)]
    public void StartPlayServerRpc()
    {
        if (!IsServer) return;
        
        isPlaying = true;
        score.Value = 0;
        hasScored = false;

        // 如果目标不存在，创建一个
        if (currentTarget == null)
        {
            CreateTarget();
        }
        
        // 开始定时转移位置
        StartCoroutine(PeriodicRepositioning());
    }

    // 创建目标物体
    private void CreateTarget()
    {
        if (!IsServer || !isPlaying) return;

        Vector3 randomPosition = GetRandomWallPosition();
        currentTarget = Instantiate(targetPrefab, randomPosition, Quaternion.identity);
        currentTarget.GetComponent<NetworkObject>().Spawn();

        // 添加碰撞检测监听
        ColorChangeByCollision colorChange = currentTarget.GetComponent<ColorChangeByCollision>();
        if (colorChange != null)
        {
            StartCoroutine(MonitorTargetCollisions(colorChange));
        }
    }

    // 获取随机墙面位置
    private Vector3 GetRandomWallPosition()
    {
        return new Vector3(
            UnityEngine.Random.Range(spawnBounds.min.x, spawnBounds.max.x),
            UnityEngine.Random.Range(spawnBounds.min.y, spawnBounds.max.y),
            wallTransform.position.z
        );
    }

    // 定时转移位置的协程
    private IEnumerator PeriodicRepositioning()
    {
        while (isPlaying)
        {
            yield return new WaitForSeconds(3f);
            if (!isRepositioning && isPlaying)
            {
                StartCoroutine(RepositionTarget());
            }
        }
    }

    // 转移目标位置
    private IEnumerator RepositionTarget()
    {
        if (isRepositioning || !isPlaying || currentTarget == null) yield break;
        
        isRepositioning = true;
        
        // 等待短暂时间
        yield return new WaitForSeconds(0.1f);
        
        if (currentTarget != null && isPlaying)
        {
            Vector3 newPosition = GetRandomWallPosition();
            currentTarget.transform.position = newPosition;
            
            // 重置碰撞计数和计分标志
            ColorChangeByCollision colorChange = currentTarget.GetComponent<ColorChangeByCollision>();
            if (colorChange != null)
            {
                colorChange.ResetCollisionCount();
            }
            hasScored = false;
        }
        
        isRepositioning = false;
    }

    // 监视目标物体的碰撞
    private IEnumerator MonitorTargetCollisions(ColorChangeByCollision colorChange)
    {
        while (currentTarget != null && isPlaying)
        {
            // 当碰撞计数为2时（变蓝色）且还未计分
            if (colorChange.GetCollisionCount() == 2 && !hasScored)
            {
                hasScored = true;
                score.Value++;
                if (!isRepositioning)
                {
                    StartCoroutine(RepositionTarget());
                }
            }
            yield return null;
        }
    }

    // 停止游戏
    [ServerRpc(RequireOwnership = false)]
    public void StopPlayServerRpc()
    {
        if (!IsServer) return;
        
        isPlaying = false;
        StopAllCoroutines();
        
        if (currentTarget != null)
        {
            currentTarget.GetComponent<NetworkObject>().Despawn();
            Destroy(currentTarget);
            currentTarget = null;
        }
    }
} 