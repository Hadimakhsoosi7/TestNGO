using Unity.Netcode;
using UnityEngine;
using Terresquall; // برای دسترسی به کلاس VirtualJoystick
using static UnityEngine.InputSystem.InputSystem;

public class PlayerMovement : NetworkBehaviour
{
    // سرعت حرکت را کمی کاهش دادم
    [SerializeField] private float moveSpeed = 2f; 
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
            enabled = false; 
            return;
        }
    }

    // Update برای گرفتن ورودی‌های لحظه‌ای (مثل زدن کلید) و غیر فیزیکی
    void Update()
    {
        // --- گرفتن ورودی کیبورد و جوی استیک ---

        // ورودی کیبورد
        float keyboardH = Input.GetAxis("Horizontal"); // A, D
        float keyboardV = Input.GetAxis("Vertical");     // W, S
        
        // ورودی جوی استیک (ID پیش فرض 0 فرض شده است)
        float joystickH = VirtualJoystick.GetAxis("Horizontal", 0); 
        float joystickV = VirtualJoystick.GetAxis("Vertical", 0);
        
        // ادغام ورودی‌ها: بزرگترین مقدار (مطلق) از کیبورد یا جوی استیک استفاده می‌شود
        // این تضمین می‌کند که اگر کاربر یکی از آن‌ها را استفاده کند، حرکت اعمال شود.
        horizontalInput = Mathf.Abs(keyboardH) > Mathf.Abs(joystickH) ? keyboardH : joystickH;
        verticalInput = Mathf.Abs(keyboardV) > Mathf.Abs(joystickV) ? keyboardV : joystickV;
        
        // --- مدیریت RPC برای رنگ ---
        if (Input.GetKeyDown(KeyCode.E))
        {
            ChangeColorServerRpc();
        }
    }

    // FixedUpdate برای اجرای منطق فیزیکی و ارسال درخواست‌های حرکتی به سرور
    void FixedUpdate()
    {
        // در FixedUpdate، ورودی‌های ذخیره شده را برای حرکت استفاده می‌کنیم.
        Vector3 moveDirection = new Vector3(horizontalInput, 0f, verticalInput).normalized;
        
        if (moveDirection.magnitude > 0)
        {
            RequestMovementServerRpc(moveDirection);
        }
    }

    // ServerRpc برای انتقال ورودی کلاینت به سرور
    [ServerRpc]
    public void RequestMovementServerRpc(Vector3 moveDirection)
    {
        if (!IsServer) return; 

        if (characterController != null)
        {
            // از Time.fixedDeltaTime سرور استفاده می‌کنیم تا حرکت همگام باشد.
            characterController.Move(moveDirection * moveSpeed * Time.fixedDeltaTime);
        }
    }
    
    [ServerRpc]
    public void ChangeColorServerRpc()
    {
        if (!IsServer) return;

        Color newColor = new Color(
            Random.Range(0f, 1f),
            Random.Range(0f, 1f),
            Random.Range(0f, 1f)
        );

        ChangeColorClientRpc(newColor);
    }

    [ClientRpc]
    public void ChangeColorClientRpc(Color newColor)
    {
        GetComponent<MeshRenderer>().material.color = newColor;
    }
}