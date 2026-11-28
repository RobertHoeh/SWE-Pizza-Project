using System.Linq;
using UnityEngine.UIElements;

// Manager > Employees (mockup slides 83-93).
public class CompanyEmployeesScreen : UIScreen
{
    TableRows table;

    protected override void OnBind(VisualElement root)
    {
        table = new TableRows(root, root.Q<ScrollView>("EmployeeRows"), root.Q("RowActionMenu"),
            "emp-col-actions", "emp-col-id", "emp-col-name", "emp-col-dept", "emp-col-contact", "emp-col-hire");

        root.OnClick("EditEmployeeButton", EditSelected);
        root.OnClick("DeleteEmployeeButton", () =>
        {
            // TODO: delete the employee in the backend.
            table.RemoveRow(table.SelectedRow);
        });

        root.Q<TextField>("SearchField").RegisterValueChangedCallback(evt =>
        {
            var search = evt.newValue.Trim().ToLower();
            table.Filter(values => search == "" || values.Any(v => v.ToLower().Contains(search)));
        });
    }

    protected override void Refresh()
    {
        table.HideMenu();
        Root.SetField("SearchField", "");
        table.Filter(_ => true);
        // TODO: rebuild the rows in "EmployeeRows" from the backend.
    }

    void EditSelected()
    {
        table.HideMenu();
        var row = table.SelectedRow;
        var values = table.Read(row); // id, name, department, "phone\nemail", hire date
        var contact = values[3].Split('\n');

        UIManager.Instance.Modal<EmployeeModal>().Open(
            new EmployeeModal.Employee
            {
                Name = values[1],
                Department = values[2],
                Phone = contact[0],
                Email = contact.Length > 1 ? contact[1] : "",
                HireDate = values[4],
            },
            saved => table.Write(row, new[]
            {
                values[0], saved.Name, saved.Department, saved.Phone + "\n" + saved.Email, saved.HireDate,
            }));
    }
}
