using System;
using UnityEngine.UIElements;

// Purple "success" popup (mockup slides 13, 26, 29).
// Usage: UIManager.Instance.Modal<SuccessPopup>().Open(title, message, buttonText, onButton);
public class SuccessPopup : UIModal
{
    Action onButton;

    protected override void OnBind(VisualElement root)
    {
        root.OnClick("SuccessButton", () =>
        {
            var action = onButton;
            Close();
            action?.Invoke();
        });
    }

    public void Open(string title, string message, string buttonText, Action onButton)
    {
        Root.Q<Label>("SuccessTitle").text = title;
        Root.Q<Label>("SuccessMessage").text = message;
        Root.Q<Button>("SuccessButton").text = buttonText;
        this.onButton = onButton;
        Open();
    }
}
