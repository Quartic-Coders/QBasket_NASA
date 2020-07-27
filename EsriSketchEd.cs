using System.Drawing;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.UI;

namespace QBasket_demo
{
    public class EsriSketchEd
    {
        private SketchCreationMode _mode;
        private SketchEditConfiguration _config;
        private LineSymbol _outline;
        private Symbol _symbol;
        private Geometry _geometry;

        public SketchCreationMode mode
        { get => _mode; set { _mode = value; } }

        public SketchEditConfiguration config
        { get => _config; set { _config = value; } }

        public LineSymbol outline
        { get => _outline; set { _outline = value; } }

        public Symbol symbol
        { get => _symbol; set { _symbol = value; } }

        public Geometry geometry
        { get => _geometry; set { _geometry = value; } }

        public EsriSketchEd()
        {
            mode = new SketchCreationMode();
            config = new SketchEditConfiguration();
            outline = new SimpleLineSymbol();
        }

        /// <summary>
        /// Create the graphic
        /// </summary>
        /// <param name="geometry"></param>
        /// <returns></returns>
        public void ConfigRedRect()
        {
            outline.Color = Color.Red;
            outline.Width = 2;

            // Create a graphic to display the specified geometry
            symbol = new SimpleFillSymbol()
            {
                Outline = outline,
                Style = SimpleFillSymbolStyle.Null
            };

            // Create graphics area for a re-drawable rectangle
            mode = (SketchCreationMode)6;
            config = new SketchEditConfiguration();
            config.AllowMove = true;
            config.AllowRotate = false;
            config.ResizeMode = (SketchResizeMode)1;
        }

    }
}
