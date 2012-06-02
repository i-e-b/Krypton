using System;
using System.Collections.Generic;
using Krypton.Common;
using Krypton.Lights;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Krypton {
	/// <summary>
	/// A GPU-based 2D lighting engine
	/// </summary>
	public class KryptonEngine : DrawableGameComponent {
		// The Krypton Effect
		readonly string effectAssetName;

		// The goods
		readonly List<ShadowHull> hulls = new List<ShadowHull>();
		readonly List<ILight2D> rights = new List<ILight2D>();

		// World View Projection matrix, and it's min and max view bounds
		BoundingRect bounds = BoundingRect.MinMax;
		CullMode cullMode = CullMode.CullCounterClockwiseFace;
		Effect effect;

		// Blur
		Color ambientColor = new Color(0, 0, 0);
		float bluriness;
		LightMapSize lightMapSize = LightMapSize.Full;
		RenderTarget2D map;
		RenderTarget2D mapBlur;
		bool spriteBatchCompatabilityEnabled;
		Matrix wvp = Matrix.Identity;

		/// <summary>
		/// Constructs a new instance of krypton
		/// </summary>
		/// <param name="game">Your game object</param>
		/// <param name="effectAssetName">The asset name of Krypton's effect file, which must be included in your content project</param>
		public KryptonEngine(Game game, string effectAssetName) : base(game) {
			this.effectAssetName = effectAssetName;
		}

		/// <summary>
		/// Krypton's render helper. It helps render. It also needs to be re-written.
		/// </summary>
		public KryptonRenderHelper RenderHelper { get; private set; }

		/// <summary>
		/// Gets or sets a value indicating how Krypton should cull geometry. The default value is CullMode.CounterClockwise
		/// </summary>
		public CullMode CullMode {
			get { return cullMode; }
			set { cullMode = value; }
		}

		/// <summary>
		/// The collection of lights krypton uses to render shadows
		/// </summary>
		public List<ILight2D> Lights {
			get { return rights; }
		}

		/// <summary>
		/// The collection of hulls krypton uses to render shadows
		/// </summary>
		public List<ShadowHull> Hulls {
			get { return hulls; }
		}

		/// <summary>
		/// Gets or sets the matrix used to draw the light map. This should match your scene's matrix.
		/// </summary>
		public Matrix Matrix {
			get { return wvp; }
		}

		public void SetMatrix(Matrix value) {
			if (wvp == value) return;
			wvp = value;

			// This is totally ghetto, but it works for now. :)
			// Compute the world-space bounds of the given matrix
			UpdateWorldSpace(value);
		}

		void UpdateWorldSpace(Matrix value) {
			var inverse = Matrix.Invert(value);

			var v1 = Vector2.Transform(new Vector2(1, 1), inverse);
			var v2 = Vector2.Transform(new Vector2(1, -1), inverse);
			var v3 = Vector2.Transform(new Vector2(-1, -1), inverse);
			var v4 = Vector2.Transform(new Vector2(-1, 1), inverse);

			bounds.Min = v1;
			bounds.Min = Vector2.Min(bounds.Min, v2);
			bounds.Min = Vector2.Min(bounds.Min, v3);
			bounds.Min = Vector2.Min(bounds.Min, v4);

			bounds.Max = v1;
			bounds.Max = Vector2.Max(bounds.Max, v2);
			bounds.Max = Vector2.Max(bounds.Max, v3);
			bounds.Max = Vector2.Max(bounds.Max, v4);

			bounds = BoundingRect.MinMax;
		}

		/// <summary>
		/// Gets or sets a value indicating weither or not to use SpriteBatch's matrix when drawing lightmaps
		/// </summary>
		public bool SpriteBatchCompatablityEnabled {
			get { return spriteBatchCompatabilityEnabled; }
			set { spriteBatchCompatabilityEnabled = value; }
		}

		/// <summary>
		/// Ambient color of the light map. Lights + AmbientColor = Final 
		/// </summary>
		public Color AmbientColor {
			get { return ambientColor; }
			set { ambientColor = value; }
		}

		/// <summary>
		/// Gets or sets the value used to determine light map size
		/// </summary>
		public LightMapSize LightMapSize {
			get { return lightMapSize; }
		}

		public void SetLightMapSize(LightMapSize value) {
			if (lightMapSize == value) return;
			lightMapSize = value;
			DisposeRenderTargets();
			CreateRenderTargets();
		}

		/// <summary>
		/// Gets or sets a value indicating how much to blur the final light map. If the value is zero, the lightmap will not be blurred
		/// </summary>
		public float Bluriness {
			get { return bluriness; }
			set { bluriness = Math.Max(0, value); }
		}

		/// <summary>
		/// Initializes Krpyton, and hooks itself to the graphics device
		/// </summary>
		public override void Initialize() {
			base.Initialize();

			GraphicsDevice.DeviceReset += GraphicsDeviceDeviceReset;
		}

		/// <summary>
		/// Resets kryptons graphics device resources
		/// </summary>
		void GraphicsDeviceDeviceReset(object sender, EventArgs e) {
			DisposeRenderTargets();
			CreateRenderTargets();
		}

		/// <summary>
		/// Load's the graphics related content required to draw light maps
		/// </summary>
		protected override void LoadContent() {
			// This needs to better handle content loading...
			// if the window is resized, Krypton needs to notice.
			effect = Game.Content.Load<Effect>(effectAssetName);
			RenderHelper = new KryptonRenderHelper(GraphicsDevice, effect);

			CreateRenderTargets();
		}

		/// <summary>
		/// Unload's the graphics content required to draw light maps
		/// </summary>
		protected override void UnloadContent() {
			DisposeRenderTargets();
		}

		/// <summary>
		/// Creates render targets
		/// </summary>
		void CreateRenderTargets() {
			int targetWidth = GraphicsDevice.Viewport.Width/(int) (lightMapSize);
			int targetHeight = GraphicsDevice.Viewport.Height/(int) (lightMapSize);

			map = new RenderTarget2D(GraphicsDevice, targetWidth, targetHeight, false, SurfaceFormat.Color, DepthFormat.Depth24Stencil8, 0, RenderTargetUsage.PlatformContents);
			mapBlur = new RenderTarget2D(GraphicsDevice, targetWidth, targetHeight, false, SurfaceFormat.Color, DepthFormat.Depth24Stencil8, 0, RenderTargetUsage.PlatformContents);
		}

		/// <summary>
		/// Disposes of render targets
		/// </summary>
		void DisposeRenderTargets() {
			TryDispose(map);
			TryDispose(mapBlur);
		}

		/// <summary>
		/// Attempts to dispose of disposable objects, and assigns them a null value afterward
		/// </summary>
		/// <param name="obj"></param>
		static void TryDispose(IDisposable obj) {
			if (obj != null) {
				obj.Dispose();
			}
		}

		/// <summary>
		/// Draws the light map to the current render target
		/// </summary>
		/// <param name="gameTime">N/A - Required</param>
		public override void Draw(GameTime gameTime) {
			LightMapPresent();
		}

		/// <summary>
		/// Prepares the light map to be drawn (pre-render)
		/// </summary>
		public void LightMapPrepare() {
			// Prepare the matrix with optional settings and assign it to an effect parameter
			Matrix lightMapMatrix = LightmapMatrixGet();
			effect.Parameters["Matrix"].SetValue(lightMapMatrix);

			// Obtain the original rendering states
			RenderTargetBinding[] originalRenderTargets = GraphicsDevice.GetRenderTargets();

			// Set and clear the target
			GraphicsDevice.SetRenderTarget(map);
			GraphicsDevice.Clear(ClearOptions.Target | ClearOptions.Stencil, ambientColor, 0, 1);

			// Make sure we're culling the right way!
			GraphicsDevice.RasterizerState = RasterizerStateGetFromCullMode(cullMode);

			// put the render target's size into a more friendly format
			var targetSize = new Vector2(map.Width, map.Height);

			// Render Light Maps
			foreach (var light in rights) {
				// Loop through each light within the view frustum
				if (!light.Bounds.Intersects(bounds)) continue;
				// Clear the stencil and set the scissor rect (because we're stretching geometry past the light's reach)
				GraphicsDevice.Clear(ClearOptions.Stencil, Color.Black, 0, 0);
				GraphicsDevice.ScissorRectangle = ScissorRectCreateForLight(light, lightMapMatrix, targetSize);

				// Draw the light!
				light.Draw(RenderHelper, hulls);
			}

			if (bluriness > 0) {
				// Blur the shadow map horizontally to the blur target
				GraphicsDevice.SetRenderTarget(mapBlur);
				RenderHelper.BlurTextureToTarget(map, LightMapSize.Full, BlurTechnique.Horizontal, bluriness);

				// Blur the shadow map vertically back to the final map
				GraphicsDevice.SetRenderTarget(map);
				RenderHelper.BlurTextureToTarget(mapBlur, LightMapSize.Full, BlurTechnique.Vertical, bluriness);
			}

			// Reset to the original rendering states
			GraphicsDevice.SetRenderTargets(originalRenderTargets);
		}

		/// <summary>
		/// Returns the final, modified matrix used to render the lightmap.
		/// </summary>
		/// <returns></returns>
		Matrix LightmapMatrixGet() {
			if (spriteBatchCompatabilityEnabled) {
				float xScale = (GraphicsDevice.Viewport.Width > 0) ? (1f/GraphicsDevice.Viewport.Width) : 0f;
				float yScale = (GraphicsDevice.Viewport.Height > 0) ? (-1f/GraphicsDevice.Viewport.Height) : 0f;

				// This is the default matrix used to render sprites via spritebatch
				var matrixSpriteBatch = new Matrix {
					M11 = xScale * 2f,
					M22 = yScale * 2f,
					M33 = 1f,
					M44 = 1f,
					M41 = -1f - xScale,
					M42 = 1f - yScale,
				};

				// Return krypton's matrix, compensated for use with SpriteBatch
				return wvp*matrixSpriteBatch;
			}
			// Return krypton's matrix
			return wvp;
		}

		/// <summary>
		/// Gets a pixel-space rectangle which contains the light passed in
		/// </summary>
		/// <param name="light">The light used to create the rectangle</param>
		/// <param name="matrix">the WorldViewProjection matrix being used to render</param>
		/// <param name="targetSize">The rendertarget's size</param>
		/// <returns></returns>
		static Rectangle ScissorRectCreateForLight(ILight2D light, Matrix matrix, Vector2 targetSize) {
			// This needs refining, but it works as is (I believe)
			BoundingRect lightBounds = light.Bounds;

			Vector2 min = VectorToPixel(lightBounds.Min, matrix, targetSize);
			Vector2 max = VectorToPixel(lightBounds.Max, matrix, targetSize);

			Vector2 min2 = Vector2.Min(min, max);
			Vector2 max2 = Vector2.Max(min, max);

			min = Vector2.Clamp(min2, Vector2.Zero, targetSize);
			max = Vector2.Clamp(max2, Vector2.Zero, targetSize);

			return new Rectangle((int) (min.X), (int) (min.Y), (int) (max.X - min.X), (int) (max.Y - min.Y));
		}

		/// <summary>
		/// Takes a screen-space vector and puts it in to pixel space
		/// </summary>
		static Vector2 VectorToPixel(Vector2 v, Matrix matrix, Vector2 targetSize) {
			Vector2.Transform(ref v, ref matrix, out v);

			v.X = (1 + v.X)*(targetSize.X/2f);
			v.Y = (1 - v.Y)*(targetSize.Y/2f);

			return v;
		}

		/// <summary>
		/// Retrieves a rasterize state by using the cull mode as a lookup
		/// </summary>
		/// <param name="cullMode">The cullmode used to lookup the rasterize state</param>
		/// <returns></returns>
		static RasterizerState RasterizerStateGetFromCullMode(CullMode cullMode) {
			switch (cullMode) {
			case (CullMode.CullCounterClockwiseFace):
				return RasterizerState.CullCounterClockwise;

			case (CullMode.CullClockwiseFace):
				return RasterizerState.CullClockwise;

			default:
				return RasterizerState.CullNone;
			}
		}

		/// <summary>
		/// Presents the light map to the current render target
		/// </summary>
		void LightMapPresent() {
			RenderHelper.DrawTextureToTarget(map, lightMapSize);
		}
	}
}
