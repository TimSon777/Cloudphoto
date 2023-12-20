using Amazon.S3;
using Amazon.S3.Model;
using ConsoleApp.Commands;
using ConsoleApp.Objects;
using Scriban;

namespace ConsoleApp.CommandHandlers;

public class MakeSiteCommandHandler(IAmazonS3 amazonS3, Config config) : ICommandHandler<MakeSiteCommand>
{
    public async Task HandleAsync(MakeSiteCommand command, CancellationToken cancellationToken)
    {
        var putAclRequest = new PutACLRequest
        {
            BucketName = config.BucketName,
            CannedACL = S3CannedACL.PublicRead
        };

        await amazonS3.PutACLAsync(putAclRequest, cancellationToken);

        var putBucketWebsiteRequest = new PutBucketWebsiteRequest
        {
            BucketName = config.BucketName,
            WebsiteConfiguration = new WebsiteConfiguration
            {
                IndexDocumentSuffix = "index.html",
                ErrorDocument = "error.html"
            }
        };

        await amazonS3.PutBucketWebsiteAsync(putBucketWebsiteRequest, cancellationToken);

        var listObjectsV2Request = new ListObjectsV2Request
        {
            BucketName = config.BucketName,
            Delimiter = "/"
        };

        var directories = new List<string>();
        ListObjectsV2Response response;
        do
        {
            response = await amazonS3.ListObjectsV2Async(listObjectsV2Request, cancellationToken);
            directories.AddRange(response.CommonPrefixes);
            listObjectsV2Request.ContinuationToken = response.NextContinuationToken;
        } while (response.IsTruncated);

        var albumBody = await File.ReadAllTextAsync($"{Constants.HtmlTemplatesDirectory}album.html", cancellationToken);
        var albumTemplate = Template.Parse(albumBody);

        var albumTasks = directories
            .Select(async (directory, i) => 
            {
                var listObjectsRequest = new ListObjectsRequest
                {
                    BucketName = config.BucketName,
                    Prefix = directory
                };

                var listObjectsResponse = await amazonS3.ListObjectsAsync(listObjectsRequest, cancellationToken);

                var photos = listObjectsResponse.S3Objects
                    .Select(obj => obj.Key)
                    .Where(key => key.Split("/", 2, StringSplitOptions.RemoveEmptyEntries).Length != 1)
                    .Select(key =>
                    {
                        var uri = new Uri($"https://{config.BucketName}.website.yandexcloud.net/{key}");
                        return new Photo(uri, key.Split("/", 2)[1]);
                    })
                    .ToArray();

                var albumHtml = await albumTemplate.RenderAsync(new { AlbumName = directory[..^1], Photos = photos });

                var albumKey = $"album{i}.html";
                var albumPutObjectRequest = new PutObjectRequest
                {
                    BucketName = config.BucketName,
                    Key = albumKey,
                    ContentBody = albumHtml
                };

                await amazonS3.PutObjectAsync(albumPutObjectRequest, cancellationToken);

                var uri = new Uri($"https://{config.BucketName}.website.yandexcloud.net/album{i}.html");
                return new Album(uri, directory[..^1]);
            })
            .ToArray();

        var albums = await Task.WhenAll(albumTasks);

        var body = await File.ReadAllTextAsync($"{Constants.HtmlTemplatesDirectory}index.html", cancellationToken);

        var indexTemplate = Template.Parse(body);
        var indexHtml = await indexTemplate.RenderAsync(new { Albums = albums });

        var putObjectRequest = new PutObjectRequest
        {
            Key = "index.html",
            BucketName = config.BucketName,
            ContentBody = indexHtml
        };

        await amazonS3.PutObjectAsync(putObjectRequest, cancellationToken);

        await Console.Out.WriteLineAsync($"https://{config.BucketName}.website.yandexcloud.net/");
    }
}