using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.GameContent;

namespace MGRBosses.Core
{
    public class BladeModeGore
    {
        public static List<BladeModeGore> ActiveGore { get; } = new List<BladeModeGore>();

        public static int oldCount;

        public NPC npcCopy;

        public static BladeModeGore ActivateGore(NPC targetNPC, Rectangle worldRect)
        {
            if (ActiveGore.Count(x => x.whoAmIFor == targetNPC.whoAmI) > 0)
                return null;

            oldCount = ActiveGore.Count;
            BladeModeGore gore = new BladeModeGore(targetNPC, targetNPC.position, worldRect);
            ActiveGore.Add(gore);
            return gore;
        }
        
        public BladeModeGore(NPC createFor, Vector2 worldPosition, Rectangle worldRectangle)
        {
            Texture2D tex = TextureAssets.Npc[createFor.type].Value;
            npcCopy = new NPC();
            npcCopy.SetDefaults(createFor.type);
            npcCopy.direction = createFor.direction;
            npcCopy.spriteDirection = createFor.spriteDirection;
            npcCopy.rotation = createFor.rotation;
            npcCopy.scale = createFor.scale;

            npcCopy.position = worldPosition - createFor.Size;
            testTimer = 0;
            int x = (int)(worldPosition.X)- (int)Main.screenPosition.X ;
            int y = (int)(worldPosition.Y) - (int)Main.screenPosition.Y;
            int frameHeight = (tex.Height / Main.npcFrameCount[npcCopy.type]);
            worldRect = new Rectangle(x - tex.Width, y - frameHeight / 2 - (int)npcCopy.gfxOffY, tex.Width * 2, (tex.Height / Main.npcFrameCount[npcCopy.type]) * 2);
            whoAmIFor = createFor.whoAmI;

            this.worldPosition = worldPosition;
        }

        private Rectangle worldRect;

        public float testTimer;
        public Vector2 worldPosition;

        public int whoAmIFor;

        public void Draw()
        {
            testTimer += 0.4f;

            RenderTarget2D tex2d = BladeModeSystem.screenReplicationTarget;

            if (tex2d != null)
            {
                Rectangle realRect = new Rectangle(worldRect.X, worldRect.Y, worldRect.Width, worldRect.Height / 2);
                Rectangle realRect2 = new Rectangle(worldRect.X, worldRect.Y + worldRect.Height / 2, worldRect.Width, worldRect.Height / 2);
                Main.EntitySpriteDraw(tex2d, worldPosition - new Vector2(0, testTimer) - Main.screenPosition, realRect, Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 1);
                Main.EntitySpriteDraw(tex2d, worldPosition + new Vector2(0, realRect2.Height + testTimer) - Main.screenPosition, realRect2, Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 1);
            }
        }
    }
}
