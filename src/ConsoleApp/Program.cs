using Amazon.S3;
using CommandLine;
using ConsoleApp;
using ConsoleApp.CommandHandlers;
using ConsoleApp.Commands;
using ConsoleApp.Exceptions;
using ConsoleApp.Objects;

var cts = new CancellationTokenSource();

var code = await HandleAsync(args, cts.Token);

await Console.Out.WriteLineAsync($"Exit code: {code}");
Environment.Exit(code);
return code;

static async Task<int> HandleAsync(IEnumerable<string> args, CancellationToken cancellationToken)
{
    var result = Parser.Default
        .ParseArguments<InitCommand, DownloadCommand, UploadCommand, DeleteCommand, MakeSiteCommand,
            ListCommand>(args);

    try
    {
        await result.WithParsedAsync<InitCommand>(async command =>
            await new InitCommandHandler().HandleAsync(command, cancellationToken));

        if (result.Value is not InitCommand)
        {
            var configExists = await Utils.IsConfigExistsAsync(cancellationToken);
            if (!configExists)
            {
                await Console.Error.WriteLineAsync(
                    "Config file wasn't found. Please, use command `cloudphoto init` to set it");

                return Code.Failure;
            }

            var (config, getConfigResult) = await Utils.TryGetConfigAsync(cancellationToken);

            switch (getConfigResult)
            {
                case GetConfigResult.Success:
                    var amazonS3 = new AmazonS3Client(config!.AwsAccessKey, config.AwsSecretKey, new AmazonS3Config
                    {
                        ServiceURL = config.EndpointUrl,
                        AuthenticationRegion = config.Region
                    });

                    await result.WithParsedAsync<UploadCommand>(async command =>
                        await new UploadCommandHandler(amazonS3, config).HandleAsync(command, cancellationToken));
                    await result.WithParsedAsync<DownloadCommand>(async command =>
                        await new DownloadCommandHandler(amazonS3, config).HandleAsync(command, cancellationToken));
                    await result.WithParsedAsync<ListCommand>(async command =>
                        await new ListCommandHandler(amazonS3, config).HandleAsync(command, cancellationToken));
                    await result.WithParsedAsync<DeleteCommand>(async command =>
                        await new DeleteCommandHandler(amazonS3, config).HandleAsync(command, cancellationToken));
                    await result.WithParsedAsync<MakeSiteCommand>(async command =>
                        await new MakeSiteCommandHandler(amazonS3, config).HandleAsync(command, cancellationToken));
                    break;
                case GetConfigResult.WrongProfile:
                    await Console.Error.WriteLineAsync(
                        "Profile has wrong format. Now supports only `[DEFAULT]` profile. Please, use `cloudphoto init` command to reconfigure it");
                    return Code.Failure;
                case GetConfigResult.WrongCountLines:
                    await Console.Error.WriteLineAsync(
                        $"Not all options are presented:\n{string.Join("\n", Constants.ConfigOptions)}\nPlease, use `cloudphoto init` command to reconfigure it");
                    return Code.Failure;
                case GetConfigResult.WrongOptions:
                    await Console.Error.WriteLineAsync(
                        "Your configuration file is corrupted. Please, use `cloudphoto init` command to reconfigure it");
                    return Code.Failure;
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
        return Code.Failure;
    }
    catch (AmazonS3Exception ex)
        when (ex.Message == "Access Denied")
    {
        await Console.Error.WriteLineAsync(
            "Credentials don't have rights. Make sure your account has full rights to manage the bucket");
        return Code.Failure;
    }
    catch (DirectoryNotFoundException)
    {
        await Console.Error.WriteLineAsync("Directory not found. Check options, where you set some paths");
        return Code.Failure;
    }
    catch (UnauthorizedAccessException)
    {
        await Console.Error.WriteLineAsync("Lack of permission to access the directory / file");
        return Code.Failure;
    }
    catch (AmazonS3Exception ex)
        when (ex.Message == "The specified key does not exist.")
    {
        await Console.Error.WriteLineAsync("There is no such file or directory in the cloud");
        return Code.Failure;
    }
    catch (NoPhotosException)
    {
        await Console.Error.WriteLineAsync("Photo(s) not found");
        return Code.Failure;
    }
    catch (NoAlbumException)
    {
        await Console.Error.WriteLineAsync("Album(s) not found");
        return Code.Failure;
    }
    catch (Exception ex)
    {
        await Console.Error.WriteLineAsync($"Unknown error. Internal message: {ex.Message}");
        return Code.Failure;
    }

    return result.Errors.All(e => Constants.SupportedErrors.Contains(e.ToString()!))
        ? Code.Success
        : Code.Failure;
}