namespace ConsoleApp;

public static class Utils
{
    public static string GetPathToConfigFile(bool includeFileName)
    {
        var homePath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        return includeFileName ? $"{homePath}/.config/cloudphoto/cloudphotorc" : $"{homePath}/.config/cloudphoto";
    }
}