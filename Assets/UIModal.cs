using UnityEngine;
using UnityEngine.UIElements;

// Base class for popups that open on top of the current page (success popup,
// account menu, employee / menu item / place order popups). Works like
// UIScreen, but lives in BaseLayer's "modal-layer" and several can be open.
public abstract class UIModal : MonoBehaviour
{
    [SerializeField] private VisualTreeAsset uxml;
    [SerializeField] private ModalID id;

    public ModalID Id => id;
    public bool IsOpen { get; private set; }
    protected VisualElement Root;

    public void Init(VisualElement parent)
    {
        Root = uxml.Instantiate();
        Root.AddToClassList("screen");
        // The container fills the window; only the popup's own elements should take clicks.
        Root.pickingMode = PickingMode.Ignore;
        parent.Add(Root);
        OnBind(Root);
        SetVisible(false);
    }

    /* Register callbacks here. */
    protected abstract void OnBind(VisualElement root);

    /* Called every time the popup opens (reset fields, load data...). */
    protected virtual void OnOpen() { }

    /* Called with whatever was pressed anywhere in the window while this popup is open. */
    public virtual void OnPointerDownAnywhere(VisualElement target) { }

    public void Open()
    {
        SetVisible(true);
        Root.BringToFront();
        OnOpen();
    }

    public void Close()
    {
        // an error about this popup shouldn't outlive it
        if (IsOpen && UIManager.Instance != null) UIManager.Instance.HideError();
        SetVisible(false);
    }

    void SetVisible(bool visible)
    {
        IsOpen = visible;
        Root.EnableInClassList("screen--hidden", !visible);
    }
}
