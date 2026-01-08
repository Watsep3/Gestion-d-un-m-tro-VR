using UnityEngine;

/// <summary>
/// Interface pour tous les objets cliquables
/// Tout objet qui implémente cette interface peut être sélectionné
/// </summary>
public interface IInteractable
{
    /// <summary>
    /// Appelé quand l'objet est cliqué/sélectionné
    /// </summary>
    void OnSelected();
    
    /// <summary>
    /// Appelé quand l'objet est désélectionné
    /// </summary>
    void OnDeselected();
    
    /// <summary>
    /// Appelé pour une action spéciale (ex: touche E)
    /// </summary>
    void OnAction();
    
    /// <summary>
    /// Retourne une description de l'objet
    /// </summary>
    string GetInteractionInfo();
}

/// <summary>
/// Classe de base pour les objets interactables
/// Tu peux hériter de cette classe au lieu d'implémenter IInteractable directement
/// (optionnel, mais plus simple)
/// </summary>
public class Interactable : MonoBehaviour, IInteractable
{
    [Header("Info")]
    public string objectName = "Objet";
    public string objectType = "Inconnu";
    
    /// <summary>
    /// Appelé quand on clique sur l'objet
    /// </summary>
    public virtual void OnSelected()
    {
        Debug.Log($"{objectName} sélectionné");
    }
    
    /// <summary>
    /// Appelé quand on clique ailleurs
    /// </summary>
    public virtual void OnDeselected()
    {
        Debug.Log($"{objectName} désélectionné");
    }
    
    /// <summary>
    /// Action spéciale (touche E par exemple)
    /// </summary>
    public virtual void OnAction()
    {
        Debug.Log($"Action sur {objectName}");
    }
    
    /// <summary>
    /// Retourne les infos de l'objet
    /// </summary>
    public virtual string GetInteractionInfo()
    {
        return $"{objectName} ({objectType})";
    }
}