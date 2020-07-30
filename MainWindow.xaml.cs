using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Ogc;
using Esri.ArcGISRuntime.UI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;

namespace QBasket_demo
{
    /// Namespace Classes
    ///
    /// <summary>
    /// Logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //---------------------
        // MAIN WINDOW GLOBALS
        //---------------------
        // Hold a list of Layer view models
        private ObservableCollection<LayerInfoVM> _layerInfoOC =
            new ObservableCollection<LayerInfoVM>();
        private ObservableCollection<LayerInfoVM> tempList =
            new ObservableCollection<LayerInfoVM>();
        // WMS service
        private WmsService serviceWMS;
        private Uri serviceURI;
        public WmsUriString wmsUriStartup = new WmsUriString();
        // public WMTS wmts = new WMTS();


        // Selected layer list
        public List<WmsLayerInfo> selectedLayers;
        public ArcGISMapImageLayer baseImageLayer;

        // Date variables
        private DateTime _today = DateTime.Now;
        private DateTime _firstDate = DateTime.Now;

        // Windows
        //public StartUpWindow winStartUp;
        public static MainWindow mainWin;
        public AOIWindow aoiWin;
        public ConfirmItemsWin confirmItemsWin;

        // Flags to simplify element state checking
        public bool haveSketch = false;
        public bool haveLayer = false;

        ///------------------------------------------
        /// <summary>
        /// MainWindow
        /// Main program starts here
        /// </summary>
        //--------------------------------------------
        public MainWindow()
        {
            // Create the window
            InitializeComponent();
            mainWin = this;

            /*
            // Create and show start up dialog
            winStartUp = new StartUpWindow();
            winStartUp.ShowDialog();
            winStartUp.Activate();

            // Exit application if the user exited the startup window
            if (winStartUp == null)
                Application.Current.Shutdown();
            */
            // Set Wms Capability uri
            SetWmsUri();

            // Initialize the Main window model view
            startDate_DP.SelectedDate = _firstDate;
            InitializeWMSLayer_VM();

            // Set map to current location
            BasemapView.LocationDisplay.IsEnabled = true;
            BasemapView.LocationDisplay.ShowLocation = false;
            BasemapView.LocationDisplay.AutoPanMode = LocationDisplayAutoPanMode.Recenter;
            BasemapView.LocationDisplay.InitialZoomScale = 2000000;
            BasemapView.LocationDisplay.IsEnabled = false;

            // Activate main window and set flags
            mainWin.Activate();
            haveSketch = false;
            haveLayer = false;

            // Initialize panels: AOI Win, Confirm, and Download - PANEL
            // Set panel visibility

            // Initialize AOI sketch editor - in AOI_draw.cs
            InitializeAOIsketch();

            // Create and hide AOI Window
            aoiWin = new AOIWindow();
            aoiWin.Hide();

        }   // end MainWindow

        /// <summary>
        /// Open WMS service and store the list of imagery
        /// preserving hierarchy, if any
        /// Source: Runtime wms Service catalog
        /// MOD - pass in serviceurl
        /// </summary>
        private async void InitializeWMSLayer_VM()
        {
            // Initialize the display with a basemap
            // Reset Map projection if applicable
            Map myMap = new Map(SpatialReference.Create(4326));
            baseImageLayer = new ArcGISMapImageLayer(new Uri(
                "https://services.arcgisonline.com/arcgis/rest/services/World_Imagery/MapServer"));

            // Add baseImageLayer to the Map
            myMap.OperationalLayers.Add(baseImageLayer);

            // Assign the map to the MapView
            BasemapView.Map = myMap;

            // Create the WMS Service Load data from the URI
            // Create WMS Service service with default uri
            serviceURI = new Uri(wmsUriStartup.capableUri);
            serviceWMS = new WmsService(serviceURI);

            // If service can load, initialize the app
            try
            {
                // Load the WMS Service.
                await serviceWMS.LoadAsync();

                // Get the service info (metadata) from the service.
                WmsServiceInfo info = serviceWMS.ServiceInfo;

                // Process info to get information for all the layers
                // for the given serviceUri
                // LayerInfos gets a list of sublayers for a given layer
                IList<LayerInfoVM> temp = new List<LayerInfoVM>();
                foreach (WmsLayerInfo layerInfo in info.LayerInfos)
                    LayerInfoVM.BuildLayerInfoList(new LayerInfoVM(layerInfo, null, false), _layerInfoOC);

                // NASA GIBS Specific
                #region NASA
                string epsg = wmsUriStartup.EPSG.Substring(0, 4) + ":"
                              + wmsUriStartup.EPSG.Substring(4, 4);
                ProductLabel.Content = "NASA GIBS for EODIS: "
                                       + wmsUriStartup.latency.ToUpper() + " - "
                                       + epsg.ToUpper();
                Sort_NASA_GIBS(_layerInfoOC, out temp);
                _layerInfoOC.Clear();

                foreach (LayerInfoVM layerInfo in temp)
                    _layerInfoOC.Add(layerInfo);
                #endregion NASA

                // Update the map display based on the viewModel.
                UpdateViewModel(_layerInfoOC);

                // Update Product List
                //ProductTreeView.ItemsSource = _layerInfoOC;
                ProductList.ItemsSource = _layerInfoOC;
            }   // end try
            catch (Exception e)
            {
                MessageBox.Show(e.ToString(), "WMS SERVER LOAD ERROR");
            }
        }   // end InitializeWMSLayer_VM


        ///
        /// Use start up variables to define wms Uris
        #region NASA
        public void SetWmsUri()
        {
            // Set latency and projection
            wmsUriStartup.latency = "best";
            wmsUriStartup.EPSG = "epsg4326";

            // Set and parse dates
            DateTime date = startDate_DP.SelectedDate.Value;
            if (date == null)
            {
                date = DateTime.Now;
            }
            String year = date.Year.ToString();
            String month = date.Month.ToString();
            String day = date.Day.ToString();
            wmsUriStartup.time.startDate = year + "-" + month + "-" + day;
            wmsUriStartup.time.value =
                wmsUriStartup.time.startDate + "/" + "/P1D";
            wmsUriStartup.capableUri =
                wmsUriStartup.baseUri + "/" + wmsUriStartup.service + "/" +
                wmsUriStartup.EPSG + "/" + wmsUriStartup.latency + "/" +
                wmsUriStartup.service + ".cgi?SERVICE=WMS&REQUEST=GetCapabilities&VERSION=1.3.0";

        }   // end SetWmsUri
        #endregion NASA


        /// <summary>
        /// Checkbox checked callback
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ToggleButton_OnChecked(object sender, RoutedEventArgs e)
        {
            // Update the map.
            // Note: updating selection is handled by the IsEnabled property
            UpdateViewModel(_layerInfoOC);

            // If no layer is selected, turn off select button
            if (haveLayer == false)
                AOISelect.IsEnabled = false;

            // Have a layer
            else
            {
                // if there is a sketch, turn on select button
                if (haveSketch)
                    AOISelect.IsEnabled = true;
                else
                    AOISelect.IsEnabled = false;
            }
        }   // end ToggleButton_OnChecked


        /// <summary>
        /// Updates the map view model
        /// </summary>
        private void UpdateViewModel(ObservableCollection<LayerInfoVM> productList)
        {
            // Remove all existing layers and redraw map
            if (BasemapView.Map.OperationalLayers.Count > 1)
            {
                BasemapView.Map.OperationalLayers.Clear();
                BasemapView.Map.OperationalLayers.Add(baseImageLayer);
            }

            // Get a list of selected LayerInfos
            selectedLayers =
               new List<WmsLayerInfo>(productList.Where(checkBox => checkBox.Selected).Select(checkBox => checkBox.Info).ToList());

            // Return if no layers are selected.
            if (!selectedLayers.Any())
            {
                haveLayer = false;
                return;
            }
            else
                haveLayer = true;

            WmsLayer showLayers = new WmsLayer(selectedLayers);

            // Add the layer(s) to the map.
            BasemapView.Map.OperationalLayers.Add(showLayers);

        }   // end UpdateViewModel


        // Window shutdown routine - set in Xaml file
        private void QBasketWindow_Closing(object sender, CancelEventArgs e)
        {
            Application.Current.Shutdown();
        }

        /// Sort the layer list
        /// Specific to NASA GIBS:
        /// Orbit tracks not included
        /// Want a list of items with no children
        #region NASA
        public static void Sort_NASA_GIBS(IList<LayerInfoVM> unsorted, out IList<LayerInfoVM> sorted)
        {
            LayerInfoVM firstLayer;
            IList<LayerInfoVM> temp = new List<LayerInfoVM>();

            firstLayer = unsorted[0];
            for (int i = 1; i < unsorted.Count; i++)
                temp.Add(unsorted[i]);
            sorted = temp.OrderBy(o => o.Info.Title).ToList();

        }   // end Sort_NASA_GIBS
        #endregion
    }   // end MainWindow partial class


    /// <summary>
    /// Base URI components class
    /// </summary>
    public class WmsUriString
    {
        // GIBS endpoint format for WMS
        // https://earthdata.nasa.gov/{service}/{epsg}/{latency}
        public string baseUri = "https://gibs.earthdata.nasa.gov";
        public string service = "wms";

        // Start up variables
        public string EPSG = "";
        public string latency = "";
        public TimeClass time = new TimeClass();

        // Used to store layer info and layers Uris
        public string capableUri = "";
        public string layersUri = "";
    }   // end WmsUriString

    /// <summary>
    /// GIBS Time Components
    /// </summary>
    public class TimeClass
    {
        // GIBS constants
        public string UOM = "ISO8601";
        public string current = "false";
        public string nearestValue = "1";

        // Set in Start up window
        public string startDate = String.Empty;
        public string endDate = String.Empty;

        // Defined in SetWmsUri
        public string value = String.Empty;
    }   // end timeClass


    /// --------------------------------------------------
    /// Code Below taken from Runtime WMS Service Catalog
    /// --------------------------------------------------
    /// This is a ViewModel class for maintaining the
    /// state of a layer selection.
    /// LayerInfo has two public elements:
    ///     Info: WmsLayerInfo type
    ///     Parent: LayerInfoVM type - who's your mama?
    public class LayerInfoVM : INotifyPropertyChanged
    {
        // Children are Lists of view models
        public List<LayerInfoVM> Children { get; set; }

        // Each Parent is a view model
        private LayerInfoVM Parent { get; set; }

        // Declare members
        public WmsLayerInfo Info { get; set; }
        public bool Selected { get; set; }
        public String Title { get; set; }

        /// Layer Info View Model
        public LayerInfoVM(WmsLayerInfo info, LayerInfoVM parent, bool selected)
        {
            Info = info;
            Parent = parent;
            Selected = selected;
            Title = info.Title;
        }   // end LayerInfoClass

        /// Register main callback handler for property changes
        public event PropertyChangedEventHandler PropertyChanged;

        // Override ToString to enhance display formatting.
        public override string ToString()
        {
            return Info.Title;
        }   // end ToString

        // True if a layer is selected for display.
        private bool _isEnabled;
        public bool IsEnabled
        {
            get { return _isEnabled; }
            set { Select(value); }
        }   // end IsEnabled


        // Select this layer and all child layers.
        private void Select(bool isSelected = true)
        {
            _isEnabled = isSelected;
            if (Children == null)
            {
                return;
            }

            // Select all child layers
            foreach (var child in Children)
            {
                child.Select(isSelected);
            }
            OnPropertyChanged("IsEnabled");
        }   // end Select


        /// Store the selected layers to be fetched
        private static object _selectedItem = null;
        public static object SelectedItem
        {
            get { return _selectedItem; }
            private set
            {
                if (_selectedItem != value)
                {
                    _selectedItem = value;
                }
            }
        }   // end SelectedItem


        /// Build the layer OC recursively
        public static void BuildLayerInfoList(LayerInfoVM root,
                                              IList<LayerInfoVM> result)
        {
            // Add the root node to the result list.
            result.Add(root);

            // Initialize the child collection for the root.
            root.Children = new List<LayerInfoVM>();

            // Recursively add sublayers.
            foreach (WmsLayerInfo layer in root.Info.LayerInfos)
            {

                if (!layer.Title.Contains("OrbitTracks"))
                {

                    // Create the view model for the sublayer.
                    LayerInfoVM layerVM = new LayerInfoVM(layer, root, false);

                    // Add the sublayer to the root's sublayer collection.
                    root.Children.Add(layerVM);

                    // Recursively add children.
                    BuildLayerInfoList(layerVM, result);

                }
            }   // end foreach
        }   // end BuildLayerInfoList


        // Standard event parser -
        //      calls property's respective PropertyChangedEventArgs
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }   // end LayerInfoClass

} // end namespace


/* GIBS defaults from worldview code
  geographic_bad_size = 12088

  param_dict = {
  'base': {
    'REQUEST': 'GetSnapshot',
    'FORMAT': 'image/jpeg'
  },
  'geographic': {
    'BBOX': '-90,-180,90,180',
    'CRS': 'EPSG:4326',
    'WIDTH': '768',
    'HEIGHT': '384'
  },
  'arctic': {
    'BBOX': '-4195000,-4195000,4195000,4195000',
    'CRS': 'EPSG:3413',
    'WIDTH': '512',
    'HEIGHT': '512'
  },
  'antarctic': {
    'BBOX': '-4195000,-4195000,4195000,4195000',
    'CRS': 'EPSG:3031',
    'WIDTH': '512',
    'HEIGHT': '512'
  }
}
*/
