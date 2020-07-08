using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Ogc;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.UI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using static QBasket_demo.MainWindow;

namespace QBasket_demo
{
    /// <summary>
    /// Interaction logic for Snapshot.xaml
    /// </summary>
    public partial class AOIWindow : Window
    {
        public PanelVariables panelVars = new PanelVariables();
        public WmtsTileMatrixSet tileMatixSet;
        public AOIWindow()
        {
            InitializeComponent();
        }   // end initialize

        // Dynamic Panel variables 
        public class PanelVariables
        {
            private List<string> _resolutionList = new List<string>();
            private List<string> _titleList = new List<string>();

            public List<string> resolutionList
            {
                get => _resolutionList;
                set { _resolutionList = value; }
            }

            public List<string> titleList
            {
                get => _titleList;
                set { _titleList = value; }
            }
        }   // end PanelVariables

        // Checkout (Review Cart) button callback
        private void CheckoutBtn_Click(object sender, RoutedEventArgs e)
        {
            // Set up windows
            mainWin.aoiWin.Hide();

            // Create a new Confirm window
            mainWin.confirmItemsWin = new ConfirmItemsWin();
            mainWin.confirmItemsWin.ShowDialog();
            mainWin.confirmItemsWin.Activate();
        }   // end CheckoutBtn_Click


        // Exit button callback
        private void ExitBtn_Click(object sender, RoutedEventArgs e)
        {
            Close();
            Application.Current.Shutdown();
        }   // end ExitBtn_Click


        // Return button callback
        private void ReturnBtn_Click(object sender, RoutedEventArgs e)
        {
            mainWin.AOISelect.IsEnabled = true;

            if (mainWin.confirmItemsWin != null)
                if (mainWin.confirmItemsWin.IsVisible)
                    mainWin.confirmItemsWin.Hide();
            mainWin.aoiWin.Hide();

            // Show main window w/ Select button turned on
            //mainWin.ShowDialog();
            mainWin.Activate();
            mainWin.AOISelect.IsEnabled = true;
        }   // end ReturnBtn_Click


        // Executed when any change is made to any Extent textbox
        private void Extent_Changed(object sender, RoutedEventArgs e)
        {
            if (mainWin.haveSketch && mainWin.haveLayer)
            {
                Decimal outNum;

                // Make sure the number is a valid number
                String str = ((TextBox)sender).Text;
                bool isNumber = decimal.TryParse(str, out outNum);
                if (isNumber)
                {
                    RedrawAOI(sender);
                    Zoom_SelectionChanged(sender, null);
                }
            }
        }   // end Extent_Changed


        // Redraw graphic to match new extent
        private void RedrawAOI(object sender)
        {
            string str;
            decimal outNum;
            bool isNumber;
            double minLat, minLon;
            double maxLat, maxLon;
            Envelope aoiEnv;

            // Make sure the number is a valid number
            str = ((TextBox)sender).Text;
            isNumber = decimal.TryParse(str, out outNum);

            if (isNumber)
            {
                minLat = Convert.ToDouble(MinLat.Text);
                maxLat = Convert.ToDouble(MaxLat.Text);
                minLon = Convert.ToDouble(MinLon.Text);
                maxLon = Convert.ToDouble(MaxLon.Text);

                aoiEnv = new Envelope(minLon, minLat, maxLon, maxLat);

                // Create a graphic to display the specified geometry
                GraphicsOverlay graphics_Overlay = new GraphicsOverlay();
                Symbol symbol = null;
                symbol = new SimpleFillSymbol()
                {
                    Outline = mainWin.AOI_outline,
                    Style = SimpleFillSymbolStyle.Null
                };

                // Create the graphic with polyline and symbol
                Graphic graphic = new Graphic(aoiEnv, symbol);

                // Add graphic to the graphics overlay
                mainWin.sketchOverlay.Graphics.Clear();
                mainWin.sketchOverlay.Graphics.Add(graphic);
            }
        }   // end RedrawAOI


        // Zoom Combo selection callback
        private void Zoom_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            double latDiff, lonDiff;
            double nBytes, res;
            int idx, zoomIdx;
            int numLatPix, numLonPix;
            string str;
            bool isNumber = true;
            decimal outNum;

            // if e is null, then routine was triggered by extent changed
            if (e == null)
            {
                // Need to make sure the current input is a valid number
                str = ((TextBox)sender).Text;
                isNumber = decimal.TryParse(str, out outNum);
                if (isNumber)
                    mainWin.ResetZoomLevels(ImageryTitle.SelectedIndex, "Jessie");
                /*
             // Set the zoom level specific to image selected
                mainWin.wmts.GetZoomRange(ImageryTitle.SelectedIndex, "Jessie", mainWin.wmts, 
                                  mainWin.PIX_MIN, mainWin.PIX_MAX,
                                  double.Parse(MinLat.Text), double.Parse(MaxLat.Text),
                                  double.Parse(MinLon.Text), double.Parse(MaxLon.Text));
                */
            }

            // Process only if the text is a valid number
            // default to true if not triggered by extent change
            if (isNumber)
            {
                latDiff = Math.Abs(Convert.ToDouble(MaxLat.Text) -
                        Convert.ToDouble(MinLat.Text));
                lonDiff = Math.Abs(Convert.ToDouble(MaxLon.Text) -
                                    Convert.ToDouble(MinLon.Text));

                idx = ImageryTitle.SelectedIndex;
                if (idx < 0) idx = 0;

                zoomIdx = ZoomCombo.SelectedIndex;
                if (zoomIdx < 0) zoomIdx = 1;

                if (mainWin.wmts.layerTileSets != null && mainWin.wmts.layerTileSets.Count > 0)
                {
                    if (mainWin.wmts.layerTileSets[idx].resTypes != null && mainWin.wmts.layerTileSets[idx].resTypes.Count > 0)
                    {
                        res = mainWin.wmts.layerTileSets[idx].resTypes[zoomIdx].resolution;
                        numLatPix = 10 * Convert.ToInt32(latDiff / res);
                        numLonPix = 10 * Convert.ToInt32(lonDiff / res);
                        if (numLatPix <= 10) numLatPix = 0;
                        if (numLonPix <= 10) numLonPix = 0;
                        str = numLatPix.ToString() + " px x " + numLonPix.ToString() + " px";
                        PixelSize.Text = str;

                        // Get approx file size in MB
                        nBytes = Convert.ToDouble(numLatPix * numLonPix * 3) / 8.0;
                        nBytes /= 1048576.0;
                        RawSizeText.Text = nBytes.ToString("F4") + " MB";
                    }
                }
            }   // end isNumber = true
        }   // end Zoom selection changed


        private void ImageryTitle_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ImageryTitle.SelectedIndex > -1)
                mainWin.ResetZoomLevels(ImageryTitle.SelectedIndex, "Gilligan");
            /*
            mainWin.wmts.GetZoomRange(ImageryTitle.SelectedIndex, "Gilligan", mainWin.wmts,
                  mainWin.PIX_MIN, mainWin.PIX_MAX,
                  double.Parse(MinLat.Text), double.Parse(MaxLat.Text),
                  double.Parse(MinLon.Text), double.Parse(MaxLon.Text));
            */
        }   // end ImageryTitle_SelectionChanged


        // Add layer and layer information to download List
        private void ItemSaveBtn_Click(object sender, RoutedEventArgs e)
        {
            List<double> bbox = new List<double>
            {
                double.Parse(MinLat.Text),
                double.Parse(MaxLat.Text),
                double.Parse(MinLon.Text),
                double.Parse(MaxLon.Text)
            };

            mainWin.AddDownloadItem();
            /*
            mainWin.wmts.AddDownloadItem(mainWin.wmts, ImageryTitle.SelectedIndex,
                                        ZoomCombo.SelectedIndex, Date.Text, bbox);
            */
            CheckoutBtn.IsEnabled = true;
            mainWin.MainCheckoutBtn.IsEnabled = true;

        }   // end ItemSaveBtn_Click


        // Overrides default window closing (Ctrl-F4, title bar close)
        private void QAOIWindow_Closing(object sender, CancelEventArgs e)
        {
            Debug.WriteLine("Shutting down from AOI window");
            Application.Current.Shutdown();
        }   // end QAOIWindow_Closing
    }   // end  partial class AOIWindow
}   // end namespace
