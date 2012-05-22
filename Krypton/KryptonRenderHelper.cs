using System;
using System.Collections.Generic;
using Krypton.Common;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Krypton {
	public enum BlendTechnique {
		Add = 1,
		Multiply = 2,
	} ;

	public enum BlurTechnique {
		Horizontal = 1,
		Vertical = 2,
	} ;

	public class KryptonRenderHelper {
		#region Static Unit Quad

		public static readonly VertexPositionTexture[] UnitQuad = new[] {
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

		#endregion Static Unit Quad

		readonly Effect effect;
		readonly GraphicsDevice graphicsDevice;
		readonly List<Int32> shadowHullIndicies = new List<Int32>();
		readonly List<ShadowHullVertex> shadowHullVertices = new List<ShadowHullVertex>();

		public KryptonRenderHelper (GraphicsDevice graphicsDevice, Effect effect) {
			this.graphicsDevice = graphicsDevice;
			this.effect = effect;
		}

		public GraphicsDevice GraphicsDevice {
			get { return graphicsDevice; }
		}

		public Effect Effect {
			get { return effect; }
		}

		public List<ShadowHullVertex> ShadowHullVertices {
			get { return shadowHullVertices; }
		}

		public List<Int32> ShadowHullIndicies {
			get { return shadowHullIndicies; }
		}

		public void BufferAddShadowHull (ShadowHull hull) {
			// Why do we need all of these again? (hint: we don't)

			Matrix vertexMatrix = Matrix.Identity;
			Matrix normalMatrix = Matrix.Identity;

			ShadowHullPoint point;
			ShadowHullVertex hullVertex;

			// Create the matrices (3X speed boost versus prior version)
			var cos = (float)Math.Cos(hull.Angle);
			var sin = (float)Math.Sin(hull.Angle);

			// vertexMatrix = scale * rotation * translation;
			vertexMatrix.M11 = hull.Scale.X * cos;
			vertexMatrix.M12 = hull.Scale.X * sin;
			vertexMatrix.M21 = hull.Scale.Y * -sin;
			vertexMatrix.M22 = hull.Scale.Y * cos;
			vertexMatrix.M41 = hull.Position.X;
			vertexMatrix.M42 = hull.Position.Y;

			// normalMatrix = scaleInv * rotation;
			normalMatrix.M11 = (1f / hull.Scale.X) * cos;
			normalMatrix.M12 = (1f / hull.Scale.X) * sin;
			normalMatrix.M21 = (1f / hull.Scale.Y) * -sin;
			normalMatrix.M22 = (1f / hull.Scale.Y) * cos;

			// Where are we in the buffer?
			int vertexCount = shadowHullVertices.Count;

			// Add the vertices to the buffer
			for (int i = 0; i < hull.NumPoints; i++) {
				// Transform the vertices to screen coordinates
				point = hull.Points[i];
				Vector2.Transform(ref point.Position, ref vertexMatrix, out hullVertex.Position);
				Vector2.TransformNormal(ref point.Normal, ref normalMatrix, out hullVertex.Normal);

				hullVertex.Color = new Color(0, 0, 0, 1 - hull.Opacity);

				shadowHullVertices.Add(hullVertex); // could this be sped up... ?
			}

			//// Add the indicies to the buffer
			foreach (int index in hull.Indicies) {
				shadowHullIndicies.Add(vertexCount + index); // what about this? Add range?
			}
		}

		public void DrawSquareQuad (Vector2 position, float rotation, float size, Color color) {
			size /= 2;

			size = (float)Math.Sqrt(Math.Pow(size, 2) + Math.Pow(size, 2));

			rotation += (float)Math.PI / 4;

			float cos = (float)Math.Cos(rotation) * size;
			float sin = (float)Math.Sin(rotation) * size;

			Vector3 v1 = new Vector3(+cos, +sin, 0) + new Vector3(position, 0);
			Vector3 v2 = new Vector3(-sin, +cos, 0) + new Vector3(position, 0);
			Vector3 v3 = new Vector3(-cos, -sin, 0) + new Vector3(position, 0);
			Vector3 v4 = new Vector3(+sin, -cos, 0) + new Vector3(position, 0);

			var quad = new[] {
			                 	new VertexPositionColorTexture {
			                 	                               	Position = v2,
			                 	                               	Color = color,
			                 	                               	TextureCoordinate = new Vector2(0, 0),
			                 	                               },
			                 	new VertexPositionColorTexture {
			                 	                               	Position = v1,
			                 	                               	Color = color,
			                 	                               	TextureCoordinate = new Vector2(1, 0),
			                 	                               },
			                 	new VertexPositionColorTexture {
			                 	                               	Position = v3,
			                 	                               	Color = color,
			                 	                               	TextureCoordinate = new Vector2(0, 1),
			                 	                               },
			                 	new VertexPositionColorTexture {
			                 	                               	Position = v4,
			                 	                               	Color = color,
			                 	                               	TextureCoordinate = new Vector2(1, 1),
			                 	                               },
			                 };

			graphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleStrip, quad, 0, 2);
		}

		public void DrawClippedFov (Vector2 position, float rotation, float size, Color color, float fov) {
			fov = MathHelper.Clamp(fov, 0, MathHelper.TwoPi);

			if (fov == 0) {
				return;
			}
			if (fov == MathHelper.TwoPi) {
				DrawSquareQuad(position, rotation, size, color);
				return;
			}
			Vector2 ccw = ClampToBox(fov / 2);
			Vector2 cw = ClampToBox(-fov / 2);

			Vector2 ccwTex = new Vector2(ccw.X + 1, -ccw.Y + 1) / 2f;
			Vector2 cwTex = new Vector2(cw.X + 1, -cw.Y + 1) / 2f;

			#region Vertices

			var vertices = new[] {
			                     	new VertexPositionColorTexture {
			                     	                               	Position = Vector3.Zero,
			                     	                               	Color = color,
			                     	                               	TextureCoordinate = new Vector2(0.5f, 0.5f),
			                     	                               },
			                     	new VertexPositionColorTexture {
			                     	                               	Position = new Vector3(ccw, 0),
			                     	                               	Color = color,
			                     	                               	TextureCoordinate = ccwTex
			                     	                               },
			                     	new VertexPositionColorTexture {
			                     	                               	Position = new Vector3(-1, 1, 0),
			                     	                               	Color = color,
			                     	                               	TextureCoordinate = new Vector2(0, 0),
			                     	                               },
			                     	new VertexPositionColorTexture {
			                     	                               	Position = new Vector3(1, 1, 0),
			                     	                               	Color = color,
			                     	                               	TextureCoordinate = new Vector2(1, 0),
			                     	                               },
			                     	new VertexPositionColorTexture {
			                     	                               	Position = new Vector3(1, -1, 0),
			                     	                               	Color = color,
			                     	                               	TextureCoordinate = new Vector2(1, 1),
			                     	                               },
			                     	new VertexPositionColorTexture {
			                     	                               	Position = new Vector3(-1, -1, 0),
			                     	                               	Color = color,
			                     	                               	TextureCoordinate = new Vector2(0, 1),
			                     	                               },
			                     	new VertexPositionColorTexture {
			                     	                               	Position = new Vector3(cw, 0),
			                     	                               	Color = color,
			                     	                               	TextureCoordinate = cwTex,
			                     	                               },
			                     };

			Matrix r = Matrix.CreateRotationZ(rotation) * Matrix.CreateScale(size / 2) * Matrix.CreateTranslation(new Vector3(position, 0));

			for (int i = 0; i < vertices.Length; i++) {
				VertexPositionColorTexture vertex = vertices[i];

				Vector3.Transform(ref vertex.Position, ref r, out vertex.Position);

				vertices[i] = vertex;
			}

			#endregion Vertices

			Int32[] indicies;

			#region Indicies

			if (fov <= MathHelper.Pi / 2) {
				indicies = new[] {
				                 	0, 1, 6,
				                 };
			} else if (fov <= 3 * MathHelper.Pi / 2) {
				indicies = new[] {
				                 	0, 1, 3,
				                 	0, 3, 4,
				                 	0, 4, 6,
				                 };
			} else {
				indicies = new[] {
				                 	0, 1, 2,
				                 	0, 2, 3,
				                 	0, 3, 4,
				                 	0, 4, 5,
				                 	0, 5, 6,
				                 };
			}

			#endregion Indicies

			graphicsDevice.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, vertices, 0, vertices.Length, indicies, 0, indicies.Length / 3);
		}

		public static Vector2 ClampToBox (float angle) {
			double x = Math.Cos(angle);
			double y = Math.Sin(angle);
			double absMax = Math.Max(Math.Abs(x), Math.Abs(y));

			return new Vector2((float)(x / absMax), (float)(y / absMax));
		}

		public void BufferDraw () {
			if (shadowHullIndicies.Count >= 3) {
				graphicsDevice.DrawUserIndexedPrimitives(
					PrimitiveType.TriangleList, shadowHullVertices.ToArray(), 0, shadowHullVertices.Count,
					shadowHullIndicies.ToArray(), 0, shadowHullIndicies.Count / 3);
			}
		}

		public void DrawFullscreenQuad () {
			// Draw the quad
			effect.CurrentTechnique = effect.Techniques["ScreenCopy"];

			effect.Parameters["TexelBias"].SetValue(new Vector2(0.5f / graphicsDevice.Viewport.Width, 0.5f / graphicsDevice.Viewport.Height));

			foreach (EffectPass effectPass in effect.CurrentTechnique.Passes) {
				effectPass.Apply();
				graphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleStrip, UnitQuad, 0, 2);
			}
		}

		public void BlurTextureToTarget (Texture2D texture, LightMapSize mapSize, BlurTechnique blurTechnique, float bluriness) {
			// Get the pass to use
			string passName = "";

			switch (blurTechnique) {
				case (BlurTechnique.Horizontal):
					effect.Parameters["BlurFactorU"].SetValue(1f / GraphicsDevice.PresentationParameters.BackBufferWidth);
					passName = "HorizontalBlur";
					break;

				case (BlurTechnique.Vertical):
					effect.Parameters["BlurFactorV"].SetValue(1f / graphicsDevice.PresentationParameters.BackBufferHeight);
					passName = "VerticalBlur";
					break;
			}

			float biasFactor = BiasFactorFromLightMapSize(mapSize);

			// Calculate the texel bias
			var texelBias = new Vector2 {
				X = biasFactor / graphicsDevice.Viewport.Width,
				Y = biasFactor / graphicsDevice.Viewport.Height,
			};


			effect.Parameters["Texture0"].SetValue(texture);
			effect.Parameters["TexelBias"].SetValue(texelBias);
			effect.Parameters["Bluriness"].SetValue(bluriness);
			effect.CurrentTechnique = effect.Techniques["Blur"];

			effect.CurrentTechnique.Passes[passName].Apply();
			graphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleStrip, UnitQuad, 0, 2);
		}

		public void DrawTextureToTarget (Texture2D texture, LightMapSize mapSize, BlendTechnique blend) {
			// Get the technique to use
			string techniqueName = "";

			switch (blend) {
				case (BlendTechnique.Add):
					techniqueName = "TextureToTarget_Add";
					break;

				case (BlendTechnique.Multiply):
					techniqueName = "TextureToTarget_Multiply";
					break;
			}

			float biasFactor = BiasFactorFromLightMapSize(mapSize);

			// Calculate the texel bias
			var texelBias = new Vector2 {
				X = biasFactor / graphicsDevice.ScissorRectangle.Width,
				Y = biasFactor / graphicsDevice.ScissorRectangle.Height,
			};

			effect.Parameters["Texture0"].SetValue(texture);
			effect.Parameters["TexelBias"].SetValue(texelBias);
			effect.CurrentTechnique = effect.Techniques[techniqueName];

			// Draw the quad
			foreach (EffectPass effectPass in effect.CurrentTechnique.Passes) {
				effectPass.Apply();
				graphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleStrip, UnitQuad, 0, 2);
			}
		}

		static float BiasFactorFromLightMapSize (LightMapSize mapSize) {
			switch (mapSize) {
				case (LightMapSize.Full):
					return 0.5f;

				case (LightMapSize.Fourth):
					return 0.6f;

				case (LightMapSize.Eighth):
					return 0.7f;

				default:
					return 0.0f;
			}
		}

		public void BufferAddBoundOutline (BoundingRect boundingRect) {
			int vertexCount = shadowHullVertices.Count;

			shadowHullVertices.Add(new ShadowHullVertex {
				Color = Color.Black,
				Normal = Vector2.Zero,
				Position = new Vector2(boundingRect.Left, boundingRect.Top)
			});

			shadowHullVertices.Add(new ShadowHullVertex {
				Color = Color.Black,
				Normal = Vector2.Zero,
				Position = new Vector2(boundingRect.Right, boundingRect.Top)
			});

			shadowHullVertices.Add(new ShadowHullVertex {
				Color = Color.Black,
				Normal = Vector2.Zero,
				Position = new Vector2(boundingRect.Right, boundingRect.Bottom)
			});

			shadowHullVertices.Add(new ShadowHullVertex {
				Color = Color.Black,
				Normal = Vector2.Zero,
				Position = new Vector2(boundingRect.Left, boundingRect.Bottom)
			});

			shadowHullIndicies.Add(vertexCount + 0);
			shadowHullIndicies.Add(vertexCount + 1);
			shadowHullIndicies.Add(vertexCount + 2);

			shadowHullIndicies.Add(vertexCount + 0);
			shadowHullIndicies.Add(vertexCount + 2);
			shadowHullIndicies.Add(vertexCount + 3);
		}
	}
}
