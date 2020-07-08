///The AOI class covers the Area of interest aspects
/// of the Main Window. This code is part of main window
/// class.
/// -----------------------------------------------------
/// Controls
/// Draw, Select, Cancel, Back to Map
/// -------------------------------------------------------
/// Draw -Grab control of the map interface
///       open sketch editor with rectangular selection only
/// Select - Open wmts window 
///             Populate wmts items w/selected layer info
/// Clear - Clear current graphics and remain in draw mode
/// Back To Map - Clear graphics and return control back 
///                  to the Map
/// 
/// Main outcome is the graphics envelope, 
///     which defines the AOI extent
/// 
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Ogc;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.UI;
using qWPF_lib;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace QBasket_demo
{
    public partial class MainWindow
    {
        public EsriSketchEd sketchEd = new EsriSketchEd();
        public WMTS wmts = new WMTS();

        int PIX_MIN = 64;
        int PIX_MAX = 8192;
        int TILE_WIDTH = 256;
        int TILE_HEIGHT = 256;
        string WMTS_CAP_URL;

        #region delete
        /*
        public class ResType
        {
            private double _resolution;
            private double _scaleDenom;

            private int _matrixWidth;
            private int _matrixHeight;
            private string _id;

            public double resolution
            { get => _resolution; set { _resolution = value; } }
            public double scaleDenom
            { get => _scaleDenom; set { _scaleDenom = value; } }

            public int matrixWidth
            { get => _matrixWidth; set { _matrixWidth = value; } }
            public int matrixHeight
            { get => _matrixHeight; set { _matrixHeight = value; } }

            public string id
            { get => _id; set { _id = value; } }

        }   // end ResType class

        // WMTS tile set variables
        public class TileSetVariables
        {
            private List<ResType> _resTypes = new List<ResType>();
            private int _tileWidth;
            private int _tileHeight;
            private int _maxResoution;
            private string _layerTitle;
            private string _layerName;
            private string _tileSetTitle;
            private int _zoomLvls;
            private int _maxSnapZoom;
            private int _minSnapZoom;

            public List<ResType> resTypes
            { get => _resTypes; set { _resTypes = value; } }
            public int tileWidth
            { get => _tileWidth; set { _tileWidth = value; } }
            public int tileHeight
            { get => _tileHeight; set { _tileHeight = value; } }
            public string layerTitle
            { get => _layerTitle; set { _layerTitle = value; } }
            public string layerName
            { get => _layerName; set { _layerName = value; } }
            public string tileSetTitle
            { get => _tileSetTitle; set { _tileSetTitle = value; } }
            public int maxResoution
            { get => _maxResoution; set { _maxResoution = value; } }
            public int zoomLvls
            { get => _zoomLvls; set { _zoomLvls = value; } }
            public int maxSnapZoom
            { get => _maxSnapZoom; set { _maxSnapZoom = value; } }
            public int minSnapZoom
            { get => _minSnapZoom; set { _minSnapZoom = value; } }
        }   // end TileSetVariables


        // WMTS tile set variables
        public class DownloadLayerInfo
        {
            private WmtsLayerInfo _snapInfo;
            private String _title;
            private String _name;
            private double[] _bbox = new double[4];
            private String _latency;
            private String _crs;
            private int _zoomLvl;
            private int _tileWidth;
            private int _tileHeight;
            private int _matrixWidth;
            private int _matrixHeight;
            private double _nMBytes;
            private int _pixelWidth;
            private int _pixelHeight;
            private double _resolution;
            private string _snapDate;
            private string _uriStr;

            public WmtsLayerInfo snapInfo
            { get => _snapInfo; set { _snapInfo = value; } }
            public string title
            { get => _title; set { _title = value; } }
            public string name
            { get => _name; set { _name = value; } }
            public double[] bbox
            { get => _bbox; set { _bbox = value; } }
            public string latency
            { get => _latency; set { _latency = value; } }
            public string crs
            { get => _crs; set { _crs = value; } }
            public int zoomLvl
            { get => _zoomLvl; set { _zoomLvl = value; } }
            public int tileWidth
            { get => _tileWidth; set { _tileWidth = value; } }
            public int tileHeight
            { get => _tileHeight; set { _tileHeight = value; } }
            public int matrixWidth
            { get => _matrixWidth; set { _matrixWidth = value; } }
            public int matrixHeight
            { get => _matrixHeight; set { _matrixHeight = value; } }
            public double nMBytes
            { get => _nMBytes; set { _nMBytes = value; } }
            public int pixelWidth
            { get => _pixelWidth; set { _pixelWidth = value; } }
            public int pixelHeight
            { get => _pixelHeight; set { _pixelHeight = value; } }
            public double resolution
            { get => _resolution; set { _resolution = value; } }
            public string snapDate
            { get => _snapDate; set { _snapDate = value; } }
            public string uriStr
            { get => _uriStr; set { _uriStr = value; } }
        }   // end Download layer Info
        */
        #endregion

        // Graphics overlay to host sketch graphics
        public GraphicsOverlay sketchOverlay;

        // Envelope of graphics drawn
        public Envelope AOIEnvelope;

        // Define a line symbol (for the fill outline)
        public LineSymbol AOI_outline =
            new Esri.ArcGISRuntime.Symbology.SimpleLineSymbol();

        // WMTS Globals
        #region delete
        /*
        public wmts.serviceInfo wmts.serviceInfo;
        public List<WmtsLayerInfo> wmts.selectedLayers = new List<WmtsLayerInfo>();
        public IReadOnlyList<WmtsLayerInfo> wmtsLayerInfos;
        public List<TileSetVariables> layerTileSets = new List<TileSetVariables>();
        public List<TileSetVariables> wmts.tileSets = new List<TileSetVariables>();
        public List<WmtsLayerInfo> wmtsDownloadLayers = new List<WmtsLayerInfo>();
        public List<DownloadLayerInfo> downloadInfo = new List<DownloadLayerInfo>();
        */
        #endregion 

        /// <summary>
        /// Initialize Graphics overlay for AOI
        /// </summary>
        private void InitializeAOIsketch()
        {
            // Create graphics overlay to display sketch geometry
            sketchOverlay = new GraphicsOverlay();
            BasemapView.GraphicsOverlays.Add(sketchOverlay);

            // Set the sketch editor as the page's data context
            DataContext = BasemapView.SketchEditor;

            // Set outline style for AOI rectangle
            AOI_outline.Color = Color.Red;
            AOI_outline.Width = 2;
        }   // end Initialize AIO

        /// <summary>
        /// Create the graphic
        /// </summary>
        /// <param name="geometry"></param>
        /// <returns></returns>
        private Graphic CreateGraphic(Geometry geometry)
        {
            // Create a graphic to display the specified geometry
            Symbol symbol = null;
            symbol = new SimpleFillSymbol()
            {
                // Color = Color.Red,
                Outline = AOI_outline,
                Style = SimpleFillSymbolStyle.Null
            };

            // Return a new graphic witha rectangle
            return new Graphic(geometry, symbol);
        }   // end CreateGraphic


        /// <summary>
        ///  Draw Button Callback
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void DrawButtonClick(object sender, RoutedEventArgs e)
        {

            if (selectedLayers == null)
            {
                MessageBox.Show("Please select at least one layer", "LAYER SELECTION ERROR");
                haveLayer = false;
            }
            else if (selectedLayers.Count < 1)
            {
                haveLayer = false;
                MessageBox.Show("Please select at least one layer", "LAYER SELECTION ERROR");
            }
            else try
                {
                    // Clear out previous graphics if they exist
                    sketchOverlay.Graphics.Clear();

                    // Set Buttons
                    AOIDraw.IsEnabled = true;
                    AOIClear.IsEnabled = true;
                    AOICancel.IsEnabled = true;
                    AOISelect.IsEnabled = true;

                    // Create graphics area for a redrawable rectangle w/labels
                    SketchCreationMode creationMode = (SketchCreationMode)6;
                    SketchEditConfiguration sketchEdit = new SketchEditConfiguration
                    {
                        AllowMove = true,
                        AllowRotate = false,
                        ResizeMode = (SketchResizeMode)1
                    };

                    // Let the user draw on the map view using the chosen sketch mode  
                    Esri.ArcGISRuntime.Geometry.Geometry geometry =
                     await BasemapView.SketchEditor.StartAsync(creationMode, true);

                    // Create and add a graphic from the geometry the user drew
                    Graphic graphic = CreateGraphic(geometry);
                    sketchOverlay.Graphics.Add(graphic);
                    haveSketch = true;
                }
                catch (TaskCanceledException)
                {
                    sketchOverlay.Graphics.Clear();
                    haveSketch = false;
                    AOISelect.IsEnabled = false;
                    Debug.WriteLine("Task Cancelled Exception in Draw button click");
                    // Ignore ... let the user cancel drawing
                }
                catch (Exception ex)
                {
                    // Report exceptions
                    MessageBox.Show("Error drawing graphic shape: " + ex.Message);
                }
        }   // end DrawButtonClick


        private void SelectButtonClick(object sender, RoutedEventArgs e)
        {
            Geometry geometry;

            if (selectedLayers != null)
            {
                ResetWmtsInfo();
                if ((selectedLayers.Count > 0))
                {
                    haveLayer = true;

                    // Create window if it does not exist
                    if (aoiWin == null)
                        aoiWin = new AOIWindow();

                    // Complete Current Sketch if one is being executed
                    if (BasemapView.SketchEditor.CompleteCommand.CanExecute(null))
                    {
                        geometry = BasemapView.SketchEditor.Geometry;
                        BasemapView.SketchEditor.CompleteCommand.Execute(null);
                        Graphic graphic = CreateGraphic(geometry);
                        sketchOverlay.Graphics.Add(graphic);

                        // Set Buttons
                        AOIDraw.IsEnabled = true;
                        AOISelect.IsEnabled = false;
                        AOIClear.IsEnabled = true;
                        AOICancel.IsEnabled = true;

                        // Fetch wmts layer information for the selected layers
                        InitializeWMTS();

                        // Update Geo Coords in aoi window
                        AOIEnvelope = geometry.Extent;
                        aoiWin.MinLat.Text = AOIEnvelope.YMin.ToString("F4");
                        aoiWin.MaxLat.Text = AOIEnvelope.YMax.ToString("F4");
                        aoiWin.MinLon.Text = AOIEnvelope.XMin.ToString("F4");
                        aoiWin.MaxLon.Text = AOIEnvelope.XMax.ToString("F4");

                        // Update Date/Time in aoi window 
                        DateTime date = startDate_DP.SelectedDate.Value;
                        String year = date.Year.ToString("D4");
                        String month = date.Month.ToString("D2");
                        String day = date.Day.ToString("D2");
                        aoiWin.Date.Text = year + "-" + month + "-" + day;

                        // Convert envelope to pixel values - currently degress
                        string str = AOIEnvelope.Width.ToString("F4") + " x " + AOIEnvelope.Height.ToString("F4");
                        aoiWin.PixelSize.Text = str;

                        aoiWin.ZoomCombo.SelectedIndex = 0;
                        aoiWin.ImageryTitle.SelectedIndex = 0;

                        //aoiWin.Topmost = true;                       
                        aoiWin.ShowDialog();
                        aoiWin.Activate();

                    }   // end if CompleteCommand.CanExecute

                    // If there is an existing sketch - use it
                    else if (haveSketch)
                    {
                        // Already have AOI
                        AOIDraw.IsEnabled = true;
                        AOISelect.IsEnabled = false;
                        AOIClear.IsEnabled = true;
                        AOICancel.IsEnabled = true;

                        aoiWin.ZoomCombo.SelectedIndex = 0;
                        aoiWin.ImageryTitle.SelectedIndex = 0;

                        // Start dialog
                        // aoiWin.Topmost = true;
                        aoiWin.Activate();
                        aoiWin.ShowDialog();
                    }   // end use existing sketch

                }   // end if there is more than one selected layer
            }   // end if selected layers != null
            else
                MessageBox.Show("Please select at least one layer", "LAYER SELECTION ERROR");

        }   // end SelectButtonClick


        // Intialize WMTS data
        private async void InitializeWMTS()
        {
            String str;
            WmtsService wmtsService;
            Uri wmtsServiceURI;

            // Create the WMTS Service Load data from the URI
            WMTS_CAP_URL = "https://gibs.earthdata.nasa.gov/wmts/" + wmsUriStartup.EPSG
                            + "/" + wmsUriStartup.latency + "/1.0.0/WMTSCapabilities.xml";
            Debug.WriteLine("Getting wmts capabilites " + WMTS_CAP_URL);
            wmtsServiceURI = new Uri(WMTS_CAP_URL);

            // Define an insatance of the service
            wmtsService = new WmtsService(wmtsServiceURI);

            // If service can load, initialize the app
            try
            {
                if (aoiWin == null)
                    aoiWin = new AOIWindow();

                // Load the WMS Service.
                await wmtsService.LoadAsync();

                // Get the service info (metadata) from the service.
                wmts.serviceInfo = wmtsService.ServiceInfo;

                // Get the WMTS tile information
                wmts.tileSets = new List<WMTS.TileSetVariables>();
                for (int i = 0; i < selectedLayers.Count; i++)
                {
                    wmts.tileSets.Add(new WMTS.TileSetVariables());
                    wmts.tileSets[i].tileSetTitle = wmts.serviceInfo.TileMatrixSets[i].Id;
                    wmts.tileSets[i].zoomLvls = wmts.serviceInfo.TileMatrixSets[i].TileMatrices.Count;
                    for (int j = 0; j < wmts.serviceInfo.TileMatrixSets[i].TileMatrices.Count; j++)
                    {
                        wmts.tileSets[i].resTypes.Add(new WMTS.ResType());
                        wmts.tileSets[i].resTypes[j].id =
                            wmts.serviceInfo.TileMatrixSets[i].TileMatrices[j].Id;
                        wmts.tileSets[i].resTypes[j].scaleDenom =
                            wmts.serviceInfo.TileMatrixSets[i].TileMatrices[j].ScaleDenominator;
                    }
                }

                // Obtain the read only list of WMTS layers info objects
                // for selected layers
                wmts.selectedLayers = new List<WmtsLayerInfo>();
                foreach (WmsLayerInfo wmsLayerInfo in selectedLayers)
                {
                    Debug.WriteLine("Adding " + wmsLayerInfo.Title);
                    str = wmsLayerInfo.Title;
                    foreach (WmtsLayerInfo wmtsLayerInfo in wmts.serviceInfo.LayerInfos)
                    {
                        // Got a layer - save info about this layer
                        if (str == wmtsLayerInfo.Title)
                        {
                            wmts.selectedLayers.Add(wmtsLayerInfo);
                            break;
                        }
                    }
                }

                // Set up tile matrix information for selected tiles
                wmts.layerTileSets = new List<WMTS.TileSetVariables>();
                for (int i = 0; i < wmts.selectedLayers.Count; i++)
                {
                    wmts.layerTileSets.Add(new WMTS.TileSetVariables());
                    wmts.layerTileSets[i].zoomLvls = wmts.selectedLayers[i].TileMatrixSets[0].TileMatrices.Count;
                    wmts.layerTileSets[i].tileSetTitle =
                         wmts.selectedLayers[i].TileMatrixSets[0].Id;
                    wmts.layerTileSets[i].layerTitle = wmts.selectedLayers[i].Title;
                    wmts.layerTileSets[i].layerName = wmts.selectedLayers[i].Id;
                    wmts.layerTileSets[i].tileWidth = TILE_WIDTH;
                    wmts.layerTileSets[i].tileHeight = TILE_HEIGHT;

                    // get tile matrices info
                    for (int j = 0; j < wmts.selectedLayers[i].TileMatrixSets[0].TileMatrices.Count; j++)
                    {
                        wmts.layerTileSets[i].resTypes.Add(new WMTS.ResType());
                        wmts.layerTileSets[i].resTypes[j].id =
                            wmts.selectedLayers[i].TileMatrixSets[0].TileMatrices[j].Id;
                        wmts.layerTileSets[i].resTypes[j].scaleDenom =
                            wmts.selectedLayers[i].TileMatrixSets[0].TileMatrices[j].ScaleDenominator;
                        wmts.layerTileSets[i].resTypes[j].resolution =
                            wmts.layerTileSets[i].resTypes[j].scaleDenom / 397569610;
                        wmts.layerTileSets[i].resTypes[j].matrixWidth = 2 ^ j;
                        wmts.layerTileSets[i].resTypes[j].matrixHeight = 2 ^ j;
                    }
                }

                // Set AOI window Titles
                aoiWin.panelVars.titleList = new List<string>();
                foreach (WMTS.TileSetVariables tileSet in wmts.layerTileSets)
                {
                    str = tileSet.layerTitle + "\n" +
                          "Tile Set: " + tileSet.tileSetTitle + "\t"
                          + "Zoom Levels: " + tileSet.zoomLvls;
                    aoiWin.panelVars.titleList.Add(str);
                }

                aoiWin.ImageryTitle.ItemsSource = aoiWin.panelVars.titleList;
                aoiWin.ImageryTitle.SelectedIndex = 0;

                // Set the zoom level specific to image selected
                ResetZoomLevels(aoiWin.ImageryTitle.SelectedIndex, "Skipper");
            }   // end try

            catch (Exception e)
            {
                MessageBox.Show(e.ToString(), "WMTS SERVER LOAD ERROR");
            }
        }   // end InitializeWMTS


        public void ResetWmtsInfo()
        {
            string str;

            if (wmts.serviceInfo != null)
            {
                // Create a new list of layer info selected
                wmts.tileSets = new List<WMTS.TileSetVariables>();
                for (int i = 0; i < selectedLayers.Count; i++)
                {
                    wmts.tileSets.Add(new WMTS.TileSetVariables());
                    wmts.tileSets[i].tileSetTitle = wmts.serviceInfo.TileMatrixSets[i].Id;
                    wmts.tileSets[i].zoomLvls = wmts.serviceInfo.TileMatrixSets[i].TileMatrices.Count;
                    for (int j = 0; j < wmts.serviceInfo.TileMatrixSets[i].TileMatrices.Count; j++)
                    {
                        wmts.tileSets[i].resTypes.Add(new WMTS.ResType());
                        wmts.tileSets[i].resTypes[j].id =
                            wmts.serviceInfo.TileMatrixSets[i].TileMatrices[j].Id;
                        wmts.tileSets[i].resTypes[j].scaleDenom =
                            wmts.serviceInfo.TileMatrixSets[i].TileMatrices[j].ScaleDenominator;
                    }
                }

                // Obtain the read only list of WMTS layers info objects
                // for selected layers
                wmts.selectedLayers = new List<WmtsLayerInfo>();
                foreach (WmsLayerInfo wmsLayerInfo in selectedLayers)
                {
                    Debug.WriteLine("Adding " + wmsLayerInfo.Title);
                    str = wmsLayerInfo.Title;
                    foreach (WmtsLayerInfo wmtsLayerInfo in wmts.serviceInfo.LayerInfos)
                    {
                        // Got a layer - save info about this layer
                        if (str == wmtsLayerInfo.Title)
                        {
                            wmts.selectedLayers.Add(wmtsLayerInfo);
                            break;
                        }
                    }
                }

                // Set up tile matrix information for selected tiles
                wmts.layerTileSets = new List<WMTS.TileSetVariables>();
                for (int i = 0; i < wmts.selectedLayers.Count; i++)
                {
                    wmts.layerTileSets.Add(new WMTS.TileSetVariables());
                    wmts.layerTileSets[i].zoomLvls = wmts.selectedLayers[i].TileMatrixSets[0].TileMatrices.Count;
                    wmts.layerTileSets[i].tileSetTitle =
                            wmts.selectedLayers[i].TileMatrixSets[0].Id;
                    wmts.layerTileSets[i].layerTitle = wmts.selectedLayers[i].Title;
                    wmts.layerTileSets[i].layerName = wmts.selectedLayers[i].Id;

                    wmts.layerTileSets[i].tileWidth = TILE_WIDTH;
                    wmts.layerTileSets[i].tileHeight = TILE_HEIGHT;

                    // get tile matrices info
                    for (int j = 0; j < wmts.selectedLayers[i].TileMatrixSets[0].TileMatrices.Count; j++)
                    {
                        wmts.layerTileSets[i].resTypes.Add(new WMTS.ResType());
                        wmts.layerTileSets[i].resTypes[j].id =
                            wmts.selectedLayers[i].TileMatrixSets[0].TileMatrices[j].Id;
                        wmts.layerTileSets[i].resTypes[j].scaleDenom =
                            wmts.selectedLayers[i].TileMatrixSets[0].TileMatrices[j].ScaleDenominator;
                        wmts.layerTileSets[i].resTypes[j].resolution =
                            wmts.layerTileSets[i].resTypes[j].scaleDenom / 397569610;
                        wmts.layerTileSets[i].resTypes[j].matrixWidth = 2 ^ j;
                        wmts.layerTileSets[i].resTypes[j].matrixHeight = 2 ^ j;
                    }
                }

                // Set AOI window Titles
                aoiWin.panelVars.titleList = new List<string>();
                foreach (WMTS.TileSetVariables tileSet in wmts.layerTileSets)
                {
                    str = tileSet.layerTitle + "\n" +
                            "Tile Set: " + tileSet.tileSetTitle + "\t"
                            + "Zoom Levels: " + tileSet.zoomLvls;
                    aoiWin.panelVars.titleList.Add(str);
                }
                if (aoiWin.panelVars.titleList != null)
                {
                    aoiWin.ImageryTitle.ItemsSource = aoiWin.panelVars.titleList;
                    aoiWin.ImageryTitle.SelectedIndex = 0;
                }
                else
                    aoiWin = null;

                // Set the zoom level specific to image selected
                ResetZoomLevels(aoiWin.ImageryTitle.SelectedIndex, "Betsy");
            }
        }   // end ResetLayerInfo


        // Reset Zoom list, pixel array size and max zoom 
        // using current extent values
        public void ResetZoomLevels(int idx, string flag)
        {
            int i;
            flag = "Select Zoom Level";

            // Calculate range
            double latDiff = Math.Abs(Convert.ToDouble(aoiWin.MaxLat.Text) -
                                Convert.ToDouble(aoiWin.MinLat.Text));
            double lonDiff = Math.Abs(Convert.ToDouble(aoiWin.MaxLon.Text) -
                                Convert.ToDouble(aoiWin.MinLon.Text));

            // Make sure the list exists
            if (wmts.layerTileSets != null)
            {
                if (aoiWin.ZoomCombo.SelectedIndex < 0)
                    aoiWin.ZoomCombo.SelectedIndex = 1;

                aoiWin.panelVars.resolutionList = new List<string>();

                // Set zoom level list for given index
                for (i = 0; i < wmts.layerTileSets[idx].resTypes.Count; i++)
                    aoiWin.panelVars.resolutionList.Add(wmts.layerTileSets[idx].resTypes[i].id);

                // Get the  max zoom level for this tile set
                Debug.WriteLine("getting max zoom level");
                int maxIdx = -1;
                for (i = 0; i < wmts.layerTileSets[idx].resTypes.Count; i++)
                {
                    maxIdx++;
                    double res = wmts.layerTileSets[idx].resTypes[i].resolution;
                    double latPixels = 10.0 * (latDiff / res);
                    double lonPixels = 10.0 * (lonDiff / res);

                    if ((latPixels > PIX_MAX) || (lonPixels > PIX_MAX))
                    {
                        wmts.layerTileSets[idx].maxZoom = maxIdx - 1;
                        break;
                    }
                }   // end for i
                Debug.WriteLine("max zoom idx = " + maxIdx);
                if (maxIdx == wmts.layerTileSets[idx].resTypes.Count - 1)
                    wmts.layerTileSets[idx].maxZoom = maxIdx;

                // Loop backwards to find min
                for (i = wmts.layerTileSets[idx].resTypes.Count - 1; i > -1; i--)
                {
                    double res = wmts.layerTileSets[idx].resTypes[i].resolution;
                    double latPixels = 10.0 * (latDiff / res);
                    double lonPixels = 10.0 * (lonDiff / res);

                    if ((latPixels < PIX_MIN) || (lonPixels < PIX_MIN))
                    {
                        wmts.layerTileSets[idx].minZoom = i + 1;
                        break;
                    }
                }
                if (i < 1)
                    wmts.layerTileSets[idx].minZoom = 1;
                Debug.WriteLine("min zoom idx = " + wmts.layerTileSets[idx].minZoom);
            }   // end find min and max zoom

            aoiWin.panelVars.resolutionList[wmts.layerTileSets[idx].minZoom] += " - Min";
            aoiWin.panelVars.resolutionList[wmts.layerTileSets[idx].maxZoom] += " - Max";
            aoiWin.panelVars.resolutionList[0] = flag;
            aoiWin.ZoomCombo.ItemsSource = aoiWin.panelVars.resolutionList;
            aoiWin.ZoomCombo.SelectedIndex = 0;

            // Update image pixel size
            int selectedIdx = aoiWin.ZoomCombo.SelectedIndex;
            int numLatPix = 10 * Convert.ToInt32(latDiff * wmts.layerTileSets[idx].resTypes[selectedIdx].resolution);
            int numLonPix = 10 * Convert.ToInt32(lonDiff * wmts.layerTileSets[idx].resTypes[selectedIdx].resolution);
            string str = numLatPix.ToString() + "px x " + numLonPix.ToString() + "px";
            aoiWin.PixelSize.Text = str;

            // Get approx file size in MB
            double nBytes = Convert.ToDouble(numLatPix * numLonPix * 3) / 8.0;
            nBytes /= 1048576.0;
            aoiWin.RawSizeText.Text = nBytes.ToString("F4") + "MB";
        }   // end reset zoom levels


        // Add the slected item to the download list
        // move to AOI_WIndow file?
        public void AddDownloadItem()
        {
            int idx, titleIdx;
            int zoomIdx, minZoom, maxZoom;
            int pixelWidth, pixelHeight;
            string str;
            double resolution;

            titleIdx = aoiWin.ImageryTitle.SelectedIndex;
            zoomIdx = aoiWin.ZoomCombo.SelectedIndex;

            minZoom = wmts.layerTileSets[titleIdx].minZoom;
            maxZoom = wmts.layerTileSets[titleIdx].maxZoom;
            if (zoomIdx == 0)
                MessageBox.Show("Please select a zoom level",
                                 "ZOOM LEVEL SELECTION ERROR");
            else if (zoomIdx > maxZoom)
                MessageBox.Show("Pixel extent exceeds maximum - select a lower zoom level",
                                 "ZOOM LEVEL SELECTION ERROR");
            else if (zoomIdx < minZoom)
                MessageBox.Show("Pixel extent below minimum - select a higher zoom level",
                                 "ZOOM LEVEL SELECTION ERROR");
            else
            {
                wmts.downloadInfo.Add(new WMTS.DownloadLayerInfo());
                idx = wmts.downloadInfo.Count - 1;

                // Store layer variables required for later processing    
                wmts.downloadInfo[idx].bbox[0] = Convert.ToDouble(aoiWin.MinLat.Text);
                wmts.downloadInfo[idx].bbox[1] = Convert.ToDouble(aoiWin.MinLon.Text);
                wmts.downloadInfo[idx].bbox[2] = Convert.ToDouble(aoiWin.MaxLat.Text);
                wmts.downloadInfo[idx].bbox[3] = Convert.ToDouble(aoiWin.MaxLon.Text);
                resolution = wmts.layerTileSets[titleIdx].resTypes[zoomIdx].resolution;
                pixelHeight = 10 * Math.Abs(Convert.ToInt32((wmts.downloadInfo[idx].bbox[2] - wmts.downloadInfo[idx].bbox[0]) / resolution));
                pixelWidth = 10 * Math.Abs(Convert.ToInt32((wmts.downloadInfo[idx].bbox[3] - wmts.downloadInfo[idx].bbox[1]) / resolution));

                wmts.downloadInfo[idx].info = wmts.selectedLayers[titleIdx];
                wmts.downloadInfo[idx].title = wmts.layerTileSets[titleIdx].layerTitle;
                wmts.downloadInfo[idx].name = wmts.layerTileSets[titleIdx].layerName;
                wmts.downloadInfo[idx].latency = wmsUriStartup.latency;
                wmts.downloadInfo[idx].crs = wmsUriStartup.EPSG;
                wmts.downloadInfo[idx].zoomLvl = aoiWin.ZoomCombo.SelectedIndex;
                wmts.downloadInfo[idx].resolution = wmts.layerTileSets[titleIdx].resTypes[zoomIdx].resolution;
                wmts.downloadInfo[idx].tileHeight = wmts.layerTileSets[titleIdx].tileHeight;
                wmts.downloadInfo[idx].tileWidth = wmts.layerTileSets[titleIdx].tileWidth;
                wmts.downloadInfo[idx].matrixWidth = wmts.layerTileSets[titleIdx].resTypes[aoiWin.ZoomCombo.SelectedIndex].matrixWidth;
                wmts.downloadInfo[idx].matrixHeight = wmts.downloadInfo[idx].matrixWidth;
                wmts.downloadInfo[idx].pixelWidth = pixelWidth;
                wmts.downloadInfo[idx].pixelHeight = pixelHeight;
                wmts.downloadInfo[idx].nMBytes = (Convert.ToDouble(pixelHeight * pixelWidth * 3) / 8.0) / 1048576.0;

                // Make sure the date is in the correct format yyyy-mm-dd
                wmts.downloadInfo[idx].date = aoiWin.Date.Text;
                String[] words = aoiWin.Date.Text.Split("-");
                int year = Convert.ToInt32(words[0]);
                int month = Convert.ToInt32(words[1]);
                int day = Convert.ToInt32(words[2]);
                wmts.downloadInfo[idx].date = year.ToString("D4") + "-" + month.ToString("D2") + "-" + day.ToString("D2");

                str = wmts.downloadInfo[idx].title + "\n";
                str += "Zoom Level: " + wmts.downloadInfo[idx].zoomLvl.ToString();
                str += "\tDate: " + wmts.downloadInfo[idx].date;
                str += "\nSize: " + wmts.downloadInfo[idx].pixelWidth.ToString() + " px x "
                                  + wmts.downloadInfo[idx].pixelHeight.ToString() + " px";
                str += "  -  " + wmts.downloadInfo[idx].nMBytes.ToString("F4") + " MB";
                str += "\nExtent: " + wmts.downloadInfo[idx].bbox[0].ToString("F4") + ", "
                                    + wmts.downloadInfo[idx].bbox[1].ToString("F4") + "   "
                                    + wmts.downloadInfo[idx].bbox[2].ToString("F4") + ", "
                                    + wmts.downloadInfo[idx].bbox[3].ToString("F4");

                MessageBox.Show(str, "ADDED TO CART");
            } // edn add valid download item

        } // end AddDownloadItem

        private void ClearButtonClick(object sender, RoutedEventArgs e)
        {
            // Remove all graphics from the graphics overlay
            BasemapView.SketchEditor.ClearGeometry();
            sketchOverlay.Graphics.Clear();
            haveSketch = false;

            // Set buttons and popups
            AOIDraw.IsEnabled = true;
            AOISelect.IsEnabled = false;
            AOIClear.IsEnabled = false;
            AOICancel.IsEnabled = true;

        }   // end ClearButtonClick

        // same as Back to map
        private void CancelButtonClick(object sender, RoutedEventArgs e)
        {
            sketchOverlay.Graphics.Clear();
            if (BasemapView.SketchEditor.CancelCommand.CanExecute(null))
                BasemapView.SketchEditor.CancelCommand.CanExecute(null);
            haveSketch = false;

            // Set buttons and popups
            AOIDraw.IsEnabled = true;
            AOISelect.IsEnabled = false;
            AOIClear.IsEnabled = false;
            AOICancel.IsEnabled = true;
        }   // end CancelButtonClick


        private void UpdateDatesBtn_Click(object sender, RoutedEventArgs e)
        {
            String str;
            DateTime start = startDate_DP.DisplayDate;
            // DateTime end = endDate_DP.DisplayDate;   // add back in when animation is considered
            // str = "Start Date:" + start.Date + "\n" +
            //       "End Date: " + end.Date;
            str = "Date is now\n" + start.Date;
            MessageBox.Show(str, "NEW DATE");
            UpdateDatesBtn.IsEnabled = false;
        }   // end ReloadLayerBtn_Click


        private void CheckoutBtn_Click(object sender, RoutedEventArgs e)
        {
            // Set up windows
            //mainWin.Topmost = false;
            //mainWin.aoiWin.Topmost = false;
            mainWin.aoiWin.Hide();

            // Create a new Confirm window
            mainWin.confirmItemsWin = new ConfirmItemsWin();
            mainWin.confirmItemsWin.ShowDialog();
            mainWin.confirmItemsWin.Activate();
            //mainWin.confirmItemsWin.Topmost = true;
        }   // end CheckoutBtn_Click

        private void QuitBtn_Click(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("Shutting dowm from Main Quit Button");
            Application.Current.Shutdown();
        }   // end QuitBtn_Click


        private void SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (UpdateDatesBtn != null)
            {
                UpdateDatesBtn.IsEnabled = true;
            }
        }   // end SelectedDateChanged

    }   // end  partial class MainWindow

}   // end namespace QBasket_demo