using Amazon.S3;
using Amazon.S3.Model;
using ConsoleApp.Commands;
using ConsoleApp.Objects;

namespace ConsoleApp.CommandHandlers;

public sealed class UploadCommandHandler(IAmazonS3 amazonS3, Config config) : ICommandHandler<UploadCommand>
{
    public async Task HandleAsync(UploadCommand command, CancellationToken cancellationToken)
    {
        var path = Path.Combine(Environment.CurrentDirectory, command.Path);
        var directory = new DirectoryInfo(path);

        var tasks = directory
            .GetFiles("*.*", SearchOption.TopDirectoryOnly)
            .Where(f => f.Extension.Equals(".jpg", StringComparison.OrdinalIgnoreCase)
                || f.Extension.Equals(".jpeg", StringComparison.OrdinalIgnoreCase))
            .Select(async image => await ProcessImageAsync(image, command.Album));

        await Task.WhenAll(tasks);
    }

    private async Task ProcessImageAsync(FileSystemInfo image, string album)
    {
        var request = new PutObjectRequest
        {
            Key = $"{album}/{image.Name}",
            BucketName = config.BucketName,
            FilePath = image.FullName
        };

        await amazonS3.PutObjectAsync(request);
    }
}