using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Message temporaire qui apparaît et disparaît
/// </summary>
public class ToastMessage : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI messageText;
    public Image background;
    
    [Header("Settings")]
    public float displayDuration = 3f;
    public float fadeDuration = 0.5f;
    
    private CanvasGroup canvasGroup;
    
    void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
    }
    
    /// <summary>
    /// Affiche le toast avec un message et une couleur
    /// </summary>
    public void Show(string message, Color color)
    {
        // Configurer le texte
        if (messageText != null)
        {
            messageText.text = message;
        }
        
        // Configurer la couleur du fond
        if (background != null)
        {
            background.color = color;
        }
        
        // Commencer l'animation
        StartCoroutine(ShowAndFade());
    }
    
    /// <summary>
    /// Animation d'apparition et disparition
    /// </summary>
    private IEnumerator ShowAndFade()
    {
        // Apparition instantanée
        canvasGroup.alpha = 1f;
        
        // Attendre
        yield return new WaitForSeconds(displayDuration);
        
        // Fade out
        float elapsed = 0f;
        
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            canvasGroup.alpha = 1f - (elapsed / fadeDuration);
            yield return null;
        }
        
        // Détruire l'objet
        Destroy(gameObject);
    }
}