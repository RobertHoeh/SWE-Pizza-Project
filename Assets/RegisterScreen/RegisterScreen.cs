using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine.UIElements;

// "Create an Account" form (mockup slides 6-13).
public class RegisterScreen : UIScreen
{
    static readonly string[] Fields = { "UsernameField", "EmailField", "PasswordField", "FullNameField", "PhoneField" };

    public const string PasswordRulesError =
        "Passwords must contain at least one capital letter, one lowercase letter, one number, and one symbol (-+=*/%$#@!)";

    // At least one capital, one lowercase, one number and one of the listed symbols.
    public static bool PasswordMeetsRules(string password) =>
        Regex.IsMatch(password, "[A-Z]") &&
        Regex.IsMatch(password, "[a-z]") &&
        Regex.IsMatch(password, "[0-9]") &&
        Regex.IsMatch(password, @"[-+=*/%$#@!]");

    protected override void OnBind(VisualElement root)
    {
        root.OnClick("RegisterButton", Register);
    }

    protected override void Refresh()
    {
        HideCardError();
        Root.ClearFields(Fields);
    }

    void Register()
    {
        if (Fields.Any(f => Root.FieldText(f) == ""))
        {
            ShowCardError("One or more fields are not filled in. Ensure all fields are filled before registering");
            return;
        }

        if (!PasswordMeetsRules(Root.FieldText("PasswordField")))
        {
            ShowCardError(PasswordRulesError);
            return;
        }

        // TODO: create the account with the backend.
        HideCardError();
        UIManager.Instance.Modal<SuccessPopup>().Open(
            "REGISTER SUCCESS!",
            "Congratulations, your account has been successfully created. You may now sign in to your account.",
            "Go to Sign In",
            () => UIManager.Instance.Show(ScreenID.LOGIN));
    }
}
