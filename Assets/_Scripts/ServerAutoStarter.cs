using Unity.Netcode;
using UnityEngine;
using Unity.Netcode.Transports.UTP; // اضافه کردن این خط

public class ServerAutoStarter : MonoBehaviour
{
    void Start()
    {
        // اگر بازی به صورت Headless اجرا شده باشد (شرط سرور اختصاصی)
        if (Application.isBatchMode)
        {
            var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();

            // **تنظیم آدرس برای گوش دادن به تمام IP های موجود (0.0.0.0)**
            // این باید به صورت تضمینی مشکل 127.0.0.1 را حل کند
            ushort port = 7777; 
            transport.SetConnectionData("0.0.0.0", port);
            
            Debug.Log($"Binding server to 0.0.0.0:{port}");

            // شروع سرور
            NetworkManager.Singleton.StartServer();
            Debug.Log("Dedicated Server Started Automatically.");
        }
    }
}