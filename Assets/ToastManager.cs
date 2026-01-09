using UnityEngine;

public class ToastManager : MonoBehaviour
{
    public Transform toastContainer;
    public GameObject toastPrefab;

    public void ShowToast(string message, float duration = 2f)
    {
        GameObject toastObj = Instantiate(toastPrefab, toastContainer);
        ToastUI toastUI = toastObj.GetComponent<ToastUI>();

        if (toastUI != null)
            toastUI.SetText(message);

        Destroy(toastObj, duration);
    }

    void Start()
    {
        ShowToast("Hello Toast ✅", 2f);
    }
}
