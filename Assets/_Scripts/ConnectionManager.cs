using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using TMPro; // اگر از TextMeshPro برای UI استفاده می کنید

public class ConnectionManager : MonoBehaviour
{
    // متغیر عمومی برای اتصال Input Field در Inspector
    // باید در Inspector به فیلد ورودی UI وصل شود
    public TMP_InputField ipInputField; 

    // پورت پیش فرض NGO (مطابق با تنظیمات Unity Transport شما در تصویر image_61ffd7.png)
    private ushort port = 7777; 

    // این تابع به دکمه "Connect" متصل می شود
    public void ConnectClient()
    {
        if (ipInputField == null || string.IsNullOrEmpty(ipInputField.text))
        {
            Debug.LogError("IP Address is empty or Input Field is not assigned!");
            return;
        }

        string ipAddress = ipInputField.text;

        // دسترسی به کامپوننت Unity Transport
        var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();

        // تنظیم IP و Port سرور اختصاصی ترکیه
        transport.SetConnectionData(ipAddress, port);

        Debug.Log($"Attempting to connect to: {ipAddress}:{port}");

        // شروع اتصال به عنوان کلاینت
        NetworkManager.Singleton.StartClient();
    }
}