using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine.UIElements;

// "Place Order" popup from the dashboard (mockup slides 39-76).
// Step 1 Record Order -> step 2 Customer Information -> step 3 Receipt.
public class PlaceOrderModal : UIModal
{
    class CartItem
    {
        public string Name;
        public List<string> Details;
        public int UnitPrice;
        public int Quantity = 1;
    }

    static readonly string[] StepPanels = { "StepRecordOrder", "StepCustomerInfo", "StepReceipt" };
    static readonly string[] StepLabels = { "StepLabelRecord", "StepLabelCustomer", "StepLabelReceipt" };
    static readonly string[] Tabs = { "PremadeTab", "CustomTab", "SidesTab" };
    static readonly string[] TabPanels = { "PremadePanel", "CustomPanel", "SidesPanel" };

    readonly List<CartItem> cart = new();
    ScrollView orderItems;
    int step;

    protected override void OnBind(VisualElement root)
    {
        orderItems = root.Q<ScrollView>("OrderItems");

        root.OnClick("CloseButton", Close);
        root.OnClick("NextButton", () =>
        {
            if (cart.Count == 0)
            {
                UIManager.Instance.ShowError("Add at least one item to the order before continuing");
                return;
            }
            GoToStep(1);
        });
        root.OnClick("BackButton", () => GoToStep(step - 1));

        // step 1: menu tabs and "Add to Cart"
        for (int i = 0; i < Tabs.Length; i++)
        {
            int tab = i;
            root.OnClick(Tabs[i], () => ShowTab(tab));
        }
        foreach (var button in root.Query<Button>(className: "premade__add").ToList())
            button.clicked += () => AddPremade(button.ClosestWithClass("premade"));
        root.OnClick("CustomAddToCartButton", AddCustom);
        root.OnClick("SidesAddToCartButton", AddSides);

        // step 2: customer lookup
        root.OnClick("FindCustomerButton", FindCustomer);
        root.OnClick("UseThisInfoButton", UseCustomerInfo);
        root.OnClick("UpdateCustomerButton", UseCustomerInfo); // TODO: also save the changed record in the backend.
        root.OnClick("CreateRecordButton", UseCustomerInfo);   // TODO: also create the record in the backend.

        // step 3: receipt
        root.OnClick("ReviewOrderButton", ReviewOrder);
        root.OnClick("PlaceOrderButton", PlaceOrder);
        root.OnClick("OkButton", Close);
        root.OnClick("CancelOrderButton", () =>
        {
            // TODO: cancel the order in the backend.
            Close();
        });
    }

    protected override void OnOpen()
    {
        cart.Clear();
        RebuildCart();
        ClearSelections(Root.Q("StepRecordOrder"));
        Root.ClearFields("LookupPhoneField", "RecordPhoneField", "RecordNameField", "RecordAddressField",
            "RecordAptField", "ReceiptPhoneField", "ReceiptEmailField");
        foreach (var name in new[] { "PhysicalToggle", "TextToggle", "EmailToggle", "DeliveryToggle" })
            Root.Q<Toggle>(name)?.SetValueWithoutNotify(false);
        ShowTab(0);
        GoToStep(0);
    }

    // ---------------- steps and tabs ----------------

    void GoToStep(int newStep)
    {
        step = newStep;
        for (int i = 0; i < StepPanels.Length; i++)
        {
            Root.SetHidden(StepPanels[i], i != step);
            Root.Q(StepLabels[i])?.EnableInClassList("po-step--active", i == step);
        }
        Root.SetHidden("NextButton", step != 0);
        Root.SetHidden("BackButton", step == 0);

        // each step starts on its first panel
        if (step == 1) ShowPanel("LookupPanel", "FindingPanel", "RecordPanel");
        if (step == 2) ShowPanel("DeliveryPanel", "ReviewPanel", "PlacedPanel");
    }

    void ShowPanel(string visible, params string[] others)
    {
        Root.SetHidden(visible, false);
        foreach (var other in others) Root.SetHidden(other, true);
    }

    void ShowTab(int tab)
    {
        for (int i = 0; i < Tabs.Length; i++)
        {
            Root.Q(Tabs[i])?.EnableInClassList("po-tab--active", i == tab);
            Root.SetHidden(TabPanels[i], i != tab);
        }
    }

    // ---------------- step 1: cart ----------------

    void AddPremade(VisualElement pizza)
    {
        var size = SelectedRadio(pizza);
        if (size == null)
        {
            UIManager.Instance.ShowError("Select size before adding to cart");
            return;
        }

        var name = pizza.Q(className: "premade__info")?.Q<Label>()?.text ?? "Pizza";
        var toppings = CheckedToggles(pizza);
        var details = new List<string> { size.text + " size" };
        details.AddRange(toppings.Select(t => t.text));
        AddToCart(name, details, Price(size) + toppings.Sum(Price));
        ClearSelections(pizza);
    }

    void AddCustom()
    {
        var panel = Root.Q("CustomPanel");
        var crust = SelectedRadio(panel.Q("CrustGroup"));
        var size = SelectedRadio(panel.Q("SizeGroup"));
        var sauce = SelectedRadio(panel.Q("SauceGroup"));

        if (crust == null) { UIManager.Instance.ShowError("Select crust before adding to cart"); return; }
        if (size == null) { UIManager.Instance.ShowError("Select size before adding to cart"); return; }
        if (sauce == null) { UIManager.Instance.ShowError("Select sauce before adding to cart"); return; }

        var toppings = CheckedToggles(panel);
        var details = new List<string> { crust.text + " crust", size.text, sauce.text };
        details.AddRange(toppings.Select(t => t.text));
        AddToCart("Custom Pizza", details, Price(size) + toppings.Sum(Price));
        ClearSelections(panel);
    }

    void AddSides()
    {
        var panel = Root.Q("SidesPanel");
        var picks = new[] { SelectedRadio(panel.Q("BeverageGroup")), SelectedRadio(panel.Q("SideGroup")) }
            .Where(r => r != null).ToList();

        if (picks.Count == 0)
        {
            UIManager.Instance.ShowError("Select a Item before adding to cart");
            return;
        }
        foreach (var pick in picks) AddToCart(pick.text, new List<string>(), Price(pick));
        ClearSelections(panel);
    }

    void AddToCart(string name, List<string> details, int price)
    {
        // the same item again just raises its quantity
        var existing = cart.FirstOrDefault(c => c.Name == name && c.Details.SequenceEqual(details));
        if (existing != null) existing.Quantity++;
        else cart.Add(new CartItem { Name = name, Details = details, UnitPrice = price });
        UIManager.Instance.HideError();
        RebuildCart();
    }

    void RebuildCart()
    {
        orderItems.Clear();
        foreach (var item in cart)
        {
            var box = new VisualElement();
            box.AddToClassList("po-item");

            var head = new VisualElement();
            head.AddToClassList("po-item__head");
            head.Add(NewLabel(item.Name, "po-item__name"));

            var qty = new VisualElement();
            qty.AddToClassList("po-qty");
            qty.Add(NewButton("-", "po-qty__btn", () => ChangeQuantity(item, -1)));
            qty.Add(NewLabel(item.Quantity.ToString(), "po-qty__count"));
            qty.Add(NewButton("+", "po-qty__btn", () => ChangeQuantity(item, +1)));
            head.Add(qty);

            head.Add(NewLabel(item.UnitPrice * item.Quantity + "$", "po-item__price"));
            box.Add(head);

            foreach (var detail in item.Details)
                box.Add(NewLabel("•        " + Capitalize(detail), "po-item__detail"));

            orderItems.Add(box);
        }
        Root.Q<Label>("OrderTotal").text = cart.Sum(c => c.UnitPrice * c.Quantity) + "$";
    }

    void ChangeQuantity(CartItem item, int change)
    {
        item.Quantity += change;
        if (item.Quantity <= 0) cart.Remove(item);
        RebuildCart();
    }

    // ---------------- step 2: customer ----------------

    void FindCustomer()
    {
        var phone = Root.FieldText("LookupPhoneField");
        if (Regex.Replace(phone, "[^0-9]", "").Length < 10)
        {
            UIManager.Instance.ShowError("Fill out Customer Phone Number completely");
            return;
        }

        ShowPanel("FindingPanel", "LookupPanel", "RecordPanel");

        // TODO: look the customer up in the backend instead of this placeholder,
        // which only "finds" the sample customer from the mockups after a short wait.
        Root.schedule.Execute(() =>
        {
            bool found = phone == "770-362-3213";
            ShowRecord(found, phone, found ? "John Something" : "", found ? "32 Charlotte Ave.\nMarietta GA" : "");
        }).StartingIn(800);
    }

    void ShowRecord(bool found, string phone, string name, string address)
    {
        ShowPanel("RecordPanel", "LookupPanel", "FindingPanel");
        Root.Q<Label>("RecordTitle").text = found ? "Customer Information Found!" : "Customer Information not found";
        Root.SetHidden("RecordSubtitle", found);
        Root.SetHidden("FoundButtons", !found);
        Root.SetHidden("CreateRecordButton", found);
        Root.SetField("RecordPhoneField", phone);
        Root.SetField("RecordNameField", name);
        Root.SetField("RecordAddressField", address);
        Root.SetField("RecordAptField", "");
    }

    void UseCustomerInfo()
    {
        if (Root.FieldText("RecordNameField") == "" || Root.FieldText("RecordAddressField") == "")
        {
            UIManager.Instance.ShowError("Must not leave Address and/or Name Empty");
            return;
        }
        // TODO: check the address is real with the backend, else
        // ShowError("Must be a Real Address")
        Root.SetField("ReceiptPhoneField", Root.FieldText("RecordPhoneField"));
        GoToStep(2);
    }

    // ---------------- step 3: receipt ----------------

    void ReviewOrder()
    {
        if (Root.Q<Toggle>("EmailToggle").value && Root.FieldText("ReceiptEmailField") == "")
        {
            UIManager.Instance.ShowError("Must enter a Email Address");
            return;
        }

        Root.Q<Label>("ReviewPhone").text = Root.FieldText("RecordPhoneField");
        Root.Q<Label>("ReviewName").text = Root.FieldText("RecordNameField");
        Root.Q<Label>("ReviewAddress").text = Root.FieldText("RecordAddressField");
        var apt = Root.FieldText("RecordAptField");
        var aptLabel = Root.Q<Label>("ReviewApt");
        aptLabel.text = apt == "" ? "None" : apt;
        aptLabel.EnableInClassList("info-table__placeholder", apt == "");

        // TODO: get the wait time from the backend. If it is over 60 minutes for a delivery:
        // ShowError("Distance is too far. Time must be 60 minutes or under")
        Root.Q<Label>("ReviewWaitTime").text = "30 mins";
        ShowPanel("ReviewPanel", "DeliveryPanel", "PlacedPanel");
    }

    void PlaceOrder()
    {
        // TODO: send the order (cart, customer, receipt choices) to the backend.
        Root.Q<Label>("PlacedWaitTime").text = Root.Q<Label>("ReviewWaitTime").text;
        ShowPanel("PlacedPanel", "DeliveryPanel", "ReviewPanel");
        Root.SetHidden("BackButton", true);
    }

    // ---------------- helpers ----------------

    static RadioButton SelectedRadio(VisualElement container) =>
        container?.Query<RadioButton>().ToList().FirstOrDefault(r => r.value);

    static List<Toggle> CheckedToggles(VisualElement container) =>
        container.Query<Toggle>().ToList().Where(t => t.value).ToList();

    // Price shown next to an option ("7$" -> 7).
    static int Price(VisualElement option)
    {
        var text = option.parent?.Q<Label>(className: "opt__price")?.text ?? "";
        return int.TryParse(Regex.Replace(text, "[^0-9]", ""), out var price) ? price : 0;
    }

    static void ClearSelections(VisualElement container)
    {
        foreach (var r in container.Query<RadioButton>().ToList()) r.value = false;
        foreach (var t in container.Query<Toggle>().ToList()) t.value = false;
    }

    static string Capitalize(string s) => s.Length == 0 ? s : char.ToUpper(s[0]) + s.Substring(1);

    static Label NewLabel(string text, string className)
    {
        var label = new Label(text);
        label.AddToClassList(className);
        return label;
    }

    static Button NewButton(string text, string className, System.Action onClick)
    {
        var button = new Button(onClick) { text = text };
        button.AddToClassList(className);
        return button;
    }
}
