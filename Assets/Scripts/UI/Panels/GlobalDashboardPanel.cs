using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

/// <summary>
/// Dashboard permanent affichant les métriques globales du jeu
/// </summary>
public class GlobalDashboardPanel : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("Texte affichant le nombre total de passagers")]
    public TextMeshProUGUI passengersText;
    
    [Tooltip("Texte affichant le nombre de retards actifs")]
    public TextMeshProUGUI delaysText;
    
    [Tooltip("Texte affichant le temps de jeu écoulé")]
    public TextMeshProUGUI timeText;
    
    [Header("Colors")]
    public Color normalColor = Color.white;
    public Color warningColor = Color.yellow;
    public Color dangerColor = Color.red;
    
    void Update()
    {
        // Mise à jour automatique chaque frame si le jeu est en cours
        if (GameManager.Instance != null && GameManager.Instance.currentState == AppState.Running)
        {
            UpdateDisplay();
        }
    }
    
    /// <summary>
    /// Met à jour l'affichage avec les données actuelles
    /// </summary>
    public void UpdateDisplay()
    {
        if (GameManager.Instance == null)
        {
            Debug.LogWarning("GlobalDashboard: GameManager not found!");
            return;
        }
        
        // Récupérer les données à jour depuis GameManager
        int totalPassengers = GameManager.Instance.totalPassengers;
        int delayCount = GameManager.Instance.delayCount;
        float gameTime = GameManager.Instance.gameTime;
        
        // Mettre à jour le texte des passagers
        if (passengersText != null)
        {
            passengersText.text = string.Format("Passagers: {0}", totalPassengers);
            
            // Couleur selon le niveau
            if (totalPassengers > 1500)
                passengersText.color = dangerColor;
            else if (totalPassengers > 1000)
                passengersText.color = warningColor;
            else
                passengersText.color = normalColor;
        }
        
        // Mettre à jour le texte des retards
        if (delaysText != null)
        {
            delaysText.text = string.Format("Retards: {0}", delayCount);
            
            // Couleur selon le nombre de retards
            if (delayCount >= 4)
                delaysText.color = dangerColor;
            else if (delayCount >= 2)
                delaysText.color = warningColor;
            else
                delaysText.color = normalColor;
        }
        
        // Mettre à jour le temps
        if (timeText != null)
        {
            int minutes = Mathf.FloorToInt(gameTime / 60f);
            int seconds = Mathf.FloorToInt(gameTime % 60f);
            timeText.text = string.Format("Temps: {0}:{1:00}", minutes, seconds);
        }
    }
}