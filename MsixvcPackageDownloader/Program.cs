using MsixvcPackageDownloader.Models;
using XboxWebApi.Authentication;
using XboxWebApi.Authentication.Model;
using XboxWebApi.Common;

namespace MsixvcPackageDownloader
{
    /// <summary>
    /// Main program class for the MSIX package downloader application.
    /// This application provides functionality to download MSIX packages from Xbox Live services.
    /// </summary>
    internal class Program
    {
        private static XToken? _updateToken;

        /// <summary>
        /// Main entry point for the application.
        /// This is just a POC for this endpoint.
        /// </summary>
        /// <param name="args">Command line arguments. If provided, the first argument should be a ContentId for CLI mode.</param>
        static async Task Main(string[] args)
        {
            // Check for help request
            if (args.Length > 0 && (args[0] == "--help" || args[0] == "-h" || args[0] == "help"))
            {
                DisplayHelp();
                return;
            }

            bool cliMode = args.Length > 0;
            if (!cliMode)
                Console.WriteLine("Initializing...");

            AuthenticationService? authService = null;

            if (File.Exists(Configuration.TokenPath))
            {
                authService = await AuthenticationService.LoadFromJsonFileAsync(Configuration.TokenPath);
                if (authService.XToken?.Valid != true)
                {
                    if (!cliMode)
                        Console.WriteLine("Token expired, please reauthenticate!");
                    authService = null;
                }
            }

            if (authService == null)
            {
                var requestUrl =
                    AuthenticationService.GetWindowsLiveAuthenticationUrl(
                        new WindowsLiveAuthenticationQuery(clientId: Configuration.XboxLiveClientId));

                if (!cliMode)
                {
                    Console.WriteLine(
                        "Please sign-in at this url in your browser, then paste the resulting URL back into this window and press enter.");
                    Console.WriteLine($"Url: {requestUrl}");
                }

                var resultingUrl = File.Exists(Configuration.AuthUrlFilename) 
                    ? File.ReadAllText(Configuration.AuthUrlFilename) 
                    : Console.ReadLine();
                
                if (string.IsNullOrEmpty(resultingUrl))
                    return;

                if (!resultingUrl.Contains("refresh_token"))
                    resultingUrl += $"&refresh_token={Configuration.DefaultRefreshToken}";

                var response = AuthenticationService.ParseWindowsLiveResponse(resultingUrl);

                authService = new AuthenticationService(response);
                authService.UserToken = await AuthenticationService.AuthenticateXASUAsync(authService.AccessToken);
                authService.XToken = await AuthenticationService.AuthenticateXSTSAsync(authService.UserToken, authService.DeviceToken, authService.TitleToken);
            }

            await authService.DumpToJsonFileAsync(Configuration.TokenPath);

            if (authService.UserToken != null && authService.DeviceToken != null)
            {
                await GetUpdateXSTSToken(authService.UserToken, authService.DeviceToken);
            }
            else
            {
                Console.WriteLine("Warning: Authentication tokens are not available. Some functionality may not work.");
            }

            if (!cliMode)
                Console.WriteLine("Initialization finished!");

            using var updateHttpClient = new HttpClient(); // Use 'using' for proper disposal

            if (cliMode)
            {
                var contentId = args[0];
                await ProcessContentId(contentId, updateHttpClient, authService, true);
            }
            else
            {
                while (true)
                {
                    Console.WriteLine("Please enter the ContentId of the package you want to fetch download links for:");
                    var contentId = Console.ReadLine();
                    if (string.IsNullOrEmpty(contentId))
                        continue;

                    await ProcessContentId(contentId, updateHttpClient, authService, false);
                }
            }
        }

        /// <summary>
        /// Processes a content ID to fetch and display package information.
        /// </summary>
        /// <param name="contentId">The content ID of the package to process.</param>
        /// <param name="updateHttpClient">The HTTP client to use for requests.</param>
        /// <param name="authService">The authentication service instance.</param>
        /// <param name="cliMode">Whether the application is running in CLI mode.</param>
        private static async Task ProcessContentId(string contentId, HttpClient updateHttpClient, AuthenticationService authService, bool cliMode)
        {
            if (_updateToken?.Valid != true)
            {
                if (authService.UserToken?.Valid != true || authService.DeviceToken?.Valid != true)
                {
                    if (!await authService.AuthenticateAsync())
                    {
                        Console.WriteLine("Could not regenerate update token. Please restart the app and reauthenticate!");
                        return;
                    }
                }

                if (authService.UserToken != null && authService.DeviceToken != null)
                {
                    await GetUpdateXSTSToken(authService.UserToken, authService.DeviceToken);
                }
            }

            if (!Guid.TryParse(contentId, out _))
            {
                Console.WriteLine("Error: You entered an invalid content id.");
                return;
            }

            var updateUrl = Configuration.PackageServiceBaseUrl + contentId;
            var updateRequest = new HttpRequestMessage(HttpMethod.Get, updateUrl);
            
            if (_updateToken != null)
            {
                updateRequest.Headers.Add("Authorization", $"XBL3.0 x={_updateToken.UserInformation.Userhash};{_updateToken.Jwt}");
            }

            var updateResult = await updateHttpClient.SendAsync(updateRequest);
            if (!updateResult.IsSuccessStatusCode)
            {
                Console.WriteLine($"Failed to fetch package information. Status Code: {updateResult.StatusCode}");
                return;
            }

            try
            {
                var updateData = await updateResult.Content.ReadAsJsonAsync<GetBasePackageResponse>();
                if (!cliMode)
                    Console.WriteLine("Got response!");

                if (updateData?.PackageFound == true && updateData.PackageFiles != null)
                {
                    var relevantFiles = updateData.PackageFiles.Where(file => 
                        !Configuration.ExcludedFileExtensions.Any(ext => file.FileName.EndsWith(ext, StringComparison.OrdinalIgnoreCase)));

                    foreach (var file in relevantFiles)
                    {
                        if (file.CdnRootPaths?.Count > 0)
                        {
                            var downloadUrl = file.CdnRootPaths[0] + file.RelativeUrl;
                            var output = cliMode 
                                ? downloadUrl 
                                : $"{file.FileName} | Size: {file.FileSize} | Link: {downloadUrl}";
                            Console.WriteLine(output);
                        }
                    }
                }
                else
                {
                    Console.WriteLine("Error: Server did not find requested package.");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error while parsing server response: {e.Message}");
            }
        }

        /// <summary>
        /// Gets an update XSTS token for accessing Xbox Live services.
        /// </summary>
        /// <param name="userToken">The user token for authentication.</param>
        /// <param name="deviceToken">The device token for authentication.</param>
        /// <returns>A task that represents the asynchronous operation. The task result indicates whether the operation was successful.</returns>
        public static async Task<bool> GetUpdateXSTSToken(UserToken userToken, DeviceToken deviceToken)
        {
            try
            {
                var httpClient = AuthenticationService.ClientFactory(Configuration.XstsAuthUrl);
                var request = new HttpRequestMessage(HttpMethod.Post, "xsts/authorize");
                var xstsTokenRequest = new XSTSRequest(userToken, Configuration.UpdateServiceAudience, deviceToken: deviceToken);

                request.Headers.Add("x-xbl-contract-version", "1");
                request.Content = new JsonContent(xstsTokenRequest);

                var response = await httpClient.SendAsync(request);
                
                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"Failed to get update XSTS token. Status Code: {response.StatusCode}");
                    return false;
                }
                
                var responseData = await response.Content.ReadAsJsonAsync<XASResponse>();
                _updateToken = new XToken(responseData);
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error while getting update XSTS token: {e.Message}");
                return false;
            }
        }

        /// <summary>
        /// Displays help information for the application.
        /// </summary>
        private static void DisplayHelp()
        {
            Console.WriteLine("MSIX Package Downloader");
            Console.WriteLine("=======================");
            Console.WriteLine();
            Console.WriteLine("A .NET application that downloads MSIX packages from Xbox Live services.");
            Console.WriteLine();
            Console.WriteLine("Usage:");
            Console.WriteLine("  MsixvcPackageDownloader [ContentId]   Download package for specified Content ID");
            Console.WriteLine("  MsixvcPackageDownloader               Run in interactive mode");
            Console.WriteLine("  MsixvcPackageDownloader --help        Show this help message");
            Console.WriteLine();
            Console.WriteLine("Examples:");
            Console.WriteLine("  MsixvcPackageDownloader 9WZDNCRFJ3TJ");
            Console.WriteLine("  dotnet run -- 9WZDNCRFJ3TJ");
            Console.WriteLine();
            Console.WriteLine("Note: First run requires Xbox Live authentication through your browser.");
        }
    }
}