using Amazon.S3;
using Amazon.S3.Model;
using ConsoleApp.Commands;
using ConsoleApp.Exceptions;
using ConsoleApp.Objects;

namespace ConsoleApp.CommandHandlers;

public sealed class ListCommandHandler(IAmazonS3 amazonS3, Config config) : ICommandHandler<ListCommand>
{
    public async Task HandleAsync(ListCommand command, CancellationToken cancellationToken)
    {
        if (command.Album is null)
        {
            await HandleAlbumsAsync(cancellationToken);
        }
        else
        {
            await HandlePhotosAsync(command.Album, cancellationToken);
        }
    }

    private async Task HandleAlbumsAsync(CancellationToken cancellationToken)
    {
        var request = new ListObjectsV2Request
        {
            BucketName = config.BucketName,
            Delimiter = "/"
        };

        var directories = new List<string>();
        ListObjectsV2Response response;
        do
        {
            response = await amazonS3.ListObjectsV2Async(request, cancellationToken);
            directories.AddRange(response.CommonPrefixes);
            request.ContinuationToken = response.NextContinuationToken;
        } while (response.IsTruncated);

        if (directories.Count == 0)
        {
            throw new NoObjectsException();
        }

        for (var i = 0; i < directories.Count; i++)
        {
            await Console.Out.WriteLineAsync($"{i}. {directories[i]}");
        }
    }

    private async Task HandlePhotosAsync(string album, CancellationToken cancellationToken)
    {
        var request = new ListObjectsRequest
        {
            BucketName = config.BucketName,
            Prefix = $"{album}/"
        };

        var names = await amazonS3.Paginators
            .ListObjects(request).S3Objects
            .Select(x => x.Key.Split("/")[1])
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .ToListAsync(cancellationToken);

        if (names.Count == 0)
        {
            throw new NoObjectsException();
        }

        for (var i = 0; i < names.Count; i++)
        {
            await Console.Out.WriteLineAsync($"{i}. {names[i]}");
        }
    }
}