using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Krypton {
	public static class Unit {
		public static readonly VertexPositionTexture[] Quad = new[] {
		    	new VertexPositionTexture {
		    	                          	Position = new Vector3(-1, 1, 0),
		    	                          	TextureCoordinate = new Vector2(0, 0),
		    	                          },
		    	new VertexPositionTexture {
		    	                          	Position = new Vector3(1, 1, 0),
		    	                          	TextureCoordinate = new Vector2(1, 0),
		    	                          },
		    	new VertexPositionTexture {
		    	                          	Position = new Vector3(-1, -1, 0),
		    	                          	TextureCoordinate = new Vector2(0, 1),
		    	                          },
		    	new VertexPositionTexture {
		    	                          	Position = new Vector3(1, -1, 0),
		    	                          	TextureCoordinate = new Vector2(1, 1),
		    	                          },
		    };

	}
}
