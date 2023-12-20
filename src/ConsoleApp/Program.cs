using Amazon.S3;
using CommandLine;
using ConsoleApp;
using ConsoleApp.CommandHandlers;
using ConsoleApp.Commands;
using ConsoleApp.Exceptions;
using ConsoleApp.Objects;

var cts = new CancellationTokenSource();
var result = Parser.Default.ParseArguments<InitCommand, VersionCommand, DownloadCommand, UploadCommand, DeleteCommand, MakeSiteCommand, ListCommand>(args);

try
{
    await result.WithParsedAsync<InitCommand>(async command =>
        await new InitCommandHandler().HandleAsync(command, cts.Token));
    await result.WithParsedAsync<VersionCommand>(async command =>
        await new VersionCommandHandler().HandleAsync(command, cts.Token));

    if (result.Value is not InitCommand && result.Value is not VersionCommand)
    {
        var configExists = await Utils.IsConfigExistsAsync(cts.Token);
        if (!configExists)
        {
            await Console.Error.WriteLineAsync(
                "Config file wasn't found. Please, use command `cloudphoto init` to set it");

            return (int)Code.ConfigFileNotFound;
        }

        var (config, getConfigResult) = await Utils.TryGetConfigAsync(cts.Token);

        switch (getConfigResult)
        {
            case GetConfigResult.Success:
                var amazonS3 = new AmazonS3Client(config!.AwsAccessKey, config.AwsSecretKey, new AmazonS3Config
                {
                    ServiceURL = config.EndpointUrl,
                    AuthenticationRegion = config.Region
                });

                await result.WithParsedAsync<UploadCommand>(async command =>
                    await new UploadCommandHandler(amazonS3, config).HandleAsync(command, cts.Token));
                await result.WithParsedAsync<DownloadCommand>(async command =>
                    await new DownloadCommandHandler(amazonS3, config).HandleAsync(command, cts.Token));
                await result.WithParsedAsync<ListCommand>(async command =>
                    await new ListCommandHandler(amazonS3, config).HandleAsync(command, cts.Token));
                await result.WithParsedAsync<DeleteCommand>(async command =>
                    await new DeleteCommandHandler(amazonS3, config).HandleAsync(command, cts.Token));
                await result.WithParsedAsync<MakeSiteCommand>(async command =>
                    await new MakeSiteCommandHandler(amazonS3, config).HandleAsync(command, cts.Token));
                break;
            case GetConfigResult.WrongProfile:
                await Console.Error.WriteLineAsync(
                    "Profile has wrong format. Now supports only `[DEFAULT]` profile. Please, use `cloudphoto init` command to reconfigure it");
                return (int)Code.WrongProfile;
            case GetConfigResult.WrongCountLines:
                await Console.Error.WriteLineAsync(
                    $"Not all options are presented:\n{string.Join("\n", Constants.ConfigOptions)}\nPlease, use `cloudphoto init` command to reconfigure it");
                return (int)Code.WrongCountLines;
            case GetConfigResult.WrongOptions:
                await Console.Error.WriteLineAsync(
                    "Your configuration file is corrupted. Please, use `cloudphoto init` command to reconfigure it");
                return (int)Code.WrongOptions;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}
catch (AmazonS3Exception ex)
    when (ex.Message ==
          "The request signature we calculated does not match the signature you provided. Check your key and signing method.")
{
    await Console.Error.WriteLineAsync("Authentication failed. You may have entered the wrong credentials");
    return (int)Code.InvalidCredentials;
}
catch (AmazonS3Exception ex)
    when (ex.Message == "Access Denied")
{
    await Console.Error.WriteLineAsync("Credentials don't have sufficient rights");
    return (int)Code.AccessDenied;
}
catch (DirectoryNotFoundException)
{
    await Console.Error.WriteLineAsync("Directory not found");
    return (int)Code.DirectoryDoesntExist;
}
catch (UnauthorizedAccessException)
{
    await Console.Error.WriteLineAsync("Lack of permission to write to the directory");
    return (int)Code.DirectoryAccessDenied;
}
catch (AmazonS3Exception ex)
    when (ex.Message == "The specified key does not exist.")
{
    await Console.Error.WriteLineAsync("File / Directory doesn't exist");
    return (int)Code.YandexCloudDirectoryDoesntExist;
}
catch (NoObjectsException)
{
    await Console.Error.WriteLineAsync("Objects were not found.");
    return (int)Code.ObjectsNotFound;
}

return (int)Code.Success;