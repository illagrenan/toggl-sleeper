using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Net;
using System.Text;

namespace TogglSleeper.toggl
{
    internal class TogglApi
    {
        private String _userToken;

        public String GetMe()
        {
            JObject parsedRequest = this.DoRequest("https://www.toggl.com/api/v8/me", RequestType.GET);

            try
            {
                return (String)parsedRequest["data"]["fullname"];
            }
            catch (NullReferenceException e)
            {
                System.Diagnostics.Debug.WriteLine(e.Message + "\n" + e.ToString());
                throw new ConnectionFailedException();
            }
        }

        public JObject DoRequest(String url, String method)
        {
            string ApiToken = _userToken;
            string userpass = ApiToken + ":api_token";
            string userpassB64 = Convert.ToBase64String(Encoding.Default.GetBytes(userpass.Trim()));
            string authHeader = "Basic " + userpassB64;

            HttpWebRequest authRequest = (HttpWebRequest)WebRequest.Create(url);
            authRequest.Headers.Add("Authorization", authHeader);
            authRequest.Method = method;
            authRequest.ContentType = "application/x-www-form-urlencoded";
            authRequest.ContentLength = 0;
            authRequest.UserAgent = ".NET Framework Client";
            authRequest.Accept = "*/*";
            authRequest.AutomaticDecompression = DecompressionMethods.GZip;
            authRequest.Credentials = CredentialCache.DefaultCredentials;
            authRequest.CookieContainer = new CookieContainer();
            authRequest.KeepAlive = false;

            // Get the headers associated with the request
            WebHeaderCollection myWebHeaderCollection = authRequest.Headers;
            myWebHeaderCollection.Add("Accept-Language", "en;q=0.8");

            System.Diagnostics.Debug.WriteLine("Outgoing request: " + method + " " + url);
            System.Diagnostics.Debug.WriteLine(authRequest);

            try
            {
                using (WebResponse response = authRequest.GetResponse())
                {
                    using (var stream = response.GetResponseStream())
                    {
                        using (var reader = new StreamReader(stream))
                        {
                            var result = reader.ReadToEnd();

                            JObject resultJson = JObject.Parse(result.ToString());
                            System.Diagnostics.Debug.WriteLine(result.ToString());

                            return resultJson;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.Message + "\n" + e.ToString());
            }

            return new JObject();
        }

        public int GetRunningTaskId()
        {
            JObject parsedRequest = this.DoRequest("https://www.toggl.com/api/v8/time_entries/current", RequestType.GET);

            try
            {
                return (int)parsedRequest["data"]["id"];
            }
            catch (InvalidOperationException e)
            {
                System.Diagnostics.Debug.WriteLine(e.Message + "\n" + e.ToString());
                throw new NoRunningTaskException();
            }
        }

        public void StopRunningTask(int taskId)
        {
            this.DoRequest("https://www.toggl.com/api/v8/time_entries/" + taskId.ToString() + "/stop", RequestType.PUT);
        }

        internal void ChangeToken(string newToken)
        {
            this._userToken = newToken;
        }
    }
}