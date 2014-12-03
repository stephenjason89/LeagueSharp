using System.Collections.Generic;
using System.Linq;
using System;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;
using System.Media;

namespace GagongXerath
{
    class Program
    {
        private const string ChampName = "Yasuo";
        private static Obj_AI_Hero Player = ObjectManager.Player;
        private static SoundPlayer welcome = new SoundPlayer(GagongXerath.Properties.Resources.Welcome);
        private static SoundPlayer ballstotheface = new SoundPlayer(GagongXerath.Properties.Resources.BallsToTheFace);
        private static SoundPlayer imkillingthebitch = new SoundPlayer(GagongXerath.Properties.Resources.ImKillingTheBitch);
        private static SoundPlayer ohdontyoudare = new SoundPlayer(GagongXerath.Properties.Resources.OhDontYouDare);
        private static SoundPlayer ohidiot = new SoundPlayer(GagongXerath.Properties.Resources.OhIdiot);
        private static SoundPlayer whosthebitchnow = new SoundPlayer(GagongXerath.Properties.Resources.WhosTheBitchNow);
        private static SoundPlayer yourdeadmeatasshole = new SoundPlayer(GagongXerath.Properties.Resources.YourDeadMeatAsshole);
        private static SoundPlayer diefucker = new SoundPlayer(GagongXerath.Properties.Resources.DieFucker);
        private static SoundPlayer goingsomewhereasshole = new SoundPlayer(GagongXerath.Properties.Resources.GoingSomewhereAsshole);
        private static SoundPlayer ilovethisgame = new SoundPlayer(GagongXerath.Properties.Resources.ILoveThisGame);
        private static int LastPlayedSound = 0;

        //Collision
        private static int WallCastT;
        private static Vector2 YasuoWallCastedPos;
        private static GameObject YasuoWall = null;


        //Create spells
        private static List<Spell> SpellList = new List<Spell>();
        private static Spell Q;
        private static Spell W;
        private static Spell E;
        private static Spell R;
        private static Spell QE;
        private static int QWLastcast = 0;

        //Summoner spells
        public static SpellSlot IgniteSlot;
        public static SpellSlot FlashSlot;
        private static int FlashLastCast = 0;

        //Key binds
        public static MenuItem comboKey;
        public static MenuItem harassKey;
        public static MenuItem laneclearKey;
        public static MenuItem lanefreezeKey;
        
        //Items
        public static Items.Item DFG;

        //Orbwalker instance
        private static Orbwalking.Orbwalker Orbwalker;

        private static Menu Menu;
        private static Menu orbwalkerMenu;

        static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        static void Game_OnGameLoad(EventArgs args)
        {
            if (Player.BaseSkinName != ChampName) return;
            
            //Spells data
            Q = new Spell(SpellSlot.Q, 800);
            Q.SetSkillshot(0.74f, 125f, float.MaxValue, false, SkillshotType.SkillshotCircle);

            W = new Spell(SpellSlot.W, 930);
            W.SetSkillshot(0.25f, 140f, 1400f, false, SkillshotType.SkillshotCircle);

            E = new Spell(SpellSlot.E, 700);
            E.SetSkillshot(0.25f, (float)(45 * 0.5), 2500, false, SkillshotType.SkillshotCone);         

            R = new Spell(SpellSlot.R, 675);
            R.SetTargetted(0.5f, 1100f);

            QE = new Spell(SpellSlot.E, 1292);
            QE.SetSkillshot(0.98f, 55f, 6500f, false, SkillshotType.SkillshotLine);


            IgniteSlot = Player.GetSpellSlot("SummonerDot");
            FlashSlot = Player.GetSpellSlot("summonerflash");

            DFG = Utility.Map.GetMap()._MapType == Utility.Map.MapType.TwistedTreeline ? new Items.Item(3188, 750) : new Items.Item(3128, 750);
            SpellList.Add(Q);
            SpellList.Add(W);
            SpellList.Add(E);
            SpellList.Add(R);
            
            //Base menu
            Menu = new Menu("GagongXerath", "GagongXerath", true);
            orbwalkerMenu = new Menu("Orbwalker", "Orbwalker");
            //SimpleTs
            Menu.AddSubMenu(new Menu("SimpleTs", "SimpleTs"));
            SimpleTs.AddToMenu(Menu.SubMenu("SimpleTs"));

            //Orbwalker
            orbwalkerMenu.AddItem(new MenuItem("Orbwalker_Mode", "Regular Orbwalker").SetValue(false));
            Menu.AddSubMenu(orbwalkerMenu);
            chooseOrbwalker(Menu.Item("Orbwalker_Mode").GetValue<bool>()); //uncomment this line

            //Combo
            Menu.AddSubMenu(new Menu("Combo", "Combo"));
            Menu.SubMenu("Combo").AddItem(new MenuItem("UseQ", "Use Q").SetValue(true));
            Menu.SubMenu("Combo").AddItem(new MenuItem("UseW", "Use W").SetValue(true));
            Menu.SubMenu("Combo").AddItem(new MenuItem("UseE", "Use E").SetValue(true));
            Menu.SubMenu("Combo").AddItem(new MenuItem("UseR", "Use R").SetValue(true));
            Menu.SubMenu("Combo").AddItem(new MenuItem("UseQE", "Use QE").SetValue(true));
            
            //Harass
            Menu.AddSubMenu(new Menu("Harass", "Harass"));
            Menu.SubMenu("Harass").AddItem(new MenuItem("UseQH", "Use Q").SetValue(true));
            Menu.SubMenu("Harass").AddItem(new MenuItem("HarassAAQ", "Harass with Q if enemy AA").SetValue(false));
            Menu.SubMenu("Harass").AddItem(new MenuItem("HarassTurret", "Disable Harass if Inside Enemy Turret").SetValue(false));
            Menu.SubMenu("Harass").AddItem(new MenuItem("UseWH", "Use W").SetValue(false));
            Menu.SubMenu("Harass").AddItem(new MenuItem("UseEH", "Use E").SetValue(false));
            Menu.SubMenu("Harass").AddItem(new MenuItem("UseQEH", "Use QE").SetValue(false));
            Menu.SubMenu("Harass").AddItem(new MenuItem("HarassMana", "Only Harass if mana >").SetValue(new Slider(0, 0, 100)));
            Menu.SubMenu("Harass").AddItem(new MenuItem("HarassActiveT", "Harass (toggle)!").SetValue(new KeyBind("Y".ToCharArray()[0], KeyBindType.Toggle,true)));

            //Farming menu:
            Menu.AddSubMenu(new Menu("Farm", "Farm"));
            Menu.SubMenu("Farm").AddItem(new MenuItem("UseQFarm", "Use Q").SetValue(new StringList(new[] { "Freeze", "LaneClear", "Both", "No" }, 2)));
            Menu.SubMenu("Farm").AddItem(new MenuItem("UseWFarm", "Use W").SetValue(new StringList(new[] { "Freeze", "LaneClear", "Both", "No" }, 1)));
            Menu.SubMenu("Farm").AddItem(new MenuItem("UseEFarm", "Use E").SetValue(new StringList(new[] { "Freeze", "LaneClear", "Both", "No" }, 3)));

            //JungleFarm menu:
            Menu.AddSubMenu(new Menu("JungleFarm", "JungleFarm"));
            Menu.SubMenu("JungleFarm").AddItem(new MenuItem("UseQJFarm", "Use Q").SetValue(true));
            Menu.SubMenu("JungleFarm").AddItem(new MenuItem("UseWJFarm", "Use W").SetValue(true));
            Menu.SubMenu("JungleFarm").AddItem(new MenuItem("UseEJFarm", "Use E").SetValue(true));

            //Auto KS
            Menu.AddSubMenu(new Menu("Auto KS", "AutoKS"));
            Menu.SubMenu("AutoKS").AddItem(new MenuItem("UseQKS", "Use Q").SetValue(true));
            Menu.SubMenu("AutoKS").AddItem(new MenuItem("UseWKS", "Use W").SetValue(true));
            Menu.SubMenu("AutoKS").AddItem(new MenuItem("UseEKS", "Use E").SetValue(true));
            Menu.SubMenu("AutoKS").AddItem(new MenuItem("UseQEKS", "Use QE").SetValue(true));
            Menu.SubMenu("AutoKS").AddItem(new MenuItem("UseRKS", "Use R").SetValue(true));
            Menu.SubMenu("AutoKS").AddItem(new MenuItem("AutoKST", "AutoKS (toggle)!").SetValue(new KeyBind("U".ToCharArray()[0], KeyBindType.Toggle,true)));
            
            //Auto Flash Kill
            Menu.SubMenu("AutoKS").AddItem(new MenuItem("UseFK1", "Q+E Flash Kill").SetValue(true));
            Menu.SubMenu("AutoKS").AddItem(new MenuItem("UseFK2", "DFG+R Flash Kill").SetValue(true));
            Menu.SubMenu("AutoKS").AddSubMenu(new Menu("Use Flash Kill on", "FKT"));
            foreach (var enemy in ObjectManager.Get<Obj_AI_Hero>().Where(enemy => enemy.Team != Player.Team))
                Menu.SubMenu("AutoKS").SubMenu("FKT").AddItem(new MenuItem("FKT" + enemy.BaseSkinName, enemy.BaseSkinName).SetValue(true));
            Menu.SubMenu("AutoKS").AddItem(new MenuItem("MaxE", "Max Enemies").SetValue(new Slider(2, 1, 5)));
            Menu.SubMenu("AutoKS").AddItem(new MenuItem("FKMANA", "Only Flash if mana > FC").SetValue(false));
            
            //Misc
            Menu.AddSubMenu(new Menu("Misc", "Misc"));
            Menu.SubMenu("Misc").AddItem(new MenuItem("AntiGap", "Anti Gap Closer").SetValue(true));
            Menu.SubMenu("Misc").AddItem(new MenuItem("Interrupt", "Auto Interrupt Spells").SetValue(true));
            Menu.SubMenu("Misc").AddItem(new MenuItem("Packets", "Packet Casting").SetValue(false));
            Menu.SubMenu("Misc").AddItem(new MenuItem("IgniteALLCD", "Only ignite if all skills on CD").SetValue(false));
            if (Menu.Item("Orbwalker_Mode").GetValue<bool>()) Menu.SubMenu("Misc").AddItem(new MenuItem("OrbWAA", "AA while orbwalking").SetValue(true));
            Menu.SubMenu("Misc").AddItem(new MenuItem("Sound1", "Startup Sound").SetValue(true));
            Menu.SubMenu("Misc").AddItem(new MenuItem("Sound2", "In Game Sound").SetValue(true));
            Menu.SubMenu("Misc").AddItem(new MenuItem("YasuoWall", "Don't try to use skillshots on Yasuo's Wall").SetValue(true));
            //QE Settings
            Menu.AddSubMenu(new Menu("QE Settings", "QEsettings"));
            Menu.SubMenu("QEsettings").AddItem(new MenuItem("QEDelay", "QE Delay").SetValue(new Slider(0, 0, 150)));
            Menu.SubMenu("QEsettings").AddItem(new MenuItem("QEMR", "QE Max Range %").SetValue(new Slider(100, 0, 100)));
            Menu.SubMenu("QEsettings").AddItem(new MenuItem("UseQEC", "QE to Enemy Near Cursor").SetValue(new KeyBind("T".ToCharArray()[0], KeyBindType.Press)));

            //R
            Menu.AddSubMenu(new Menu("R Settings", "Rsettings"));
            Menu.SubMenu("Rsettings").AddSubMenu(new Menu("Dont R if it can be killed with", "DontRw"));
            Menu.SubMenu("Rsettings").SubMenu("DontRw").AddItem(new MenuItem("DontRwParam", "Damage From").SetValue(new StringList(new[] { "All", "Either one", "None" }, 0)));
            Menu.SubMenu("Rsettings").SubMenu("DontRw").AddItem(new MenuItem("DontRwQ", "Q").SetValue(true));
            Menu.SubMenu("Rsettings").SubMenu("DontRw").AddItem(new MenuItem("DontRwW", "W").SetValue(true));
            Menu.SubMenu("Rsettings").SubMenu("DontRw").AddItem(new MenuItem("DontRwE", "E").SetValue(true));
            Menu.SubMenu("Rsettings").SubMenu("DontRw").AddItem(new MenuItem("DontRwA", "1 x AA").SetValue(true));

            Menu.SubMenu("Rsettings").AddSubMenu(new Menu("Dont use R on", "DontR"));
            foreach (var enemy in ObjectManager.Get<Obj_AI_Hero>().Where(enemy => enemy.Team != Player.Team))
                Menu.SubMenu("Rsettings").SubMenu("DontR").AddItem(new MenuItem("DontR" + enemy.BaseSkinName, enemy.BaseSkinName).SetValue(false));
            Menu.SubMenu("Rsettings").AddSubMenu(new Menu("OverKill target by xx%", "okR"));
            foreach (var enemy in ObjectManager.Get<Obj_AI_Hero>().Where(enemy => enemy.Team != Player.Team))
                Menu.SubMenu("Rsettings").SubMenu("okR").AddItem(new MenuItem("okR" + enemy.BaseSkinName, enemy.BaseSkinName).SetValue(new Slider(0, 0, 100)));

            //Drawings
            Menu.AddSubMenu(new Menu("Drawings", "Drawing"));
            Menu.SubMenu("Drawing").AddItem(new MenuItem("DrawQ", "Q Range").SetValue(new Circle(true, Color.FromArgb(100, 255, 0, 255))));
            Menu.SubMenu("Drawing").AddItem(new MenuItem("DrawW", "W Range").SetValue(new Circle(false, Color.FromArgb(100, 255, 0, 255))));
            Menu.SubMenu("Drawing").AddItem(new MenuItem("DrawE", "E Range").SetValue(new Circle(false, Color.FromArgb(100, 255, 0, 255))));
            Menu.SubMenu("Drawing").AddItem(new MenuItem("DrawR", "R Range").SetValue(new Circle(false, Color.FromArgb(100, 255, 0, 255))));
            Menu.SubMenu("Drawing").AddItem(new MenuItem("DrawQE", "QE Range").SetValue(new Circle(true, Color.FromArgb(100, 255, 0, 255))));
            Menu.SubMenu("Drawing").AddItem(new MenuItem("DrawQEC", "QE Cursor indicator").SetValue(new Circle(false, Color.FromArgb(100, 255, 0, 255))));
            Menu.SubMenu("Drawing").AddItem(new MenuItem("DrawQEMAP", "QE Target Parameters").SetValue(true));
            Menu.SubMenu("Drawing").AddItem(new MenuItem("DrawWMAP", "W Target Parameters").SetValue(true));
            Menu.SubMenu("Drawing").AddItem(new MenuItem("Gank", "Gankable Enemy Indicator").SetValue(true));
            Menu.SubMenu("Drawing").AddItem(new MenuItem("DrawHPFill", "After Combo HP Fill").SetValue(true));
            Menu.SubMenu("Drawing").AddItem(new MenuItem("HUD", "Heads-up Display").SetValue(true));
            Menu.SubMenu("Drawing").AddItem(new MenuItem("KillText", "Kill Text").SetValue(true));
            Menu.SubMenu("Drawing").AddItem(new MenuItem("KillTextHP", "% HP After Combo Text").SetValue(true));

            //Add main menu
            Menu.AddToMainMenu();
            if (Menu.Item("Sound1").GetValue<bool>()) playSound(welcome);
            GameObject.OnCreate += OnCreate;
            GameObject.OnDelete += OnDelete;
            Game.OnGameUpdate += Game_OnGameUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
            Obj_AI_Base.OnProcessSpellCast += Obj_AI_Base_OnProcessSpellCast;
            AntiGapcloser.OnEnemyGapcloser += AntiGapcloser_OnEnemyGapcloser;
            Interrupter.OnPossibleToInterrupt += Interrupter_OnPossibleToInterrupt;
            if (Menu.Item("Orbwalker_Mode").GetValue<bool>()) Orbwalking.BeforeAttack += Orbwalking_BeforeAttack;

            Game.PrintChat("<font color = \"#FF0020\">Gagong Xerath</font> by <font color = \"#22FF10\">stephenjason89</font>");
            Game.PrintChat("<font color = \"#87CEEB\">PLEASE DONATE IF YOU LIKE MY WORK. </font>");
            Game.PrintChat("<font color = \"#87CEEB\">DONATE LINKS CAN BE FOUND ON MY SIGNATURE, </font>");
            Game.PrintChat("<font color = \"#87CEEB\">OR BELOW THE INSTALL BUTTON OF THE ASSEMBLY ON MY PAGE </font>");
        }

        private static void chooseOrbwalker(bool mode)
        {
            if (mode)
            {
                Orbwalker = new Orbwalking.Orbwalker(orbwalkerMenu);
                comboKey = Menu.Item("Orbwalk");
                harassKey = Menu.Item("Farm");
                laneclearKey = Menu.Item("LaneClear");
                lanefreezeKey = Menu.Item("LaneClear");
                Game.PrintChat("Regular Orbwalker Loaded");
            }
            else
            {
                xSLxOrbwalker.AddToMenu(orbwalkerMenu);
                comboKey = Menu.Item("Combo_Key");
                harassKey = Menu.Item("Harass_Key");
                laneclearKey = Menu.Item("LaneClear_Key");
                lanefreezeKey = Menu.Item("LaneFreeze_Key");
                Game.PrintChat("xSLx Orbwalker Loaded");
            }
        }
        private static void OnCreate(GameObject obj, EventArgs args)
        {
            //if(Player.Distance(obj.Position) < 300)
            //Game.PrintChat("OBJ: " + obj.Name);
            if (Player.Distance(obj.Position) < 1500)
            {
                //Q
                if (obj != null && obj.IsValid &&
                    System.Text.RegularExpressions.Regex.IsMatch(
                        obj.Name, "_w_windwall.\\.troy",
                        System.Text.RegularExpressions.RegexOptions.IgnoreCase))
                {
                    YasuoWall = obj;
                }
              
            }
        }
        private static void OnDelete(GameObject obj, EventArgs args)
        {
            //if(Player.Distance(obj.Position) < 300)
            //Game.PrintChat("OBJ: " + obj.Name);
            if (Player.Distance(obj.Position) < 1500)
            {
                //Q
                if (obj != null && obj.IsValid && System.Text.RegularExpressions.Regex.IsMatch(
                        obj.Name, "_w_windwall.\\.troy",
                        System.Text.RegularExpressions.RegexOptions.IgnoreCase))
                {
                    YasuoWall = null;
                }

            }
        }
        private static bool detectCollision(Obj_AI_Hero target = null)
        {

            if (YasuoWall == null || !Menu.Item("YasuoWall").GetValue<bool>())
            {
                Game.PrintChat("True");
                return true;
            }
            else
            {
                Game.PrintChat("else");
                var level = YasuoWall.Name.Substring(YasuoWall.Name.Length - 6, 1);
                var wallWidth = (300 + 50 * Convert.ToInt32(level));
                var wallDirection = (YasuoWall.Position.To2D() - YasuoWallCastedPos).Normalized().Perpendicular();
                var wallStart = YasuoWall.Position.To2D() + wallWidth / 2 * wallDirection;
                var wallEnd = wallStart - wallWidth * wallDirection;

                var intersection = Geometry.Intersection(wallStart, wallEnd, Player.Position.To2D(), target.Position.To2D());
                var intersections = new List<Vector2>();

                if (intersection.Point.IsValid() && Environment.TickCount + Game.Ping + R.Delay - WallCastT < 4000)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
        }
        private static void playSound(SoundPlayer sound = null)
        {
            if (sound != null)
            {
                try
                {
                    sound.Play();
                }
                catch { }
            }
            else if (Environment.TickCount - LastPlayedSound > 45000 && Menu.Item("Sound2").GetValue<bool>()) 
            {
                Random rnd = new Random();
                switch (rnd.Next(1, 11))
                {
                    case 1:
                        playSound(imkillingthebitch);
                        break;
                    case 2:
                        playSound(ballstotheface);
                        break;
                    case 3:
                        playSound(diefucker);
                        break;
                    case 4:
                        playSound(goingsomewhereasshole);
                        break;
                    case 5:
                        playSound(ilovethisgame);
                        break;
                    case 6:
                        playSound(imkillingthebitch);
                        break;
                    case 7:
                        playSound(ohdontyoudare);
                        break;
                    case 8:
                        playSound(ohidiot);
                        break;
                    case 9:
                        playSound(whosthebitchnow);
                        break;
                    case 10:
                        playSound(yourdeadmeatasshole);
                        break;
                }
                LastPlayedSound = Environment.TickCount;     
                
            }
        }
        static void Game_OnGameUpdate(EventArgs args)
        {
            if (Player.IsDead) return;
            
            //Combo
            if (comboKey.GetValue<KeyBind>().Active)
            {
                detectCollision();
            }
            
            //Harass
            else if (harassKey.GetValue<KeyBind>().Active || Menu.Item("HarassActiveT").GetValue<KeyBind>().Active)
            {
                if (Menu.Item("HarassTurret").GetValue<bool>())
                {
                    var turret = ObjectManager.Get<Obj_AI_Turret>().FirstOrDefault(t => t.IsValidTarget(Q.Range));
                    if (turret == null) Harass();
                }
                else Harass();
            }
            //Auto KS
            if (Menu.Item("AutoKST").GetValue<KeyBind>().Active)
            {
                AutoKS();
            }
            //Farm
            if (!comboKey.GetValue<KeyBind>().Active) { 
                var lc = laneclearKey.GetValue<KeyBind>().Active;
                if (lc || lanefreezeKey.GetValue<KeyBind>().Active)
                    Farm(lc);
                if (laneclearKey.GetValue<KeyBind>().Active)
                    JungleFarm();
            }
        }

        private static void Combo()
        {
            UseSpells(Menu.Item("UseQ").GetValue<bool>(), //Q
                      Menu.Item("UseW").GetValue<bool>(), //W
                      Menu.Item("UseE").GetValue<bool>(), //E
                      Menu.Item("UseR").GetValue<bool>(), //R
                      Menu.Item("UseQE").GetValue<bool>() //QE
                      );
        }

        private static void Harass()
        {
            if (Player.Mana / Player.MaxMana * 100 < Menu.Item("HarassMana").GetValue<Slider>().Value) return;
            UseSpells(Menu.Item("UseQH").GetValue<bool>(), //Q
                      Menu.Item("UseWH").GetValue<bool>(), //W
                      Menu.Item("UseEH").GetValue<bool>(), //E
                      false,                               //R
                      Menu.Item("UseQEH").GetValue<bool>() //QE 
                      );
        }
        private static void Farm(bool laneClear)
        {
            if (!Orbwalking.CanMove(40)) return;
            var rangedMinionsQ = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, Q.Range + Q.Width + 30,
            MinionTypes.Ranged);
            var allMinionsQ = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, Q.Range + Q.Width + 30,
            MinionTypes.All);
            var rangedMinionsW = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, W.Range + W.Width + 30,
            MinionTypes.Ranged);
            var allMinionsW = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, W.Range + W.Width + 30,
            MinionTypes.All);
            var useQi = Menu.Item("UseQFarm").GetValue<StringList>().SelectedIndex;
            var useWi = Menu.Item("UseWFarm").GetValue<StringList>().SelectedIndex;
            var useEi = Menu.Item("UseEFarm").GetValue<StringList>().SelectedIndex;
            var useQ = (laneClear && (useQi == 1 || useQi == 2)) || (!laneClear && (useQi == 0 || useQi == 2));
            var useW = (laneClear && (useWi == 1 || useWi == 2)) || (!laneClear && (useWi == 0 || useWi == 2));
            var useE = (laneClear && (useEi == 1 || useEi == 2)) || (!laneClear && (useEi == 0 || useEi == 2));
            if (useQ && Q.IsReady())
                if (laneClear)
                {
                    var fl1 = Q.GetCircularFarmLocation(rangedMinionsQ, Q.Width);
                    var fl2 = Q.GetCircularFarmLocation(allMinionsQ, Q.Width);
                    if (fl1.MinionsHit >= 3)
                    {
                        Q.Cast(fl1.Position, Menu.Item("Packets").GetValue<bool>());
                    }
                    else if (fl2.MinionsHit >= 2 || allMinionsQ.Count == 1)
                    {
                        Q.Cast(fl2.Position, Menu.Item("Packets").GetValue<bool>());
                    }
                }
                else
                    foreach (var minion in allMinionsQ)
                        if (!Orbwalking.InAutoAttackRange(minion) &&
                        minion.Health < 0.75 * Player.GetSpellDamage(minion, SpellSlot.Q))
                            Q.Cast(minion, Menu.Item("Packets").GetValue<bool>());
            if (useW && W.IsReady() && allMinionsW.Count > 3)
            {
                if (laneClear)
                {
                    if (Player.Spellbook.GetSpell(SpellSlot.W).ToggleState == 1)
                    {
                        //WObject
                        var gObjectPos = GetGrabableObjectPos(false);
                        if (gObjectPos.To2D().IsValid() && Environment.TickCount - W.LastCastAttemptT > Game.Ping + 150)
                        {
                            W.Cast(gObjectPos, Menu.Item("Packets").GetValue<bool>());
                        }
                    }
                    else if (Player.Spellbook.GetSpell(SpellSlot.W).ToggleState != 1)
                    {
                        var fl1 = Q.GetCircularFarmLocation(rangedMinionsW, W.Width);
                        var fl2 = Q.GetCircularFarmLocation(allMinionsW, W.Width);
                        if (fl1.MinionsHit >= 3 && W.InRange(fl1.Position.To3D()))
                        {
                            W.Cast(fl1.Position, Menu.Item("Packets").GetValue<bool>());
                        }
                        else if (fl2.MinionsHit >= 1 && W.InRange(fl2.Position.To3D()) && fl1.MinionsHit <= 2)
                        {
                            W.Cast(fl2.Position, Menu.Item("Packets").GetValue<bool>());
                        }
                    }
                }
            }
        }
        private static void JungleFarm()
        {
            var useQ = Menu.Item("UseQJFarm").GetValue<bool>();
            var useW = Menu.Item("UseWJFarm").GetValue<bool>();
            var useE = Menu.Item("UseEJFarm").GetValue<bool>();
            var mobs = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, W.Range, MinionTypes.All,
            MinionTeam.Neutral, MinionOrderTypes.MaxHealth);
            if (mobs.Count > 0)
            {
                var mob = mobs[0];
                if (Q.IsReady() && useQ)
                {
                    Q.Cast(mob, Menu.Item("Packets").GetValue<bool>());
                }
                if (W.IsReady() && useW && Environment.TickCount - Q.LastCastAttemptT > 800)
                {
                    W.Cast(mob, Menu.Item("Packets").GetValue<bool>());
                }
                if (useE && E.IsReady())
                {
                    E.Cast(mob, Menu.Item("Packets").GetValue<bool>());
                }
            }
        }

        private static void Obj_AI_Base_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {   
            //Last cast time of spells
            if (sender.IsMe)
            {
                if (args.SData.Name.ToString() == "SyndraQ")
                    Q.LastCastAttemptT = Environment.TickCount;
                if (args.SData.Name.ToString() == "SyndraW" || args.SData.Name.ToString() == "syndrawcast")
                    W.LastCastAttemptT = Environment.TickCount;
                if (args.SData.Name.ToString() == "SyndraE" || args.SData.Name.ToString() == "syndrae5")
                    E.LastCastAttemptT = Environment.TickCount;
            }
            
            //Harass when enemy do attack
            if (Menu.Item("HarassAAQ").GetValue<bool>() && sender.Type == Player.Type && sender.Team != Player.Team && args.SData.Name.ToLower().Contains("attack") && Player.Distance(sender, true) <= Math.Pow(Q.Range, 2) && Player.Mana / Player.MaxMana * 100 > Menu.Item("HarassMana").GetValue<Slider>().Value)  
            {
                UseQ((Obj_AI_Hero)sender);
            }
            if (sender.IsValid && sender.Team == ObjectManager.Player.Team && args.SData.Name == "YasuoWMovingWall")
            {
                WallCastT = Environment.TickCount;
                YasuoWallCastedPos = sender.ServerPosition.To2D();
            }
        }
        
        //Anti gapcloser
        private static void AntiGapcloser_OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (!Menu.Item("AntiGap").GetValue<bool>()) return;

            if (E.IsReady() && Player.Distance(gapcloser.Sender, true) <= Math.Pow(QE.Range, 2) && gapcloser.Sender.IsValidTarget(QE.Range))
            {
                if (Q.IsReady() && Player.Spellbook.GetSpell(SpellSlot.Q).ManaCost + Player.Spellbook.GetSpell(SpellSlot.E).ManaCost <= Player.Mana)
                {
                    UseQE((Obj_AI_Hero)gapcloser.Sender);
                }
                else if (Player.Distance(gapcloser.Sender, true) <= Math.Pow(E.Range, 2))
                    E.Cast(gapcloser.End, Menu.Item("Packets").GetValue<bool>());
            }
        }

        //Interrupt dangerous spells
        private static void Interrupter_OnPossibleToInterrupt(Obj_AI_Base unit, InterruptableSpell spell)
        {
            if (!Menu.Item("Interrupt").GetValue<bool>()) return;

            if (E.IsReady() && Player.Distance(unit, true) <= Math.Pow(E.Range, 2) && unit.IsValidTarget(E.Range))
            {
                if (Q.IsReady())
                    UseQE((Obj_AI_Hero)unit);
                else
                    E.Cast(unit, Menu.Item("Packets").GetValue<bool>());
            }
            else if (Q.IsReady() && E.IsReady() && Player.Distance(unit, true) <= Math.Pow(QE.Range, 2))
                UseQE((Obj_AI_Hero)unit);
        }

        static void Orbwalking_BeforeAttack(Orbwalking.BeforeAttackEventArgs args)
        {
            bool orbwalkAA = false;
            if(Menu.Item("OrbWAA").GetValue<bool>()) orbwalkAA = !Q.IsReady() && (!W.IsReady() || !E.IsReady());
            if (comboKey.GetValue<KeyBind>().Active)
                args.Process = orbwalkAA;
        }

        private static float GetComboDamage(Obj_AI_Hero enemy, bool UQ, bool UW, bool UE, bool UR, bool UDFG = true)
        {
            var damage = 0d;
            var combomana = 0d;
            bool useR = Menu.Item("DontR" + enemy.BaseSkinName) != null && Menu.Item("DontR" + enemy.BaseSkinName).GetValue<bool>() == false;
            
            //Add R Damage
            if (R.IsReady() && UR && useR)
            {
                combomana += Player.Spellbook.GetSpell(SpellSlot.R).ManaCost;
                if (combomana <= Player.Mana) damage += GetRDamage(enemy);
                else combomana -= Player.Spellbook.GetSpell(SpellSlot.R).ManaCost;
            }

            //Add Q Damage
            if (Q.IsReady() && UQ)
            {
                combomana += Player.Spellbook.GetSpell(SpellSlot.Q).ManaCost;
                if (combomana <= Player.Mana) damage += Player.GetSpellDamage(enemy, SpellSlot.Q);
                else combomana -= Player.Spellbook.GetSpell(SpellSlot.Q).ManaCost;
            }

            //Add E Damage
            if (E.IsReady() && UE)
            {
                combomana += Player.Spellbook.GetSpell(SpellSlot.E).ManaCost;
                if (combomana <= Player.Mana) damage += Player.GetSpellDamage(enemy, SpellSlot.E);
                else combomana -= Player.Spellbook.GetSpell(SpellSlot.E).ManaCost;
            }

            //Add W Damage
            if (W.IsReady() && UW)
            {
                combomana += Player.Spellbook.GetSpell(SpellSlot.W).ManaCost;
                if (combomana <= Player.Mana) damage += Player.GetSpellDamage(enemy, SpellSlot.W);
                else combomana -= Player.Spellbook.GetSpell(SpellSlot.W).ManaCost;
            }
            

            //Add damage DFG
            if (UDFG && DFG.IsReady()) damage += Player.GetItemDamage(enemy, Damage.DamageItems.Dfg) / 1.2;

            //DFG multiplier
            return (float)((DFG.IsReady() && UDFG || DFGBuff(enemy)) ? damage * 1.2 : damage);
        }

        private static float GetRDamage(Obj_AI_Hero enemy)
        {
            if (!R.IsReady()) return 0f;
            float damage = 45 + R.Level * 45 + Player.FlatMagicDamageMod * 0.2f; 
            return (float)Player.CalcDamage(enemy, Damage.DamageType.Magical, damage) * Player.Spellbook.GetSpell(SpellSlot.R).Ammo;
        }

        private static float GetIgniteDamage(Obj_AI_Hero enemy)
        {
            if (IgniteSlot == SpellSlot.Unknown || Player.SummonerSpellbook.CanUseSpell(IgniteSlot) != SpellState.Ready) return 0f;
            return (float)Player.GetSummonerSpellDamage(enemy, Damage.SummonerSpell.Ignite);
        }

        private static bool DFGBuff(Obj_AI_Hero enemy)
        {
            return (enemy.HasBuff("deathfiregraspspell", true) || enemy.HasBuff("itemblackfiretorchspell", true)) ? true : false;
        }
       
        //Check R Only If QEW on CD
        private static bool RCheck(Obj_AI_Hero enemy)
        {
            double AA = 0;
            if(Menu.Item("DontRwA").GetValue<bool>()) AA = Player.GetAutoAttackDamage(enemy);
            //Menu check
            if (Menu.Item("DontRwParam").GetValue<StringList>().SelectedIndex==2) return true;

            //If can be killed by all the skills that are checked
            else if (Menu.Item("DontRwParam").GetValue<StringList>().SelectedIndex == 0 && GetComboDamage(enemy, Menu.Item("DontRwQ").GetValue<bool>(), Menu.Item("DontRwW").GetValue<bool>(), Menu.Item("DontRwE").GetValue<bool>(), false, false) + AA >= enemy.Health) return false;
            //If can be killed by either any of the skills
            else if (Menu.Item("DontRwParam").GetValue<StringList>().SelectedIndex == 1 && (GetComboDamage(enemy, Menu.Item("DontRwQ").GetValue<bool>(),false,false,false,false) >= enemy.Health || GetComboDamage(enemy, Menu.Item("DontRwW").GetValue<bool>(),false,false,false,false) >= enemy.Health || GetComboDamage(enemy, Menu.Item("DontRwE").GetValue<bool>(),false,false,false,false) >= enemy.Health || AA>=enemy.Health)) return false;
            
            //Check last cast times
            else if (Environment.TickCount - Q.LastCastAttemptT > 600 + Game.Ping && Environment.TickCount - E.LastCastAttemptT > 600 + Game.Ping && Environment.TickCount - W.LastCastAttemptT > 600 + Game.Ping) return true;

            else return false;
        }

        private static void AutoKS()
        {
            foreach (var enemy in ObjectManager.Get<Obj_AI_Hero>().Where(enemy => enemy.Team != Player.Team))
                if (!enemy.HasBuff("UndyingRage") && !enemy.HasBuff("JudicatorIntervention") && enemy.IsValidTarget(QE.Range) && Environment.TickCount - FlashLastCast > 650 + Game.Ping)
                {
                    if (GetComboDamage(enemy, false, false, Menu.Item("UseQEKS").GetValue<bool>(), false, false) > enemy.Health && Player.Distance(enemy, true) <= Math.Pow(QE.Range, 2))
                    {
                        UseSpells(false, //Q
                        false, //W
                        false, //E
                        false, //R
                        Menu.Item("UseQEKS").GetValue<bool>() //QE
                        );
                        playSound();
                        //Game.PrintChat("QEKS " + enemy.Name);
                    }
                    else if (GetComboDamage(enemy, false, Menu.Item("UseWKS").GetValue<bool>(), false, false, false) > enemy.Health && Player.Distance(enemy, true) <= Math.Pow(W.Range, 2))
                    {
                        UseSpells(false, //Q
                        Menu.Item("UseWKS").GetValue<bool>(), //W
                        false, //E
                        false, //R
                        false //QE
                        );
                        playSound();
                        //Game.PrintChat("WKS " + enemy.Name);
                    }
                    else if (GetComboDamage(enemy, Menu.Item("UseQKS").GetValue<bool>(), false, Menu.Item("UseEKS").GetValue<bool>(), false, false) > enemy.Health && Player.Distance(enemy, true) <= Math.Pow(Q.Range + 25f, 2))
                    {
                        UseSpells(Menu.Item("UseQKS").GetValue<bool>(), //Q
                        false, //W
                        Menu.Item("UseEKS").GetValue<bool>(), //E
                        false, //R
                        false //QE
                        );
                        playSound();
                        //Game.PrintChat("QEKSC " + enemy.Name);
                    }
                    else if (GetComboDamage(enemy, Menu.Item("UseQKS").GetValue<bool>(), Menu.Item("UseWKS").GetValue<bool>(), Menu.Item("UseEKS").GetValue<bool>(), Menu.Item("UseRKS").GetValue<bool>()) > enemy.Health && Player.Distance(enemy, true) <= Math.Pow(R.Range, 2))
                    {
                        UseSpells(Menu.Item("UseQKS").GetValue<bool>(), //Q
                        Menu.Item("UseWKS").GetValue<bool>(), //W
                        Menu.Item("UseEKS").GetValue<bool>(), //E
                        Menu.Item("UseRKS").GetValue<bool>(), //R
                        Menu.Item("UseQEKS").GetValue<bool>() //QE
                        );
                        playSound();
                        //Game.PrintChat("QWERKS " + enemy.Name);
                    }
                    else if ((GetComboDamage(enemy, false, false, Menu.Item("UseEKS").GetValue<bool>(), Menu.Item("UseRKS").GetValue<bool>(), false) > enemy.Health || GetComboDamage(enemy, false, Menu.Item("UseWKS").GetValue<bool>(), Menu.Item("UseEKS").GetValue<bool>(), false, false) > enemy.Health) && Player.Distance(enemy, true) <= Math.Pow(QE.Range, 2))
                    {
                        UseSpells(false, //Q
                        false, //W
                        false, //E
                        false, //R
                        Menu.Item("UseQEKS").GetValue<bool>() //QE
                        );
                        playSound();
                        //Game.PrintChat("QEKS " + enemy.Name);
                    }
                    //Flash Kill
                    bool UseFlash = Menu.Item("FKT" + enemy.BaseSkinName) != null && Menu.Item("FKT" + enemy.BaseSkinName).GetValue<bool>() == true;
                    bool useR = Menu.Item("DontR" + enemy.BaseSkinName) != null && Menu.Item("DontR" + enemy.BaseSkinName).GetValue<bool>() == false;
                    bool Rflash = GetComboDamage(enemy, Menu.Item("UseQKS").GetValue<bool>(), false, Menu.Item("UseEKS").GetValue<bool>(), false, false) < enemy.Health;
                    PredictionOutput ePos = R.GetPrediction(enemy);
                    if ((FlashSlot != SpellSlot.Unknown || Player.SummonerSpellbook.CanUseSpell(FlashSlot) == SpellState.Ready) && UseFlash
                        && Player.Distance(ePos.UnitPosition, true) <= Math.Pow(Q.Range + 25f + 395, 2) && Player.Distance(ePos.UnitPosition, true) > Math.Pow(Q.Range + 25f + 200, 2))

                    if ((GetComboDamage(enemy, Menu.Item("UseQKS").GetValue<bool>(), false, Menu.Item("UseEKS").GetValue<bool>(), false, false) > enemy.Health && Menu.Item("UseFK1").GetValue<bool>())
                        || (GetComboDamage(enemy, false, false, false, Menu.Item("UseRKS").GetValue<bool>()) > enemy.Health && Menu.Item("UseFK2").GetValue<bool>() && Player.Distance(ePos.UnitPosition, true) <= Math.Pow(R.Range + 390, 2) && Environment.TickCount-R.LastCastAttemptT>Game.Ping + 150 && Player.Distance(ePos.UnitPosition, true) > Math.Pow(R.Range + 200, 2)))
                    {
                        var totmana = 0d;
                        if (Menu.Item("FKMANA").GetValue<bool>())
                        {
                            foreach (var spell in SpellList)
                                { // Total Combo Mana
                                    totmana += Player.Spellbook.GetSpell(spell.Slot).ManaCost;
                                }
                        }
                        if (!(totmana > Player.Mana && Menu.Item("FKMANA").GetValue<bool>()) || !Menu.Item("FKMANA").GetValue<bool>())
                        {
                            var NearbyE = ePos.UnitPosition.CountEnemysInRange(1000);
                            if (NearbyE <= Menu.Item("MaxE").GetValue<Slider>().Value)
                            {
                                Vector3 FlashPos = Player.ServerPosition - Vector3.Normalize(Player.ServerPosition - ePos.UnitPosition) * 400;
                                if (!Utility.IsWall(FlashPos)) { 
                                    if (Rflash)
                                    { 
                                        if (useR)
                                        {   //Use Ult after flash if can't be killed by QE
                                            Player.SummonerSpellbook.CastSpell(FlashSlot, FlashPos);
                                            UseSpells(false, //Q
                                            false, //W
                                            false, //E
                                            Menu.Item("UseRKS").GetValue<bool>(), //R
                                            false //QE
                                            );
                                            playSound();
                                        }
                                    }
                                    else
                                    {   //Q & E after flash
                                        Player.SummonerSpellbook.CastSpell(FlashSlot, FlashPos);
                                    }
                                FlashLastCast = Environment.TickCount;
                                }
                            }
                        }
                    }

                }
        }

        private static void UseSpells(bool UQ, bool UW, bool UE, bool UR, bool UQE)
        {   
            //Set Target
            var QTarget = SimpleTs.GetTarget(Q.Range + 25f, SimpleTs.DamageType.Magical);
            var WTarget = SimpleTs.GetTarget(W.Range + W.Width, SimpleTs.DamageType.Magical);
            var RTarget = SimpleTs.GetTarget(R.Range, SimpleTs.DamageType.Magical);
            var QETarget = SimpleTs.GetTarget(QE.Range, SimpleTs.DamageType.Magical);
            bool UseR = false;
            detectCollision(QETarget);
            //Use DFG
            if (DFG.IsReady() && RTarget != null && GetComboDamage(RTarget, UQ, UW, UE, UR) + GetIgniteDamage(RTarget) > RTarget.Health)
            {
                //DFG
                if (Player.Distance(RTarget, true) <= Math.Pow(DFG.Range, 2) && GetComboDamage(RTarget, UQ, UW, UE, false, false) + GetIgniteDamage(QTarget) < RTarget.Health)
                    if((UR && R.IsReady()) || (UQ && Q.IsReady())) DFG.Cast(RTarget);
            }
            
            //Harass Combo Key Override
            if (RTarget != null && (harassKey.GetValue<KeyBind>().Active || laneclearKey.GetValue<KeyBind>().Active) && comboKey.GetValue<KeyBind>().Active && Player.Distance(RTarget, true) <= Math.Pow(R.Range, 2) && !RTarget.HasBuff("UndyingRage") && !RTarget.HasBuff("JudicatorIntervention"))
            {
                    DFG.Cast(QTarget);
                    if (Menu.Item("DontR" + RTarget.BaseSkinName) != null && Menu.Item("DontR" + RTarget.BaseSkinName).GetValue<bool>() == false && UR)
                    {
                        detectCollision(RTarget);
                        R.CastOnUnit(RTarget, Menu.Item("Packets").GetValue<bool>());
                        R.LastCastAttemptT = Environment.TickCount;
                    }
            }

            //R, Ignite 
            foreach (var enemy in ObjectManager.Get<Obj_AI_Hero>().Where(enemy => enemy.Team != Player.Team && enemy.Distance(Player) <= R.Range && !enemy.IsDead))
                if (!enemy.HasBuff("UndyingRage") && !enemy.HasBuff("JudicatorIntervention"))
                {
                    //R
                    UseR = Menu.Item("DontR" + enemy.BaseSkinName).GetValue<bool>() == false && UR;
                    var okR = Menu.Item("okR" + enemy.BaseSkinName).GetValue<Slider>().Value * .01 + 1;
                    if (UseR && Player.Distance(enemy, true) <= Math.Pow(R.Range, 2) && (DFGBuff(enemy) ? GetRDamage(enemy) * 1.2 : GetRDamage(enemy)) > enemy.Health * okR && RCheck(enemy))
                    {
                        if (!(Player.GetSpellDamage(enemy, SpellSlot.Q) > enemy.Health && Player.Spellbook.GetSpell(SpellSlot.Q).CooldownExpires - Game.Time < 2 && Player.Spellbook.GetSpell(SpellSlot.Q).CooldownExpires - Game.Time >= 0 && enemy.IsStunned) && Environment.TickCount - Q.LastCastAttemptT > 500 + Game.Ping)
                        {
                            detectCollision(enemy);
                            R.CastOnUnit(enemy, Menu.Item("Packets").GetValue<bool>());
                            R.LastCastAttemptT = Environment.TickCount;
                        }
                            
                    }
                    //Ignite
                    if (Player.Distance(enemy, true) <= 600 * 600 && GetIgniteDamage(enemy) > enemy.Health)
                        if (Menu.Item("IgniteALLCD").GetValue<bool>())
                        {
                            if (!Q.IsReady() && !W.IsReady() && !E.IsReady() && !R.IsReady()) Player.SummonerSpellbook.CastSpell(IgniteSlot, enemy);
                        }
                        else Player.SummonerSpellbook.CastSpell(IgniteSlot, enemy);
                }
           
            //Use QE
            if (UQE && QETarget != null && Q.IsReady() && (E.IsReady() || (Player.Spellbook.GetSpell(SpellSlot.E).CooldownExpires - Game.Time < 1 && Player.Spellbook.GetSpell(SpellSlot.E).Level > 0)) && Player.Spellbook.GetSpell(SpellSlot.Q).ManaCost + Player.Spellbook.GetSpell(SpellSlot.E).ManaCost <= Player.Mana)
            {
                UseQE(QETarget);
            }

            //Use Q
            else if (UQ && QTarget != null)
            {
                UseQ(QTarget);
            }

            //Use E
            if (UE && E.IsReady() && Environment.TickCount - W.LastCastAttemptT > Game.Ping + 150 && Environment.TickCount - QWLastcast > Game.Ping)
                foreach (var enemy in ObjectManager.Get<Obj_AI_Hero>().Where(enemy => enemy.Team != Player.Team))
                {
                    if(enemy.IsValidTarget(E.Range))
                        if (GetComboDamage(enemy, UQ, UW, UE, UR) > enemy.Health && Player.Distance(enemy, true) <= Math.Pow(E.Range, 2))
                            E.Cast(enemy, Menu.Item("Packets").GetValue<bool>());
                        else if (Player.Distance(enemy, true) <= Math.Pow(QE.Range, 2))
                            UseE(enemy);
                }
            //Use W
            if (UW) UseW(QETarget, WTarget);
        }
        private static Vector3 GetGrabableObjectPos(bool onlyOrbs)
        {
            if (!onlyOrbs)
                foreach (var minion in ObjectManager.Get<Obj_AI_Minion>().Where(minion => minion.IsValidTarget(W.Range))
                )
                    return minion.ServerPosition;
            return OrbManager.GetOrbToGrab((int)W.Range);
        }

        private static void UseQ(Obj_AI_Hero Target)
        {
            if (!Q.IsReady()) return;
            PredictionOutput Pos = Q.GetPrediction(Target, true);
            if (Pos.Hitchance >= HitChance.VeryHigh)
                Q.Cast(Pos.CastPosition, Menu.Item("Packets").GetValue<bool>());
        }
        private static void UseW(Obj_AI_Hero QETarget, Obj_AI_Hero WTarget)
        {
            //Use W1
            if (QETarget != null && W.IsReady() && Player.Spellbook.GetSpell(SpellSlot.W).ToggleState == 1)
            {
                Vector3 gObjectPos = GetGrabableObjectPos(false);

                if (gObjectPos.To2D().IsValid() && Environment.TickCount - Q.LastCastAttemptT > Game.Ping && Environment.TickCount - E.LastCastAttemptT > 750 + Game.Ping && Environment.TickCount - W.LastCastAttemptT > 600 + Game.Ping)
                {
                    bool grabsomething = false;
                    if (WTarget != null)
                    {
                        PredictionOutput Pos2 = W.GetPrediction(WTarget, true);
                        if (Pos2.Hitchance >= HitChance.Low) grabsomething = true;
                    }
                    if (grabsomething || QETarget.IsStunned)
                        W.Cast(gObjectPos, Menu.Item("Packets").GetValue<bool>());
                }
            }

            //Use W2
            if (W.IsReady() && Player.Spellbook.GetSpell(SpellSlot.W).ToggleState != 1 && WTarget != null && !(OrbManager.WObject(false).Name.ToLower() == "heimertblue"))
            {
                W.UpdateSourcePosition(OrbManager.WObject(false).ServerPosition);
                PredictionOutput Pos = W.GetPrediction(WTarget, true);
                if (Pos.Hitchance >= HitChance.High)
                    W.Cast(Pos.CastPosition, Menu.Item("Packets").GetValue<bool>());
            }
        }
        private static void UseE(Obj_AI_Hero Target)
        {
            foreach (var orb in OrbManager.GetOrbs(true).Where(orb => orb.To2D().IsValid() && Player.Distance(orb, true) < Math.Pow(E.Range, 2)))
                {
                    Vector2 SP = orb.To2D() + Vector2.Normalize(Player.ServerPosition.To2D() - orb.To2D()) * 100f;
                    Vector2 EP = orb.To2D() + Vector2.Normalize(orb.To2D() - Player.ServerPosition.To2D()) * 592;
                    QE.Delay = E.Delay + Player.Distance(orb) / E.Speed;
                    QE.UpdateSourcePosition(orb);
                    var PPo = QE.GetPrediction(Target).UnitPosition.To2D();
                    if (PPo.Distance(SP, EP, true, true) <= Math.Pow(QE.Width + Target.BoundingRadius, 2))
                        E.Cast(orb, Menu.Item("Packets").GetValue<bool>());                
                }
        }
        
        private static void UseQE(Obj_AI_Hero Target)
        {
            if (!Q.IsReady() || !E.IsReady()) return;
            Vector3 SPos = Prediction.GetPrediction(Target, E.Delay).UnitPosition;
            if (Player.Distance(SPos, true) > Math.Pow(E.Range, 2))
            {
                Vector3 orb = Player.ServerPosition + Vector3.Normalize(SPos - Player.ServerPosition) * E.Range;
                QE.Delay = E.Delay + Player.Distance(orb) / E.Speed;
                var TPos = QE.GetPrediction(Target);
                if (TPos.Hitchance >= HitChance.Medium)
                {
                    UseQE2(Target, orb);
                }
            }
            else
            {
                PredictionOutput Pos = Q.GetPrediction(Target, true);
                if (Pos.Hitchance >= HitChance.VeryHigh)
                    UseQE2(Target, Pos.UnitPosition);
            }
        }
        private static void UseQE2(Obj_AI_Hero Target, Vector3 Pos)
        {
            if (Player.Distance(Pos, true) <= Math.Pow(E.Range, 2))
            {
                Vector3 SP = Pos + Vector3.Normalize(Player.ServerPosition - Pos) * 100f;
                Vector3 EP = Pos + Vector3.Normalize(Pos - Player.ServerPosition) * 592;
                QE.Delay = E.Delay + Player.ServerPosition.Distance(Pos) / E.Speed;
                QE.UpdateSourcePosition(Pos);
                var PPo = QE.GetPrediction(Target).UnitPosition.To2D().ProjectOn(SP.To2D(), EP.To2D());
                if (PPo.IsOnSegment && PPo.SegmentPoint.Distance(Target, true) <= Math.Pow(QE.Width + Target.BoundingRadius, 2))
                {
                    int Delay = 280 - (int)(Player.Distance(Pos) / 2.5) + Menu.Item("QEDelay").GetValue<Slider>().Value;
                    Utility.DelayAction.Add(Math.Max(0, Delay), () => E.Cast(Pos, Menu.Item("Packets").GetValue<bool>()));
                    QE.LastCastAttemptT = Environment.TickCount;
                    Q.Cast(Pos, Menu.Item("Packets").GetValue<bool>());
                    UseE(Target);
                }
            }
        }
        private static void Drawing_OnDraw(EventArgs args)
        {
            var menuItem = Menu.Item("DrawQE").GetValue<Circle>();
            if (menuItem.Active) Utility.DrawCircle(Player.Position, QE.Range, menuItem.Color);
            menuItem = Menu.Item("DrawQEC").GetValue<Circle>();
            foreach (var enemy in ObjectManager.Get<Obj_AI_Hero>().Where(enemy => enemy.Team != Player.Team))
            {
                if (enemy.IsVisible && !enemy.IsDead)
                { //Draw Combo Damage to Enemy HP bars
                    
                    Vector2 hpBarPos = enemy.HPBarPosition;
                    hpBarPos.X += 45;
                    hpBarPos.Y += 18;
                    var KillText = "";
                    var combodamage = GetComboDamage(enemy, Menu.Item("UseQ").GetValue<bool>(), Menu.Item("UseW").GetValue<bool>(), Menu.Item("UseE").GetValue<bool>(), Menu.Item("UseR").GetValue<bool>());
                    var PercentHPleftAfterCombo = (enemy.Health - combodamage) / enemy.MaxHealth;
                    var PercentHPleft = enemy.Health / enemy.MaxHealth;
                    if (PercentHPleftAfterCombo < 0) PercentHPleftAfterCombo = 0;
                    double comboXPos = hpBarPos.X - 36 + (107 * PercentHPleftAfterCombo);
                    double currentHPXPos = hpBarPos.X - 36 + (107 * PercentHPleft);
                    var barcolor = Color.FromArgb(100, 0, 220, 0);
                    var barcolorline = Color.WhiteSmoke;
                    if (combodamage + Player.GetSpellDamage(enemy, SpellSlot.Q) + Player.GetAutoAttackDamage(enemy) * 2 > enemy.Health)
                    {
                        KillText = "Killable by: Full Combo + 1Q + 2AA";
                        if (combodamage >= enemy.Health) KillText = "Killable by: Full Combo";
                        barcolor = Color.FromArgb(100, 255, 255, 0);
                        barcolorline = Color.SpringGreen;
                        var linecolor = barcolor;
                        if (GetComboDamage(enemy, Menu.Item("UseQ").GetValue<bool>(), Menu.Item("UseW").GetValue<bool>(), Menu.Item("UseE").GetValue<bool>(), false) > enemy.Health)
                        {
                            KillText = "Killable by: Q + W + E";
                            barcolor = Color.FromArgb(130, 255, 70, 0);
                            linecolor = Color.FromArgb(150, 255, 0, 0);
                        }
                        if (Menu.Item("Gank").GetValue<bool>() )
                        {
                            Vector3 Pos = Player.Position + Vector3.Normalize(enemy.Position - Player.Position) * 100;
                            Vector2 myPos = Drawing.WorldToScreen(Pos);
                            Pos = Player.Position + Vector3.Normalize(enemy.Position - Player.Position) * 350;
                            Vector2 ePos = Drawing.WorldToScreen(Pos);
                            Drawing.DrawLine(myPos.X, myPos.Y, ePos.X, ePos.Y, 1, linecolor);
                        }
                    }
                    var KillTextPos = Drawing.WorldToScreen(enemy.Position);
                    var HPleftText = Math.Round(PercentHPleftAfterCombo*100) + "%";
                    Drawing.DrawLine((float)comboXPos, hpBarPos.Y, (float)comboXPos, (float)hpBarPos.Y + 5, 1, barcolorline);
                    if (Menu.Item("KillText").GetValue<bool>()) Drawing.DrawText(KillTextPos[0] - 105, KillTextPos[1] + 25, barcolor, KillText);
                    if (Menu.Item("KillTextHP").GetValue<bool>()) Drawing.DrawText(hpBarPos.X + 98, hpBarPos.Y + 5, barcolor, HPleftText);
                    if (Menu.Item("DrawHPFill").GetValue<bool>())
                    {
                        var diff = currentHPXPos - comboXPos;
                        for (int i = 0; i < diff; i++)
                        {
                            Drawing.DrawLine((float)comboXPos + (float)i, hpBarPos.Y + 2, (float)comboXPos + (float)i, (float)hpBarPos.Y + 10, 1, barcolor);
                        }
                    }
                }
                
                //Draw QE to cursor circle
                if (Menu.Item("UseQEC").GetValue<KeyBind>().Active && E.IsReady() && Q.IsReady() && menuItem.Active)
                Utility.DrawCircle(Game.CursorPos, 150f, (enemy.Distance(Game.CursorPos, true) <= 150 * 150) ? Color.Red : menuItem.Color, 3);
            }

            foreach (var spell in SpellList)
            { // Draw Spell Ranges
                menuItem = Menu.Item("Draw" + spell.Slot).GetValue<Circle>();
                if (menuItem.Active)
                    Utility.DrawCircle(Player.Position, spell.Range, menuItem.Color);
            }

            // Dashboard Indicators
            if (Menu.Item("HUD").GetValue<bool>()) { 
                if (Menu.Item("HarassActiveT").GetValue<KeyBind>().Active) Drawing.DrawText(Drawing.Width * 0.90f, Drawing.Height * 0.68f, System.Drawing.Color.Yellow, "Auto Harass : On");
                else Drawing.DrawText(Drawing.Width * 0.90f, Drawing.Height * 0.68f, System.Drawing.Color.DarkRed, "Auto Harass : Off");

                if (Menu.Item("AutoKST").GetValue<KeyBind>().Active) Drawing.DrawText(Drawing.Width * 0.90f, Drawing.Height * 0.66f, System.Drawing.Color.Yellow, "Auto KS : On");
                else Drawing.DrawText(Drawing.Width * 0.90f, Drawing.Height * 0.66f, System.Drawing.Color.DarkRed, "Auto KS : Off");
            }
            // Draw QE MAP
            if (Menu.Item("DrawQEMAP").GetValue<bool>()) { 
                var QETarget = SimpleTs.GetTarget(QE.Range, SimpleTs.DamageType.Magical);
                Vector3 SPos = Prediction.GetPrediction(QETarget, E.Delay).UnitPosition;
                if (Player.Distance(SPos, true) > Math.Pow(E.Range, 2) && (E.IsReady() || Player.Spellbook.GetSpell(SpellSlot.E).CooldownExpires - Game.Time < 2) && Player.Spellbook.GetSpell(SpellSlot.E).Level>0)
                {
                    Color color = Color.Red;
                    Vector3 orb = Player.Position + Vector3.Normalize(SPos - Player.Position) * E.Range;
                    QE.Delay = E.Delay + Player.Distance(orb) / E.Speed;
                    var TPos = QE.GetPrediction(QETarget);
                    if (TPos.Hitchance >= HitChance.Medium) color = Color.Green;
                    if(Player.Spellbook.GetSpell(SpellSlot.Q).ManaCost + Player.Spellbook.GetSpell(SpellSlot.E).ManaCost > Player.Mana) color = Color.DarkBlue;
                    Vector3 Pos = Player.Position + Vector3.Normalize(TPos.UnitPosition - Player.Position) * 700;
                    Utility.DrawCircle(Pos, Q.Width, color);
                    Utility.DrawCircle(TPos.UnitPosition, Q.Width / 2, color);
                    Vector3 SP1 = Pos + Vector3.Normalize(Player.Position - Pos) * 100f;
                    Vector2 SP = Drawing.WorldToScreen(SP1);
                    Vector3 EP1 = Pos + Vector3.Normalize(Pos - Player.Position) * 592;
                    Vector2 EP = Drawing.WorldToScreen(EP1);
                    Drawing.DrawLine(SP.X, SP.Y, EP.X, EP.Y, 2, color);

                }
                
            }
            if (Menu.Item("DrawWMAP").GetValue<bool>() && Player.Spellbook.GetSpell(SpellSlot.W).Level > 0)
            {
                Color color2 = Color.FromArgb(100, 255, 0, 0); ;
                var WTarget = SimpleTs.GetTarget(W.Range + W.Width, SimpleTs.DamageType.Magical);
                PredictionOutput Pos2 = W.GetPrediction(WTarget, true);
                if (Pos2.Hitchance >= HitChance.High)
                {
                    color2 = Color.FromArgb(100, 50, 150, 255); ;
                }
                Utility.DrawCircle(Pos2.UnitPosition, W.Width, color2);
            }
        }
    }
}
