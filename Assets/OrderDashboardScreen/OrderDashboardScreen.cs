using UnityEngine.UIElements;

// Order dashboard (mockup slides 30, 77, 81, 82).
public class OrderDashboardScreen : UIScreen
{
    VisualElement managementMenu;

    protected override void OnBind(VisualElement root)
    {
        managementMenu = root.Q("ManagementMenu");

        root.OnClick("ManagementControlsButton", () => managementMenu.SetHidden(!managementMenu.ClassListContains("hidden")));
        root.OnClick("EmployeesButton", () => UIManager.Instance.Show(ScreenID.COMPANY_EMPLOYEES));
        root.OnClick("MenuItemsButton", () => UIManager.Instance.Show(ScreenID.MANAGE_MENU));
        root.OnClick("PlaceOrderButton", () => UIManager.Instance.OpenModal(ModalID.PLACE_ORDER));
    }

    protected override void Refresh()
    {
        Root.SetHidden("ManagementControlsButton", !Session.IsManager);
        managementMenu.SetHidden(true);
        // TODO: rebuild the rows in "OrderRows" from the backend's current orders.
    }
}
