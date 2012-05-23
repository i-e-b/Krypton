using System;
using System.Collections.Generic;
using Krypton.Common;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Krypton.Lights {
	public class Light2D : ILight2D {
		float angle;
		Color color = Color.White;
		float fov = MathHelper.TwoPi;
		float intensity = 1;
		bool isOn = true;
		Vector2 position = Vector2.Zero;
		float range = 1;
		ShadowType shadowType = ShadowType.Solid;
		Texture2D texture;

		#region Parameters

		/// <summary>
		/// The light's position
		/// </summary>
		public Vector2 Position {
			get { return position; }
			set { position = value; }
		}

		/// <summary>
		/// The X coordinate of the light's position
		/// </summary>
		public float X {
			get { return position.X; }
			set { position.X = value; }
		}

		/// <summary>
		/// The Y coordinate of the light's position
		/// </summary>
		public float Y {
			get { return position.Y; }
			set { position.Y = value; }
		}

		/// <summary>
		/// The light's angle
		/// </summary>
		public float Angle {
			get { return angle; }
			set { angle = value; }
		}

		/// <summary>
		/// The texture used as the base light map, from which shadows will be subtracted
		/// </summary>
		public Texture2D Texture {
			get { return texture; }
			set { texture = value; }
		}

		/// <summary>
		/// The color used to tint the light's texture
		/// </summary>
		public Color Color {
			get { return color; }
			set { color = value; }
		}

		/// <summary>
		/// The light's maximum radius (or "half width", if you will)
		/// </summary>
		public float Range {
			get { return range; }
			set { range = value; }
		}

		/// <summary>
		/// Gets or sets the light's field of view. This value determines the angles at which the light will cease to draw
		/// </summary>
		public float Fov {
			get { return fov; }
			set { fov = MathHelper.Clamp(value, 0, MathHelper.TwoPi); }
		}

		/// <summary>
		/// Gets or sets the light's intensity. Think pixel = (tex * color) ^ (1 / intensity)
		/// </summary>
		public float Intensity {
			get { return intensity; }
			set { intensity = MathHelper.Clamp(value, 0.01f, 3f); }
		}

		/// <summary>
		/// Gets or sets a value indicating what type of shadows this light should cast
		/// </summary>
		public ShadowType ShadowType {
			get { return shadowType; }
			set { shadowType = value; }
		}

		#endregion Parameters

		/// <summary>
		/// Gets or sets a value indicating weither or not to draw the light
		/// </summary>
		public bool IsOn {
			get { return isOn; }
			set { isOn = value; }
		}

		#region ILight2D Members

		/// <summary>
		/// Draws shadows from the light's position outward
		/// </summary>
		/// <param name="helper">A render helper for drawing shadows</param>
		/// <param name="hulls">The shadow hulls used to draw shadows</param>
		public void Draw (KryptonRenderHelper helper, List<ShadowHull> hulls) {
			// Draw the light only if it's on
			if (!isOn) return;

			// Make sure we only render the following hulls
			helper.ShadowHullVertices.Clear();
			helper.ShadowHullIndicies.Clear();

			// Loop through each hull
			foreach (var hull in hulls) {
				// Add the hulls to the buffer only if they are within the light's range
				if (hull.Visible && IsInRange(hull.Position - Position, hull.MaxRadius * Math.Max(hull.Scale.X, hull.Scale.Y) + Range)) {
					helper.BufferAddShadowHull(hull);
				}
			}

			// Set the effect and parameters
			helper.Effect.Parameters["LightPosition"].SetValue(position);
			helper.Effect.Parameters["Texture0"].SetValue(texture);
			helper.Effect.Parameters["LightIntensityFactor"].SetValue(1 / (intensity * intensity));


			switch (shadowType) {
				case (ShadowType.Solid):
					helper.Effect.CurrentTechnique = helper.Effect.Techniques["PointLight_Shadow_Solid"];
					break;

				case (ShadowType.Illuminated):
					helper.Effect.CurrentTechnique = helper.Effect.Techniques["PointLight_Shadow_Illuminated"];
					break;

				case (ShadowType.Occluded):
					helper.Effect.CurrentTechnique = helper.Effect.Techniques["PointLight_Shadow_Occluded"];
					break;

				default:
					throw new NotImplementedException("Shadow Type does not exist: " + shadowType);
			}

			foreach (var pass in helper.Effect.CurrentTechnique.Passes) {
				pass.Apply();
				helper.BufferDraw();
			}

			helper.Effect.CurrentTechnique = helper.Effect.Techniques["PointLight_Light"];
			foreach (var pass in helper.Effect.CurrentTechnique.Passes) {
				pass.Apply();
				helper.DrawClippedFov(position, angle, range * 2, color, fov);
			}

			helper.Effect.CurrentTechnique = helper.Effect.Techniques["ClearTarget_Alpha"];
			foreach (var pass in helper.Effect.CurrentTechnique.Passes) {
				pass.Apply();
				helper.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleStrip, Unit.Quad, 0, 2);
			}
		}

		/// <summary>
		/// Gets the world-space bounds which contain the light
		/// </summary>
		public BoundingRect Bounds {
			get {
				BoundingRect rect;

				rect.Min.X = position.X - range;
				rect.Min.Y = position.Y - range;
				rect.Max.X = position.X + range;
				rect.Max.Y = position.Y + range;

				return rect;
			}
		}

		#endregion

		/// <summary>
		/// Determines if a vector's length is less than a specified value
		/// </summary>
		/// <param name="offset">Offset</param>
		/// <param name="dist">Distance</param>
		/// <returns></returns>
		static bool IsInRange (Vector2 offset, float dist) {
			// a^2 + b^2 < c^2 ?
			return offset.X * offset.X + offset.Y * offset.Y < dist * dist;
		}
	}
}
