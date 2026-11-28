using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UIElements;

// Tools > Pizza UI > Set Up Scene
// Builds (or rebuilds) the "UI" object in the open scene: a UIDocument showing
// BaseLayer.uxml, the UIManager, and one child object per page / popup with
// its uxml and id filled in. Any PanelRenderer objects are switched off.
// Safe to run again after adding a page: existing objects are reused.
public static class UISceneSetup
{
    static readonly (Type type, ScreenID id, string uxml)[] Screens =
    {
        (typeof(LogInScreen), ScreenID.LOGIN, "Assets/LogInScreen/LogInScreen.uxml"),
        (typeof(RegisterCodeScreen), ScreenID.REGISTER_CODE, "Assets/RegisterCodeScreen/RegisterCodeScreen.uxml"),
        (typeof(RegisterScreen), ScreenID.REGISTER, "Assets/RegisterScreen/RegisterScreen.uxml"),
        (typeof(ForgotPasswordScreen), ScreenID.FORGOT_PASSWORD, "Assets/ForgotPasswordScreen/ForgotPasswordScreen.uxml"),
        (typeof(ResetPasswordScreen), ScreenID.RESET_PASSWORD, "Assets/ResetPasswordScreen/ResetPasswordScreen.uxml"),
        (typeof(OrderDashboardScreen), ScreenID.ORDER_DASHBOARD, "Assets/OrderDashboardScreen/OrderDashboardScreen.uxml"),
        (typeof(EmployeeInfoScreen), ScreenID.EMPLOYEE_INFO, "Assets/EmployeeInfoScreen/EmployeeInfoScreen.uxml"),
        (typeof(CompanyEmployeesScreen), ScreenID.COMPANY_EMPLOYEES, "Assets/CompanyEmployeesScreen/CompanyEmployeesScreen.uxml"),
        (typeof(ManageMenuScreen), ScreenID.MANAGE_MENU, "Assets/ManageMenuScreen/ManageMenuScreen.uxml"),
    };

    static readonly (Type type, ModalID id, string uxml)[] Modals =
    {
        (typeof(SuccessPopup), ModalID.SUCCESS, "Assets/Common/SuccessPopup.uxml"),
        (typeof(AccountMenu), ModalID.ACCOUNT_MENU, "Assets/Common/AccountMenu.uxml"),
        (typeof(EmployeeModal), ModalID.EMPLOYEE, "Assets/EmployeeModal/EmployeeModal.uxml"),
        (typeof(MenuItemModal), ModalID.MENU_ITEM, "Assets/MenuItemModal/MenuItemModal.uxml"),
        (typeof(PlaceOrderModal), ModalID.PLACE_ORDER, "Assets/PlaceOrderModal/PlaceOrderModal.uxml"),
    };

    [MenuItem("Tools/Pizza UI/Set Up Scene")]
    public static void SetUp()
    {
        foreach (var renderer in UnityEngine.Object.FindObjectsByType<PanelRenderer>(FindObjectsInactive.Exclude))
        {
            Undo.RecordObject(renderer.gameObject, "Set Up UI Scene");
            renderer.gameObject.SetActive(false);
            Debug.Log($"UI setup: switched off PanelRenderer object '{renderer.gameObject.name}'");
        }

        var ui = GameObject.Find("UI");
        if (ui == null)
        {
            ui = new GameObject("UI");
            Undo.RegisterCreatedObjectUndo(ui, "Set Up UI Scene");
        }

        var document = GetOrAdd<UIDocument>(ui);
        document.panelSettings = Load<PanelSettings>("Assets/UI Toolkit/PanelSettings.asset");
        document.visualTreeAsset = Load<VisualTreeAsset>("Assets/BaseLayer.uxml");

        var manager = new SerializedObject(GetOrAdd<UIManager>(ui));
        manager.FindProperty("document").objectReferenceValue = document;
        manager.FindProperty("globalStyles").objectReferenceValue = Load<StyleSheet>("Assets/SharedStyles.uss");
        manager.FindProperty("toastUxml").objectReferenceValue = Load<VisualTreeAsset>("Assets/Common/Toast.uxml");
        manager.FindProperty("startScreen").enumValueIndex = (int)ScreenID.LOGIN;

        var screenList = manager.FindProperty("screenList");
        screenList.ClearArray();
        foreach (var (type, id, uxml) in Screens)
            Append(screenList, MakeChild(ui, type, (int)id, uxml));

        var modalList = manager.FindProperty("modalList");
        modalList.ClearArray();
        foreach (var (type, id, uxml) in Modals)
            Append(modalList, MakeChild(ui, type, (int)id, uxml));

        manager.ApplyModifiedProperties();
        EditorSceneManager.MarkSceneDirty(ui.scene);
        Selection.activeGameObject = ui;
        Debug.Log("UI setup: done. Save the scene to keep it.");
    }

    static Component MakeChild(GameObject ui, Type type, int id, string uxmlPath)
    {
        var existing = ui.transform.Find(type.Name);
        var child = existing != null ? existing.gameObject : null;
        if (child == null)
        {
            child = new GameObject(type.Name);
            Undo.RegisterCreatedObjectUndo(child, "Set Up UI Scene");
            child.transform.SetParent(ui.transform, false);
        }

        var component = child.GetComponent(type);
        if (component == null) component = Undo.AddComponent(child, type);
        var so = new SerializedObject(component);
        so.FindProperty("uxml").objectReferenceValue = Load<VisualTreeAsset>(uxmlPath);
        so.FindProperty("id").enumValueIndex = id;
        so.ApplyModifiedProperties();
        return component;
    }

    static void Append(SerializedProperty list, UnityEngine.Object value)
    {
        list.arraySize++;
        list.GetArrayElementAtIndex(list.arraySize - 1).objectReferenceValue = value;
    }

    static T GetOrAdd<T>(GameObject go) where T : Component
    {
        var component = go.GetComponent<T>();
        return component != null ? component : Undo.AddComponent<T>(go);
    }

    static T Load<T>(string path) where T : UnityEngine.Object
    {
        var asset = AssetDatabase.LoadAssetAtPath<T>(path);
        if (asset == null) Debug.LogError($"UI setup: could not load {path}");
        return asset;
    }
}
