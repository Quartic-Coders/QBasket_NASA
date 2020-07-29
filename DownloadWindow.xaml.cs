using Esri.ArcGISRuntime.Portal;
using Esri.ArcGISRuntime.Security;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using static QBasket_demo.MainWindow;
using MessageBox = System.Windows.MessageBox;


// download filenames needs to be better generalized to ensure no overwriting
// need to check if Item exists
namespace QBasket_demo
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class DownloadWindow : Window
    {
        bool downloading = false;
        String downloadDir = String.Empty;
        List<String> downloadedFiles = new List<string>();
        BackgroundWorker dataFetcher;

        public AGOL_User agolUser = new AGOL_User();
        //Task getPortal = agolUser.GetUserPortal();

        public DownloadWindow()
        {
            InitializeComponent();

            // Initialize  in UI items
            OutList.ItemsSource = mainWin.confirmItemsWin.confirmList;
            Out_NumItems.Text = mainWin.confirmItemsWin.NumItems.Text;
            Out_TotalSize.Text = mainWin.confirmItemsWin.TotalSize.Text;

        }   // end DownloadWindow


        // Exit button callback
        private void ExitBtn_Click(object sender, RoutedEventArgs e)
        {
            bool carryOn = false;
            if (downloading == false)
            {
                System.Windows.Application.Current.Shutdown();
            }
            else
                carryOn = showWarning();

            if (carryOn == false)
            {
                System.Windows.Application.Current.Shutdown();
            }
        }   // end ExitBtn_Click


        // Cancel Button callback
        private void CancelBtn_Click(object sender, RoutedEventArgs e)
        {
            bool carryOn = false;
            if (downloading)
            {
                carryOn = showWarning();
            }
            if (!carryOn)
            {
                Hide();
            }
        }   // endCancelBtn_Click


        // Default window closing for F4 and title bar closing
        private void QBasketFormatWin_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            bool carryOn = false;
            if (downloading == false)
            {
                System.Windows.Application.Current.Shutdown();
            }
            else
                carryOn = showWarning();

            if (carryOn == false)
            {
                System.Windows.Application.Current.Shutdown();
            }
        }   // end QBasketFormatWin_Closing


        // Download warning
        private bool showWarning()
        {
            // Configure the message box to be displayed
            string messageBoxText = "Downloading in progress - Continue downloading?";
            string caption = "Download Caution";

            MessageBoxButton button = MessageBoxButton.YesNo;
            MessageBoxImage icon = MessageBoxImage.Warning;

            // Display the message box
            MessageBoxResult result =
                System.Windows.MessageBox.Show(messageBoxText, caption, button, icon);

            if (result == MessageBoxResult.Yes)
                return (true);
            else return false;
        }


        // DownloadBtn_Click button callback
        private void DownloadBtn_Click(object sender, RoutedEventArgs e)
        {

            Debug.WriteLine("calling DownloadBtn_Click");
            DownloadImagery();
        }   // end DownloadBtn_Click


        // Download imagery to a local directory
        private async void DownloadImagery()
        {
            DialogResult result = new DialogResult();
            FolderBrowserDialog dialog = new FolderBrowserDialog();

            Debug.WriteLine("in DownloadBtn_Click");

            // If a local download - get download directory
            if (imageFileRB.IsChecked == true)
            {
                dialog.Description = "Select download directory";
                dialog.UseDescriptionForTitle = true;
                result = dialog.ShowDialog();
                if (result.ToString() == "OK")
                    downloadDir = dialog.SelectedPath;
                else if (result.ToString() == "Cancel")
                    return;
            }
            // otherwise get AGOL authorization
            else
            {
                Debug.WriteLine("\nAGOL login selected");

                // Setting up the authorization
                Debug.WriteLine("Get portal authorization");
                agolUser.OAuthPortal();

                // Get user credentials
                Debug.WriteLine("Setting credential request parameters");
                CredentialRequestInfo loginInfo = new CredentialRequestInfo
                {
                    // Use the OAuth implicit grant flow
                    GenerateTokenOptions = new GenerateTokenOptions
                    { TokenAuthenticationType = TokenAuthenticationType.OAuthImplicit },

                    // Indicate the url (portal) to authenticate with (ArcGIS Online)
                    ServiceUri = new Uri(agolUser.serviceUrl)
                };

                Debug.WriteLine("Getting credentials");
                try
                {
                    Debug.WriteLine("Getting Authentication Manager");
                    // Get a reference to the (singleton) AuthenticationManager for the app
                    Esri.ArcGISRuntime.Security.AuthenticationManager thisAM =
                        Esri.ArcGISRuntime.Security.AuthenticationManager.Current;

                    // Call GetCredentialAsync on the AuthenticationManager to invoke the challenge handler
                    Debug.WriteLine("Getting credentials");
                    await thisAM.GetCredentialAsync(loginInfo, false);
                    Debug.WriteLine("Got credentials");
                }
                catch (OperationCanceledException)
                {
                    // user canceled the login
                    throw new Exception("Portal log in was canceled.");
                }
                Debug.WriteLine("Credential request info set");

                Debug.WriteLine("Have Credential - continuing");
                try {
                    // Create the portal
                    // Get the ArcGIS Online portal (will use credential from login above)
                    Debug.WriteLine("Creating the portal");
                    agolUser.portal = await ArcGISPortal.CreateAsync();

                    // Get the user's content (items in the root folder and a collection of sub-folders)
                    Debug.WriteLine("getting User content");
                    agolUser.userContent = await agolUser.portal.User.GetContentAsync();
                }
                catch (Exception ex)
                {
                    // user canceled the login
                    MessageBox.Show("2. Portal Login exception: " + ex.Message, "ArcGIS Online Login Exception");
                    Debug.WriteLine("Exception: " + ex.Message);

                    await Esri.ArcGISRuntime.Security.AuthenticationManager.Current.RemoveAndRevokeAllCredentialsAsync();
                    return;
                }
            }   // end else


            // Set up the background worker
            // for fetching the data
            dataFetcher = new BackgroundWorker();
            if (imageFileRB.IsChecked == true)
            {
                dataFetcher.DoWork += Worker_FetchFiles;
            }
            else
            {
                dataFetcher.DoWork += Worker_AGOL_KMZ;
            }

            dataFetcher.WorkerReportsProgress = false;
            dataFetcher.RunWorkerCompleted += FileFetchCompleted;

            // Show progress bar/main window and hide download window
            mainWin.DownloadPanel.Visibility = Visibility.Visible;
            mainWin.aoiWin.Visibility = Visibility.Visible;
            //mainWin.aoiWin.Show();
            downloading = true;

            //Start the worker
            dataFetcher.RunWorkerAsync();
        }   // end DownloadImagery



        // Copy stream as bytes to output file
        public static void CopyStream(Stream input, Stream output)
        {
            byte[] buffer = new byte[8 * 1024];
            int len;

            // Write as long as there is data
            while ((len = input.Read(buffer, 0, buffer.Length)) > 0)
                output.Write(buffer, 0, len);
        }   // end CopyStream


        // Worker routine
        private void Worker_FetchFiles(object sender, DoWorkEventArgs e)
        {
            string MIME = "image/tiff";
            string latMin, latMax, lonMin, lonMax;
            String baseUri = "https://wvs.earthdata.nasa.gov/api/v1/snapshot?REQUEST=GetSnapshot";
            String timeUri = "&TIME=";
            String bboxUri = "&BBOX=";
            String crsUri = "&CRS=EPSG:4326";
            String formatUri = "&FORMAT=" + MIME;
            String widthUri = "&WIDTH=";
            String heightUri = "&HEIGHT=";
            String layerUri = "&LAYERS=";
            String filename;

            foreach (WMTS.DownloadLayerInfo info in mainWin.wmts.downloadInfo)
            {
                info.uriStr = baseUri + layerUri + info.name
                                                 + bboxUri + info.bbox[0] + "," + info.bbox[1] + ","
                                                 + info.bbox[2] + "," + info.bbox[3]
                                                 + crsUri + timeUri + info.date + formatUri
                                                 + widthUri + info.pixelWidth.ToString() + heightUri
                                                 + info.pixelHeight.ToString();
                Uri uri = new Uri(info.uriStr);

                WebRequest request = WebRequest.Create(uri);
                WebResponse response = request.GetResponse();
                Stream dataStream = response.GetResponseStream();

                if (dataStream != null)
                {
                    // Get the filename and add it to the list of downloaded files
                    if (info.bbox[0] < 0)
                        latMin = info.bbox[0].ToString("F2") + "S";
                    else
                        latMin = info.bbox[0].ToString("F2") + "N";
                    if (info.bbox[2] < 0)
                        latMax = info.bbox[2].ToString("F2") + "S";
                    else
                        latMax = info.bbox[2].ToString("F2") + "N";

                    if (info.bbox[1] < 0)
                        lonMin = info.bbox[1].ToString("F2") + "W";
                    else
                        lonMin = info.bbox[1].ToString("F2") + "E";
                    if (info.bbox[3] < 0)
                        lonMax = info.bbox[3].ToString("F2") + "W";
                    else
                        lonMax = info.bbox[3].ToString("F2") + "E";

                    filename = downloadDir + "/" + info.name + "_"
                        + info.date + "_" + info.latency
                        + "_Zoom-" + info.zoomLvl + "_"
                        + latMin + "_" + lonMin + "_x_"
                        + latMax + "_" + lonMax + ".tiff";


                    // Download the file
                    using (FileStream output = File.OpenWrite(filename))
                        CopyStream(dataStream, output);

                    // Close these up otherwise bad things will happen
                    response.Close();
                    dataStream.Close();
                    downloadedFiles.Add(filename);
                }   // end if data stream
            }   // end foreach download item
        }   // end Worker_FetchFiles


        // Worker routine
        private void Worker_AGOL_KMZ(object sender, DoWorkEventArgs e)
        {
            String latMin, latMax, lonMin, lonMax;
            String filename;
            PortalItem item;
            String uploadMIME = "application/octet-stream";
            String downloadMIME = "application/vnd.google-earth.kmz";

            String baseUri = "https://wvs.earthdata.nasa.gov/api/v1/snapshot?REQUEST=GetSnapshot";
            String timeUri = "&TIME=";
            String bboxUri = "&BBOX=";
            String crsUri = "&CRS=EPSG:4326";
            String formatUri = "&FORMAT=" + downloadMIME;
            String widthUri = "&WIDTH=";
            String heightUri = "&HEIGHT=";
            String layerUri = "&LAYERS=";


            foreach (WMTS.DownloadLayerInfo info in mainWin.wmts.downloadInfo)
            {
                // Create URI used to fetch the imagery
                info.uriStr = baseUri + layerUri + info.name
                                                 + bboxUri + info.bbox[0] + "," + info.bbox[1] + ","
                                                 + info.bbox[2] + "," + info.bbox[3]
                                                 + crsUri + timeUri + info.date + formatUri
                                                 + widthUri + info.pixelWidth.ToString() + heightUri
                                                 + info.pixelHeight.ToString();
                Debug.WriteLine("Snapshot URI:" + info.uriStr);
                Uri uri = new Uri(info.uriStr);

                // Get the data using web calls
                WebRequest request = WebRequest.Create(uri);
                WebResponse response = request.GetResponse();
                Stream dataStream = response.GetResponseStream();

                if (dataStream != null)
                {
                    #region GetFileName
                    // Create the filename for this image
                    if (info.bbox[0] < 0)
                        latMin = info.bbox[0].ToString("F2") + "S";
                    else
                        latMin = info.bbox[0].ToString("F2") + "N";
                    if (info.bbox[2] < 0)
                        latMax = info.bbox[2].ToString("F2") + "S";
                    else
                        latMax = info.bbox[2].ToString("F2") + "N";

                    if (info.bbox[1] < 0)
                        lonMin = info.bbox[1].ToString("F2") + "W";
                    else
                        lonMin = info.bbox[1].ToString("F2") + "E";
                    if (info.bbox[3] < 0)
                        lonMax = info.bbox[3].ToString("F2") + "W";
                    else
                        lonMax = info.bbox[3].ToString("F2") + "E";

                    filename = info.name + "_"
                            + info.date + "_" + info.latency
                            + "_Zoom-" + info.zoomLvl + "_"
                            + latMin + "_" + lonMin + "_x_"
                            + latMax + "_" + lonMax + ".kmz";
                    #endregion
                    Debug.WriteLine("Uploading " + filename);

                    // Check for duplicate item filenames
                    string msg = String.Empty;
                    Boolean noDuplicates = true;

                    // Check for a duplicate files in the files just uploaded
                    foreach (String fileDownloaded in downloadedFiles)
                    {
                        if (String.Equals(fileDownloaded, filename))
                        {
                            noDuplicates = false;
                            msg = "\n The file\n" + filename + "\nalready exists in download list";
                            msg += "\n Item will not be created\n";
                            MessageBox.Show(msg, "DUPLICATE DOWNLOADED FILE");
                            break;
                        }
                    }

                    foreach (PortalItem pItem in agolUser.userContent.Items)
                    {
                        if ((String.Equals(filename, pItem.Name)) ||
                            (String.Equals(filename, pItem.Title)))
                        {
                            noDuplicates = false;
                            msg = "\n" + filename;
                            msg += "\n This item already exists in User AGOL Content\n";
                            msg += "\n";
                            msg += "\n Portal Item Details";
                            msg += "\n Name:  " + pItem.Name;
                            msg += "\n Title: " + pItem.Title;
                            msg += "\n Item ID: " + pItem.ItemId;
                            msg += "\n";
                            MessageBox.Show(msg, "DUPLICATE PORTAL ITEM NAME");
                            break;
                        }
                    }

                    // Generate portal item and content to upload
                    if (noDuplicates)
                    {
                        Debug.WriteLine(filename + " is not a duplicate portal item");
                        try
                        {
                            if (agolUser.portal == null)
                            {
                                MessageBox.Show("Null ArcGIS User portal\n + Writing file: " + filename);
                            }
                            else
                            {
                                item = new PortalItem(agolUser.portal, PortalItemType.KML, filename)
                                {
                                    // Add a brief summary for the item
                                    Snippet = "NASA Snapshot image created by Q Basket",

                                    // Add a description of the item in more detail
                                    Description = "NASA snapshot KMZ file uploaded to ArcGIS Online user account by Q Basket"
                                };

                                if (item != null)
                                {
                                    // provide relevant tags to the item
                                    item.Tags.Add("Snapshot");
                                    item.Tags.Add("NASA");
                                    item.Tags.Add("Q Basket");
                                    item.Tags.Add("Quartic Solutions");
                                    item.AccessInformation = "NASA GIBS SNAPSHOT SERVER";
                                    item.SpatialReferenceName = "WGS-84";
                                    item.Name = filename;
                                    item.Type = PortalItemType.KML;

                                    // add the new item (without content) to the portal
                                    MediaTypeHeaderValue mediaType = new MediaTypeHeaderValue(uploadMIME);

                                    PortalItemContentParameters itemContent =
                                        new PortalItemContentParameters(dataStream, filename, mediaType);

                                    // Launch the upload on a separate thread
                                    var t = Task.Run(() => agolUser.portal.User.AddPortalItemAsync(item, itemContent));
                                    t.Wait();
                                    downloadedFiles.Add(filename);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("ERROR: " + ex.ToString(), "CREATE PORTAL ITEM ERROR");
                        }

                    }   // end if noDuplicates
                    // Close these up otherwise bad things will happen
                    response.Close();
                    dataStream.Close();
                }   // end if data stream

            }   // end foreach download item
        }   // end Worker_AGOL


        // RunWorkerCompletedEventArgs callback
        private void FileFetchCompleted(object sender, RunWorkerCompletedEventArgs e)
        {

            // Turn downloading flag off and output downloaded file list
            downloading = false;


            String str;

            if (imageFileRB.IsChecked == true)
                str = "Imagery files saved to " + downloadDir;
            else
                str = "Imagery files uploaded to ArcGIS Online";
                foreach (String fileStr in downloadedFiles)
            {
                str += "\t" + fileStr + "\n";
            }
            MessageBox.Show(str, "IMAGERY SAVED");


            // Make sure there are no items in the checkout list, close the window
            // and go back to the AOI window
            // items not clearing - add flag to say files downloaded
            downloadedFiles.Clear();
            mainWin.wmts.downloadInfo.Clear();
            mainWin.confirmItemsWin.confirmList.Clear();
            mainWin.confirmItemsWin.ConfirmList.ItemsSource = null;
            mainWin.confirmItemsWin.ConfirmList.ItemsSource =
                mainWin.confirmItemsWin.confirmList;
            mainWin.confirmItemsWin.NumItems.Text = "0";
            mainWin.confirmItemsWin.TotalSize.Text = "0";
            mainWin.confirmItemsWin.Download.IsEnabled = false;

            // PANEL - reset to show correct window
            mainWin.Activate();
            mainWin.Topmost = true;
            mainWin.aoiWin.CheckoutBtn.IsEnabled = false;

            if (mainWin.aoiWin != null)
            {
                if (mainWin.aoiWin.Visibility != Visibility.Visible)
                {
                    mainWin.aoiWin.Activate();
                    mainWin.aoiWin.ShowDialog();
                    mainWin.aoiWin.Topmost = true;
                }
            }
            mainWin.confirmItemsWin.Close();
            mainWin.DownloadPanel.Visibility = Visibility.Collapsed;

        }   // end Worker_RunWorkerCompleted
    } // end DownloadWindow class

}   // end namespace

