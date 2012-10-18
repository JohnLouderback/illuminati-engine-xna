﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace IlluminatiEngine.PostProcessing
{
    public class DepthOfField : BasePostProcess
    {
        public float FocusDistance = 50;
        public float FocusRange = 5;

        public DepthOfField(Game game)
            : base(game)
        {

        }

        public override void Draw(GameTime gameTime)
        {
            if (effect == null)
                effect = AssetManager.GetAsset<Effect>("Shaders/PostProcessing/DepthOfField");

            effect.Parameters["SceneTex"].SetValue(orgBuffer);
            effect.Parameters["SceneDepthTex"].SetValue(DepthBuffer);
            effect.Parameters["DoFParams"].SetValue(new Vector4(FocusDistance, FocusRange, camera.Viewport.MinDepth, camera.Viewport.MaxDepth / (camera.Viewport.MaxDepth - camera.Viewport.MinDepth)));

            // Set Params.
            base.Draw(gameTime);
        }
    }
}
