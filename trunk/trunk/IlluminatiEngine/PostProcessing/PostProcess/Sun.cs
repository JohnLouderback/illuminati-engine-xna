﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace IlluminatiEngine.PostProcessing
{
    public class Sun : BasePostProcess
    {
        public Vector3 Position;
        public Color Color = Color.White;
        public float Intensity = 1f;
        public float Size = 1500;

        public Sun(Game game, Vector3 position)
            : base(game)
        {
            Position = position;
        }

        public override void Draw(GameTime gameTime)
        {
            if (effect == null)
                effect = AssetManager.GetAsset<Effect>("Shaders/PostProcessing/Sun");

            effect.Parameters["depthMap"].SetValue(DepthBuffer);
            effect.Parameters["cameraPosition"].SetValue(camera.Position);
            effect.Parameters["lightPosition"].SetValue(Position);
            effect.Parameters["Color"].SetValue(Color.ToVector3());
            effect.Parameters["lightIntensity"].SetValue(Intensity);
            effect.Parameters["SunSize"].SetValue(Size);

            effect.Parameters["VP"].SetValue(camera.View * camera.Projection);
            effect.Parameters["flare"].SetValue(AssetManager.GetAsset<Texture2D>("Textures/flare"));

            // Set Params.
            base.Draw(gameTime);
        }
    }
}
