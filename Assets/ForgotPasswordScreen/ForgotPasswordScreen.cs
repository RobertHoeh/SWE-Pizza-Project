using System.Text.RegularExpressions;
using UnityEngine.UIElements;

// "Forgot Your Password?" (mockup slides 22-23).
public class ForgotPasswordScreen : UIScreen
{
    protected override void OnBind(VisualElement root)
    {
        root.OnClick("SendLinkButton", SendLink);
    }

    protected override void Refresh()
    {
        HideCardError();
        Root.ClearFields("EmailField");
    }

    void SendLink()
    {
        var email = Root.FieldText("EmailField");
        if (!Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
        {
            ShowCardError("Please enter a valid email address");
            return;
        }

        // TODO: have the backend email a reset link. The link should open the
        // Reset Password page; until then go straight there.
        UIManager.Instance.Show(ScreenID.RESET_PASSWORD);
    }
}
