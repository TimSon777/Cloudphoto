using Amazon.S3;
using CommandLine;
using ConsoleApp.CommandHandlers;
using ConsoleApp.Commands;

var cts = new CancellationTokenSource();
var result = Parser.Default.ParseArguments<InitCommand, VersionCommand>(args);

try
{
    await result.WithParsedAsync<VersionCommand>(async command =>
        await new VersionCommandHandler().HandleAsync(command, cts.Token));
    await result.WithParsedAsync<InitCommand>(async command =>
        await new InitCommandHandler().HandleAsync(command, cts.Token));
}
catch (AmazonS3Exception ex)
    when (ex.Message ==
          "The request signature we calculated does not match the signature you provided. Check your key and signing method.")
{
    await Console.Error.WriteLineAsync("Authentication failed. You may have entered the wrong credentials");
}
catch (AmazonS3Exception ex)
    when (ex.Message == "Access Denied")
{
    await Console.Error.WriteLineAsync("Credentials don't have sufficient rights");
}
