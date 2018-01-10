using LaserPewer.Utilities;
using Svg;
using System;
using System.Diagnostics;
using System.Windows;

namespace LaserPewer.Model
{
    public class Document
    {
        public event EventHandler Modified;

        public string FileName { get; private set; }
        public Drawing Drawing { get; private set; }
        public Size Size { get; private set; }

        private Vector _offset;
        public Vector Offset
        {
            get { return _offset; }
            set
            {
                _offset = value;
                Modified?.Invoke(this, null);
            }
        }

        public bool LoadSVG(string fileName)
        {
            SvgScraper svgScraper = new SvgScraper();
            Size svgSize;

            try
            {
                SvgDocument svgDocument = SvgDocument.Open(fileName);
                svgSize = new Size(
                    Precision.Round3(svgScraper.GetWidth(svgDocument)),
                    Precision.Round3(svgScraper.GetHeight(svgDocument)));
                svgDocument.Draw(svgScraper);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                return false;
            }

            Drawing svgDrawing = svgScraper.CreateDrawing(new Rect(0.0, -svgSize.Height, svgSize.Width, svgSize.Height));

            FileName = fileName;
            Drawing = svgDrawing;
            Size = svgSize;
            Modified?.Invoke(this, null);

            return true;
        }
    }
}
