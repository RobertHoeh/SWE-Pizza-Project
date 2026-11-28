using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

// Small shortcuts used by the screen and popup scripts.
public static class UIHelpers
{
    // Calls action when the named button is clicked.
    public static void OnClick(this VisualElement root, string buttonName, Action action)
    {
        var button = root.Q<Button>(buttonName);
        if (button == null)
        {
            Debug.LogWarning($"No button named '{buttonName}' in {root.name}");
            return;
        }
        button.clicked += action;
    }

    // Trimmed value of the named TextField ("" if it doesn't exist).
    public static string FieldText(this VisualElement root, string fieldName) =>
        root.Q<TextField>(fieldName)?.value?.Trim() ?? "";

    public static void SetField(this VisualElement root, string fieldName, string value) =>
        root.Q<TextField>(fieldName)?.SetValueWithoutNotify(value ?? "");

    public static void ClearFields(this VisualElement root, params string[] fieldNames)
    {
        foreach (var name in fieldNames) root.SetField(name, "");
    }

    public static void SetHidden(this VisualElement element, bool hidden) =>
        element?.EnableInClassList("hidden", hidden);

    public static void SetHidden(this VisualElement root, string name, bool hidden) =>
        root.Q(name).SetHidden(hidden);

    // True if any RadioButton inside the element is selected.
    public static bool AnyRadioSelected(this VisualElement element) =>
        element != null && element.Query<RadioButton>().ToList().Any(r => r.value);

    // Walks up from element and returns the first ancestor (or itself) with the class.
    public static VisualElement ClosestWithClass(this VisualElement element, string className)
    {
        for (var e = element; e != null; e = e.parent)
            if (e.ClassListContains(className)) return e;
        return null;
    }

    public static bool IsInside(this VisualElement element, VisualElement container)
    {
        for (var e = element; e != null; e = e.parent)
            if (e == container) return true;
        return false;
    }
}
