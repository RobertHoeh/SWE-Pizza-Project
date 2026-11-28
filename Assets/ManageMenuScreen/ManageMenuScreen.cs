using UnityEngine.UIElements;

// Manager > Menu Items (mockup slides 94-111).
public class ManageMenuScreen : UIScreen
{
    TableRows table;

    protected override void OnBind(VisualElement root)
    {
        table = new TableRows(root, root.Q<ScrollView>("MenuRows"), root.Q("RowActionMenu"),
            "menu-col-actions", "menu-col-id", "menu-col-name", "menu-col-type", "menu-col-sizes", "menu-col-cost");

        root.OnClick("NewItemButton", () =>
            UIManager.Instance.Modal<MenuItemModal>().OpenAdd(item =>
                table.AddRow(new[] { table.NextId().ToString(), item.Name, item.Type, item.Sizes, item.Cost })));

        root.OnClick("EditItemButton", EditSelected);
        root.OnClick("DeleteItemButton", () =>
        {
            // TODO: delete the item in the backend.
            table.RemoveRow(table.SelectedRow);
        });
    }

    protected override void Refresh()
    {
        table.HideMenu();
        // TODO: rebuild the rows in "MenuRows" from the backend.
    }

    void EditSelected()
    {
        table.HideMenu();
        var row = table.SelectedRow;
        var values = table.Read(row); // id, name, type, sizes, cost

        UIManager.Instance.Modal<MenuItemModal>().OpenEdit(
            new MenuItemModal.Item { Name = values[1], Type = values[2], Sizes = values[3], Cost = values[4] },
            item => table.Write(row, new[] { values[0], item.Name, item.Type, item.Sizes, item.Cost }));
    }
}
