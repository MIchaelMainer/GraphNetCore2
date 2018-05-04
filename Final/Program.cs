using Microsoft.Graph;
using Newtonsoft.Json.Linq;
using System;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace GraphConsoleApp1
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                GraphServiceClient client = GetAuthenticatedClient();

                var users = client.Users.Request().GetAsync().Result;
                Console.WriteLine("Success! The first user returned from Microsoft Graph is: {0}", users[0].UserPrincipalName);
                Console.WriteLine("You now have the basic setup to create a headless Microsoft Graph application that can run on Windows, Mac, or Linux.");
                Console.ReadLine();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        public static string userToken = null;

        private static GraphServiceClient graphClient = null;

        public static GraphServiceClient GetAuthenticatedClient()
        {
            // From app registration registration.
            const string clientId = "TODO_SET_CLIENT_ID";
            const string password = "TODO_SET_CLIENT_SECRET";

            // Form url
            const string tenantId = "TODO_SET_TENANT_ID";
            string getTokenUrl = $"https://login.microsoftonline.com/{tenantId}/oauth2/v2.0/token";

            // Form the POST body.
            const string grantType = "client_credentials";
            const string myScopes = "https://graph.microsoft.com/.default"; // Indicates that it should use scopes in the registration.
            string postBody = $"client_id={clientId}&scope={myScopes}&client_secret={password}&grant_type={grantType}";

            // Create Microsoft Graph client.
            try
            {
                graphClient = new GraphServiceClient(
                    "https://graph.microsoft.com/v1.0",
                    new DelegateAuthenticationProvider(
                        async (requestMessage) =>
                        {
                            // TODO: Create the HttpRequestMessage to request a token for our app.
                            HttpRequestMessage httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, getTokenUrl);
                            httpRequestMessage.Content = new StringContent(postBody, Encoding.UTF8, "application/x-www-form-urlencoded");

                            // TODO: Create the HttpClient, send the request, and get the HttpResponseMessage.
                            HttpClient client = new HttpClient();
                            HttpResponseMessage httpResponseMessage = await client.SendAsync(httpRequestMessage);
                            
                            // TODO: Get the access token from the response and inject the access token into the GraphServiceClient object.
                            string responseBody = await httpResponseMessage.Content.ReadAsStringAsync();
                            userToken = JObject.Parse(responseBody).GetValue("access_token").ToString();
                            requestMessage.Headers.Authorization = new AuthenticationHeaderValue("bearer", userToken);
                        }));
                return graphClient;
            }

            catch (Exception ex)
            {
                Debug.WriteLine("Could not create a graph client: " + ex.Message);
            }

            return graphClient;
        }
    }
}
