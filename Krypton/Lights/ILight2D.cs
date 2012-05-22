using System.Collections.Generic;
using Krypton.Common;

namespace Krypton.Lights
{
    public interface ILight2D
    {
        BoundingRect Bounds { get; }

        void Draw(KryptonRenderHelper renderHelper, List<ShadowHull> hulls);
    }
}
