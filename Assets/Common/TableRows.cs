using System;
using UnityEngine.UIElements;

// Shared code for the Company Employees and Manage Menu tables: reading and
// writing cells, building rows, and the small "..." row menu.
public class TableRows
{
    readonly ScrollView rows;
    readonly VisualElement menu;
    readonly string[] columns;
    readonly string actionsColumn;

    // The row whose "..." was pressed last.
    public VisualElement SelectedRow { get; private set; }

    // columns: the class of each text column, in order ("emp-col-id", ...).
    public TableRows(VisualElement screenRoot, ScrollView rows, VisualElement menu, string actionsColumn, params string[] columns)
    {
        this.rows = rows;
        this.menu = menu;
        this.columns = columns;
        this.actionsColumn = actionsColumn;

        foreach (var row in rows.Query(className: "table__row").ToList()) BindRow(row);

        // Close the row menu when anything else is pressed.
        screenRoot.RegisterCallback<PointerDownEvent>(evt =>
        {
            var target = evt.target as VisualElement;
            if (target.IsInside(menu) || target.ClosestWithClass("dots") != null) return;
            HideMenu();
        }, TrickleDown.TrickleDown);
    }

    public void HideMenu() => menu.SetHidden(true);

    // Text of every column in the row, in the order given to the constructor.
    public string[] Read(VisualElement row)
    {
        var values = new string[columns.Length];
        for (int i = 0; i < columns.Length; i++)
            values[i] = row.Q(className: columns[i])?.Q<Label>()?.text ?? "";
        return values;
    }

    public void Write(VisualElement row, string[] values)
    {
        for (int i = 0; i < columns.Length && i < values.Length; i++)
        {
            var label = row.Q(className: columns[i])?.Q<Label>();
            if (label != null) label.text = values[i];
        }
    }

    public VisualElement AddRow(string[] values)
    {
        var row = new VisualElement();
        row.AddToClassList("table__row");
        foreach (var column in columns)
        {
            var cell = new VisualElement();
            cell.AddToClassList("table__cell");
            cell.AddToClassList(column);
            var label = new Label();
            label.AddToClassList("table__text");
            cell.Add(label);
            row.Add(cell);
        }

        var actions = new VisualElement();
        actions.AddToClassList("table__cell");
        actions.AddToClassList("table__cell--center");
        actions.AddToClassList(actionsColumn);
        var dots = new Button();
        dots.AddToClassList("dots");
        for (int i = 0; i < 3; i++)
        {
            var dot = new VisualElement();
            dot.AddToClassList("dots__dot");
            dots.Add(dot);
        }
        actions.Add(dots);
        row.Add(actions);

        Write(row, values);
        rows.Add(row);
        BindRow(row);
        return row;
    }

    public void RemoveRow(VisualElement row)
    {
        HideMenu();
        row.RemoveFromHierarchy();
    }

    // Shows only the rows for which keep(row values) is true.
    public void Filter(Func<string[], bool> keep)
    {
        foreach (var row in rows.Query(className: "table__row").ToList())
            row.style.display = keep(Read(row)) ? DisplayStyle.Flex : DisplayStyle.None;
    }

    public int NextId()
    {
        int max = 0;
        foreach (var row in rows.Query(className: "table__row").ToList())
            if (int.TryParse(Read(row)[0], out var id) && id > max) max = id;
        return max + 1;
    }

    void BindRow(VisualElement row)
    {
        var dots = row.Q<Button>(className: "dots");
        if (dots == null) return;
        dots.clicked += () =>
        {
            bool sameRowOpen = SelectedRow == row && !menu.ClassListContains("hidden");
            SelectedRow = row;
            if (sameRowOpen)
            {
                HideMenu();
                return;
            }
            // Line the menu up with the row's dots; its right edge comes from the USS.
            menu.style.top = dots.worldBound.y - menu.parent.worldBound.y;
            menu.SetHidden(false);
            menu.BringToFront();
        };
    }
}
