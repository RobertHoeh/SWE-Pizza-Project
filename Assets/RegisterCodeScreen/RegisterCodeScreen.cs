using UnityEngine.UIElements;

// "Create an Account" code entry (mockup slides 2-5).
public class RegisterCodeScreen : UIScreen
{
    protected override void OnBind(VisualElement root)
    {
        root.OnClick("GoToRegistrationButton", Submit);
    }

    protected override void Refresh()
    {
        HideCardError();
        Root.ClearFields("CodeField");
    }

    void Submit()
    {
        var code = Root.FieldText("CodeField");
        if (code == "")
        {
            ShowCardError("Code field is not filled in. Ensure the field is filled in before submitting.");
            return;
        }

        // TODO: check the code with the backend.
        if (code.Length != 7)
        {
            ShowCardError("You have entered an incorrect code. Try again or contact manager for help");
            return;
        }

        UIManager.Instance.Show(ScreenID.REGISTER);
    }
}
