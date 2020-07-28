using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using Esri.ArcGISRuntime.Ogc;

namespace QBasket_demo
{
    public class WMTS
    {       
        private WmtsServiceInfo _serviceInfo;
        private List<WmtsLayerInfo> _selectedLayers = new List<WmtsLayerInfo>();
        private IReadOnlyList<WmtsLayerInfo> _layerInfo;
        private List<TileSetVariables> _layerTileSets = new List<TileSetVariables>();
        private List<TileSetVariables> _tileSets = new List<TileSetVariables>();
        private List<WmtsLayerInfo> _downloadLayers = new List<WmtsLayerInfo>();
        private List<DownloadLayerInfo> _downloadInfo = new List<DownloadLayerInfo>();

        public WmtsServiceInfo serviceInfo
        { get => _serviceInfo; set { _serviceInfo = value; } }
        public List<WmtsLayerInfo> selectedLayers
        { get => _selectedLayers; set { _selectedLayers = value; } }
        public IReadOnlyList<WmtsLayerInfo> layerInfos
        { get => _layerInfo; set { _layerInfo = value; } }
        public List<TileSetVariables> layerTileSets
        { get => _layerTileSets; set { _layerTileSets = value; } }
        public List<TileSetVariables> tileSets
        { get => _tileSets; set { _tileSets = value; } }
        public List<WmtsLayerInfo> downloadLayers
        { get => _downloadLayers; set { _downloadLayers = value; } }
        public List<DownloadLayerInfo> downloadInfo
        { get => _downloadInfo; set { _downloadInfo = value; } }

        /// <summary>
        /// WMTS Tile Resolution Class
        /// </summary>
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


        /// <summary>
        /// WMTS Tile set variables Class
        /// </summary>
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
            private int _minZoom;
            private int _maxZoom;

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
            public int minZoom
            { get => _minZoom; set { _minZoom = value; } }
            public int maxZoom
            { get => _maxZoom; set { _maxZoom = value; } }
        }   // end TileSetVariables


        /// <summary>
        ///  WMTS Layer info required when downloading data
        /// </summary>
        public class DownloadLayerInfo
        {
            private WmtsLayerInfo _info;
            private String _title;
            private String _name;
            private double[] _bbox = new double[4];
            private String _latency;
            private String _crs;
            private String _spatialRef;
            private int _zoomLvl;
            private int _tileWidth;
            private int _tileHeight;
            private int _matrixWidth;
            private int _matrixHeight;
            private double _nMBytes;
            private int _pixelWidth;
            private int _pixelHeight;
            private double _resolution;
            private string _date;
            private string _uriStr;

            public WmtsLayerInfo info
            { get => _info; set { _info = value; } }
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
            public string spatialRef
            { get => _spatialRef; set { _spatialRef = value; } }
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
            public string date
            { get => _date; set { _date = value; } }
            public string uriStr
            { get => _uriStr; set { _uriStr = value; } }
        }   // end Download layer Info


        /// <summary>
        /// Initialize WMTS Class
        /// </summary>
        /// <param name="uriStr"></param>
        /// <param name="selectedLayers"></param>
        /// <param name="data">WMTS.Data variable </param>
        /// <param name="tileWidth"></param>
        /// <param name="tileHeight"></param>
        /// <param name="mapScale"></param>
        // Intialize WMTS data
        public async void WMTS_Init(string uriStr, List<WmsLayerInfo> selectedLayers,
                                    WMTS wmts, int tileWidth, int tileHeight, int mapScale)
        {
            String str;
            WmtsService wmtsService;
            Uri wmtsServiceURI;

            // Create the WMTS Service Load data from the URI
            wmtsServiceURI = new Uri(uriStr);

            // Define an insatance of the service
            wmtsService = new WmtsService(wmtsServiceURI);

            // If service can load, initialize the app
            try
            {
                // Load the WMS Service.
                await wmtsService.LoadAsync();

                // Get the service info (metadata) from the service.
                wmts.serviceInfo = wmtsService.ServiceInfo;

                // Get the WMTS tile information
                wmts.tileSets = new List<TileSetVariables>();
                for (int i = 0; i < selectedLayers.Count; i++)
                {
                    wmts.tileSets.Add(new TileSetVariables());
                    wmts.tileSets[i].tileSetTitle = wmts.serviceInfo.TileMatrixSets[i].Id;
                    wmts.tileSets[i].zoomLvls = wmts.serviceInfo.TileMatrixSets[i].TileMatrices.Count;
                    for (int j = 0; j < wmts.serviceInfo.TileMatrixSets[i].TileMatrices.Count; j++)
                    {
                        wmts.tileSets[i].resTypes.Add(new ResType());
                        wmts.tileSets[i].resTypes[j].id =
                            wmts.serviceInfo.TileMatrixSets[i].TileMatrices[j].Id;
                        wmts.tileSets[i].resTypes[j].scaleDenom =
                            wmts.serviceInfo.TileMatrixSets[i].TileMatrices[j].ScaleDenominator;
                    }   // end for j
                }   // end for i

                // Obtain the read only list of WMTS layers info objects
                // for selected layers
                wmts.selectedLayers = new List<WmtsLayerInfo>();
                foreach (WmsLayerInfo wmsLayerInfo in selectedLayers)
                {
                    str = wmsLayerInfo.Title;
                    foreach (WmtsLayerInfo wmtsLayerInfo in wmts.serviceInfo.LayerInfos)
                    {
                        // Got a layer - save info about this layer
                        if (str == wmtsLayerInfo.Title)
                        {
                            wmts.selectedLayers.Add(wmtsLayerInfo);
                            break;
                        }   // end if
                    }   // end foreach WmtsLayerInfo
                }   // end foreach wmsLayerInfo

                // Set up tile matrix information for selected tiles
                wmts.layerTileSets = new List<TileSetVariables>();
                for (int i = 0; i < wmts.selectedLayers.Count; i++)
                {
                    wmts.layerTileSets.Add(new TileSetVariables());
                    wmts.layerTileSets[i].zoomLvls = wmts.selectedLayers[i].TileMatrixSets[0].TileMatrices.Count;
                    wmts.layerTileSets[i].tileSetTitle =
                         wmts.selectedLayers[i].TileMatrixSets[0].Id;
                    wmts.layerTileSets[i].layerTitle = wmts.selectedLayers[i].Title;
                    wmts.layerTileSets[i].layerName = wmts.selectedLayers[i].Id;
                    wmts.layerTileSets[i].tileWidth = tileWidth;
                    wmts.layerTileSets[i].tileHeight = tileHeight;

                    // Get tile matrices info
                    for (int j = 0; j < wmts.selectedLayers[i].TileMatrixSets[0].TileMatrices.Count; j++)
                    {
                        wmts.layerTileSets[i].resTypes.Add(new ResType());
                        wmts.layerTileSets[i].resTypes[j].id =
                            wmts.selectedLayers[i].TileMatrixSets[0].TileMatrices[j].Id;
                        wmts.layerTileSets[i].resTypes[j].scaleDenom =
                            wmts.selectedLayers[i].TileMatrixSets[0].TileMatrices[j].ScaleDenominator;
                        wmts.layerTileSets[i].resTypes[j].resolution =
                            wmts.layerTileSets[i].resTypes[j].scaleDenom / mapScale;
                        wmts.layerTileSets[i].resTypes[j].matrixWidth = 2 ^ j;
                        wmts.layerTileSets[i].resTypes[j].matrixHeight = 2 ^ j;
                    }   // end for j
                }   // end for i
            }   // end  try
            catch (Exception ex)
            {
                MessageBox.Show("ERROR: " + ex.ToString(), "InitializeWMTS Error");
            }
        }  // end InitializeWMTS


        /// <summary>
        /// Update the WMTS Data, using the new list of selected layers
        /// </summary>
        /// <param name="data">WMTS.Data variable </param>
        /// <param name="selectedLayers">Layers selection by the user</param>
        /// <param name="tileWidth">Tile width of the tile matices</param>
        /// <param name="tileHeight">Tile height of the tile matrices</param>
        /// <param name="mapScale">Map sacle for the Tile data</param>
        public void WMTS_ResetInfo(WMTS wmts, List<WmsLayerInfo> selectedLayers,
                                   int tileWidth, int tileHeight, int mapScale)
        {
            string str;
            mapScale = 397569610;   /* HARD CODED */

            if (wmts.serviceInfo != null)
            {
                // Create a new list of layer info selected
                wmts.tileSets = new List<TileSetVariables>();
                for (int i = 0; i < selectedLayers.Count; i++)
                {
                    wmts.tileSets.Add(new TileSetVariables());
                    wmts.tileSets[i].tileSetTitle = wmts.serviceInfo.TileMatrixSets[i].Id;
                    wmts.tileSets[i].zoomLvls = wmts.serviceInfo.TileMatrixSets[i].TileMatrices.Count;
                    for (int j = 0; j < wmts.serviceInfo.TileMatrixSets[i].TileMatrices.Count; j++)
                    {
                        wmts.tileSets[i].resTypes.Add(new ResType());
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
                wmts.layerTileSets = new List<TileSetVariables>();
                for (int i = 0; i < wmts.selectedLayers.Count; i++)
                {
                    wmts.layerTileSets.Add(new TileSetVariables());
                    wmts.layerTileSets[i].zoomLvls = wmts.selectedLayers[i].TileMatrixSets[0].TileMatrices.Count;
                    wmts.layerTileSets[i].tileSetTitle =
                            wmts.selectedLayers[i].TileMatrixSets[0].Id;
                    wmts.layerTileSets[i].layerTitle = wmts.selectedLayers[i].Title;
                    wmts.layerTileSets[i].layerName = wmts.selectedLayers[i].Id;

                    wmts.layerTileSets[i].tileWidth = tileWidth;
                    wmts.layerTileSets[i].tileHeight = tileHeight;

                    // get tile matrices info
                    for (int j = 0; j < wmts.selectedLayers[i].TileMatrixSets[0].TileMatrices.Count; j++)
                    {
                        wmts.layerTileSets[i].resTypes.Add(new ResType());
                        wmts.layerTileSets[i].resTypes[j].id =
                            wmts.selectedLayers[i].TileMatrixSets[0].TileMatrices[j].Id;
                        wmts.layerTileSets[i].resTypes[j].scaleDenom =
                            wmts.selectedLayers[i].TileMatrixSets[0].TileMatrices[j].ScaleDenominator;
                        wmts.layerTileSets[i].resTypes[j].resolution =
                            wmts.layerTileSets[i].resTypes[j].scaleDenom / mapScale;
                        wmts.layerTileSets[i].resTypes[j].matrixWidth = 2 ^ j;
                        wmts.layerTileSets[i].resTypes[j].matrixHeight = 2 ^ j;
                    }   // end for j
                }   // end for i
            }   // end if
        }   // end ResetWmtsInfo


        /// <summary>
        /// Add a data item to the WMTS.Data list
        /// </summary>
        /// <param name="data">WMTS.Data variable </param>
        /// <param name="layerIdx">Selected Layer index</param>
        /// <param name="zoomIdx">Selected Zoom idx</param>
        /// <param name="date"></param>
        /// <param name="bbox"></param>
        public void AddDownloadItem(WMTS wmts, int layerIdx, int zoomIdx, string date, List<double> bbox)
        {
            int idx;
            int minZoom, maxZoom;
            int pixelWidth, pixelHeight;

            double resolution;

            minZoom = wmts.layerTileSets[layerIdx].minZoom;
            maxZoom = wmts.layerTileSets[layerIdx].maxZoom;
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
                wmts.downloadInfo.Add(new DownloadLayerInfo());
                idx = wmts.downloadInfo.Count - 1;

                // Store layer variables required for later processing
                wmts.downloadInfo[idx].bbox[0] = bbox[0];
                wmts.downloadInfo[idx].bbox[1] = bbox[1];
                wmts.downloadInfo[idx].bbox[2] = bbox[2];
                wmts.downloadInfo[idx].bbox[3] = bbox[3];
                resolution = wmts.layerTileSets[layerIdx].resTypes[zoomIdx].resolution;
                pixelHeight =
                    10 * Math.Abs(Convert.ToInt32((wmts.downloadInfo[idx].bbox[2] - wmts.downloadInfo[idx].bbox[0]) / resolution));
                pixelWidth =
                    10 * Math.Abs(Convert.ToInt32((wmts.downloadInfo[idx].bbox[3] - wmts.downloadInfo[idx].bbox[1]) / resolution));

                wmts.downloadInfo[idx].info = wmts.selectedLayers[layerIdx];
                wmts.downloadInfo[idx].title = wmts.layerTileSets[layerIdx].layerTitle;
                wmts.downloadInfo[idx].name = wmts.layerTileSets[layerIdx].layerName;
                // data.downloadInfo[idx].latency = wmsUriStartup.latency;
                // data.downloadInfo[idx].crs = wmsUriStartup.EPSG;
                wmts.downloadInfo[idx].zoomLvl = zoomIdx;
                wmts.downloadInfo[idx].resolution = wmts.layerTileSets[layerIdx].resTypes[zoomIdx].resolution;
                wmts.downloadInfo[idx].tileHeight = wmts.layerTileSets[layerIdx].tileHeight;
                wmts.downloadInfo[idx].tileWidth = wmts.layerTileSets[layerIdx].tileWidth;
                wmts.downloadInfo[idx].matrixWidth = wmts.layerTileSets[layerIdx].resTypes[zoomIdx].matrixWidth;
                wmts.downloadInfo[idx].matrixHeight = wmts.downloadInfo[idx].matrixWidth;
                wmts.downloadInfo[idx].pixelWidth = pixelWidth;
                wmts.downloadInfo[idx].pixelHeight = pixelHeight;
                wmts.downloadInfo[idx].nMBytes = (Convert.ToDouble(pixelHeight * pixelWidth * 3) / 8.0) / 1048576.0;

                // Make sure the date is in the correct format yyyy-mm-dd
                String[] words = date.Split("-");
                int year = Convert.ToInt32(words[0]);
                int month = Convert.ToInt32(words[1]);
                int day = Convert.ToInt32(words[2]);
                wmts.downloadInfo[idx].date = year.ToString("D4") + "-" + month.ToString("D2") + "-" + day.ToString("D2");
            } // end else
        }   // end AddItem


        /// <summary>
        /// Determine the min and max zoom levels for a given
        /// Pixel min and max size
        /// </summary>
        /// <param name="idx">Zoom index selected</param>
        /// <param name="flag">Sring to indcate where routine is called from</param>
        /// <param name="data">WMTS.Data variable </param>
        /// <param name="pixMin">Minimum acceptable pixel size </param>
        /// <param name="pixMax">Maximum accepted pixel size</param>
        /// <param name="latMin">Minimum Latitude of selected region</param>
        /// <param name="latMax">Maximum Latitude of selected region</param>
        /// <param name="lonMin">Minimum Longitude of selected region</param>
        /// <param name="lonMax">Maximum Longitude of selected region<</param>
        public void GetZoomRange(int idx, string flag, WMTS wmts,
                                  int pixMin, int pixMax,
                                  double latMin, double latMax,
                                  double lonMin, double lonMax)
        {
            int i;
            int pixLat, pixLon;
            double res;
            double latDiff = latMax - latMin;
            double lonDiff = lonMax - lonMin;

            // Get the  max zoom level for this tile set
            int maxIdx = 0;
            for (i = 0; i < wmts.layerTileSets[idx].resTypes.Count; i++)
            {
                res = wmts.layerTileSets[idx].resTypes[i].resolution;
                pixLat = (int)(10.0 * (latDiff / res));
                pixLon = (int)(10.0 * (lonDiff / res));

                if ((pixLat > pixMax) || (pixLon > pixMax))
                {
                    wmts.layerTileSets[idx].maxZoom = maxIdx - 1;
                    break;
                }
                maxIdx++;

            }   // end for i

            if (maxIdx == wmts.layerTileSets[idx].resTypes.Count - 1)
                wmts.layerTileSets[idx].maxZoom = maxIdx;

            // Loop backwards to find min
            for (i = wmts.layerTileSets[idx].resTypes.Count - 1; i > -1; i--)
            {
                res = wmts.layerTileSets[idx].resTypes[i].resolution;
                pixLat = (int)(10.0 * (latDiff / res));
                pixLon = (int)(10.0 * (lonDiff / res));

                if ((pixLat < pixMin) || (pixLon < pixMin))
                {
                    wmts.layerTileSets[idx].minZoom = i + 1;
                    break;
                }
            }
            if (i < 1)
                wmts.layerTileSets[idx].minZoom = 1;
        }   // end ResetZoom
    }
}
