#region

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;
using System.Media;


using System.IO;

#endregion


namespace Tracker
{
    public static class GankAlerter
    {
        public static Menu Config;
        private static SoundPlayer danger = new SoundPlayer(Tracker.Properties.Resources.danger);
        private static SoundPlayer activated = new SoundPlayer(Tracker.Properties.Resources.activated);
        private static SoundPlayer deactivated = new SoundPlayer(Tracker.Properties.Resources.deactivated);
        private static SoundPlayer logon = new SoundPlayer(Tracker.Properties.Resources.hev_logon);
        private static SoundPlayer shutdown = new SoundPlayer(Tracker.Properties.Resources.hev_shutdown);
        private static SoundPlayer voiceoff = new SoundPlayer(Tracker.Properties.Resources.voice_off);
        private static SoundPlayer voiceon = new SoundPlayer(Tracker.Properties.Resources.voice_on);
        static GankAlerter()
        {
            playSound(logon);
            //playSound(deactivated);
         
            //Used for detecting ganks:
            Game.OnGameUpdate += GameOnOnGameUpdate;
        }    
        

        public static void AttachToMenu(Menu menu)
        {
            Config = menu.AddSubMenu(new Menu("Gank Tracker", "Gank Tracker"));
            Config.AddItem(new MenuItem("Enabled", "Enabled").SetValue(true));
            Config.AddItem(new MenuItem("ComboActive", "Combo").SetValue(new KeyBind(32, KeyBindType.Press)));
        }

        private static void GameOnOnGameUpdate(EventArgs args)
        {
            if (Config.Item("Enabled").GetValue<bool>())
            {
                Game.PrintChat("testa");
                
            }
            if (Config.Item("ComboActive").GetValue<KeyBind>().Active)
            {
                playSound(danger);
                
            }
        }

        private static void playSound(SoundPlayer sound){
                try
                {
                    sound.Play();
                }
                catch(Exception ex)
                {
                    
                }
        }

    }
    
}
