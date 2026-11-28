using System;
using UnityEngine.UIElements;

// "ADD NEW ITEM" / "EDIT ITEM" popup from Manage Menu (mockup slides 97-108).
public class MenuItemModal : UIModal
{
    public class Item
    {
        public string Name, Type, Sizes, Cost;
    }

    bool adding;
    Action<Item> onSaved;

    protected override void OnBind(VisualElement root)
    {
        root.OnClick("SubmitButton", Submit);
        root.OnClick("CloseButton", Close);
        root.OnClick("RemoveImageButton", () => SetImage(null));
    }

    // onSaved is called with the new item after "Create new item".
    public void OpenAdd(Action<Item> onSaved)
    {
        adding = true;
        this.onSaved = onSaved;
        Fill(new Item(), "ADD NEW ITEM", "Create new item");
    }

    // onSaved is called with the changed item after "Modify item".
    public void OpenEdit(Item item, Action<Item> onSaved)
    {
        adding = false;
        this.onSaved = onSaved;
        // The table shows "N/A" for items without sizes; the field is left empty instead.
        Fill(new Item { Name = item.Name, Type = item.Type, Sizes = item.Sizes == "N/A" ? "" : item.Sizes, Cost = item.Cost },
            "EDIT ITEM", "Modify item");
    }

    void Fill(Item item, string title, string submitText)
    {
        Root.Q<Label>("ModalTitle").text = title;
        Root.Q<Button>("SubmitButton").text = submitText;
        Root.SetField("ItemNameField", item.Name);
        Root.SetField("ItemTypeField", item.Type);
        Root.SetField("SizesField", item.Sizes);
        Root.SetField("CostField", item.Cost);
        // TODO: load the item's picture from the backend.
        SetImage(null);
        Open();
    }

    void Submit()
    {
        var item = new Item
        {
            Name = Root.FieldText("ItemNameField"),
            Type = Root.FieldText("ItemTypeField"),
            Sizes = Root.FieldText("SizesField"),
            Cost = Root.FieldText("CostField"),
        };

        if (item.Name == "" || item.Type == "" || item.Cost == "")
        {
            UIManager.Instance.ShowError("One or more fields are not present. Ensure all fields are filled before creating an item");
            return;
        }
        // TODO: if a picture was dropped on a type that can't have one, show
        // ShowError("Image not allowed on this item type! Remove image or change item type.")

        if (item.Sizes == "") item.Sizes = "N/A";

        // TODO: save the item with the backend.
        Close();
        onSaved?.Invoke(item);
        UIManager.Instance.ShowSuccess(adding ? "Item successfully added" : "Item successfully modified");
    }

    // TODO: call this with the picture the user drops on "ImageDropZone".
    public void SetImage(UnityEngine.Texture2D image)
    {
        var box = Root.Q("ItemImage");
        box.style.backgroundImage = image == null ? StyleKeyword.None : new StyleBackground(image);
        Root.SetHidden("DropHint", image != null);
        Root.SetHidden("RemoveImageButton", image == null);
    }
}
