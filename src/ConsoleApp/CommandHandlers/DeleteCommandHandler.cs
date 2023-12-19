using Amazon.S3;
using Amazon.S3.Model;
using ConsoleApp.Commands;
using ConsoleApp.Exceptions;
using ConsoleApp.Objects;

namespace ConsoleApp.CommandHandlers;

public sealed class DeleteCommandHandler(IAmazonS3 amazonS3, Config config) : ICommandHandler<DeleteCommand>
{
    public async Task HandleAsync(DeleteCommand command, CancellationToken cancellationToken)
    {
        if (command.Photo is null)
        {
            await HandleAlbumAsync(command.Album, cancellationToken);
        }
        else
        {
            await HandlePhotoAsync($"{command.Album}/{command.Photo}", cancellationToken);
        }
    }

    private async Task HandleAlbumAsync(string album, CancellationToken cancellationToken)
    {
        var listObjectsV2Request = new ListObjectsV2Request
        {
            BucketName = config.BucketName,
            Prefix = $"{album}/"
        };

        
        ListObjectsV2Response listObjectsV2Response;
        var count = 0;
        do
        {
            listObjectsV2Response = await amazonS3.ListObjectsV2Async(listObjectsV2Request, cancellationToken);
            var deleteObjectsRequest = new DeleteObjectsRequest
            {
                BucketName = config.BucketName,
                Objects = listObjectsV2Response.S3Objects.Select(x => new KeyVersion { Key = x.Key }).ToList()
            };

            count += listObjectsV2Response.S3Objects.Count;

            await amazonS3.DeleteObjectsAsync(deleteObjectsRequest, cancellationToken);
            listObjectsV2Request.ContinuationToken = listObjectsV2Response.NextContinuationToken;
        } while (listObjectsV2Response.IsTruncated);

        if (count == 0)
        {
            throw new NoObjectsException();
        }

        await Console.Out.WriteLineAsync($"{count} files deleted");
    }

    private async Task HandlePhotoAsync(string key, CancellationToken cancellationToken)
    {
        var getObjectRequest = new GetObjectRequest
        {
            BucketName = config.BucketName,
            Key = key
        };

        var _ = await amazonS3.GetObjectAsync(getObjectRequest, cancellationToken);

        var deleteObjectRequest = new DeleteObjectRequest
        {
            BucketName = config.BucketName,
            Key = key
        };

        await amazonS3.DeleteObjectAsync(deleteObjectRequest, cancellationToken);
    }
}