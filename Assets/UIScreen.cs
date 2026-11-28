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

    Button loginButton;

    public void Init(VisualElement parent)
    {
        Root = uxml.Instantiate();
        Root.AddToClassList("screen");
        parent.Add(Root);
        BindNavBar();
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
        RefreshNavBar();
        Refresh();
    }

    public virtual void Hide() => Root.AddToClassList("screen--hidden");

    // ---- navbar: every page has its own copy of NavBar.uxml ----

    void BindNavBar()
    {
        Root.OnClick("LogoButton", GoHome);
        Root.OnClick("DashboardButton", GoHome);
        Root.OnClick("LoginButton", AccountButtonClicked);
        Root.OnClick("DropdownButton", AccountButtonClicked);
        loginButton = Root.Q<Button>("LoginButton");
    }

    void RefreshNavBar()
    {
        if (loginButton != null)
            loginButton.text = Session.LoggedIn ? Session.Username : "Log in";
    }

    static void GoHome() =>
        UIManager.Instance.Show(Session.LoggedIn ? ScreenID.ORDER_DASHBOARD : ScreenID.LOGIN);

    static void AccountButtonClicked()
    {
        if (Session.LoggedIn) UIManager.Instance.ToggleModal(ModalID.ACCOUNT_MENU);
        else UIManager.Instance.Show(ScreenID.LOGIN);
    }

    // ---- red error box inside the login / register / password cards ----

    protected void ShowCardError(string message)
    {
        var text = Root.Q<Label>("ErrorText");
        if (text != null) text.text = "<b>Error</b>: " + message;
        Root.SetHidden("ErrorBanner", false);
    }

    protected void HideCardError() => Root.SetHidden("ErrorBanner", true);
}
