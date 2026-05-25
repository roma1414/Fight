using UnityEngine;
using UnityEngine.UIElements;

public class SelectMove : MonoBehaviour
{
    [SerializeField] UIDocument uiDocument;
    private VisualElement root;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        root = uiDocument.rootVisualElement;

        var button1 = root.Q<VisualElement>("Button1");
        button1.RegisterCallback<ClickEvent>(OnButton1ClickEvent);
        var button2 = root.Q<VisualElement>("Button2");
        button2.RegisterCallback<ClickEvent>(OnButton2ClickEvent);
        var button3 = root.Q<VisualElement>("Button3");
        button3.RegisterCallback<ClickEvent>(OnButton3ClickEvent);
    }

    private void OnButton1ClickEvent(ClickEvent evt)
    {
        Debug.LogError("Button 1 Clicked");
    }

    private void OnButton2ClickEvent(ClickEvent evt)
    {
        Debug.LogError("Button 2 Clicked");
    }

    private void OnButton3ClickEvent(ClickEvent evt)
    {
        Debug.LogError("Button 3 Clicked");
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
