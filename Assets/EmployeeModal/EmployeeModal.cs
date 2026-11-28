using System;
using System.Linq;
using UnityEngine.UIElements;

// "EMPLOYEE" popup opened from Company Employees > "..." > Edit Info (mockup slides 85-89).
// "Edit Employee" unlocks the fields; pressing it again saves them.
public class EmployeeModal : UIModal
{
    public class Employee
    {
        public string Name, Department, Phone, Email, HireDate;
    }

    static readonly string[] Fields = { "NameField", "DepartmentField", "PhoneField", "EmailField", "HireDateField" };

    Button editButton;
    bool editing;
    Action<Employee> onSaved;

    protected override void OnBind(VisualElement root)
    {
        editButton = root.Q<Button>("EditEmployeeButton");
        root.OnClick("EditEmployeeButton", EditOrSave);
        root.OnClick("CloseButton", Close);
    }

    // onSaved is called with the new values after a successful save.
    public void Open(Employee employee, Action<Employee> onSaved)
    {
        this.onSaved = onSaved;
        Root.SetField("NameField", employee.Name);
        Root.SetField("DepartmentField", employee.Department);
        Root.SetField("PhoneField", employee.Phone);
        Root.SetField("EmailField", employee.Email);
        Root.SetField("HireDateField", employee.HireDate);
        SetEditing(false);
        Open();
    }

    void EditOrSave()
    {
        if (!editing)
        {
            SetEditing(true);
            return;
        }

        if (Fields.Any(f => Root.FieldText(f) == ""))
        {
            UIManager.Instance.ShowError("One or more fields is not filled out");
            return;
        }

        if (!string.Equals(Root.FieldText("DepartmentField"), "Employee", StringComparison.OrdinalIgnoreCase))
        {
            UIManager.Instance.ShowError("Can not create/change Roles other than Employee");
            return;
        }

        // TODO: save the employee with the backend.
        SetEditing(false);
        onSaved?.Invoke(new Employee
        {
            Name = Root.FieldText("NameField"),
            Department = Root.FieldText("DepartmentField"),
            Phone = Root.FieldText("PhoneField"),
            Email = Root.FieldText("EmailField"),
            HireDate = Root.FieldText("HireDateField"),
        });
        UIManager.Instance.ShowSuccess("Information successfully saved.");
    }

    void SetEditing(bool value)
    {
        editing = value;
        foreach (var f in Fields) Root.Q<TextField>(f).isReadOnly = !value;
        editButton.text = value ? "Save Employee" : "Edit Employee";
    }
}
