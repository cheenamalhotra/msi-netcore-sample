using System;
using System.IO;
using System.Net;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;

namespace TestMSIAuthentication
{
    class Program
    {
        static void Main(string[] args)
        {
            if(args.Length != 2){
                throw new ArgumentNullException("args[Server, Database]");
            }
            MSITokenProvider provider = new MSITokenProvider();
            String accessToken = provider.GetMSIAuthToken();
            using (SqlConnection con = new SqlConnection($"Server={args[0]};Database={args[1]};"))
            {
                con.AccessToken = accessToken;
                con.Open();
                Console.WriteLine("Connected!");
                using (SqlCommand cmd = con.CreateCommand())
                {
                    cmd.CommandText = "SELECT @@VERSION";
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        reader.Read();
                        Console.WriteLine(reader.GetString(0));
                    }
                }
            }
        }
    }

    class MSITokenProvider
    {
        public string GetMSIAuthToken()
        {
            string MSIEndpoint = Environment.GetEnvironmentVariable("MSI_ENDPOINT");
            string MSISecret = Environment.GetEnvironmentVariable("MSI_SECRET");
            string URL = "";
            string resource = "https://database.windows.net/";

            /*
            * IsAzureApp is used for identifying if the current client application is running in a Virtual Machine
            * (without MSI environment variables) or App Service/Function (with MSI environment variables) as the APIs to
            * be called for acquiring MSI Token are different for both cases.
            */
            bool IsAzureApp = null != MSIEndpoint && !"".Equals(MSIEndpoint) && null != MSISecret && !"".Equals(MSISecret);

            if(IsAzureApp){
                URL = MSIEndpoint + "?api-version=2017-09-01&resource=" + resource;
            } else 
            {
                URL = "http://169.254.169.254/metadata/identity/oauth2/token?api-version=2018-02-01&resource=" + resource;
            }

            try
            {
                
                // Build request to acquire managed identities for Azure resources token
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(URL);
                request.Method = "GET";
                if (IsAzureApp) {
                    request.Headers["Secret"] = MSISecret;
                } else {
                    request.Headers["Metadata"] = "true";
                }
                // Call /token endpoint
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();

                // Pipe response Stream to a StreamReader, and extract access token
                StreamReader streamResponse = new StreamReader(response.GetResponseStream());
                string stringResponse = streamResponse.ReadToEnd();

                MSIAccessToken tokenObject = JsonConvert.DeserializeObject<MSIAccessToken>(stringResponse);
                return tokenObject.access_token;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace);
            }
            return null;
        }
    }

    class MSIAccessToken
    {
        public string access_token { get; set; }
        public string client_id { get; set; }
        public long expires_in { get; set; }
        public long expires_on { get; set; }
        public long ext_expires_in { get; set; }
        public long not_before { get; set; }
        public string resource { get; set; }
        public string token_type { get; set; }
    }
}
