using Unity.Netcode;
using UnityEngine;
using Random = UnityEngine.Random;

public class PlayerMovement : NetworkBehaviour
{
    [SerializeField] private float moveSpeed = 2f; // مقدار پیش فرض را کمی کاهش دادم
    private CharacterController characterController;

    // متغیرهای موقتی برای نگهداری ورودی در هر فریم
    private float horizontalInput;
    private float verticalInput;

    public override void OnNetworkSpawn()
    {
        characterController = GetComponent<CharacterController>();
        if (characterController == null)
        {
            Debug.LogError("CharacterController component not found on Player!");
        }

        // تنها کلاینت مالک (Owner) می‌تواند ورودی را پردازش کند.
        if (!IsOwner)
        {
            // اسکریپت برای غیر مالکان غیرفعال می شود.
            enabled = false; 
            return;
        }
    }

    // Update برای گرفتن ورودی‌های لحظه‌ای (مثل زدن کلید) و غیر فیزیکی
    void Update()
    {
        // --- مدیریت ورودی ---
        // ورودی‌ها را در اینجا می‌گیریم و ذخیره می‌کنیم
        horizontalInput = Input.GetAxis("Horizontal"); // A, D
        verticalInput = Input.GetAxis("Vertical");     // W, S

        // --- مدیریت RPC برای رنگ ---
        if (Input.GetKeyDown(KeyCode.E))
        {
            ChangeColorServerRpc();
        }
    }

    // FixedUpdate برای اجرای منطق فیزیکی و ارسال درخواست‌های حرکتی به سرور
    void FixedUpdate()
    {
        // در OnNetworkSpawn، اسکریپت برای غیر مالکان غیرفعال شده است، پس نیازی به چک کردن IsOwner نیست.

        Vector3 moveDirection = new Vector3(horizontalInput, 0f, verticalInput).normalized;
        
        if (moveDirection.magnitude > 0)
        {
            // درخواست حرکت فقط در FixedUpdate ارسال می شود
            RequestMovementServerRpc(moveDirection);
        }
        // اگر بخواهید گرانش یا اصطکاک را روی CharacterController اعمال کنید، می‌توانید آن را در اینجا مدیریت کنید.
    }

    // ServerRpc برای انتقال ورودی کلاینت به سرور
    // این ServerRpc در FixedUpdate فریم کلاینت فراخوانی می‌شود، اما در FixedUpdate سرور اجرا می‌شود.
    [ServerRpc]
    public void RequestMovementServerRpc(Vector3 moveDirection)
    {
        // این کد فقط روی سرور اختصاصی اجرا می شود
        if (!IsServer) return; 

        // حرکت را روی سرور اعمال می کنیم 
        if (characterController != null)
        {
            // از Time.fixedDeltaTime سرور استفاده می‌کنیم تا حرکت مستقل از فریم ریت کلاینت‌ها باشد
            characterController.Move(moveDirection * moveSpeed * Time.fixedDeltaTime);
        }
        
        // نکته: NetworkTransform (با Authority = Server) به طور خودکار این تغییر موقعیت را به همه کلاینت ها مخابره می کند.
    }
    
    // ServerRpc برای درخواست تغییر رنگ
    [ServerRpc]
    public void ChangeColorServerRpc()
    {
        if (!IsServer) return;

        Color newColor = new Color(
            Random.Range(0f, 1f),
            Random.Range(0f, 1f),
            Random.Range(0f, 1f)
        );

        // ClientRpc برای ارسال رنگ تولید شده به تمام کلاینت ها
        ChangeColorClientRpc(newColor);
    }

    // ClientRpc برای دریافت دستور تغییر رنگ از سرور
    [ClientRpc]
    public void ChangeColorClientRpc(Color newColor)
    {
        // این کد روی همه کلاینت ها اجرا می شود
        GetComponent<MeshRenderer>().material.color = newColor;
    }
}