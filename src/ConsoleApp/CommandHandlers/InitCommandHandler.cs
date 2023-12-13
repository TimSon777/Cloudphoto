using Amazon.S3;
using Amazon.S3.Model;
using ConsoleApp.Commands;

namespace ConsoleApp.CommandHandlers;

public sealed class InitCommandHandler : ICommandHandler<InitCommand>
{
    public async Task HandleAsync(InitCommand command, CancellationToken cancellationToken)
    {
        var accessKey = await ReadParamAsync("Access Key");
        var secretKey = await ReadParamAsync("Secret Key");

        var amazonS3 = new AmazonS3Client(accessKey, secretKey, new AmazonS3Config
        {
            ServiceURL = Constants.S3EndpointUrl,
            AuthenticationRegion = Constants.Region
        });

        await Console.Out.WriteLineAsync("Wait, checking credentials");

        var listBucketsResponse = await amazonS3.ListBucketsAsync(cancellationToken);

        string bucketName;

        while (true)
        {
            bucketName = await ReadParamAsync("Bucket Name");

            try
            {
                if (listBucketsResponse.Buckets.Any(b => b.BucketName == bucketName))
                {
                    await Console.Out.WriteLineAsync("Linking a program to a bucket occurs successfully");
                }
                else
                {
                    await amazonS3.PutBucketAsync(bucketName, cancellationToken);
                    await Console.Out.WriteLineAsync("The bucket was successfully created and linked to the program");
                }
                
                break;
            }
            catch (BucketAlreadyExistsException)
            {
                await Console.Error.WriteLineAsync("Bucket already exists. Note, that the bucket namespace is shared by all users of the system. Try again, please");
            }
        }

        var pathToFile = Utils.GetPathToConfigFile(includeFileName: true);
        var pathToDirectory = Utils.GetPathToConfigFile(includeFileName: false);

        if (!Directory.Exists(pathToDirectory))
        {
            Directory.CreateDirectory(pathToDirectory);
        }

        var fileBody =
            $"""
             [DEFAULT]
             bucket = {bucketName}
             aws_access_key_id = {accessKey}
             aws_secret_access_key = {secretKey}
             region = {Constants.Region}
             endpoint_url = {Constants.S3EndpointUrl}
             """;

        await File.WriteAllTextAsync(pathToFile, fileBody, cancellationToken);
    }

    private static async Task<string> ReadParamAsync(string displayParamName)
    {
        await Console.Out.WriteLineAsync($"Enter {displayParamName}");

        string? value;
        var isFirst = true;
        do
        {
            if (!isFirst)
            {
                await Console.Error.WriteLineAsync($"{displayParamName} can't be empty. Please enter a valid value");
            }

            isFirst = false;
            value = await Console.In.ReadLineAsync();
        } while (string.IsNullOrWhiteSpace(value));

        return value;
    }
}