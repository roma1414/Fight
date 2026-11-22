using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FighterPanelData
{
    protected Fighter           Fighter;
    protected GameObject        FighterPanel;
    protected List<GameObject>  SourceCollection;


    public FighterPanelData(Fighter fighter, GameObject fighterPanel, List<GameObject> sourceCollection)
    {
        Fighter = fighter;
        FighterPanel = fighterPanel;
        SourceCollection = sourceCollection;
    }

    public Fighter GetFighter() { return Fighter; }
    public GameObject GetFighterPanel() { return FighterPanel; }
    public List<GameObject> GetSourceCollection() { return SourceCollection; }
    public void SetFighter(Fighter fighter) { Fighter = fighter; }
    public void SetFighterPanel(GameObject fighterPanel) { FighterPanel = fighterPanel; }
    public void SetSourceCollection(List<GameObject> sourceCollection) {SourceCollection = sourceCollection; }
}
