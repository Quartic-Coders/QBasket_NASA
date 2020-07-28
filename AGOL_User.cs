using Esri.ArcGISRuntime.Portal;
using Esri.ArcGISRuntime.Security;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using System.Windows.Threading;
using static QBasket_demo.MainWindow;


namespace QBasket_demo
{

    public class AGOL_User
    {
        private string _serviceUrl = "https://www.arcgis.com/sharing/rest";
        private string _appID = "mJXUsUntsC9ygMva";
        private string _redirectUrl = "urn:ietf:wg:oauth:2.0:oob";
        private ServerInfo _portalServerInfo;
        private PortalUserContent _userContent;
        private ArcGISPortal _portal;
        private Credential _credential;
        private CredentialRequestInfo _loginInfo;

        public string serviceUrl
        { get => _serviceUrl; set { _serviceUrl = value; } }
        public string appID
        { get => _appID; set { _appID = value; } }
        public string redirectUrl
        { get => _redirectUrl; set { _redirectUrl = value; } }
        public ServerInfo portalServerInfo
        { get => _portalServerInfo; set { _portalServerInfo = value; } }
        public PortalUserContent userContent
        { get => _userContent; set { _userContent = value; } }
        public ArcGISPortal portal
        { get => _portal; set { _portal = value; } }
        public Credential credential
        { get => _credential; set { _credential = value; } }
        public CredentialRequestInfo loginInfo
        { get => _loginInfo; set { _loginInfo = value; } }



        public async void GetUserPortal()
        {

            // Set up the authorization
            OAuthPortal();

            // Challenge the user for portal credentials (OAuth credential request for arcgis.com)
            CredentialRequestInfo loginInfo = new CredentialRequestInfo
            {
                // Use the OAuth implicit grant flow
                GenerateTokenOptions = new GenerateTokenOptions
                {
                    TokenAuthenticationType = TokenAuthenticationType.OAuthImplicit
                },

                // Indicate the url (portal) to authenticate with (ArcGIS Online)
                ServiceUri = new Uri(serviceUrl)
            };
            /*
            try
            {
                // Get a reference to the AuthenticationManager
                AuthenticationManager thisAM = AuthenticationManager.Current;
                await AuthenticationManager.Current.RemoveAndRevokeAllCredentialsAsync();
                // Call GetCredentialAsync on the AuthenticationManager to invoke the challenge handler
                await thisAM.GetCredentialAsync(loginInfo, false);
            }
            catch (OperationCanceledException)
            {
                // user canceled the login
                MessageBox.Show("Portal log in was canceled.", "ArcGIS Login Canceled");
                return;
            }
            */
            try
            {
                // Get a reference to the AuthenticationManager
                Debug.WriteLine("Getting Authentication Manager");
                AuthenticationManager thisAM = AuthenticationManager.Current;
                await AuthenticationManager.Current.RemoveAndRevokeAllCredentialsAsync();

                // Call GetCredentialAsync on the AuthenticationManager to invoke the challenge handler
                Debug.WriteLine("Getting Credentials Async call");
                try
                {
                    await thisAM.GetCredentialAsync(loginInfo, false);
                }
                catch (OperationCanceledException)
                {
                    // user canceled the login
                    MessageBox.Show("Portal log in was canceled.", "ArcGIS Online Login Canceled");
                    Debug.WriteLine("revoking authentication\n");

                    await AuthenticationManager.Current.RemoveAndRevokeAllCredentialsAsync();
                    return;
                }
                Debug.WriteLine("back from getting credentials");

                // Create the portal
                // Get the ArcGIS Online portal (will use credential from login above)
                Debug.WriteLine("creating the portal");
                portal = await ArcGISPortal.CreateAsync();

                // Get the user's content (items in the root folder and a collection of sub-folders)
                Debug.WriteLine("getting User content");

                userContent = await portal.User.GetContentAsync();
            }
            catch (Exception ex)
            {
                // user canceled the login
                MessageBox.Show("Portal Login exception: " + ex.Message, "ArcGIS Online Login Exception");
                Debug.WriteLine("revoking authentication");

                await AuthenticationManager.Current.RemoveAndRevokeAllCredentialsAsync();
                return;
            }
        }


        #region Esri OAuth helpers
        public void OAuthPortal()
        {
            // Register the server information with the AuthenticationManager
            Debug.WriteLine("OAuthPortal - Setting portal server info");
            ServerInfo portalServerInfo = new ServerInfo
            {
                ServerUri = new Uri(serviceUrl),
                OAuthClientInfo = new OAuthClientInfo
                {
                    ClientId = appID,
                    RedirectUri = new Uri(redirectUrl)
                },

                // Use OAuthImplicit as OAuthAuthorizationCode
                TokenAuthenticationType = TokenAuthenticationType.OAuthImplicit
            };

            // Get a reference to the (singleton) AuthenticationManager for the app
            Debug.WriteLine("OauthPortal - Getting current authentication manager");
            AuthenticationManager thisAM = AuthenticationManager.Current;

            // Register the server information
            Debug.WriteLine("OAuthPortal - Registering portal");
            thisAM.RegisterServer(portalServerInfo);

            // Use the OAuthAuthorize to create a
            //  web view that contains the OAuth challenge handler.
            Debug.WriteLine("OAuthPortal - Setting AUthorization handler");
            thisAM.OAuthAuthorizeHandler = new OAuthAuthorize();

            // Create a new ChallengeHandler that uses a method in this class to challenge for credentials
            Debug.WriteLine("Creating new challenge handler");
            thisAM.ChallengeHandler = new ChallengeHandler(CreateCredentialAsync);
            Debug.WriteLine("leaving OAuthPortal");
        }


        /// <summary>
        /// ChallengeHandler function for AuthenticationManager that will be called
        /// whenever access to a secured resource is attempted
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        public async Task<Credential> CreateCredentialAsync(CredentialRequestInfo info)
        {
            // Challenge the user for OAuth credentials
            Debug.WriteLine("In createCredentialAsync");
            //credential = null;

                Debug.WriteLine("In CreateCredentialAsync - creating credential \n service url = " + info.ServiceUri);
                credential = await AuthenticationManager.Current.GenerateCredentialAsync(info.ServiceUri);

            return credential;
        }
        #endregion
    }

    #region Helper class to display the OAuth authorization challenge
    public class OAuthAuthorize : IOAuthAuthorizeHandler
    {
        // Window to contain the OAuth UI
        private Window _window;

        // Use a TaskCompletionSource to track the completion of the authorization
        private TaskCompletionSource<IDictionary<string, string>> _tcs;

        // URL for the authorization callback result (the redirect URI configured for your application)
        private string _callbackUrl;

        // URL that handles the OAuth request
        private string _authorizeUrl;

        // Function to handle authorization requests, takes the URIs for the secured service,
        // the authorization endpoint, and the redirect URI
        public Task<IDictionary<string, string>> AuthorizeAsync(Uri serviceUri, Uri authorizeUri, Uri callbackUri)
        {
            // If the TaskCompletionSource or Window are not null, authorization is in progress
            if (_tcs != null || _window != null)
            {
                // Allow only one authorization process at a time
                throw new Exception();
            }

            // Store the authorization and redirect URLs
            _authorizeUrl = authorizeUri.AbsoluteUri;
            _callbackUrl = callbackUri.AbsoluteUri;

            // Create a task completion source
            _tcs = new TaskCompletionSource<IDictionary<string, string>>();

            // Call a function to show the login controls, make sure it runs on the UI thread for this app
            Dispatcher dispatcher = Application.Current.Dispatcher;
            if (dispatcher == null || dispatcher.CheckAccess())
                AuthorizeOnUIThread(_authorizeUrl);
            else
            {
                Action authorizeOnUIAction = () => AuthorizeOnUIThread(_authorizeUrl);
                dispatcher.BeginInvoke(authorizeOnUIAction);
            }

            // Return the task associated with the TaskCompletionSource
            return _tcs != null ? _tcs.Task : null;
        }


        /// <summary>
        /// Challenge for OAuth credentials on the UI thread
        /// </summary>
        /// <param name="authorizeUrl"></param>
        public void AuthorizeOnUIThread(string authorizeUrl)
        {
            // Create a WebBrowser control to display the authorize page
            WebBrowser webBrowser = new WebBrowser();

            // Handle the navigation event for the browser to check
            // for a response to the redirect URL
            webBrowser.Navigating += WebBrowserOnNavigating;

            // Display the web browser in a new window
            _window = new Window
            {
                Content = webBrowser,
                Height = 430,
                Width = 395,
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };


            // Set the app's window as the owner of the browser window (if main window closes, so will the browser)
            if (Application.Current != null && Application.Current.MainWindow != null)
            {
                _window.Owner = Application.Current.MainWindow;
            }

            // Handle the window closed event then navigate to the authorize url
            _window.Closed += OnWindowClosed;
            webBrowser.Navigate(authorizeUrl);

            // Display the Window
            _window.ShowDialog();
        }

        /// <summary>
        /// Browser window closing handle
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnWindowClosed(object sender, EventArgs e)
        {
            // If the browser window closes, return the focus to the main window
            if (_window != null && _window.Owner != null)
            {
                _window.Owner.Focus();
            }

            // If the task wasn't completed, the user must have closed the
            // window without logging in
            if (_tcs != null && !_tcs.Task.IsCompleted)
            {
                // Set the task completion source exception to indicate a
                // canceled operation
                _tcs.SetException(new OperationCanceledException());
            }

            // Set the task completion source and window to null to
            // indicate the authorization process is complete
            _tcs = null;
            _window = null;
        }


        /// <summary>
        /// Handle browser navigation (content changing)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void WebBrowserOnNavigating(object sender, NavigatingCancelEventArgs e)
        {
            // Check for a response to the callback url
            const string portalApprovalMarker = "/oauth2/approval";
            WebBrowser webBrowser = sender as WebBrowser;
            Uri uri = e.Uri;

            // If no browser, uri, task completion source, or an empty url, return
            if (webBrowser == null || uri == null ||
                _tcs == null || String.IsNullOrEmpty(uri.AbsoluteUri))
                return;

            // Check for redirect
            bool isRedirected = uri.AbsoluteUri.StartsWith(_callbackUrl) ||
                _callbackUrl.Contains(portalApprovalMarker) &&
                uri.AbsoluteUri.Contains(portalApprovalMarker);

            if (isRedirected)
            {
                // If the web browser is redirected to the callbackUrl:
                //    -close the window
                //    -decode the parameters (returned as fragments or query)
                //    -return these parameters as result of the Task
                e.Cancel = true;
                TaskCompletionSource<IDictionary<string, string>> tcs = _tcs;
                _tcs = null;
                if (_window != null)
                {
                    _window.Close();
                }

                // Call a helper function to decode the response parameters
                IDictionary<string, string> authResponse = DecodeParameters(uri);

                // Set the result for the task completion source
                tcs.SetResult(authResponse);
            }
        }   // end WebBrowserOnNavigating


        /// <summary>
        /// Create a dictionary of key value pairs returned in
        /// the OAuth authorization response URI query string
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        private static IDictionary<string, string> DecodeParameters(Uri uri)
        {
            string response = "";

            // Get the values from the URI fragment or query string
            if (!String.IsNullOrEmpty(uri.Fragment))
            {
                response = uri.Fragment.Substring(1);
            }
            else
            {
                if (!String.IsNullOrEmpty(uri.Query))
                {
                    response = uri.Query.Substring(1);
                }
            }

            // Parse parameters into key / value pairs
            Dictionary<string, string> keyValueDictionary = new Dictionary<string, string>();
            string[] keysAndValues = response.Split(new[] { '&' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string kvString in keysAndValues)
            {
                string[] pair = kvString.Split('=');
                string key = pair[0];
                string value = "";
                if (key.Length > 1)
                {
                    value = Uri.UnescapeDataString(pair[1]);
                }

                keyValueDictionary.Add(key, value);
            }

            // Return the dictionary of string keys/values
            return keyValueDictionary;
        }   // end DecodeParameters
        #endregion
    }
}
