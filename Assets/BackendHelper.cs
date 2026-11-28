using System;
using System.Collections;
using System.Text;
using Unity.VisualScripting.AssemblyQualifiedNameParser;
using UnityEngine;
using UnityEngine.Networking;

// Fix for records (from the internet)
namespace System.Runtime.CompilerServices
{
    internal static class IsExternalInit { }
}

public class Backend : MonoBehaviour
{
    [SerializeField] string backendURL;

    static string BackendURL;

    private void Awake()
    {
        BackendURL = backendURL;

        EmployeeAPI.sectionURL = $"{backendURL}/employee";
    }

    public static IEnumerator Get<TRes>(string url, Action<TRes> onDone, Action<string> onError = null)
    {
        using var req = UnityWebRequest.Get(url);
        yield return req.SendWebRequest();

        if (req.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError($"{req.responseCode} {req.error}");
            onError?.Invoke(req.error);
            yield break;
        }

        var parsed = JsonUtility.FromJson<TRes>(req.downloadHandler.text);

        onDone.Invoke(parsed);
    }

    public static IEnumerator Post<TReq, TRes>(string url, TReq data, Action<TRes> onDone, Action<string> onError = null)
    {
        string json = JsonUtility.ToJson(data);

        using var req = UnityWebRequest.Post(url, json, "application/json");
        req.timeout = 10;

        yield return req.SendWebRequest();

        if (req.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError($"{req.responseCode} {req.error}\n{req.downloadHandler.text}");
            onError?.Invoke(null);
            yield break;
        }

        var parsed = JsonUtility.FromJson<TRes>(req.downloadHandler.text);

        onDone.Invoke(parsed);
    }

    public static IEnumerator Put<TReq, TRes>(string url, TReq data, Action<TRes> onDone, Action<string> onError = null)
    {
        string json = JsonUtility.ToJson(data);

        byte[] bytes = Encoding.UTF8.GetBytes(json);

        using var req = UnityWebRequest.Put(url, json);
        req.SetRequestHeader("Content-Type", "application/json");

        yield return req.SendWebRequest();

        if (req.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError($"{req.responseCode} {req.error}\n{req.downloadHandler.text}");
            onError?.Invoke(null);
            yield break;
        }

        var parsed = JsonUtility.FromJson<TRes>(req.downloadHandler.text);

        onDone.Invoke(parsed);
    }

    public static IEnumerator Delete<TRes>(string url, Action<TRes> onDone, Action<string> onError = null)
    {
        using var req = UnityWebRequest.Delete(url);

        yield return req.SendWebRequest();

        if (req.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError($"{req.responseCode} {req.error}");
            onError?.Invoke(null);
            yield break;
        }

        var parsed = JsonUtility.FromJson<TRes>(req.downloadHandler.text);

        onDone.Invoke(parsed);
    }
}

public static class EmployeeAPI
{
    public static string sectionURL;

    public static IEnumerator Get(int EmployeeID, Action<EmployeeRecord> onDone, Action<string> onError = null) =>
        Backend.Get($"{sectionURL}/{EmployeeID}", onDone, onError);

    public static IEnumerator Post(EmployeeRecord data, Action<EmployeeRecord> onDone, Action<string> onError = null) =>
        Backend.Post(sectionURL, data, onDone, onError);

    public static IEnumerator Put(int EmployeeID, EmployeeRecord data, Action<EmployeeRecord> onDone, Action<string> onError = null) =>
        Backend.Put($"{sectionURL}/{EmployeeID}", data, onDone, onError);

    public static IEnumerator Delete(int EmployeeID, Action<string> onDone, Action<string> onError = null) =>
        Backend.Delete($"{sectionURL}/{EmployeeID}", onDone, onError);
}
