using System;
using System.Collections.Generic;
using Krypton.Common;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Krypton.Lights {
	public enum ShadowType {
		Solid = 1,
		Illuminated = 2,
		Occluded = 3
	} ;

	public class Light2D : ILight2D {
		float mAngle;
		Color mColor = Color.White;
		float mFov = MathHelper.TwoPi;
		float mIntensity = 1;
		bool mIsOn = true;
		Vector2 mPosition = Vector2.Zero;
		float mRange = 1;
		ShadowType mShadowType = ShadowType.Solid;
		Texture2D mTexture;

		#region Parameters

		/// <summary>
		/// The light's position
		/// </summary>
		public Vector2 Position {
			get { return mPosition; }
			set { mPosition = value; }
		}

		/// <summary>
		/// The X coordinate of the light's position
		/// </summary>
		public float X {
			get { return mPosition.X; }
			set { mPosition.X = value; }
		}

		/// <summary>
		/// The Y coordinate of the light's position
		/// </summary>
		public float Y {
			get { return mPosition.Y; }
			set { mPosition.Y = value; }
		}

		/// <summary>
		/// The light's angle
		/// </summary>
		public float Angle {
			get { return mAngle; }
			set { mAngle = value; }
		}

		/// <summary>
		/// The texture used as the base light map, from which shadows will be subtracted
		/// </summary>
		public Texture2D Texture {
			get { return mTexture; }
			set { mTexture = value; }
		}

		/// <summary>
		/// The color used to tint the light's texture
		/// </summary>
		public Color Color {
			get { return mColor; }
			set { mColor = value; }
		}

		/// <summary>
		/// The light's maximum radius (or "half width", if you will)
		/// </summary>
		public float Range {
			get { return mRange; }
			set { mRange = value; }
		}

		/// <summary>
		/// Gets or sets the light's field of view. This value determines the angles at which the light will cease to draw
		/// </summary>
		public float Fov {
			get { return mFov; }
			set { mFov = MathHelper.Clamp(value, 0, MathHelper.TwoPi); }
		}

		/// <summary>
		/// Gets or sets the light's intensity. Think pixel = (tex * color) ^ (1 / intensity)
		/// </summary>
		public float Intensity {
			get { return mIntensity; }
			set { mIntensity = MathHelper.Clamp(value, 0.01f, 3f); }
		}

		/// <summary>
		/// Gets or sets a value indicating what type of shadows this light should cast
		/// </summary>
		public ShadowType ShadowType {
			get { return mShadowType; }
			set { mShadowType = value; }
		}

		#endregion Parameters

		/// <summary>
		/// Gets or sets a value indicating weither or not to draw the light
		/// </summary>
		public bool IsOn {
			get { return mIsOn; }
			set { mIsOn = value; }
		}

		#region ILight2D Members

		/// <summary>
		/// Draws shadows from the light's position outward
		/// </summary>
		/// <param name="helper">A render helper for drawing shadows</param>
		/// <param name="hulls">The shadow hulls used to draw shadows</param>
		public void Draw(KryptonRenderHelper helper, List<ShadowHull> hulls) {
			// Draw the light only if it's on
			if (!mIsOn)
				return;

			// Make sure we only render the following hulls
			helper.ShadowHullVertices.Clear();
			helper.ShadowHullIndicies.Clear();

			// Loop through each hull
			foreach (ShadowHull hull in hulls) {
				//if(hull.Bounds.Intersects(this.Bounds))
				// Add the hulls to the buffer only if they are within the light's range
				if (hull.Visible && IsInRange(hull.Position - Position, hull.MaxRadius*Math.Max(hull.Scale.X, hull.Scale.Y) + Range)) {
					helper.BufferAddShadowHull(hull);
				}
			}

			// Set the effect and parameters
			helper.Effect.Parameters["LightPosition"].SetValue(mPosition);
			helper.Effect.Parameters["Texture0"].SetValue(mTexture);
			helper.Effect.Parameters["LightIntensityFactor"].SetValue(1/(mIntensity*mIntensity));


			switch (mShadowType) {
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
				throw new NotImplementedException("Shadow Type does not exist: " + mShadowType);
			}

			foreach (EffectPass pass in helper.Effect.CurrentTechnique.Passes) {
				pass.Apply();
				helper.BufferDraw();
			}

			helper.Effect.CurrentTechnique = helper.Effect.Techniques["PointLight_Light"];
			foreach (EffectPass pass in helper.Effect.CurrentTechnique.Passes) {
				pass.Apply();
				helper.DrawClippedFov(mPosition, mAngle, mRange*2, mColor, mFov);
			}

			helper.Effect.CurrentTechnique = helper.Effect.Techniques["ClearTarget_Alpha"];
			foreach (EffectPass pass in helper.Effect.CurrentTechnique.Passes) {
				pass.Apply();
				helper.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleStrip, KryptonRenderHelper.UnitQuad, 0, 2);
			}
		}

		/// <summary>
		/// Gets the world-space bounds which contain the light
		/// </summary>
		public BoundingRect Bounds {
			get {
				BoundingRect rect;

				rect.Min.X = mPosition.X - mRange;
				rect.Min.Y = mPosition.Y - mRange;
				rect.Max.X = mPosition.X + mRange;
				rect.Max.Y = mPosition.Y + mRange;

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
		static bool IsInRange(Vector2 offset, float dist) {
			// a^2 + b^2 < c^2 ?
			return offset.X*offset.X + offset.Y*offset.Y < dist*dist;
		}
	}
}
