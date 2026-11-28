// Who is signed in. Screens read this to fill in the navbar and to show
// manager-only controls.
// TODO: fill this from the backend's sign-in response (see LogInScreen.SignIn).
public static class Session
{
    public static string Username { get; private set; }
    public static bool IsManager { get; private set; }
    public static bool LoggedIn => !string.IsNullOrEmpty(Username);

    public static void LogIn(string username, bool isManager)
    {
        Username = username;
        IsManager = isManager;
    }

    public static void LogOut()
    {
        Username = null;
        IsManager = false;
    }
}
