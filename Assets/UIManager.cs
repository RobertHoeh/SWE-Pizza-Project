using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [SerializeField] UIDocument document;
    [SerializeField] StyleSheet globalStyles;
    [SerializeField] List<UIScreen> screenList;
    [SerializeField] List<UIModal> modalList;
    [SerializeField] VisualTreeAsset toastUxml;
    [SerializeField] ScreenID startScreen;

    readonly Dictionary<ScreenID, UIScreen> screens = new();
    readonly Dictionary<ModalID, UIModal> modals = new();

    UIScreen current;

    VisualElement toast;
    Label toastText;
    Button toastUndo;
    Action toastUndoAction;
    IVisualElementScheduledItem toastTimer;

    private void Awake() => Instance = this;

    void Start()
    {
        var root = document.rootVisualElement;
        root.styleSheets.Add(globalStyles);

        var screensLayer = root.Q("screens-layer");
        foreach (var screen in screenList)
        {
            screen.Init(screensLayer);
            screens[screen.Id] = screen;
        }

        var modalLayer = root.Q("modal-layer");
        foreach (var modal in modalList)
        {
            modal.Init(modalLayer);
            modals[modal.Id] = modal;
        }

        InitToast(root.Q("toast-layer"));

        // Lets open popups react to clicks outside of them (e.g. close the account menu).
        root.RegisterCallback<PointerDownEvent>(evt =>
        {
            var target = evt.target as VisualElement;
            foreach (var modal in modals.Values)
                if (modal.IsOpen) modal.OnPointerDownAnywhere(target);
        }, TrickleDown.TrickleDown);

        Show(startScreen);
    }

    // ---- pages ----

    public void Show(ScreenID id)
    {
        CloseAllModals();
        HideToast();

        if (current != null)
        {
            current.Hide();
        }

        current = screens[id];
        current.Show();
    }

    // ---- popups ----

    public UIModal OpenModal(ModalID id)
    {
        var modal = modals[id];
        modal.Open();
        return modal;
    }

    public void CloseModal(ModalID id) => modals[id].Close();

    public void ToggleModal(ModalID id)
    {
        if (modals[id].IsOpen) CloseModal(id);
        else OpenModal(id);
    }

    public void CloseAllModals()
    {
        foreach (var modal in modals.Values) modal.Close();
    }

    // Gets a popup's script so you can call its own Open(...) method,
    // e.g. UIManager.Instance.Modal<SuccessPopup>().Open(...)
    public T Modal<T>() where T : UIModal
    {
        foreach (var modal in modals.Values)
            if (modal is T typed) return typed;
        throw new InvalidOperationException($"No {typeof(T).Name} in the UIManager's modal list");
    }

    // ---- toast (red error / green success banner) ----

    public void ShowError(string message) => ShowToast("<b>Error</b>: " + message, false, null);

    // undo: when given, an "Undo" button is shown and calls it.
    public void ShowSuccess(string message, Action undo = null) =>
        ShowToast("<b>Success</b>: " + message, true, undo);

    void ShowToast(string richText, bool success, Action undo)
    {
        if (toast == null) return;
        toastText.text = richText;
        toast.EnableInClassList("toast--success", success);
        toast.EnableInClassList("toast--error", !success);
        toastUndoAction = undo;
        toastUndo.SetHidden(undo == null);
        toast.parent.SetHidden(false);
        toast.parent.parent.BringToFront();

        toastTimer?.Pause();
        toastTimer = toast.schedule.Execute(HideToast).StartingIn(4000);
    }

    public void HideToast()
    {
        toastTimer?.Pause();
        toast?.parent.SetHidden(true);
    }

    // Hides the banner only if it is showing an error (success messages stay up).
    public void HideError()
    {
        if (toast != null && toast.ClassListContains("toast--error")) HideToast();
    }

    void InitToast(VisualElement layer)
    {
        if (layer == null || toastUxml == null) return;
        var container = toastUxml.Instantiate();
        container.AddToClassList("screen");
        container.pickingMode = PickingMode.Ignore;
        layer.Add(container);

        toast = container.Q("Toast");
        toastText = container.Q<Label>("ToastText");
        toastUndo = container.Q<Button>("UndoButton");
        toastUndo.clicked += () =>
        {
            var undo = toastUndoAction;
            HideToast();
            undo?.Invoke();
        };
        HideToast();
    }
}
