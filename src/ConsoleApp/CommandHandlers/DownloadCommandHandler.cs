using System.Net;
using Amazon.S3;
using Amazon.S3.Model;
using ConsoleApp.Commands;
using ConsoleApp.Exceptions;
using ConsoleApp.Objects;

namespace ConsoleApp.CommandHandlers;

public sealed class DownloadCommandHandler(IAmazonS3 amazonS3, Config config) : ICommandHandler<DownloadCommand>
{
    public async Task HandleAsync(DownloadCommand command, CancellationToken cancellationToken)
    {
        var listObjectsRequest = new ListObjectsRequest
        {
            BucketName = config.BucketName,
            Prefix = $"{command.Album}/"
        };

        var path = Utils.GetFullPath(command.Path);

        await Utils.EnsureAlbumExistsAsync(amazonS3, config, command.Album, cancellationToken);

        var results = await amazonS3.Paginators
            .ListObjects(listObjectsRequest).S3Objects
            .SelectAwait(async (obj, i) => await TryProcessImageAsync(i, obj.Key, command.Verbose, path, cancellationToken))
            .ToListAsync(cancellationToken);

        await Console.Out.WriteLineAsync($"Finished. Files processed {results.Count(r => r)}/{results.Count}");
    }

    private async Task<bool> TryProcessImageAsync(int index, string key, bool verbose, string path, CancellationToken cancellationToken)
    {
        var request = new GetObjectRequest
        {
            BucketName = config.BucketName,
            Key = key
        };
        
        var response = await amazonS3.GetObjectAsync(request, cancellationToken);
        var name = response.Key.Split("/")[1];
        var pathToFile = Path.Combine(path, name);
        
        if (response.HttpStatusCode != HttpStatusCode.OK)
        {
            if (verbose)
            {
                await Console.Error.WriteLineAsync($"File {index} failed: while getting from cloud. Actual status code: {response.HttpStatusCode}");
            }
            
            return false;
        }

        try
        {
            await using var fileStream = File.OpenWrite(pathToFile);
            await response.ResponseStream.CopyToAsync(fileStream, cancellationToken);
        }
        catch (Exception)
        {
            if (verbose)
            {
                await Console.Error.WriteLineAsync($"File {index} failed: while saving to directory");
            }
            
            return false;
        }

        if (verbose)
        {
            await Console.Out.WriteLineAsync($"File {index} processed");
        }

        return true;
    }
}