namespace MsixvcPackageDownloader;

/// <summary>
/// Configuration constants for the MSIX package downloader application.
/// </summary>
public static class Configuration
{
    /// <summary>
    /// The filename for storing authentication tokens.
    /// </summary>
    public const string TokenFilename = "token.json";
    
    /// <summary>
    /// The filename for storing authentication URL for development purposes.
    /// </summary>
    public const string AuthUrlFilename = "authUrl.txt";
    
    /// <summary>
    /// The Xbox Live client ID used for authentication.
    /// </summary>
    public const string XboxLiveClientId = "00000000402b5328";
    
    /// <summary>
    /// The base URL for the Xbox Live package service.
    /// </summary>
    public const string PackageServiceBaseUrl = "https://packagespc.xboxlive.com/GetBasePackage/";
    
    /// <summary>
    /// The Xbox Live authentication service URL.
    /// </summary>
    public const string XstsAuthUrl = "https://xsts.auth.xboxlive.com";
    
    /// <summary>
    /// The Xbox Live update service audience.
    /// </summary>
    public const string UpdateServiceAudience = "http://update.xboxlive.com";
    
    /// <summary>
    /// The default refresh token value used for authentication.
    /// </summary>
    public const string DefaultRefreshToken = "thisisunused";
    
    /// <summary>
    /// File extensions to exclude from package processing.
    /// </summary>
    public static readonly string[] ExcludedFileExtensions = [".phf", ".xsp"];
    
    /// <summary>
    /// Gets the full path to the token file.
    /// </summary>
    public static string TokenPath => Path.Join(AppContext.BaseDirectory, TokenFilename);
}