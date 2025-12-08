using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LobbyManager : NetworkBehaviour
{
    // نام Scene بازی شما
    private const string GameplaySceneName = "GameplayScene"; 

    [Header("UI References")]
    public Text playerCountText;
    public Button joinButton;

    // یک NetworkVariable برای همگام سازی تعداد پلیرها در شبکه
    private NetworkVariable<int> currentPlayers = new NetworkVariable<int>(0);

    private void Start()
    {
        // مطمئن شوید که دکمه Join فقط در Scene لابی و برای Host/Client دیده شود
        if (joinButton != null)
        {
            joinButton.onClick.AddListener(OnJoinGameClicked);
            joinButton.interactable = false; // تا زمان اتصال غیرفعال باشد
        }
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            // تنها سرور مقدار اولیه را تنظیم و Listener را برای تغییرات تعداد پلیرها اضافه می‌کند
            currentPlayers.Value = NetworkManager.Singleton.ConnectedClients.Count;
            NetworkManager.Singleton.OnClientConnectedCallback += HandleClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback += HandleClientDisconnected;
        }

        // تمام کلاینت‌ها (و سرور) باید به تغییرات گوش دهند
        currentPlayers.OnValueChanged += UpdateLobbyUI;
        
        // به‌روزرسانی اولیه UI
        UpdateLobbyUI(currentPlayers.Value, currentPlayers.Value);

        // دکمه Join را برای کلاینت‌های متصل فعال کن
        if (joinButton != null && NetworkManager.Singleton.IsClient)
        {
             joinButton.interactable = true;
        }
    }

    public override void OnNetworkDespawn()
    {
        if (IsServer)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= HandleClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback -= HandleClientDisconnected;
        }
        currentPlayers.OnValueChanged -= UpdateLobbyUI;
    }

    // --- توابع سرور (فقط روی سرور اختصاصی اجرا می شوند) ---
    private void HandleClientConnected(ulong clientId)
    {
        if (IsServer)
        {
            currentPlayers.Value = NetworkManager.Singleton.ConnectedClients.Count;
        }
    }

    private void HandleClientDisconnected(ulong clientId)
    {
        if (IsServer)
        {
            currentPlayers.Value = NetworkManager.Singleton.ConnectedClients.Count;
        }
    }
    
    // --- توابع UI کلاینت و سرور ---
    
    private void UpdateLobbyUI(int oldValue, int newValue)
    {
        if (playerCountText != null)
        {
            playerCountText.text = $"Players Online: {newValue}";
        }
    }

    // فقط کلاینت/Host می‌تواند درخواست ورود به بازی را بدهد
    private void OnJoinGameClicked()
    {
        // برای شروع بازی، کلاینت باید به سرور اعلام کند
        RequestStartGameServerRpc();
    }
    
    // ServerRpc: درخواست کلاینت برای سرور که Scene بازی را بارگذاری کند
    [ServerRpc(RequireOwnership = false)] // RequireOwnership = false برای اجازه دادن به هر کلاینتی برای فراخوانی
    private void RequestStartGameServerRpc()
    {
        if (!IsServer) return;

        // سرور، Scene بازی را برای همه کلاینت‌ها بارگذاری می‌کند
        // توجه: این فقط باید توسط Host اصلی (نه Dedicated Server) برای تغییر Scene فراخوانی شود،
        // اما در Dedicated Server شما باید اطمینان حاصل کنید که منطق Join درست عمل کند.
        // در اینجا ما فرض می‌کنیم که سرور اختصاصی ما منتظر فرمان Start/Join است.
        
        // اگر می‌خواهید سرور بلافاصله بعد از اتصال اولین پلیر یا رسیدن به یک تعداد مشخص شروع شود، 
        // می‌توانید این منطق را در HandleClientConnected قرار دهید.
        
        // در این مثال ساده، ما Scene را به صورت دستی بارگذاری می کنیم
        NetworkManager.Singleton.SceneManager.LoadScene(GameplaySceneName, LoadSceneMode.Single);
    }
}