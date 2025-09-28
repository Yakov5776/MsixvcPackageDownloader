namespace MsixvcPackageDownloader.Models;

public class GetBasePackageResponse
{
    public bool PackageFound { get; set; }
    public Guid ContentId { get; set; }
    public required string VersionId { get; set; }
    public required List<PackageFile> PackageFiles { get; set; }
    public required string Version { get; set; }
    public required PackageMetadata PackageMetadata { get; set; }
    public required string HashOfHashes { get; set; }
}