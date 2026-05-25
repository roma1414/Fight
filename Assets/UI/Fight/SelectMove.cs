using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class SelectMove : MonoBehaviour
{
    [SerializeField] UIDocument uiDocument;
    protected VisualElement root;
    protected Fighter mFighter;
    protected List<Move> mMoves;

    private void OnOffensiveClickEvent(ClickEvent evt)
    {
        mMoves = mFighter.GetOffensiveMoves();
    }
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        root = uiDocument.rootVisualElement;

        var offensive = root.Q<VisualElement>("OffensiveTab");
        offensive.RegisterCallback<ClickEvent>(OnButton1ClickEvent);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
