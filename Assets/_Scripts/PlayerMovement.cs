using Unity.Netcode;
using UnityEngine;

public class PlayerMovement : NetworkBehaviour
{
    // سرعت حرکت
    [SerializeField] private float moveSpeed = 5f; 

    // متد Update فقط در صورتی که این پلیر متعلق به کاربر محلی باشد اجرا می شود
    public override void OnNetworkSpawn()
    {
        // اگر مالک این آبجکت نباشیم، متد Update معمولی را غیر فعال می کنیم.
        if (!IsOwner) 
        {
            // برای صرفه جویی در پردازش می توانید کامپوننت حرکت را غیر فعال کنید
            // در این مثال ساده فقط از if (!IsOwner) در Update استفاده می کنیم.
        }
    }

    void Update()
    {
        // فقط کلاینتی که مالک این پلیر است می تواند آن را حرکت دهد
        if (!IsOwner) return;

        // گرفتن ورودی ها
        float horizontalInput = Input.GetAxis("Horizontal"); // A, D
        float verticalInput = Input.GetAxis("Vertical");     // W, S

        // محاسبه و اعمال حرکت
        Vector3 movement = new Vector3(horizontalInput, 0f, verticalInput) * moveSpeed * Time.deltaTime;
        transform.position += movement;
    }
}