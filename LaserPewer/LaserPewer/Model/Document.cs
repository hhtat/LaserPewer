using System.Collections.Generic;

namespace LaserPewer.Model
{
    public class Document
    {
        private List<Drawing> drawings;
        public IReadOnlyList<Drawing> Drawings { get { return drawings.AsReadOnly(); } }

        public bool Stale { get; private set; }

        public Document()
        {
            drawings = new List<Drawing>();
        }

        public void Clear()
        {
            drawings.Clear();
            Stale = true;
        }

        public void Add(Drawing drawing)
        {
            drawings.Add(drawing);
            Stale = true;
        }

        public void Refresh()
        {
            Stale = false;
        }
    }
}
