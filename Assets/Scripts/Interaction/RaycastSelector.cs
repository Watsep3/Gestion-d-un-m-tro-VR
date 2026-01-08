using UnityEngine;

/// <summary>
/// Détecte les objets sous la souris avec un raycast
/// et affiche une surbrillance visuelle
/// </summary>
public class RaycastSelector : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("Distance maximale du raycast")]
    public float maxRayDistance = 100f;
    
    [Tooltip("Layer des objets sélectionnables")]
    public LayerMask selectableLayer;
    
    [Header("Visual Feedback")]
    [Tooltip("Material de surbrillance (jaune/cyan)")]
    public Material highlightMaterial;
    
    [Tooltip("Couleur de surbrillance")]
    public Color highlightColor = Color.cyan;
    
    // Variables privées
    private Camera mainCamera;
    private GameObject currentHighlightedObject;
    private Material originalMaterial;
    
    void Start()
    {
        // Récupère la caméra principale
        mainCamera = Camera.main;
        
        if (mainCamera == null)
        {
            Debug.LogError("RaycastSelector: Pas de caméra principale trouvée!");
        }
    }
    
    /// <summary>
    /// Détecte l'objet sous la souris avec un raycast
    /// </summary>
    public GameObject GetObjectUnderMouse()
    {
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
        
        if (target == null) return;
        
        // Récupérer le Renderer de l'objet
        Renderer renderer = target.GetComponent<Renderer>();
        
        if (renderer == null)
        {
            // Peut-être que le Renderer est sur un enfant
            renderer = target.GetComponentInChildren<Renderer>();
        }
        
        if (renderer != null)
        {
            // Sauvegarder le material original
            originalMaterial = renderer.material;
            
            // Créer un nouveau material avec la couleur de surbrillance
            Material highlightMat = new Material(renderer.material);
            highlightMat.color = highlightColor;
            
            // Option: Ajouter un effet d'émission (glow)
            highlightMat.EnableKeyword("_EMISSION");
            highlightMat.SetColor("_EmissionColor", highlightColor * 0.5f);
            
            // Appliquer le material
            renderer.material = highlightMat;
            
            // Mémoriser l'objet surbrillancé
            currentHighlightedObject = target;
            
            Debug.Log($"Surbrillance activée sur: {target.name}");
        }
    }
    
    /// <summary>
    /// Retire la surbrillance
    /// </summary>
    public void HideHighlight()
    {
        if (currentHighlightedObject != null)
        {
            Renderer renderer = currentHighlightedObject.GetComponent<Renderer>();
            
            if (renderer == null)
            {
                renderer = currentHighlightedObject.GetComponentInChildren<Renderer>();
            }
            
            if (renderer != null && originalMaterial != null)
            {
                // Remettre le material original
                renderer.material = originalMaterial;
            }
            
            Debug.Log($"Surbrillance retirée de: {currentHighlightedObject.name}");
            
            currentHighlightedObject = null;
            originalMaterial = null;
        }
    }
}