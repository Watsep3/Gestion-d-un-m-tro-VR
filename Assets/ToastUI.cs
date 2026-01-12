using TMPro;
using UnityEngine;

public class ToastUI : MonoBehaviour
{
    public TextMeshProUGUI toastText;

    private void Awake()
    {
        if (toastText == null)
            toastText = GetComponentInChildren<TextMeshProUGUI>();
    }

    public void SetText(string message)
    {
        if (toastText != null)
            toastText.text = message;
        else
            Debug.LogError("toastText est NULL dans ToastUI !");
    }
}


