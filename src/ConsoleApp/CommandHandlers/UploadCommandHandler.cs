using Amazon.S3;
using Amazon.S3.Model;
using ConsoleApp.Commands;
using ConsoleApp.Exceptions;
using ConsoleApp.Objects;

namespace ConsoleApp.CommandHandlers;

public sealed class UploadCommandHandler(IAmazonS3 amazonS3, Config config) : ICommandHandler<UploadCommand>
{
    public async Task HandleAsync(UploadCommand command, CancellationToken cancellationToken)
    {
        var path = Utils.GetFullPath(command.Path);

        var photos = new DirectoryInfo(path)
            .GetFiles("*.*", SearchOption.TopDirectoryOnly)
            .Where(f => f.Extension.Equals(".jpg", StringComparison.OrdinalIgnoreCase)
                        || f.Extension.Equals(".jpeg", StringComparison.OrdinalIgnoreCase))
            .ToArray();

        if (photos.Length == 0)
        {
            throw new NoPhotosException();
        }

        await using (var _ = File.OpenWrite(photos[0].FullName))
        {
        }

        var tasks = photos
            .Select(async (image, index) => await ProcessImageAsync(image, index, command.Verbose, command.Album))
            .ToArray();

        var results = await Task.WhenAll(tasks);

        await Console.Out.WriteLineAsync($"{results.Count(r => r)}/{results.Length} photos uploaded");
    }

    private async Task<bool> ProcessImageAsync(FileSystemInfo image, int index, bool verbose, string album)
    {
        try
        {
            var request = new PutObjectRequest
            {
                Key = $"{album}/{image.Name}",
                BucketName = config.BucketName,
                FilePath = image.FullName
            };
            
            await amazonS3.PutObjectAsync(request);

            if (verbose)
            {
                await Console.Out.WriteLineAsync($"{index}. {image.Name} processed");
            }

            return true;
        }
        catch (Exception ex)
        {
            if (verbose)
            {
                await Console.Out.WriteLineAsync($"{index}. {image.Name} doesn't processed ({ex.Message})");
            }

            return false;
        }
    }
}