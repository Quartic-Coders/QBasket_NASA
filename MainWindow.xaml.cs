
// TODO List
//Bugs - 
// AOIwin -> Draw -> Select - errors on select null list
// Error after Besty - happens when no layers are selected after initial set up
// WHen the imagery title list is null - xaml throws a binding error becuase it cannot find 
//      its respective image title list - > never let list get null!
//  System.Windows.Data Error: 4 : 
//      Cannot find source for binding with reference 'RelativeSource FindAncestor, 
//      AncestorType='System.Windows.Controls.ItemsControl', AncestorLevel='1''. 
//      BindingExpression:Path=HorizontalContentAlignment; 
//      DataItem=null; target element is 'ComboBoxItem' (Name='');
//      target property is 'HorizontalContentAlignment' (type 'HorizontalAlignment')
//-----------------------------------------
// Is the adopted wms vm the best - can the vm be reimplemented using a
//  similar approach to the confirmItems Panel Checkbox list?
//
// Sort imagery list - list is sorted but lose something 
// above may help
// change date on aoiwin to date selection - not text
// need to do to make sure date is always in correct format
//
// Output options local, arcgis online, cloud account
//
// Create login in panel for arcgis online account
//
// Responsive ui - ongoing updates
// Implement date options
// Animate layers
// find double range slider for date
// find better slider - add labels
// animate time slider layer - two-way bind with other dates
// add basemap options
// WMTS
// fetch tiles
//      

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

        // WMS service
        private WmsService serviceWMS;
        private Uri serviceURI;
        public WmsUriString wmsUriStartup = new WmsUriString();
        // public WMTS wmts = new WMTS();


        // Selected layer list
        public List<WmsLayerInfo> sortedLayers;
        public List<WmsLayerInfo> selectedLayers;
        ArcGISMapImageLayer baseImageLayer;

        // Date variables
        private DateTime _today = DateTime.Now;
        private DateTime _firstDate = DateTime.Now;

        // Windows
        public StartUpWindow winStartUp;
        public static MainWindow mainWin;
        public AOIWindow aoiWin;
        public ConfirmItemsWin confirmItemsWin;
        public OutputFormatWindow outputFormatWin;

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

            // Create and show start up dialog
            winStartUp = new StartUpWindow();
            winStartUp.ShowDialog();
            // winStartUp.Topmost = true;
            winStartUp.Activate();

            // Exit application if the user exited the startup window
            if (winStartUp == null)
                Application.Current.Shutdown();
            /*
            else
            {
                Debug.WriteLine("Latency = " + startUpVars.getLatency());
                Debug.WriteLine("Projection = " + startUpVars.getProjection());
            }
            */
            // Process Starup window into meaningful URIs
            GetWmsUri();

            // Initialize the Main window model view
            startDate_DP.SelectedDate = _firstDate;
            // endDate_DP.SelectedDate = _today;    // add back in when animation is considered
            InitializeWMSLayer_VM();

            // Set map to current location
            BasemapView.LocationDisplay.IsEnabled = true;
            BasemapView.LocationDisplay.ShowLocation = false;
            BasemapView.LocationDisplay.AutoPanMode = LocationDisplayAutoPanMode.Recenter;
            BasemapView.LocationDisplay.InitialZoomScale = 2000000;
            BasemapView.LocationDisplay.IsEnabled = false;

            // Initialize AOI sketch - in AOI_draw.cs
            InitializeAOIsketch();

            // Create and hide AOI Window
            aoiWin = new AOIWindow();
            aoiWin.Hide();

            // Put the main window on top and set flags
            // mainWin.Topmost = true;
            mainWin.Activate();
            haveSketch = false;
            haveLayer = false;

        }   // end MainWindow

        /// <summary>
        /// Open WMS service and store the list of imagery
        /// perserving heirarchy, if any
        /// Source: Runtime wms Service catalog
        /// MOD - pass in serviceurl
        /// </summary>
        private async void InitializeWMSLayer_VM()
        {
            // Initialize the display with a basemap
            // BasemapView is UI esri mapview name in XAML
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
                foreach (var layerInfo in info.LayerInfos)
                    LayerInfoVM.BuildLayerInfoList(new LayerInfoVM(layerInfo, null, false), _layerInfoOC);


                // Sort Layers
                // This sorts list but list does not get passed to Itemsource
                /*
                List<WmsLayerInfo> sortedWmsInfo = new List<WmsLayerInfo>();
                ObservableCollection<LayerInfoVM> SortedList = new ObservableCollection<LayerInfoVM>( _layerInfoOC.OrderBy(o => o.Info.Title).ToList())
                _layerInfoOC = new ObservableCollection<LayerInfoVM>(SortedList);
                foreach (LayerInfoVM layerInfo in _layerInfoOC)
                    Debug.WriteLine(layerInfo.Info.Title + "  " + layerInfo.Info.Name);
                Debug.WriteLine("Counts: " + _layerInfoOC.Count + "  " + SortedList.Count);
                */

                // Update the map display based on the viewModel.
                UpdateViewModel(_layerInfoOC);

                // Update UI element
                ProductLabel.Content = "NASA GIBS - " + wmsUriStartup.EPSG + " - "
                                                      + wmsUriStartup.latency;
                ProductTreeView.ItemsSource = _layerInfoOC.Take(1);
            }   // end try
            catch (Exception e)
            {
                MessageBox.Show(e.ToString(), "WMS SERVER LOAD ERROR");
            }
        }   // end InitializeWMSLayer_VM


        ///
        /// Use start up variables to define wms Uris
        public void GetWmsUri()
        {
            // Retrieve URL components from the start up window
            // Set latency
            if (winStartUp.bestRB.IsChecked == true)
                wmsUriStartup.latency = "best";
            else if (winStartUp.stdRB.IsChecked == true)
                wmsUriStartup.latency = "std";
            else if (winStartUp.nrtRB.IsChecked == true)
                wmsUriStartup.latency = "nrt";
            else
                wmsUriStartup.latency = "all";

            // Set projection
            if (winStartUp.epsg4326_RB.IsChecked == true)
                wmsUriStartup.EPSG = "epsg4326";
            else if (winStartUp.epsg3857_RB.IsChecked == true)
                wmsUriStartup.EPSG = "epsg3857";
            else if (winStartUp.epsg3413_RB.IsChecked == true)
                wmsUriStartup.EPSG = "epsg3413";
            else if (winStartUp.epsg3031_RB.IsChecked == true)
                wmsUriStartup.EPSG = "epsg3031";

            // Set and parse dates
            DateTime date = startDate_DP.SelectedDate.Value;
            String year = date.Year.ToString();
            String month = date.Month.ToString();
            String day = date.Day.ToString();
            wmsUriStartup.time.startDate = year + "-" + month + "-" + day;
            wmsUriStartup.time.value =
                wmsUriStartup.time.startDate + "/" + "/P1D";
            /*
             * Put back in when animation is considered
             * 
            date = endDate_DP.SelectedDate.Value;
            year = date.Year.ToString();
            month = date.Month.ToString();
            day = date.Day.ToString();
            wmsUriStartup.time.endDate = year + "-" + month + "-" + day;

            wmsUriStartup.time.value =
                wmsUriStartup.time.startDate + "/" + wmsUriStartup.time.endDate + "/P1D";
            */
            wmsUriStartup.capableUri =
                wmsUriStartup.baseUri + "/" + wmsUriStartup.service + "/" +
                wmsUriStartup.EPSG + "/" + wmsUriStartup.latency + "/" +
                wmsUriStartup.service + ".cgi?SERVICE=WMS&REQUEST=GetCapabilities&VERSION=1.3.0";

        }   // end GetWmsUri


        /// <summary>
        /// Animate time slider w/selected range
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DateSliderValueChanged(object sender, RoutedEventArgs e)
        {

        }

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
            if (haveSketch && haveLayer)
                AOISelect.IsEnabled = true;
            else
                AOISelect.IsEnabled = false;
        }   // end ToggleButton_OnChecked


        /// <summary>
        /// Updates the map view model
        /// </summary>
        private void UpdateViewModel(ObservableCollection<LayerInfoVM> displayList)
        {
            // Remove all existing layers and redraw map
            if (BasemapView.Map.OperationalLayers.Count > 1)
            {
                BasemapView.Map.OperationalLayers.Clear();
                BasemapView.Map.OperationalLayers.Add(baseImageLayer);
            }

            // Get a list of selected LayerInfos.
            selectedLayers =
                new List<WmsLayerInfo>(displayList.Where(vm => vm.IsEnabled).Select(vm => vm.Info).ToList());

            // Return if no layers are selected.
            if (!selectedLayers.Any())
            {
                haveLayer = false;
                return;
            }
            else
                haveLayer = true;

            // Create a new WmsLayer from the selected layers.
            WmsLayer showLayers = new WmsLayer(selectedLayers);

            // Add the layer(s) to the map.
            BasemapView.Map.OperationalLayers.Add(showLayers);

        }   // end UpdateViewModel

        private void QBasketWindow_Closing(object sender, CancelEventArgs e)
        {
            Debug.WriteLine("Closing from Main window");
            Application.Current.Shutdown();
        }


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
        public string startDate = "2013-05-29";
        public string endDate = "2013-12-21";

        // Defined in GetWmsUri
        public string value = "2012-05-08/2013-05-29/P1D";
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
                if ((layer.Title.Contains("Land Surface Reflectance")) ||
                     (layer.Title.Contains("Digital")))
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
   arctic_bad_size = 9949
  antarctic_bad_size = 4060
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
