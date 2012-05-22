using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Krypton
{
    public struct ShadowHullVertex : IVertexType
    {
        /// <summary>
        /// The position of the vertex
        /// </summary>
        public Vector2 Position;

        /// <summary>
        /// The normal of the vertex
        /// </summary>
        public Vector2 Normal;

        /// <summary>
        /// The color of vertex
        /// </summary>
        public Color Color;

        private static readonly VertexDeclaration VertexDec;

        /// <summary>
        /// 
        /// </summary>
        public VertexDeclaration VertexDeclaration { get { return VertexDec; } }

        static ShadowHullVertex()
        {
            var elements = new[]
            {
                new VertexElement(0, VertexElementFormat.Vector2, VertexElementUsage.Position, 0),
                new VertexElement(8, VertexElementFormat.Vector2, VertexElementUsage.Normal,0),
                new VertexElement(16, VertexElementFormat.Color, VertexElementUsage.Color,0),
            };

            VertexDec = new VertexDeclaration(elements);
        }

    	/// <summary>
    	/// 
    	/// </summary>
    	/// <param name="position"></param>
    	/// <param name="normal"></param>
    	/// <param name="color"></param>
    	public ShadowHullVertex(Vector2 position, Vector2 normal, Color color)
        {
            Position = position;
            Normal = normal;
            Color = color;
        }
    }
}
