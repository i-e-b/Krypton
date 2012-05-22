using System;
using Krypton;
using Krypton.Lights;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace KryptonTestbed {
	/// <summary>
	/// This is the main type for your game
	/// </summary>
	public class KryptonDemoGame : Game {
		protected readonly GraphicsDeviceManager Graphics;

		readonly KryptonEngine krypton;
		readonly Random mRandom = new Random();
		Light2D mLight2D;

		Texture2D mLightTexture;
		const int NumHorzontalHulls = 20;
		const int NumLights = 25;
		const int NumVerticalHulls = 20;

		const float VerticalUnits = 50;

		public KryptonDemoGame () {
			// Setup the graphics device manager with some default settings
			Graphics = new GraphicsDeviceManager(this) { PreferredBackBufferWidth = 1280, PreferredBackBufferHeight = 720 };

			// Allow the window to be resized (to demonstrate render target recreation)
			Window.AllowUserResizing = true;

			// Setup the content manager with some default settings
			Content.RootDirectory = "Content";

			// Create Krypton
			krypton = new KryptonEngine(this, "KryptonEffect");
		}

		protected override void Initialize () {
			// Make sure to initialize krpyton, unless it has been added to the Game's list of Components
			krypton.Initialize();

			base.Initialize();
		}

		protected override void LoadContent () {
			// Create a new simple point light texture to use for the lights
			mLightTexture = LightTextureBuilder.CreatePointLight(GraphicsDevice, 512);

			// Create some lights and hulls
			CreateLights(mLightTexture, NumLights);
			CreateHulls(NumHorzontalHulls, NumVerticalHulls);

			// Create a light we can control
			mLight2D = new Light2D {
				Texture = mLightTexture,
				X = 0,
				Y = 0,
				Range = 25,
				Color = Color.Multiply(Color.CornflowerBlue, 2.0f),
				ShadowType = ShadowType.Occluded
			};

			krypton.Lights.Add(mLight2D);
		}

		void CreateLights (Texture2D texture, int count) {
			// Make some random lights!
			for (int i = 0; i < count; i++) {
				var r = (byte)(mRandom.Next(255 - 64) + 64);
				var g = (byte)(mRandom.Next(255 - 64) + 64);
				var b = (byte)(mRandom.Next(255 - 64) + 64);

				var light = new Light2D {
					Texture = texture,
					Range = (float)(mRandom.NextDouble() * 5 + 5),
					Color = new Color(r, g, b),
					//Intensity = (float)(this.mRandom.NextDouble() * 0.25 + 0.75),
					Intensity = 1f,
					Angle = MathHelper.TwoPi * (float)mRandom.NextDouble(),
					X = (float)(mRandom.NextDouble() * 50 - 25),
					Y = (float)(mRandom.NextDouble() * 50 - 25),
				};

				// Here we set the light's field of view
				if (i % 2 == 0) {
					light.Fov = MathHelper.PiOver2 * (float)(mRandom.NextDouble() * 0.75 + 0.25);
				}

				krypton.Lights.Add(light);
			}
		}

		void CreateHulls (int x, int y) {
			const float w = 50;
			const float h = 50;

			// Make lines of lines of hulls!
			for (int j = 0; j < y; j++) {
				// Make lines of hulls!
				for (int i = 0; i < x; i++) {
					float posX = (((i + 0.5f) * w) / x) - w / 2 + (j % 2 == 0 ? w / x / 2 : 0);
					float posY = (((j + 0.5f) * h) / y) - h / 2; // +(i % 2 == 0 ? h / y / 4 : 0);

					ShadowHull hull = ShadowHull.CreateRectangle(Vector2.One * 1f);
					hull.Position.X = posX;
					hull.Position.Y = posY;
					hull.Scale.X = (float)(mRandom.NextDouble() * 0.75f + 0.25f);
					hull.Scale.Y = (float)(mRandom.NextDouble() * 0.75f + 0.25f);

					krypton.Hulls.Add(hull);
				}
			}
		}

		protected override void UnloadContent () { }

		protected override void Update (GameTime gameTime) {
			// Make sure the user doesn't want to quit (but why would they?)
			if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState(PlayerIndex.One).IsKeyDown(Keys.Escape))
				Exit();

			// make it much simpler to deal with the time :)
			var t = (float)gameTime.ElapsedGameTime.TotalSeconds;

			const int speed = 5;

			// Allow for randomization of lights and hulls, to demonstrait that each hull and light is individually rendered
			if (Keyboard.GetState().IsKeyDown(Keys.R)) {
				// randomize lights
				foreach (Light2D light in krypton.Lights) {
					light.Position += Vector2.UnitY * (float)(mRandom.NextDouble() * 2 - 1) * t * speed;
					light.Position += Vector2.UnitX * (float)(mRandom.NextDouble() * 2 - 1) * t * speed;
					light.Angle -= MathHelper.TwoPi * (float)(mRandom.NextDouble() * 2 - 1) * t * speed;
				}

				// randomize hulls
				foreach (var hull in krypton.Hulls) {
					hull.Position += Vector2.UnitY * (float)(mRandom.NextDouble() * 2 - 1) * t * speed;
					hull.Position += Vector2.UnitX * (float)(mRandom.NextDouble() * 2 - 1) * t * speed;
					hull.Angle -= MathHelper.TwoPi * (float)(mRandom.NextDouble() * 2 - 1) * t * speed;
				}
			}

			KeyboardState keyboard = Keyboard.GetState();

			// Light Position Controls
			if (keyboard.IsKeyDown(Keys.Up))
				mLight2D.Y += t * speed;

			if (keyboard.IsKeyDown(Keys.Down))
				mLight2D.Y -= t * speed;

			if (keyboard.IsKeyDown(Keys.Right))
				mLight2D.X += t * speed;

			if (keyboard.IsKeyDown(Keys.Left))
				mLight2D.X -= t * speed;

			// Shadow Type Controls
			if (keyboard.IsKeyDown(Keys.D1))
				mLight2D.ShadowType = ShadowType.Solid;

			if (keyboard.IsKeyDown(Keys.D2))
				mLight2D.ShadowType = ShadowType.Illuminated;

			if (keyboard.IsKeyDown(Keys.D3))
				mLight2D.ShadowType = ShadowType.Occluded;

			// Shadow Opacity Controls
			if (keyboard.IsKeyDown(Keys.O))
				krypton.Hulls.ForEach(x => x.Opacity = MathHelper.Clamp(x.Opacity - t, 0, 1));

			if (keyboard.IsKeyDown(Keys.P))
				krypton.Hulls.ForEach(x => x.Opacity = MathHelper.Clamp(x.Opacity + t, 0, 1));

			base.Update(gameTime);
		}

		protected override void Draw (GameTime gameTime) {
			// Create a world view projection matrix to use with krypton
			Matrix world = Matrix.Identity;
			Matrix view = Matrix.CreateTranslation(new Vector3(0, 0, 0) * -1f);
			Matrix projection = Matrix.CreateOrthographic(VerticalUnits * GraphicsDevice.Viewport.AspectRatio, VerticalUnits, 0, 1);
			Matrix wvp = world * view * projection;

			// Assign the matrix and pre-render the lightmap.
			// Make sure not to change the position of any lights or shadow hulls after this call, as it won't take effect till the next frame!
			krypton.Matrix = wvp;
			krypton.LightMapPrepare();

			// Make sure we clear the backbuffer *after* Krypton is done pre-rendering
			GraphicsDevice.Clear(Color.White);

			// ----- DRAW STUFF HERE ----- //
			// By drawing here, you ensure that your scene is properly lit by krypton.
			// Drawing after KryptonEngine.Draw will cause you objects to be drawn on top of the lightmap (can be useful, fyi)
			// ----- DRAW STUFF HERE ----- //

			// Draw hulls
			DebugDrawHulls(true);

			// Draw krypton (This can be omited if krypton is in the Component list. It will simply draw krypton when base.Draw is called
			krypton.Draw(gameTime);

			if (Keyboard.GetState().IsKeyDown(Keys.H)) {
				// Draw hulls
				DebugDrawHulls(false);
			}

			if (Keyboard.GetState().IsKeyDown(Keys.L)) {
				// Draw hulls
				DebugDrawLights();
			}

			base.Draw(gameTime);
		}

		void DebugDrawHulls (bool drawSolid) {
			krypton.RenderHelper.Effect.CurrentTechnique = krypton.RenderHelper.Effect.Techniques["DebugDraw"];

			GraphicsDevice.RasterizerState = new RasterizerState {
				CullMode = CullMode.None,
				FillMode = drawSolid ? FillMode.Solid : FillMode.WireFrame,
			};

			// Clear the helpers vertices
			krypton.RenderHelper.ShadowHullVertices.Clear();
			krypton.RenderHelper.ShadowHullIndicies.Clear();

			foreach (var hull in krypton.Hulls) {
				krypton.RenderHelper.BufferAddShadowHull(hull);
			}


			foreach (var effectPass in krypton.RenderHelper.Effect.CurrentTechnique.Passes) {
				effectPass.Apply();
				krypton.RenderHelper.BufferDraw();
			}
		}

		void DebugDrawLights () {
			krypton.RenderHelper.Effect.CurrentTechnique = krypton.RenderHelper.Effect.Techniques["DebugDraw"];

			GraphicsDevice.RasterizerState = new RasterizerState {
				CullMode = CullMode.None,
				FillMode = FillMode.WireFrame,
			};

			// Clear the helpers vertices
			krypton.RenderHelper.ShadowHullVertices.Clear();
			krypton.RenderHelper.ShadowHullIndicies.Clear();

			foreach (Light2D light in krypton.Lights) {
				krypton.RenderHelper.BufferAddBoundOutline(light.Bounds);
			}

			foreach (EffectPass effectPass in krypton.RenderHelper.Effect.CurrentTechnique.Passes) {
				effectPass.Apply();
				krypton.RenderHelper.BufferDraw();
			}
		}
	}
}
