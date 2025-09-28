namespace MsixvcPackageDownloader.Models;

public class PackageMetadata
{
    public required List<string> CdnRoots { get; set; }
    public required List<string> BackgroundCdnRootPaths { get; set; }
    public required List<PackageMetadataFile> Files { get; set; }
    public ulong EstimatedTotalDownloadSize { get; set; }
}