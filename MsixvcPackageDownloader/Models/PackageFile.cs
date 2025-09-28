namespace MsixvcPackageDownloader.Models;

public class PackageFile : BasePackageFile
{
    public Guid ContentId { get; set; }
    public required string VersionId { get; set; }
    public required string FileName { get; set; }
    public ulong FileSize { get; set; }
    public required string FileHash { get; set; }
    public required string KeyBlob { get; set; }
    public required List<string> CdnRootPaths { get; set; }
    public required List<string> BackgroundCdnRootPaths { get; set; }
    // RelativeUrl is inherited from BasePackageFile and doesn't need to be redeclared
    public uint UpdateType { get; set; } // TODO: I think this is an enum - find out all values
    public Guid? DeltaVersionId { get; set; }
    public uint LicenseUsageType { get; set; }
    public ulong Clock { get; set; }
    public DateTime ModifiedDate { get; set; }
}