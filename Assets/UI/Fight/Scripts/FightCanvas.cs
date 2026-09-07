using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UIElements;
using TMPro;

public class NewMonoBehaviourScript : MonoBehaviour
{
    [SerializeField] protected Fight    Fight;
    [SerializeField] protected Fighter  SelectedFighter;
    [SerializeField] protected TMP_Text NameText, RoundText, HealthText, ManaText;
    protected bool                      SortDescending = true;
    protected VisualElement             CurrentlySelectedTab;
    protected VisualElement             CurrentlySelectedSubTab;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
