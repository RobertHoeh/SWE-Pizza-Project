using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [SerializeField] UIDocument document;
    [SerializeField] StyleSheet globalStyles;
    [SerializeField] List<UIScreen> screenList;
    [SerializeField] ScreenID startScreen;

    readonly Dictionary<ScreenID, UIScreen> screens = new();

    UIScreen current;

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

        Show(startScreen);
    }

    public void Show(ScreenID id)
    {
        if (current != null)
        {
            current.Hide();
        }

        current = screens[id];
        current.Show();
    }
}
