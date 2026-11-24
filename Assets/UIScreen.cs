using UnityEngine;
using UnityEngine.UIElements;

public abstract class UIScreen : MonoBehaviour
{
    // Be sure to set these two fields using the editor before using this class.
    // The VisualTreeAsset links to the uxml file, and the ScreenId enum decides
    // how to refer to this in the code.
    [SerializeField] private VisualTreeAsset uxml;
    [SerializeField] private ScreenID id;

    // public backing field to enforce readonly constraints
    public ScreenID Id => id;
    protected VisualElement Root;

    public void Init(VisualElement parent)
    {
        Root = uxml.Instantiate();
        Root.AddToClassList("screen");
        parent.Add(Root);
        OnBind(Root);
        Hide();
    }

    /* Should be used for registering callbacks and whatnot */
    protected abstract void OnBind(VisualElement root);

    /* Used for refreshing information when showing the page
     * (for getting information from the backend and such) */
    protected virtual void Refresh() { }

    public virtual void Show()
    {
        Root.RemoveFromClassList("screen--hidden");

    }

    public virtual void Hide() => Root.AddToClassList("screen--hidden");

}
