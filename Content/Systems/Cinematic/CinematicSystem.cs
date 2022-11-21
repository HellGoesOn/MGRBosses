using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;

namespace MGRBosses.Content.Systems.Cinematic
{
    public class CinematicSystem : ModSystem
    {
        public List<CinematicScene> Scenes { get; private set; }

        public override void Load()
        {
            Scenes = new List<CinematicScene>();
        }

        public override void Unload()
        {
            Scenes.Clear();
            Scenes = null;
        }

        public override void UpdateUI(GameTime gameTime)
        {
            if (Main.gameMenu)
                Scenes.Clear();

            base.UpdateUI(gameTime);
        }

        public override void PostUpdateEverything()
        {
            if(Scenes.Count > 0)
                Scenes[0].Update();

            Scenes.RemoveAll(x => x.HasEnded);
        }

        public override void ModifyScreenPosition()
        {
            if (Scenes.Count > 0)
                Main.screenPosition = Scenes[0].ScreenPosition;
        }

        public static CinematicScene AddCinematicScene(bool noDefaultPosition = false)
        {
            var result = new CinematicScene();
            if(!noDefaultPosition)
            result.screenPosition = Main.screenPosition + new Vector2(Main.screenWidth * 0.5f, Main.screenHeight * 0.5f);
            ModContent.GetInstance<CinematicSystem>().Scenes.Add(result);

            return result;
        }
    }
}
