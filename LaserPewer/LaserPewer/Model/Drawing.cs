using LaserPewer.Geometry;
using System.Collections.Generic;

namespace LaserPewer.Model
{
    public class Drawing
    {
        public IReadOnlyList<Path> Paths { get; private set; }

        public Drawing(List<Path> paths)
        {
            Paths = paths;
        }
    }
}
