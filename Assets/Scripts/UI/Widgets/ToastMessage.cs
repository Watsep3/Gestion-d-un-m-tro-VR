using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class ToastMessage : MonoBehaviour
{
    public TextMeshProUGUI messageText;
    public Image background;
    public float displayDuration = 2f;
    
    public void Show(string message, Color color) 
    {
        messageText.text = message;
        background.color = color;
        StartCoroutine(FadeOut());
    }
    
    private IEnumerator FadeOut() 
    {
        yield return new WaitForSeconds(displayDuration);
        Destroy(gameObject);
    }
}