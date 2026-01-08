using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IInteractable
{
    void OnSelected();
    void OnDeselected();
    void OnAction();
    string GetInteractionInfo();
}

public class Interactable : MonoBehaviour, IInteractable
{
    public string objectName;
    public string objectType; // "Station", "Train", "Line"
    
    public virtual void OnSelected() { }
    public virtual void OnDeselected() { }
    public virtual void OnAction() { }
    public virtual string GetInteractionInfo() { return objectName; }
}
