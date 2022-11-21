using Microsoft.Xna.Framework.Input;
using Terraria.ModLoader;

namespace MGRBosses.Common
{
    public class InputSystem : ModSystem
    {
        public static ModKeybind BladeMode { get; private set; }
        public static ModKeybind Parry { get; private set; }

        public override void Load()
        {
            BladeMode = KeybindLoader.RegisterKeybind(Mod, "Blade Mode", Keys.C);
            Parry = KeybindLoader.RegisterKeybind(Mod, "Parry", Keys.Z);
        }

        public override void Unload()
        {
            BladeMode = null;
            Parry = null;
        }
    }
}
