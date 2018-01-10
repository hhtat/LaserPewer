using System.ComponentModel;

namespace LaserPewer.Utilities
{
    public enum Corner : ushort
    {
        [Description("Top left")]
        TopLeft = 1,
        [Description("Top right")]
        TopRight = 2,
        [Description("Bottom left")]
        BottomLeft = 3,
        [Description("Bottom right")]
        BottomRight = 4,
    }
}
