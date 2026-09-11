using CoolapkUWP.Models.Update;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Web.Http;
using Windows.Web.Http.Filters;
using HttpClient = System.Net.Http.HttpClient;
using HttpResponseMessage = System.Net.Http.HttpResponseMessage;
using HttpStatusCode = System.Net.HttpStatusCode;

namespace CoolapkUWP.Helpers
{
    public static class UpdateHelper
    {
#if CANARY
        private const string DevOps_API = "https://dev.azure.com/{0}/{1}/_apis/pipelines/{2}/runs";
        private const string DevOps_Artifact_API = "https://dev.azure.com/{0}/{1}/_apis/pipelines/{2}/runs/{3}/artifacts?artifactName={4}&$expand=signedContent";

        public static Task<UpdateInfo> CheckUpdateAsync(string organization, string project, uint pipelineId, bool isBackground = false)
        {
            PackageVersion currentVersion = Package.Current.Id.Version;
            return CheckUpdateAsync(organization, project, pipelineId, currentVersion, isBackground);
        }

        public static async Task<UpdateInfo> CheckUpdateAsync(string organization, string project, uint pipelineId, PackageVersion currentVersion, bool isBackground = false)
        {
            if (string.IsNullOrEmpty(organization))
            {
                throw new ArgumentNullException(nameof(organization));
            }

            if (string.IsNullOrEmpty(project))
            {
                throw new ArgumentNullException(nameof(project));
            }

            try
            {
                using (HttpClientHandler clientHandler = new HttpClientHandler())
                using (HttpClient client = new HttpClient(clientHandler))
                {
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    using (HttpBaseProtocolFilter filter = new HttpBaseProtocolFilter())
                    {
                        Uri host = new Uri("https://dev.azure.com");
                        HttpCookieManager cookieManager = filter.CookieManager;
                        foreach (HttpCookie item in cookieManager.GetCookies(host))
                        {
                            clientHandler.CookieContainer.SetCookies(host, item.ToString());
                        }
                    }

                    string url = string.Format(DevOps_API, organization, project, pipelineId);
                    HttpResponseMessage response = await client.GetAsync(url).ConfigureAwait(false);
                    response.EnsureSuccessStatusCode();
                    if (response.StatusCode != HttpStatusCode.OK) { return null; }
                    string responseBody = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                    RunsInfo result = JsonConvert.DeserializeObject<RunsInfo>(responseBody);

                    if (result != null)
                    {
                        RunInfo run = result.Value.FirstOrDefault(x => x.Result == "succeeded");

                        if (run != null)
                        {
                            SystemVersionInfo newVersionInfo = GetAsVersionInfo(run.CreatedDate, (int)run.ID);

                            UpdateInfo updateInfo = new UpdateInfo
                            {
                                CreatedAt = run.CreatedDate,
                                PublishedAt = run.FinishedDate,
                                ReleaseUrl = run.Links?.Web?.Href,
                                IsExistNewVersion = newVersionInfo > currentVersion,
                                Version = newVersionInfo
                            };

                            try
                            {
                                url = string.Format(DevOps_Artifact_API, organization, project, pipelineId, run.ID, "MSIX%20Package");
                                response = await client.GetAsync(url).ConfigureAwait(false);
                                if (response.StatusCode == HttpStatusCode.OK)
                                {
                                    responseBody = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                                    Asset asset = JsonConvert.DeserializeObject<Asset>(responseBody);
                                    updateInfo.Assets = new[] { asset };
                                }
                            }
                            catch (Exception e)
                            {
                                SettingsHelper.LogManager.GetLogger(nameof(UpdateHelper)).Warn(e.ExceptionToMessage(), e);
                            }

                            return updateInfo;
                        }
                    }
                }
            }
            catch (HttpRequestException e)
            {
                SettingsHelper.LogManager.GetLogger(nameof(UpdateHelper)).Error(e.ExceptionToMessage(), e);
                if (!isBackground) { UIHelper.ShowHttpExceptionMessage(e); }
            }

            return null;
        }

        private static SystemVersionInfo GetAsVersionInfo(DateTimeOffset dateTimeOffset, int id)
        {
            DateTime dateTime = dateTimeOffset.UtcDateTime;
            int major = int.Parse(dateTime.ToString("yy"));
            int minor = int.Parse(dateTime.ToString("MMdd"));
            return new SystemVersionInfo(major, minor, id);
        }
#else
        private const string KKPP_API = "https://v2.kkpp.cc/repos/{0}/{1}/releases/latest";
        private const string GITHUB_API = "https://api.github.com/repos/{0}/{1}/releases/latest";

        public static Task<UpdateInfo> CheckUpdateAsync(string username, string repository, bool isBackground = false)
        {
            PackageVersion currentVersion = Package.Current.Id.Version;
#if FEATURE2
            currentVersion.Major += 1;
            if (currentVersion.Minor > 0)
            {
                currentVersion.Minor -= 1;
            }
#endif
            return CheckUpdateAsync(username, repository, currentVersion);
        }

        public static async Task<UpdateInfo> CheckUpdateAsync(string username, string repository, PackageVersion currentVersion, bool isBackground = false)
        {
            if (string.IsNullOrEmpty(username))
            {
                throw new ArgumentNullException(nameof(username));
            }

            if (string.IsNullOrEmpty(repository))
            {
                throw new ArgumentNullException(nameof(repository));
            }

            try
            {
                using (HttpClientHandler clientHandler = new HttpClientHandler())
                using (HttpClient client = new HttpClient(clientHandler))
                {
                    using (HttpBaseProtocolFilter filter = new HttpBaseProtocolFilter())
                    {
                        Uri host = new Uri("https://api.github.com");
                        HttpCookieManager cookieManager = filter.CookieManager;
                        foreach (HttpCookie item in cookieManager.GetCookies(host))
                        {
                            clientHandler.CookieContainer.SetCookies(host, item.ToString());
                        }
                    }

                    client.DefaultRequestHeaders.Add("User-Agent", username);
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    string url = string.Format(GITHUB_API, username, repository);
                    HttpResponseMessage response = await client.GetAsync(url).ConfigureAwait(false);
                    response.EnsureSuccessStatusCode();
                    if (response.StatusCode != HttpStatusCode.OK) { return null; }
                    string responseBody = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                    UpdateInfo result = JsonConvert.DeserializeObject<UpdateInfo>(responseBody);

                    if (result != null)
                    {
                        SystemVersionInfo newVersionInfo = GetAsVersionInfo(result.TagName);

                        result.IsExistNewVersion = newVersionInfo > currentVersion;
                        result.Version = newVersionInfo;

                        return result;
                    }
                }
            }
            catch (HttpRequestException e)
            {
                SettingsHelper.LogManager.GetLogger(nameof(UpdateHelper)).Error(e.ExceptionToMessage(), e);
                if (!isBackground) { UIHelper.ShowHttpExceptionMessage(e); }
            }

            return null;
        }

        private static SystemVersionInfo GetAsVersionInfo(string version)
        {
            int[] numbs = GetVersionNumbers(version).Split('.').Select(int.Parse).ToArray();
            return numbs.Length <= 1
                ? new SystemVersionInfo(numbs[0], 0, 0, 0)
                : numbs.Length <= 2
                    ? new SystemVersionInfo(numbs[0], numbs[1], 0, 0)
                    : numbs.Length <= 3
                        ? new SystemVersionInfo(numbs[0], numbs[1], numbs[2], 0)
                        : new SystemVersionInfo(numbs[0], numbs[1], numbs[2], numbs[3]);
        }

        private static string GetVersionNumbers(string version)
        {
            const string allowedChars = "01234567890.";
            return new string(version.Where(allowedChars.Contains).ToArray());
        }
#endif
    }
}
