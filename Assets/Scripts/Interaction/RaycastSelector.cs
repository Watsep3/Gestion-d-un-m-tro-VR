using UnityEngine;

/// <summary>
/// Détecte les objets sous la souris avec un raycast
/// et affiche une surbrillance visuelle
/// </summary>
public class RaycastSelector : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("Distance maximale du raycast")]
    public float maxRayDistance = 1000f;
    
    [Tooltip("Layer des objets sélectionnables (-1 = tous)")]
    public LayerMask selectableLayer = -1;
    
    [Header("Visual Feedback")]
    [Tooltip("Couleur de surbrillance")]
    public Color highlightColor = Color.yellow;
    
    [Tooltip("Intensité de l'émission (glow)")]
    [Range(0f, 2f)]
    public float emissionIntensity = 0.5f;
    
    [Header("Outline Settings")]
    [Tooltip("Utiliser un outline au lieu de changer la couleur")]
    public bool useOutline = false;
    
    [Tooltip("Épaisseur de l'outline")]
    public float outlineWidth = 0.1f;
    
    // Variables privées
    private Camera mainCamera;
    private GameObject currentHighlightedObject;
    private Color originalColor;
    private Renderer currentRenderer;
    private Material currentMaterial; // ⚠️ On garde une référence au material modifié
    
    void Start()
    {
        // Récupère la caméra principale
        mainCamera = Camera.main;
        
        if (mainCamera == null)
        {
            mainCamera = FindObjectOfType<Camera>();
        }
        
        if (mainCamera == null)
        {
            Debug.LogError("❌ RaycastSelector: Pas de caméra principale trouvée!");
        }
        else
        {
            Debug.Log($"✅ RaycastSelector: Caméra trouvée ({mainCamera.name})");
        }
    }
    
    /// <summary>
    /// Détecte l'objet sous la souris avec un raycast
    /// </summary>
    public GameObject GetObjectUnderMouse()
    {
        if (mainCamera == null)
        {
            Debug.LogError("❌ GetObjectUnderMouse: Caméra manquante!");
            return null;
        }
        
        // Créer un rayon depuis la caméra vers la position de la souris
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        
        // Lancer le raycast
        if (Physics.Raycast(ray, out hit, maxRayDistance, selectableLayer))
        {
            // On a touché quelque chose !
            return hit.collider.gameObject;
        }
        
        // Rien touché
        return null;
    }
    
    /// <summary>
    /// Affiche la surbrillance sur un objet
    /// </summary>
    public void ShowHighlight(GameObject target)
    {
        // Si on surbrillance déjà quelque chose, on le retire d'abord
        if (currentHighlightedObject != null)
        {
            HideHighlight();
        }
        
        if (target == null)
        {
            Debug.LogWarning("⚠️ ShowHighlight: target est null");
            return;
        }
        
        // ⚠️ Ne PAS appliquer de surbrillance sur les objets IInteractable
        // Ils gèrent leur propre couleur de sélection
        IInteractable interactable = target.GetComponent<IInteractable>();
        if (interactable != null)
        {
            Debug.Log($"⏭️ {target.name} est IInteractable, pas de surbrillance RaycastSelector");
            return;
        }
        
        // Récupérer le Renderer de l'objet
        Renderer renderer = target.GetComponent<Renderer>();
        
        if (renderer == null)
        {
            // Peut-être que le Renderer est sur un enfant
            renderer = target.GetComponentInChildren<Renderer>();
        }
        
        if (renderer != null)
        {
            currentRenderer = renderer;
            currentMaterial = renderer.material; // On obtient le material actuel
            
            // ✅ Sauvegarder la couleur originale du material
            originalColor = currentMaterial.color;
            
            if (useOutline)
            {
                // TODO: Implémenter un système d'outline
                // Pour l'instant, on utilise la méthode de couleur
                ApplyColorHighlight();
            }
            else
            {
                ApplyColorHighlight();
            }
            
            // Mémoriser l'objet surbrillancé
            currentHighlightedObject = target;
            
            Debug.Log($"✨ Surbrillance activée sur: {target.name} (couleur originale: {originalColor})");
        }
        else
        {
            Debug.LogWarning($"⚠️ Pas de Renderer trouvé sur {target.name}");
        }
    }
    
    /// <summary>
    /// Applique la surbrillance par changement de couleur
    /// </summary>
    private void ApplyColorHighlight()
    {
        if (currentMaterial == null)
        {
            Debug.LogWarning("⚠️ ApplyColorHighlight: currentMaterial est null!");
            return;
        }
        
        // ✅ Modifier directement le material existant (pas en créer un nouveau)
        currentMaterial.color = highlightColor;
        
        // Ajouter un effet d'émission (glow) si le shader le supporte
        if (currentMaterial.HasProperty("_EmissionColor"))
        {
            currentMaterial.EnableKeyword("_EMISSION");
            currentMaterial.SetColor("_EmissionColor", highlightColor * emissionIntensity);
        }
    }
    
    /// <summary>
    /// Retire la surbrillance
    /// </summary>
    public void HideHighlight()
    {
        if (currentHighlightedObject != null && currentRenderer != null && currentMaterial != null)
        {
            // ✅ Restaurer la couleur originale
            currentMaterial.color = originalColor;
            
            // Désactiver l'émission si elle était activée
            if (currentMaterial.HasProperty("_EmissionColor"))
            {
                currentMaterial.DisableKeyword("_EMISSION");
                currentMaterial.SetColor("_EmissionColor", Color.black);
            }
            
            Debug.Log($"✨ Surbrillance retirée de: {currentHighlightedObject.name} (couleur restaurée: {originalColor})");
            
            currentHighlightedObject = null;
            currentRenderer = null;
            currentMaterial = null;
        }
    }
    
    /// <summary>
    /// Vérifie si un objet est actuellement surbrillancé
    /// </summary>
    public bool IsHighlighted(GameObject obj)
    {
        return currentHighlightedObject == obj;
    }
    
    /// <summary>
    /// Retourne l'objet actuellement surbrillancé
    /// </summary>
    public GameObject GetHighlightedObject()
    {
        return currentHighlightedObject;
    }
    
    /// <summary>
    /// Change la couleur de surbrillance
    /// </summary>
    public void SetHighlightColor(Color newColor)
    {
        highlightColor = newColor;
        
        // Si un objet est déjà surbrillancé, réappliquer la nouvelle couleur
        if (currentHighlightedObject != null && currentMaterial != null)
        {
            ApplyColorHighlight();
        }
    }
    
    /// <summary>
    /// Dessine un rayon de debug dans la scène
    /// </summary>
    void OnDrawGizmos()
    {
        if (mainCamera != null && Application.isPlaying)
        {
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            Gizmos.color = Color.red;
            Gizmos.DrawRay(ray.origin, ray.direction * maxRayDistance);
        }
    }
}