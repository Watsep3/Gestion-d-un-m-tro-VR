using UnityEngine;

public class PlayerCam : MonoBehaviour
{
    public float sensX = 100f;
    public float sensY = 100f;
    public Transform orientation;

    float xRotation;
    float yRotation;

    bool cursorUnlocked = false;

    void Start()
    {
        LockCursor();
    }

    void Update()
    {
        // Toggle avec Echap
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            cursorUnlocked = !cursorUnlocked;

            if (cursorUnlocked)
                UnlockCursor();
            else
                LockCursor();
        }

        // Si curseur libre → PAS de rotation caméra
        if (cursorUnlocked) return;

        float mouseX = Input.GetAxis("Mouse X") * sensX * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * sensY * Time.deltaTime;

        yRotation += mouseX;
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        transform.rotation = Quaternion.Euler(xRotation, yRotation, 0);
        orientation.rotation = Quaternion.Euler(0, yRotation, 0);
    }

    void LockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void UnlockCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}
