using UnityEngine.UIElements;

// "Sign in to Access Dashboard" (mockup slides 1, 15-21, 27-29).
public class LogInScreen : UIScreen
{
    protected override void OnBind(VisualElement root)
    {
        root.OnClick("SignInButton", SignIn);
        root.OnClick("CreateAccountLink", () => UIManager.Instance.Show(ScreenID.REGISTER_CODE));
        root.OnClick("ForgotPasswordLink", () => UIManager.Instance.Show(ScreenID.FORGOT_PASSWORD));
    }

    protected override void Refresh()
    {
        HideCardError();
        Root.ClearFields("UsernameField", "PasswordField");
    }

    void SignIn()
    {
        var username = Root.FieldText("UsernameField");
        var password = Root.FieldText("PasswordField");

        if (username == "" || password == "")
        {
            ShowCardError("Password or username missing");
            return;
        }

        // TODO: check the username and password with the backend. On failure use
        //   ShowCardError("You have entered an incorrect username.") or
        //   ShowCardError("You have entered an incorrect password.")
        // Placeholder until then: any username starting with "manager" signs in as a manager.
        bool isManager = username.ToLower().StartsWith("manager");
        Session.LogIn(username, isManager);
        HideCardError();

        UIManager.Instance.Modal<SuccessPopup>().Open(
            "LOGIN SUCCESSFUL",
            "You have successfully signed into your account.",
            "Go to Dashboard",
            () => UIManager.Instance.Show(ScreenID.ORDER_DASHBOARD));
    }
}
