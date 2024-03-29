﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace IlluminatiEngine.PostProcessing
{
    public class PostProcessingManager
    {
        protected Game Game;
        public Texture2D Scene;
        public Texture2D DepthBuffer;

        //public RenderTarget2D newScene;

        protected List<BasePostProcessingEffect> postProcessingEffects = new List<BasePostProcessingEffect>();

        public Vector2 HalfPixel;

        public SpriteBatch spriteBatch
        {
            get { return (SpriteBatch)Game.Services.GetService(typeof(SpriteBatch)); }
        }

        public PostProcessingManager(Game game)
        {
            Game = game;
        }

        public void AddEffect(BasePostProcessingEffect ppEfect)
        {
            postProcessingEffects.Add(ppEfect);
        }

        public virtual void Update(GameTime gameTime)
        {
            int maxEffect = postProcessingEffects.Count;
            for (int e = 0; e < maxEffect; e++)
            {
                if (postProcessingEffects[e].Enabled)
                {
                    postProcessingEffects[e].Update(gameTime);
                }
            }
        }

        public virtual void Draw(GameTime gameTime, Texture2D scene, Texture2D depth, Texture2D normal)
        {
            if (HalfPixel == Vector2.Zero)
                HalfPixel = -new Vector2(.5f / (float)Game.GraphicsDevice.Viewport.Width,
                                     .5f / (float)Game.GraphicsDevice.Viewport.Height);

            int maxEffect = postProcessingEffects.Count;

            Scene = scene;

            for (int e = 0; e < maxEffect; e++)
            {
                if (postProcessingEffects[e].Enabled)
                {
                    if (postProcessingEffects[e].HalfPixel == Vector2.Zero)
                        postProcessingEffects[e].HalfPixel = HalfPixel;

                    postProcessingEffects[e].orgScene = scene;
                    postProcessingEffects[e].Draw(gameTime, Scene, depth, normal);
                    Scene = postProcessingEffects[e].lastScene;
                }
            }

            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Opaque,SamplerState.PointClamp,DepthStencilState.Default,RasterizerState.CullCounterClockwise);
            spriteBatch.Draw(Scene, new Rectangle(0, 0, Game.GraphicsDevice.Viewport.Width, Game.GraphicsDevice.Viewport.Height), Color.White);
            spriteBatch.End();
        }
    }
}
