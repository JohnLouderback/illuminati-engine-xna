﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using IlluminatiEngine.Renderer.Deferred;

namespace IlluminatiEngine.PostProcessing
{
    public class BasePostProcess
    {
        public Vector2 HalfPixel;

        public IAssetManager AssetManager
        {
            get { return ((IAssetManager)Game.Services.GetService(typeof(IAssetManager))); }
        }

        public ICameraService camera
        {
            get { return ((ICameraService)Game.Services.GetService(typeof(ICameraService))); }
        }

        public SpriteBatch spriteBatch
        {
            get { return (SpriteBatch)Game.Services.GetService(typeof(SpriteBatch)); }
        }

        public Texture2D DepthBuffer;

        public Texture2D BackBuffer;
        public Texture2D orgBuffer;
        public Texture2D normalBuffer;

        public bool Enabled = true;

        Texture2D b;

        public SpriteSortMode SortMode = SpriteSortMode.Immediate;
        public BlendState Blend = BlendState.Opaque;
        public SamplerState Sampler = SamplerState.AnisotropicClamp;

        public SurfaceFormat newSceneSurfaceFormat = SurfaceFormat.Color;

        protected Effect effect;

        protected Game Game;
        public RenderTarget2D newScene;

        ScreenQuad sq;

        public bool UsesVertexShader = false;

        public BasePostProcess(Game game)
        {
            Game = game;
            
        }
        public virtual void Update(GameTime gameTime)
        { 
            
        }
        public virtual void Draw(GameTime gameTime)
        {
            if (Enabled)
            {
                if (sq == null)
                {
                    sq = new ScreenQuad(Game);
                    sq.Initialize();
                }
                if (!UsesVertexShader)
                    spriteBatch.Begin(SortMode, Blend, Sampler, DepthStencilState.None, RasterizerState.CullCounterClockwise);
                else
                {
                    Game.GraphicsDevice.SamplerStates[0] = Sampler;
                }

                effect.CurrentTechnique.Passes[0].Apply();

                if (UsesVertexShader)
                    sq.Draw(-Vector2.One, Vector2.One);
                else
                {
                    spriteBatch.Draw(BackBuffer, new Rectangle(0, 0, Game.GraphicsDevice.Viewport.Width, Game.GraphicsDevice.Viewport.Height), Color.White);
                    spriteBatch.End();
                }
            }
        }
    }
}
