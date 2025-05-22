using UnityEngine;
using TMPro;
using System.Net;
using System.Linq;

public class IPDisplay : MonoBehaviour
{
    public TextMeshProUGUI ipText;  // IP显示文本组件

    void Start()
    {
        if (ipText != null)
        {
            string localIP = GetLocalIPAddress();
            ipText.text = $"YourIP: {localIP}";
        }
    }

    private string GetLocalIPAddress()
    {
        // 获取本机所有IP地址
        var host = Dns.GetHostEntry(Dns.GetHostName());
        
        // 查找IPv4地址
        var ip = host.AddressList
            .FirstOrDefault(ip => ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork);

        return ip?.ToString() ?? "未找到IP地址";
    }
} 