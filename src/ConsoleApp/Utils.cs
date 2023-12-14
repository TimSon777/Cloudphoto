using ConsoleApp.Objects;

namespace ConsoleApp;

public static class Utils
{
    public static string GetPathToConfigFile(bool includeFileName)
    {
        var homePath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        return includeFileName ? $"{homePath}/.config/cloudphoto/cloudphotorc" : $"{homePath}/.config/cloudphoto";
    }

    public static async Task SaveConfigAsync(Config config, CancellationToken cancellationToken)
    {
        var pathToFile = GetPathToConfigFile(includeFileName: true);
        var pathToDirectory = GetPathToConfigFile(includeFileName: false);

        if (!Directory.Exists(pathToDirectory))
        {
            Directory.CreateDirectory(pathToDirectory);
        }

        var fileBody =
            $"""
             [DEFAULT]
             bucket = {config.BucketName}
             aws_access_key_id = {config.AwsAccessKey}
             aws_secret_access_key = {config.AwsSecretKey}
             region = {Constants.Region}
             endpoint_url = {Constants.S3EndpointUrl}
             """;

        await File.WriteAllTextAsync(pathToFile, fileBody, cancellationToken);
    }

    public static Task<bool> IsConfigExistsAsync(CancellationToken _)
    {
        var pathToFile = GetPathToConfigFile(includeFileName: true);
        return Task.FromResult(File.Exists(pathToFile));
    }

    public static async Task<(Config?, GetConfigResult)> TryGetConfigAsync(CancellationToken cancellationToken)
    {
        var pathToFile = GetPathToConfigFile(includeFileName: true);
        var notPreparedLines = await File.ReadAllLinesAsync(pathToFile, cancellationToken);

        var lines = notPreparedLines
            .Where(line => !string.IsNullOrWhiteSpace(line))
            .Select(line => line.Trim())
            .ToList();

        if (lines.Count < 6)
        {
            return (null, GetConfigResult.WrongCountLines);
        }

        if (!lines[0].Equals("[default]", StringComparison.CurrentCultureIgnoreCase))
        {
            return (null, GetConfigResult.WrongProfile);
        }

        try
        {
            var config = new Config
            {
                BucketName = GetValue("bucket"),
                AwsAccessKey = GetValue("aws_access_key_id"),
                AwsSecretKey = GetValue("aws_secret_access_key"),
                Region = GetValue("region"),
                EndpointUrl = GetValue("endpoint_url")
            };

            return (config, GetConfigResult.Success);
        }
        catch (Exception)
        {
            return (null, GetConfigResult.WrongOptions);
        }

        string GetValue(string paramName) => lines.First(line => line.StartsWith(paramName, StringComparison.CurrentCultureIgnoreCase)).Split(" = ")[1];
    }
}