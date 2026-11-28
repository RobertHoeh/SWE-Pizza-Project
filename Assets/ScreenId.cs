// Every full page the UIManager can show. Each value is paired with one
// UIScreen component in the scene (set in that component's "Id" field).
public enum ScreenID
{
    LOGIN,
    REGISTER_CODE,
    REGISTER,
    FORGOT_PASSWORD,
    RESET_PASSWORD,
    ORDER_DASHBOARD,
    EMPLOYEE_INFO,
    COMPANY_EMPLOYEES,
    MANAGE_MENU
}

// Every popup that opens on top of the current page. Each value is paired
// with one UIModal component in the scene.
public enum ModalID
{
    SUCCESS,
    ACCOUNT_MENU,
    EMPLOYEE,
    MENU_ITEM,
    PLACE_ORDER
}
