using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Camera Settings")]
    public Transform player; // Referensi pemain
    public float smoothSpeed = 0.125f; // Kecepatan pergerakan kamera
    public Vector3 offset; // Offset untuk posisi kamera

    private void LateUpdate()
    {
        // Check if player is not null before trying to access its position
        if (player != null)
        {
            // Tentukan posisi target kamera dengan offset, hanya mengubah sumbu X
            Vector3 desiredPosition = new Vector3(player.position.x + offset.x, transform.position.y, transform.position.z);

            // Lakukan pergerakan halus menuju posisi yang diinginkan
            Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);

            // Atur posisi kamera
            transform.position = smoothedPosition;
        }
        else
        {
            // Optionally, log or handle the situation when the player is destroyed
            Debug.LogWarning("Player reference is missing or destroyed!");
        }
    }
}
