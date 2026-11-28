using UnityEngine.UIElements;

// "Reset Password" (mockup slides 24-26). Reached from Forgot Password and
// from the Employee Information page.
public class ResetPasswordScreen : UIScreen
{
    protected override void OnBind(VisualElement root)
    {
        root.OnClick("ChangePasswordButton", ChangePassword);
    }

    protected override void Refresh()
    {
        HideCardError();
        Root.ClearFields("NewPasswordField", "ConfirmPasswordField");
    }

    void ChangePassword()
    {
        var password = Root.FieldText("NewPasswordField");
        var confirm = Root.FieldText("ConfirmPasswordField");

        if (password == "" || confirm == "")
        {
            ShowCardError("One or more fields are not filled in.");
            return;
        }
        if (password != confirm)
        {
            ShowCardError("Passwords do not match");
            return;
        }
        if (!RegisterScreen.PasswordMeetsRules(password))
        {
            ShowCardError(RegisterScreen.PasswordRulesError);
            return;
        }

        // TODO: save the new password with the backend.
        HideCardError();
        UIManager.Instance.Modal<SuccessPopup>().Open(
            "PASSWORD SUCCESSFULLY CHANGED!",
            "Congratulations, your password has been successfully changed. You may now sign in to your account",
            "Go to Sign In",
            () =>
            {
                Session.LogOut();
                UIManager.Instance.Show(ScreenID.LOGIN);
            });
    }
}
