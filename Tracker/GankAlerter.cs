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
        private static Obj_AI_Hero Player = ObjectManager.Player;
        private static readonly Dictionary<Obj_AI_Hero, int> Enemies = new Dictionary<Obj_AI_Hero, int>();
        private static SoundPlayer danger = new SoundPlayer(Tracker.Properties.Resources.danger);
        private static SoundPlayer activated = new SoundPlayer(Tracker.Properties.Resources.activated);
        private static SoundPlayer deactivated = new SoundPlayer(Tracker.Properties.Resources.deactivated);
        private static SoundPlayer logon = new SoundPlayer(Tracker.Properties.Resources.hev_logon);
        private static SoundPlayer shutdown = new SoundPlayer(Tracker.Properties.Resources.hev_shutdown);
        private static SoundPlayer voiceoff = new SoundPlayer(Tracker.Properties.Resources.voice_off);
        private static SoundPlayer voiceon = new SoundPlayer(Tracker.Properties.Resources.voice_on);
        private static int LastGankAttempt = 0;
        static GankAlerter()
        {
            playSound(activated);
            foreach (Obj_AI_Hero hero in ObjectManager.Get<Obj_AI_Hero>())
            {
                if (hero.IsEnemy)
                {
                Enemies.Add(hero, Environment.TickCount);
                }
            }
            //playSound(deactivated);
         
            //Used for detecting ganks:
            Game.OnGameUpdate += GameOnOnGameUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
        }    
        

        public static void AttachToMenu(Menu menu)
        {
            Config = menu.AddSubMenu(new Menu("Gank Tracker", "Gank Tracker"));
            Config.AddItem(new MenuItem("Enabled", "Enabled").SetValue(true));
        }

        private static void GameOnOnGameUpdate(EventArgs args)
        {
            
            
        }

        private static void playSound(SoundPlayer sound)
        {
                try
                {
                    sound.Play();
                }
                catch
                {
                    
                }
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            if (Config.Item("Enabled").GetValue<bool>() && Environment.TickCount-LastGankAttempt>15000+Game.Ping)
            {
                foreach (var enemy in Enemies)
                {
                    Obj_AI_Hero hero = enemy.Key;
                    if (!hero.IsValid)
                        continue;
                    bool hasSmite = false;
                    foreach (SpellDataInst spell in hero.SummonerSpellbook.Spells)
                    {
                        if (spell.Name.ToLower().Contains("smite"))
                        {
                            hasSmite = true;
                            break;
                        }
                    }

                    if (Player.Distance(hero, true) <= Math.Pow(2500, 2) && hasSmite)
                    {
                        playSound(danger);
                        LastGankAttempt = Environment.TickCount;
                    }
                }
            }
            
        }

    }
    
}
