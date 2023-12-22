namespace ConsoleApp;

public static class Constants
{
    public const string S3EndpointUrl = "https://storage.yandexcloud.net";
    public const string Region = "ru-central1";

    public static readonly string[] ConfigOptions =
    [
        "bucket",
        "aws_access_key_id",
        "aws_secret_access_key",
        "region",
        "endpoint_url"
    ];

    public static readonly string HtmlTemplatesDirectory = $@"{AppContext.BaseDirectory}Resources\HtmlTemplates\";

    public static readonly string[] SupportedErrors =
    [
        "CommandLine.HelpRequestedError",
        "CommandLine.HelpVerbRequestedError",
        "CommandLine.VersionRequestedError"
    ];
}