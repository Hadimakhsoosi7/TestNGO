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

        // چک کردن برای زدن کلید E
        if (Input.GetKeyDown(KeyCode.E))
        {
            ChangeColorServerRpc();
        }
    }

    [ServerRpc]
    public void ChangeColorServerRpc()
    {
        // رنگ تصادفی را فقط روی سرور تعیین می کنیم
        Color newColor = new Color(
            Random.Range(0f, 1f),
            Random.Range(0f, 1f),
            Random.Range(0f, 1f)
        );

        // سپس از یک ClientRpc برای ارسال این رنگ جدید به همه کلاینت ها استفاده می کنیم
        ChangeColorClientRpc(newColor);
    }

    [ClientRpc]
    public void ChangeColorClientRpc(Color newColor)
    {
        // تغییر رنگ مش (Mesh) پلیر
        // فرض می کنیم که MeshRenderer روی همان آبجکت است
        GetComponent<MeshRenderer>().material.color = newColor;
    }


}