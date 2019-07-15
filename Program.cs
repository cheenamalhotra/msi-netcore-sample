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
            // Build request to acquire managed identities for Azure resources token
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create("http://169.254.169.254/metadata/identity/oauth2/token?api-version=2018-02-01&resource=https://database.windows.net/");
            request.Headers["Metadata"] = "true";
            request.Method = "GET";

            try
            {
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
