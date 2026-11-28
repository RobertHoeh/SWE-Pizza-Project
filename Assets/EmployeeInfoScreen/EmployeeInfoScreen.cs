using System.Linq;
using UnityEngine.UIElements;

// "EMPLOYEE INFORMATION" for the signed-in user (mockup slides 32-38).
// "Edit Information" unlocks the fields; pressing it again saves them.
public class EmployeeInfoScreen : UIScreen
{
    static readonly string[] Fields = { "NameField", "DepartmentField", "PhoneField", "EmailField", "UsernameField" };

    Button editButton;
    bool editing;
    string[] savedValues;

    protected override void OnBind(VisualElement root)
    {
        editButton = root.Q<Button>("EditInfoButton");
        root.OnClick("EditInfoButton", EditOrSave);
        root.OnClick("ResetPasswordButton", () => UIManager.Instance.Show(ScreenID.RESET_PASSWORD));
    }

    protected override void Refresh()
    {
        // TODO: load the signed-in employee's details from the backend.
        if (Session.LoggedIn) Root.SetField("UsernameField", Session.Username);
        SetEditing(false);
    }

    void EditOrSave()
    {
        if (!editing)
        {
            savedValues = Fields.Select(f => Root.FieldText(f)).ToArray();
            SetEditing(true);
            return;
        }

        if (Fields.Any(f => Root.FieldText(f) == ""))
        {
            UIManager.Instance.ShowError("One or more fields is not filled out");
            return;
        }

        if (!Session.IsManager && Root.FieldText("DepartmentField") != savedValues[1])
        {
            UIManager.Instance.ShowError("Only managers can edit department information. Please change back or contact your manager");
            return;
        }

        // TODO: save the changes with the backend.
        var previous = savedValues;
        SetEditing(false);
        UIManager.Instance.ShowSuccess("Information successfully changed.", undo: () =>
        {
            // TODO: also undo the change in the backend.
            for (int i = 0; i < Fields.Length; i++) Root.SetField(Fields[i], previous[i]);
        });
    }

    void SetEditing(bool value)
    {
        editing = value;
        foreach (var f in Fields) Root.Q<TextField>(f).isReadOnly = !value;
        editButton.text = value ? "Save Information" : "Edit Information";
    }
}
