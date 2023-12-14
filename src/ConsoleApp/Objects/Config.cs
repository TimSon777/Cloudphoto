namespace ConsoleApp.Objects;

public sealed class Config
{
    public required string BucketName { get; set; } = default!;
    
    public required string AwsAccessKey { get; set; } = default!;

    public required string AwsSecretKey { get; set; } = default!;

    public required string Region { get; set; } = default!;

    public required string EndpointUrl { get; set; } = default!;
}