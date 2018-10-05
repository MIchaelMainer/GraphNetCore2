# Microsoft Graph .Net and .NET Core 2.0

This sample and walkthrough shows how you can use Visual Studio 2017 for Mac to create a .NET Core 2.0 console application by using the [OAuth 2.0 client credentials](https://docs.microsoft.com/en-us/azure/active-directory/develop/active-directory-v2-protocols-oauth-client-creds) grant type. This provides the basic flow for creating a Microsoft Graph daemon application that can run on Mac, Linux, and Windows.

> As of the publication of this sample, NetCore 2.0 doesn't have a UI system so it can't provide a UI for user consent.  

Here's a [video walkthrough](https://youtu.be/4s7HKire298) of this sample. 

## Prerequisites
* Visual Studio 2017 for Mac. These instructions will also work for Visual Studio 2017 (Windows).
* An tenant administrator account to provide consent. Get yourself a demo tenant if you haven't already. 

## Clone this repo

`git clone https://github.com/MIchaelMainer/GraphNetCore2.git`

There is a Final and a Start directory in this repo. The Start directory contains the sample in the state where you will need to fill in the blanks to make this work for your app registration. The Final directory contains the state of the project as it will look after you've completed your project. The following steps assume that you are using the start solution. Open the Start solution in Visual Studio 2017 for Mac. 

> Please note that all of the steps will work for Visual Studio on the PC and Visual Studio Code on PC, Mac, and Linux.

You can also recreate this sample by creating a new .Net Core project in Visual Studio 2017 for Mac.

## Register your application

1. Go to [apps.dev.microsoft.com](https://apps.dev.microsoft.com) and sign in.
2. Go to **My Applications** and select **Add an app**.
3. Give your app a name and then select **Create**. You will land on the app registration page.
4. Select and copy the **Application Id** and paste the value into the `clientId` variable in the Program.cs file.
5. Next, select **Generate New Password** under **Application Secrets**.
6. Select and copy the generated password and paste the value into the `password` variable in the Program.cs file.
7. Next, select **Add Platform** under **Platforms** and select **Web**. You can probably use other platform types; I haven't tried them yet.
8. Deselect **Allow Implicit Flow** if it is selected.
9. Add a **Redirect URL** value. This value is arbitrary for purpose of this demo. For this demo, use the value of `http://localhost`. Save this value as we will use this when we form the URL to get admin consent.
10. Under **Microsoft Graph Permissions**, deselect any **Delegated Permissions** and add the `Directory.ReadWrite.All` permission under **Application Permissions**.
11. Scroll to the bottom of the page and select **Save**. We've completed the application registration.

We now have a valid application registration for an application that has read/write access to a tenant's user directory. An application that uses this registration still needs to have permissions granted to it. We are now ready to provide admin consent so that an application can access the resources described in the registration.

## Get admin consent for your application.

An admin will now need to give consent for the application to access resources in your tenant. During registration, you got a *clientId* and *redirect URL*. You'll also need to get a hold of the *tenantId* for the tenant. The admin account can get this from [portal.azure.com](https://portal.azure.com). Here are [instructions on how to get your tenantId](https://support.office.com/en-us/article/Find-your-Office-365-tenant-ID-6891b561-a52d-4ade-9f39-b492285e2c9b). 

Now that you have your *tenantId*, *clientId*, and *redirect URL*, we can now form the URL to provide consent. The URL template takes the following form:
`https://login.microsoftonline.com/{tenantId}/adminconsent?client_id={clientId}&state=12345&redirect_uri={redirectUrl}`
More information about how to form this URL can be found in the [client credential flow documentation.](https://docs.microsoft.com/en-us/azure/active-directory/develop/active-directory-v2-protocols-oauth-client-creds#request-the-permissions-from-a-directory-admin) 

Assuming that you have access to admin credentials, here's what you'll do to grant your application permission to access your tenant:
1. Copy the template URL to a text editor.
2. Replace the *tenantId*, *clientId*, and *redirectUrl* with the values you gathered earlier. 
3. Copy the completed URL and place in the a web browser. You'll be instructed to login.
4. Login with your admin credentials.
5. You will be prompted to consent to give this application permission to access users' resources. Carefully read the information and select **Accept**.
6. The web page will be redirected and the resource in the redirect URL will not be found (404). You can ignore that for the sake of this demo. You'll know that the admin consent was a success if the URL contains `admin_consent=True`.

Your application now can access user resources in your tenant. Now let's create a simple application using Visual Studio 2017 for Mac.

## Update the sample console application
We will now update the Start solution with the settings and code to make requests from the client application.
### Configure the client.

1. Open the Start solution in Visual Studio 2017 for Mac.
2. Set the clientId you got from the app registration on line 37 of program.cs.
3. Set the password you got from the app registration on line 38 of program.cs.
4. Set the tenantId on line 41 of program.cs.

### Add code to make the request to get the token.

We'll make the following additions to the AuthenticateRequestAsyncDelegate argument starting at line 57 in program.cs.

1. Create the HttpRequestMessage to request a token for our app.

        HttpRequestMessage httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, getTokenUrl);
        httpRequestMessage.Content = new StringContent(postBody, Encoding.UTF8, "application/x-www-form-urlencoded");

2. Create the HttpClient, send the request, and get the HttpResponseMessage.

        HttpClient client = new HttpClient();
        HttpResponseMessage httpResponseMessage = await client.SendAsync(httpRequestMessage);

3. Get the access token from the response and inject the access token into the GraphServiceClient object.

        string responseBody = await httpResponseMessage.Content.ReadAsStringAsync();
        userToken = JObject.Parse(responseBody).GetValue("access_token").ToString();
        requestMessage.Headers.Authorization = new AuthenticationHeaderValue("bearer", userToken);

You can now run this solution. This should leave you with a basic solution that can be modified to be ran as a service.
