using System;

[Serializable]
public class EmployeeRecord
{
    public int ID;
    public string Role;
    public string Email;
    public string Password;
    public string Name;
    public string[] Contact;
}

[Serializable]
public class CustomerRecord
{
    public string Phone;
    public string Name;
    public string ChargeAccount;
    public AddressRecord Address;
}

[Serializable]
public class AddressRecord
{
    public int ID;
    public string State;
    public AddressInfoRecord AddressInfo;
    public string StreetAddress;
    public string City;
    public int ZipCode;
}

[Serializable]
public class AddressInfoRecord
{
    public string ClosestIntersection;
    public string Subdivision;
}

[Serializable]
public class Order
{
    public int ID;
    public string OrderDate;
    public string FulfillmentType;
    public float DeliveryFee;
    public float TotalPrice;
    public int UserID;
    public string PaymentID;
}
