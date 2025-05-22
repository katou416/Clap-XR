using UnityEngine;
using Unity.Netcode;

public class ColorChangeByCollision : NetworkBehaviour
{
    private NetworkVariable<int> collisionCount = new NetworkVariable<int>(0);
    private MeshRenderer meshRenderer;

    private Color colorNoCollision = Color.white;
    private Color colorOneCollision = Color.yellow;
    private Color colorTwoCollision = Color.blue;
    private Color colorThreeCollision = Color.red;

    // 获取当前碰撞计数
    public int GetCollisionCount()
    {
        return collisionCount.Value;
    }

    // 重置碰撞计数
    public void ResetCollisionCount()
    {
        if (IsHost)
        {
            collisionCount.Value = 0;
        }
    }

    private void Start()
    {
        Debug.Log($"[ColorChangeByCollision] 脚本启动在物体: {gameObject.name}");
        
        meshRenderer = GetComponent<MeshRenderer>();
        if (meshRenderer == null)
        {
            Debug.LogError("[ColorChangeByCollision] 没有找到MeshRenderer组件！");
            return;
        }

        // 初始化颜色
        UpdateColor();

        // 当NetworkVariable值改变时更新颜色
        collisionCount.OnValueChanged += (int previousValue, int newValue) => 
        {
            Debug.Log($"[ColorChangeByCollision] 碰撞计数改变: {previousValue} -> {newValue}");
            UpdateColor();
        };
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"[ColorChangeByCollision] 触发器进入: {other.gameObject.name}, Tag: {other.tag}, IsHost: {IsHost}");
        
        // 只在主机上处理碰撞计数
        if (!IsHost) 
        {
            Debug.Log("[ColorChangeByCollision] 不是主机，忽略碰撞");
            return;
        }

        if (other.CompareTag("PlayerHand"))
        {
            collisionCount.Value = Mathf.Min(collisionCount.Value + 1, 3);
            Debug.Log($"[ColorChangeByCollision] 检测到PlayerHand标签，当前碰撞计数: {collisionCount.Value}");
        }
        else
        {
            Debug.Log($"[ColorChangeByCollision] 物体标签不是PlayerHand: {other.tag}");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        Debug.Log($"[ColorChangeByCollision] 触发器退出: {other.gameObject.name}, Tag: {other.tag}, IsHost: {IsHost}");
        
        // 只在主机上处理碰撞计数
        if (!IsHost)
        {
            Debug.Log("[ColorChangeByCollision] 不是主机，忽略碰撞");
            return;
        }

        if (other.CompareTag("PlayerHand"))
        {
            collisionCount.Value = Mathf.Max(collisionCount.Value - 1, 0);
            Debug.Log($"[ColorChangeByCollision] 检测到PlayerHand标签离开，当前碰撞计数: {collisionCount.Value}");
        }
    }

    private void UpdateColor()
    {
        Color newColor = colorNoCollision;
        
        switch (collisionCount.Value)
        {
            case 1:
                newColor = colorOneCollision;
                break;
            case 2:
                newColor = colorTwoCollision;
                break;
            case 3:
                newColor = colorThreeCollision;
                break;
        }

        meshRenderer.material.color = newColor;
        Debug.Log($"[ColorChangeByCollision] 更新颜色，当前碰撞计数: {collisionCount.Value}, 新颜色: {newColor}");
    }
}