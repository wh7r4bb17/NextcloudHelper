using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace wh7r4bb17.NextcloudHelper
    {
    public class NextCloudHelper
        {
        /// <summary>
        /// Api Overview
        /// https://docs.nextcloud.com/server/latest/developer_manual/client_apis/OCS/ocs-api-overview.html
        /// </summary>

        //Todo:
        //Update Permissions

        #region Properties
        readonly INextCloudConfig _config;
        readonly HttpClient _client;
        XmlDocument AllUserShares, ShareInfo, UserInformations;

        #endregion Properties

        #region Constructor

        /// <summary>
        /// Represents the Helper Methods for Nextcloud 
        /// </summary>
        /// <param name="config">NextCloudConfig with Base Url of the Nextcloud Instance</param>
        /// <param name="userName">Nextcloud Username</param>
        /// <param name="password">Nextcloud Password</param>
        public NextCloudHelper(INextCloudConfig config, string userName, string password)
            {
            _config = config;
            string credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{userName}:{password}"));

            _client = new HttpClient();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", credentials);
            }

        #endregion


        #region Dispose
        public void Dispose()
            {
            _client.Dispose();
            }

        #endregion Dispose

        #region Public Methods

        /// <summary>
        /// Creates a new Folder at the given Path
        /// </summary>
        /// <param name="nextcloudPath">Path on the Nextcloud Server. Always beginn with '/'</param>
        /// <returns></returns>
        public bool CreateFolder(string nextcloudPath)
            {
            HttpResponseMessage response = CreateFolderAsync(nextcloudPath).Result;
            return response.IsSuccessStatusCode;
            }


        /// <summary>
        /// Creates a new share from File or Folder
        /// </summary>
        /// <param name="nextcloudPath">Path on the Nextcloud Server. Always beginn with '/'</param>
        /// <param name="password"></param>
        /// <param name="expireDate">expire date for public link share</param>
        /// <param name="shareType">Type of the Share</param>
        /// <param name="permissions">Permissions from the Share</param>
        /// <returns></returns>
        public bool CreateShare(string nextcloudPath, string password, DateTime expireDate, ShareType shareType, Permissions permissions)
            {
            HttpResponseMessage response = CreateShareAsync(nextcloudPath, password, expireDate, shareType, permissions).Result;
            return response.IsSuccessStatusCode;
            }


        /// <summary>
        /// Returns XML Document with all Shares from current user
        /// </summary>
        /// <returns></returns>
        public XmlDocument GetAllUserShares()
            {
            GetAllUserSharesAsync().Wait();
            return AllUserShares;
            }


        /// <summary>
        /// Returns the Share download Link from File or Folder
        /// </summary>
        /// <param name="nextcloudPath">Path on the Nextcloud Server. Always beginn with '/'</param>
        /// <returns></returns>
        public string Get_DownloadLink(string nextcloudPath)
            {
            GetShareInfoAsync(nextcloudPath).Wait();
            return GetShareInfoValue("url");
            }


        /// <summary>
        /// Returns the share expiration Date from File or Folder as DateTime
        /// </summary>
        /// <param name="nextcloudPath">Path on the Nextcloud Server. Always beginn with '/'</param>
        /// <returns>DateTime</returns>
        public DateTime Get_ExpirationDate(string nextcloudPath)
            {
            GetShareInfoAsync(nextcloudPath).Wait();
            string ExpireDate = GetShareInfoValue("expiration");
            return DateTime.Parse(ExpireDate);
            }


        /// <summary>
        /// Returns free Space in bytes
        /// </summary>
        /// <returns></returns>
        public double GetFreeSpaceInBytes()
            {
            string freeBytes = GetUserInformationValue("quota");
            return double.Parse(freeBytes);
            }


        /// <summary>
        /// Returns free Space in Gigabytes
        /// </summary>
        /// <returns></returns>
        public double GetFreeSpaceInGb()
            {
            return GetFreeSpaceInBytes() / (1024 * 1024 * 1024);
            }


        /// <summary>
        /// Returns the ID of the current Share
        /// </summary>
        /// <param name="nextcloudPath">Path on the Nextcloud Server. Always beginn with '/'</param>
        /// <returns></returns>
        public string Get_ShareID(string nextcloudPath)
            {
            GetShareInfoAsync(nextcloudPath).Wait();
            return GetShareInfoValue("id");
            }


        /// <summary>
        /// Returns XML Document with Info from current Share
        /// </summary>
        /// <param name="nextcloudPath">Path on the Nextcloud Server. Always beginn with '/'</param>
        /// <returns></returns>
        /// 
        public XmlDocument GetShareInfo(string nextcloudPath)
            {
            GetShareInfoAsync(nextcloudPath).Wait();
            return ShareInfo;
            }


        /// <summary>
        /// Returns the Share Type from current Share
        /// </summary>
        /// <param name="nextcloudPath">Path on the Nextcloud Server. Always beginn with '/'</param>
        /// <returns></returns>
        public ShareType Get_ShareType(string nextcloudPath)
            {
            GetShareInfoAsync(nextcloudPath).Wait();
            string shareType = GetShareInfoValue("share_type");
            return (ShareType)int.Parse(shareType);
            }


        /// <summary>
        /// Returns XML Document with Info from current User
        /// </summary>
        /// <returns></returns>
        public XmlDocument GetUserInformation()
            {
            GetUserInformationAsync().Wait();
            return UserInformations;
            }


        /// <summary>
        /// Remove Share from a File or Folder
        /// </summary>
        /// <param name="nextcloudPath">Path on the Nextcloud Server. Always beginn with '/'</param>
        public void RemoveShare(string nextcloudPath)
            {
            GetShareInfoAsync(nextcloudPath).Wait();
            string _shareID = Get_ShareID(nextcloudPath);
            if (_shareID != string.Empty) { RemoveShareAsync(_shareID).Wait(); }
            }


        /// <summary>
        /// Upload a File to the Nextcloud Server
        /// </summary>
        /// <param name="nextcloudPath">Path on the Nextcloud Server. Always beginn with '/'</param>
        /// <param name="localFilePath">Abolute Filepath on the local maschine</param>
        /// <returns></returns>
        public bool UploadFile(string nextcloudPath, string localFilePath)
            {
            HttpResponseMessage response = UploadFileAsync(nextcloudPath, localFilePath).Result;
            return response.IsSuccessStatusCode;
            }

        #endregion Public Methods
        #region Private Methods

        HttpRequestMessage GenerateRequestMessage(string HttpMethod, string requestUri, bool AddOcsHeader = false)
            {
            HttpRequestMessage request = new HttpRequestMessage(new HttpMethod(HttpMethod), new Uri(requestUri));
            if (AddOcsHeader) { request.Headers.Add("OCS-APIRequest", "true"); }
            return request;
            }

        string GetShareInfoValue(string propertyName)
            {
            string value = string.Empty;
            try
                {
                XmlNodeList ShareInfos = ShareInfo.SelectSingleNode("/ocs/data/element").ChildNodes;
                foreach (XmlNode shareInfo in ShareInfos)
                    {
                    if (shareInfo.Name == propertyName) { value = shareInfo.InnerText; }
                    }
                }
            catch (Exception)
                { }
            return value;
            }

        string GetUserInformationValue(string propertyName)
            {
            string value = string.Empty;
            try
                {
                GetUserInformationAsync().Wait();
                XmlNodeList UserInfos = UserInformations.SelectSingleNode("/ocs/data").ChildNodes;
                foreach (XmlNode userInfo in UserInfos)
                    {
                    if (userInfo.Name == propertyName) { value = userInfo.InnerText; }
                    }
                }
            catch (Exception)
                { }
            return value;
            }

        #endregion Private Methods

        #region Async Tasks

        async Task<HttpResponseMessage> CreateFolderAsync(string nextcloudPath)
            {
            HttpRequestMessage request = GenerateRequestMessage("MKCOL", _config.WebDavAddress + nextcloudPath);
            HttpResponseMessage response = await _client.SendAsync(request);
            return response;
            }

        async Task<HttpResponseMessage> UploadFileAsync(string nextcloudPath, string localFilePath)
            {
            byte[] fileBytes = File.ReadAllBytes(localFilePath);
            ByteArrayContent requestContent = new ByteArrayContent(fileBytes);

            HttpRequestMessage request = GenerateRequestMessage("PUT", _config.WebDavAddress + nextcloudPath + "/" + Path.GetFileName(localFilePath));
            request.Content = requestContent;
            HttpResponseMessage response = await _client.SendAsync(request);
            return response;
            }

        async Task<HttpResponseMessage> GetUserInformationAsync()
            {
            HttpRequestMessage request = GenerateRequestMessage("GET", _config.OCSUserProvAddress + "/user", true);
            HttpResponseMessage response = await _client.SendAsync(request);
            string responseContent = response.Content.ReadAsStringAsync().Result;
            UserInformations = new XmlDocument();
            UserInformations.LoadXml(responseContent);
            return response;
            }

        async Task<HttpResponseMessage> CreateShareAsync(string nextcloudPath, string password, DateTime expireDate, ShareType shareType, Permissions permissions)
            {
            HttpRequestMessage request = GenerateRequestMessage("POST", _config.OCSFileSharingAddress + "/shares", true);

            List<KeyValuePair<string, string>> parameters = new List<KeyValuePair<string, string>>
                {
                new KeyValuePair<string, string>("shareType", ((int)shareType).ToString()),
                new KeyValuePair<string, string>("shareWith", null),
                new KeyValuePair<string, string>("path", nextcloudPath),
                new KeyValuePair<string, string>("permissions", ((int)permissions).ToString()),
                new KeyValuePair<string, string>("password", password),
                new KeyValuePair<string, string>("expireDate", expireDate.Date.ToString("yyyy-MM-dd"))
                };

            request.Content = new FormUrlEncodedContent(parameters);
            HttpResponseMessage response = await _client.SendAsync(request);

            string responseContent = response.Content.ReadAsStringAsync().Result;
            XmlDocument responseXML = new XmlDocument();
            responseXML.LoadXml(responseContent);
            return response;
            }

        async Task<HttpResponseMessage> GetAllUserSharesAsync()
            {
            HttpRequestMessage request = GenerateRequestMessage("GET", _config.OCSFileSharingAddress + "/shares", true);
            HttpResponseMessage response = await _client.SendAsync(request);
            string responseContent = response.Content.ReadAsStringAsync().Result;
            AllUserShares = new XmlDocument();
            AllUserShares.LoadXml(responseContent);
            return response;
            }

        async Task<HttpResponseMessage> GetShareInfoAsync(string nextcloudPath)
            {
            HttpRequestMessage request = GenerateRequestMessage("GET", _config.OCSFileSharingAddress + "/shares" + "?path=" + nextcloudPath, true);
            HttpResponseMessage response = await _client.SendAsync(request);
            string responseContent = response.Content.ReadAsStringAsync().Result;
            ShareInfo = new XmlDocument();
            ShareInfo.LoadXml(responseContent);
            return response;
            }

        async Task<HttpResponseMessage> RemoveShareAsync(string _shareId)
            {
            HttpRequestMessage request = GenerateRequestMessage("DELETE", _config.OCSFileSharingAddress + "/shares/" + _shareId, true);
            HttpResponseMessage response = await _client.SendAsync(request);
            return response;
            }

        #endregion Async Tasks
        }

    public interface INextCloudConfig
        {
        string WebDavAddress { get; }
        string OCSFileSharingAddress { get; }
        string OCSUserProvAddress { get; }
        string OCSStatusApiAddress { get; }
        }

    public class NextCloudConfig : INextCloudConfig
        {
        #region Fields
        private const string DEFAULT_WebDav_SUFFIX = "remote.php/webdav/";
        private const string DEFAULT_OcsFileSharingApi_SUFFIX = "ocs/v2.php/apps/files_sharing/api/v1/";
        private const string DEFAULT_OcsStatusApi_SUFFIX = "ocs/v2.php/apps/user_status/api/v1/user_status/";
        private const string DEFAULT_OcsUserProvisioningApi_SUFFIX = "ocs/v1.php/cloud/";
        private static string _nextcloud_base_url = "";
        private static string _webdavSuffix = "";
        private static string _ocsFilesharingSuffix = "";
        private static string _ocsUserProvSuffix = "";
        private static string _ocsStatusApiSuffix = "";

        #endregion Fields

        #region Properties
        public string WebDavAddress
            {
            get => _nextcloud_base_url + "/" + _webdavSuffix;
            }

        public string OCSFileSharingAddress
            {
            get => _nextcloud_base_url + "/" + _ocsFilesharingSuffix;
            }

        public string OCSUserProvAddress
            {
            get => _nextcloud_base_url + "/" + _ocsUserProvSuffix;
            }

        public string OCSStatusApiAddress
            {
            get => _nextcloud_base_url + "/" + _ocsStatusApiSuffix;
            }

        #endregion Properties

        #region Constructor

        /// <summary>
        /// Constructor for Nextcloud Config 
        /// </summary>
        /// <param name="nextcloud_base_url">Base Url from Nextcloud Instance like 'https://url.to.nextcloudinstance'</param>
        public NextCloudConfig(string nextcloud_base_url) : this(nextcloud_base_url, DEFAULT_WebDav_SUFFIX, DEFAULT_OcsFileSharingApi_SUFFIX, DEFAULT_OcsUserProvisioningApi_SUFFIX, DEFAULT_OcsStatusApi_SUFFIX)
            {

            }

        private NextCloudConfig(string nextcloud_base_url, string webdavSuffix, string ocsFilesharingSuffix, string ocsUserProvSuffix, string ocsStatusApiSuffix)
            {
            _nextcloud_base_url = nextcloud_base_url.TrimEnd('/');
            _webdavSuffix = webdavSuffix.TrimEnd('/');
            _ocsFilesharingSuffix = ocsFilesharingSuffix.TrimEnd('/');
            _ocsUserProvSuffix = ocsUserProvSuffix.TrimEnd('/');
            _ocsStatusApiSuffix = ocsStatusApiSuffix.TrimEnd('/');
            }

        #endregion Constructor
        }

    public enum Permissions
        {
        Read = 1,
        Update = 2,
        Create = 4,
        Delete = 8,
        Share = 16,
        All = 31
        }

    public enum ShareType
        {
        User = 0,
        Group = 1,
        PublicLink = 3,
        Email = 4,
        FederatedCloudShare = 6,
        Circle = 7,
        TalkConversation = 10
        }
    }
