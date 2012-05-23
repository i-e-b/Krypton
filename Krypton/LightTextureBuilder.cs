using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Krypton {
	public static class LightTextureBuilder {
		/// <summary>
		/// Create a Point Light based on a size.
		/// </summary>
		/// <param name="device">Your game's GraphicsDevice</param>
		/// <param name="size">Maximum Size</param>
		/// <returns>Light Texture</returns>
		public static Texture2D CreatePointLight (GraphicsDevice device, int size) {
			return CreateConicLight(device, size, MathHelper.TwoPi, 0);
		}

		/// <summary>
		/// Create a Conic Light based on the size, and field of view.
		/// </summary>
		/// <param name="device">Your game's GraphicsDevice</param>
		/// <param name="size">Maximum Size</param>
		/// <param name="fov">Maximum Field of View</param>
		/// <returns>Light Texture</returns>
		public static Texture2D CreateConicLight (GraphicsDevice device, int size, float fov) {
			return CreateConicLight(device, size, fov, 0);
		}

		/// <summary> Create a Conic Light based on the size, field of view, and near plane distance.</summary>
		/// <param name="device">Your game's GraphicsDevice</param>
		/// <param name="size">Maximum size</param>
		/// <param name="fov">Maximum Field of View</param>
		/// <param name="nearPlaneDistance">Prevents texture from being drawn at this plane distance, originating from the center of light</param>
		/// <returns>Light Texture</returns>
		public static Texture2D CreateConicLight (GraphicsDevice device, int size, float fov, float nearPlaneDistance) {
			var data = new float[size, size];
			var center = size / 2f;

			fov = fov / 2;

			for (int x = 0; x < size; x++)
				for (int y = 0; y < size; y++) {
					float distance = Vector2.Distance(new Vector2(x, y), new Vector2(center));

					Vector2 difference = new Vector2(x, y) - new Vector2(center);
					var angle = (float)Math.Atan2(difference.Y, difference.X);

					if (distance <= center && distance >= nearPlaneDistance && Math.Abs(angle) <= fov)
						data[x, y] = (center - distance) / center;
					else
						data[x, y] = 0;
				}

			var tex = new Texture2D(device, size, size);

			var data1D = new Color[size * size];
			for (int x = 0; x < size; x++)
				for (int y = 0; y < size; y++)
					data1D[x + y * size] = new Color(new Vector3(data[x, y]));

			tex.SetData(data1D);

			return tex;
		}
	}
}
