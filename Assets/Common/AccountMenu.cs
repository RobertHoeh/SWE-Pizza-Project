using UnityEngine.UIElements;

// Dropdown under the navbar's username / avatar (mockup slide 31).
public class AccountMenu : UIModal
{
    VisualElement menu;

    protected override void OnBind(VisualElement root)
    {
        menu = root.Q("AccountMenu");
        root.OnClick("AccountInfoButton", () => UIManager.Instance.Show(ScreenID.EMPLOYEE_INFO));
        root.OnClick("LogOffButton", () =>
        {
            // TODO: tell the backend the user signed out.
            Session.LogOut();
            UIManager.Instance.Show(ScreenID.LOGIN);
        });
    }

    // Close when anything outside the menu is pressed. The navbar's own
    // buttons are skipped because they toggle the menu themselves.
    public override void OnPointerDownAnywhere(VisualElement target)
    {
        if (target.IsInside(menu)) return;
        for (var e = target; e != null; e = e.parent)
            if (e.name == "LoginButton" || e.name == "DropdownButton") return;
        Close();
    }
}
