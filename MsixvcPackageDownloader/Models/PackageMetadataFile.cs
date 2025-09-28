namespace MsixvcPackageDownloader.Models;

public class PackageMetadataFile : BasePackageFile
{
    public required string Name { get; set; }
    public ulong Size { get; set; }
    public required string License { get; set; }
}