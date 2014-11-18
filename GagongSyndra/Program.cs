using System.Collections.Generic;
using System.Linq;
using System;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;


namespace GagongSyndra
{
    class Program
    {
        private const string ChampName = "Syndra";
        private static Obj_AI_Hero Player = ObjectManager.Player;

        //Create spells
        public static List<Spell> SpellList = new List<Spell>();
        private static Spell Q;
        private static Spell W;
        private static Spell E;
        private static Spell R;
        private static Spell QE;

        //Summoner spells
        public static SpellSlot IgniteSlot;
        public static SpellSlot FlashSlot;

        //Items
        public static Items.Item DFG;

        private static Menu Menu;
        static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        static void Game_OnGameLoad(EventArgs args)
        {
            if (Player.BaseSkinName != ChampName) return;
            
            
            //Spells data
            Q = new Spell(SpellSlot.Q, 800);
            Q.SetSkillshot(0.6f, 125f, float.MaxValue, false, SkillshotType.SkillshotCircle);

            W = new Spell(SpellSlot.W, 925);
            W.SetSkillshot(0.25f, 190f, 1450f, false, SkillshotType.SkillshotCircle);

            E = new Spell(SpellSlot.E, 700);
            E.SetSkillshot(0.25f, (float)(45 * 0.5), 2500, false, SkillshotType.SkillshotCone);         

            R = new Spell(SpellSlot.R, 675);
            R.SetTargetted(0.5f, 1100f);

            QE = new Spell(SpellSlot.E, 1292);
            QE.SetSkillshot(0f, 60f, 1600f, false, SkillshotType.SkillshotLine);

            IgniteSlot = Player.GetSpellSlot("SummonerDot");
            FlashSlot = Player.GetSpellSlot("summonerflash");

            DFG = Utility.Map.GetMap()._MapType == Utility.Map.MapType.TwistedTreeline ? new Items.Item(3188, 750) : new Items.Item(3128, 750);
            SpellList.Add(Q);
            SpellList.Add(W);
            SpellList.Add(E);
            SpellList.Add(R);
            
            //Base menu
            Menu = new Menu("Stephen", "Stephen", true);

            //SimpleTs
            Menu.AddSubMenu(new Menu("SimpleTs", "SimpleTs"));
            SimpleTs.AddToMenu(Menu.SubMenu("SimpleTs"));

            //Orbwalker
            Menu.AddSubMenu(new Menu("Orbwalker", "Orbwalker"));
            new Orbwalking.Orbwalker(Menu.SubMenu("Orbwalker"));

            //Combo
            Menu.AddSubMenu(new Menu("Combo", "Combo"));
            Menu.SubMenu("Combo").AddItem(new MenuItem("UseQ", "Use Q").SetValue(true));
            Menu.SubMenu("Combo").AddItem(new MenuItem("UseW", "Use W").SetValue(true));
            Menu.SubMenu("Combo").AddItem(new MenuItem("UseE", "Use E").SetValue(true));
            Menu.SubMenu("Combo").AddItem(new MenuItem("UseR", "Use R").SetValue(true));
            Menu.SubMenu("Combo").AddItem(new MenuItem("UseQE", "Use QE").SetValue(true));
            Menu.SubMenu("Combo").AddItem(new MenuItem("ComboActive", "Combo").SetValue(new KeyBind(32, KeyBindType.Press)));
            
            //Harass
            Menu.AddSubMenu(new Menu("Harass", "Harass"));
            Menu.SubMenu("Harass").AddItem(new MenuItem("UseQH", "Use Q").SetValue(true));
            Menu.SubMenu("Harass").AddItem(new MenuItem("HarassAAQ", "Harass with Q if enemy AA").SetValue(false));
            Menu.SubMenu("Harass").AddItem(new MenuItem("UseWH", "Use W").SetValue(false));
            Menu.SubMenu("Harass").AddItem(new MenuItem("UseEH", "Use E").SetValue(false));
            Menu.SubMenu("Harass").AddItem(new MenuItem("UseQEH", "Use QE").SetValue(false));
            Menu.SubMenu("Harass").AddItem(new MenuItem("HarassMana", "Only Harass if mana >").SetValue(new Slider(0, 0, 100)));
            Menu.SubMenu("Harass").AddItem(new MenuItem("HarassActive", "Harass").SetValue(new KeyBind("C".ToCharArray()[0], KeyBindType.Press)));
            Menu.SubMenu("Harass").AddItem(new MenuItem("HarassActiveT", "Harass (toggle)!").SetValue(new KeyBind("Y".ToCharArray()[0], KeyBindType.Toggle,true)));
            
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
            
            //Misc
            Menu.AddSubMenu(new Menu("Misc", "Misc"));
            Menu.SubMenu("Misc").AddItem(new MenuItem("AntiGap", "Anti Gap Closer").SetValue(true));
            Menu.SubMenu("Misc").AddItem(new MenuItem("Interrupt", "Auto Interrupt Spells").SetValue(true));

            //QE Setting
            Menu.AddSubMenu(new Menu("QE Settings", "QEsettings"));
            Menu.SubMenu("QEsettings").AddItem(new MenuItem("QEDelay", "QE Delay").SetValue(new Slider(0, 0, 150)));
            Menu.SubMenu("QEsettings").AddItem(new MenuItem("UseQEC", "QE to Enemy Near Cursor").SetValue(new KeyBind("T".ToCharArray()[0], KeyBindType.Press)));

            //R
            Menu.AddSubMenu(new Menu("R Settings", "Rsettings"));
            Menu.SubMenu("Rsettings").AddItem(new MenuItem("RallCD", "Only R when QEW is on CD").SetValue(true));
            Menu.SubMenu("Rsettings").AddSubMenu(new Menu("Dont use R on", "DontR"));
            foreach (var enemy in ObjectManager.Get<Obj_AI_Hero>().Where(enemy => enemy.Team != Player.Team))
                Menu.SubMenu("Rsettings").SubMenu("DontR").AddItem(new MenuItem("DontR" + enemy.BaseSkinName, enemy.BaseSkinName).SetValue(false));

            //Drawings
            Menu.AddSubMenu(new Menu("Drawings", "Drawing"));
            Menu.SubMenu("Drawing").AddItem(new MenuItem("DrawQ", "Q Range").SetValue(new Circle(true, Color.FromArgb(100, 255, 0, 255))));
            Menu.SubMenu("Drawing").AddItem(new MenuItem("DrawW", "W Range").SetValue(new Circle(false, Color.FromArgb(100, 255, 0, 255))));
            Menu.SubMenu("Drawing").AddItem(new MenuItem("DrawE", "E Range").SetValue(new Circle(false, Color.FromArgb(100, 255, 0, 255))));
            Menu.SubMenu("Drawing").AddItem(new MenuItem("DrawR", "R Range").SetValue(new Circle(false, Color.FromArgb(100, 255, 0, 255))));
            Menu.SubMenu("Drawing").AddItem(new MenuItem("DrawQE", "QE Range").SetValue(new Circle(true, Color.FromArgb(100, 255, 0, 255))));
            Menu.SubMenu("Drawing").AddItem(new MenuItem("DrawQEC", "QE Cursor indicator").SetValue(new Circle(false, Color.FromArgb(100, 255, 0, 255))));

            //Add main menu
            Menu.AddToMainMenu();

            Game.OnGameUpdate += Game_OnGameUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
            Obj_AI_Base.OnProcessSpellCast += Obj_AI_Base_OnProcessSpellCast;
            AntiGapcloser.OnEnemyGapcloser += AntiGapcloser_OnEnemyGapcloser;
            Interrupter.OnPossibleToInterrupt += Interrupter_OnPossibleToInterrupt;
            Orbwalking.BeforeAttack += Orbwalking_BeforeAttack;
            

            Game.PrintChat("Gagong Syndra Loaded!");
        }


        static void Game_OnGameUpdate(EventArgs args)
        {
            if (Player.IsDead) return;
           
            //Update R Range
            R.Range = R.Level == 3 ? 750f : 675f;

            //Update E Width
            E.Width = E.Level == 5 ? 45f : (float)(45 * 0.5);

            //Use QE to Mouse Position
            if (Menu.Item("UseQEC").GetValue<KeyBind>().Active && E.IsReady() && Q.IsReady())
                foreach (var enemy in ObjectManager.Get<Obj_AI_Hero>().Where(enemy => enemy.Team != Player.Team && Player.Distance(enemy, true) <= Math.Pow(QE.Range, 2)))
                {
                   if (enemy.IsValid && !enemy.IsDead && enemy.Distance(Game.CursorPos, true) <= 150 * 150)
                        UseQE(enemy);
                }

            //Combo
            if (Menu.Item("ComboActive").GetValue<KeyBind>().Active)
            {
                Combo();
            }
            
            //Harass
            else if (Menu.Item("HarassActive").GetValue<KeyBind>().Active || Menu.Item("HarassActiveT").GetValue<KeyBind>().Active)
            {
                Harass();
            }
            //Auto KS
            if (Menu.Item("AutoKST").GetValue<KeyBind>().Active)
            {
                AutoKS();
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
        }
        
        //Anti gapcloser
        private static void AntiGapcloser_OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (!Menu.Item("AntiGap").GetValue<bool>()) return;

            if (E.IsReady() && Player.Distance(gapcloser.Sender, true) <= Math.Pow(E.Range, 2))
            {
                if (Q.IsReady())
                {
                    UseQE((Obj_AI_Hero)gapcloser.Sender);
                }
                else
                    E.Cast(gapcloser.End);
            }
        }

        //Interrupt dangerous spells
        private static void Interrupter_OnPossibleToInterrupt(Obj_AI_Base unit, InterruptableSpell spell)
        {
            if (!Menu.Item("Interrupt").GetValue<bool>()) return;

            if (E.IsReady() && Player.Distance(unit, true) <= Math.Pow(E.Range, 2))
            {
                if (Q.IsReady())
                    UseQE((Obj_AI_Hero)unit);
                else
                    E.Cast(unit);
            }
            else if (Q.IsReady() && E.IsReady() && Player.Distance(unit, true) <= Math.Pow(QE.Range, 2))
                UseQE((Obj_AI_Hero)unit);
        }

        static void Orbwalking_BeforeAttack(Orbwalking.BeforeAttackEventArgs args)
        {
            if (Menu.Item("ComboActive").GetValue<KeyBind>().Active)
                args.Process = !Q.IsReady() && (!W.IsReady() || !E.IsReady());
        }

        private static float GetComboDamage(Obj_AI_Hero enemy, bool UQ, bool UW, bool UE, bool UR, bool UDFG = true)
        {
            var damage = 0d;
            var combomana = 0d;

            //Add Q Damage
            if (Q.IsReady() && UQ)
            {
                combomana += Player.Spellbook.GetSpell(SpellSlot.Q).ManaCost;
                if (combomana <= Player.Mana) damage += Player.GetSpellDamage(enemy, SpellSlot.Q);
                else combomana -= Player.Spellbook.GetSpell(SpellSlot.Q).ManaCost;
            }

            //Add R Damage
            if (R.IsReady() && UR)
            {
                combomana += Player.Spellbook.GetSpell(SpellSlot.R).ManaCost;
                if (combomana <= Player.Mana) damage += GetRDamage(enemy);
                else combomana -= Player.Spellbook.GetSpell(SpellSlot.R).ManaCost;
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
            //Menu check
            if (!Menu.Item("RallCD").GetValue<bool>()) return true;

            //If can be killed by all All other skills that are ready + 1 AA
            else if (GetComboDamage(enemy, true, true, true, false, false) + Player.GetAutoAttackDamage(enemy) >= enemy.Health) return false;

            //Check last cast times
            else if (Environment.TickCount - Q.LastCastAttemptT > 600 + Game.Ping && Environment.TickCount - E.LastCastAttemptT > 600 + Game.Ping && Environment.TickCount - W.LastCastAttemptT > 600 + Game.Ping) return true;

            else return false;
        }

        private static void AutoKS()
        {
            foreach (var enemy in ObjectManager.Get<Obj_AI_Hero>().Where(enemy => enemy.Team != Player.Team))
                if (!enemy.HasBuff("UndyingRage") && !enemy.HasBuff("JudicatorIntervention") && enemy.IsValid && !enemy.IsDead)
                {
                    if (GetComboDamage(enemy, false, false, Menu.Item("UseQEKS").GetValue<bool>(), false, false) > enemy.Health && Player.Distance(enemy, true) <= Math.Pow(QE.Range, 2))
                    {
                        UseSpells(false, //Q
                        false, //W
                        false, //E
                        false, //R
                        Menu.Item("UseQEKS").GetValue<bool>(), //QE
                        true //fromKS
                        );
                        //Game.PrintChat("QEKS " + enemy.Name);
                    }
                    else if (GetComboDamage(enemy, false, Menu.Item("UseWKS").GetValue<bool>(), false, false, false) > enemy.Health && Player.Distance(enemy, true) <= Math.Pow(W.Range, 2))
                    {
                        UseSpells(false, //Q
                        Menu.Item("UseWKS").GetValue<bool>(), //W
                        false, //E
                        false, //R
                        false, //QE
                        true //fromKS
                        ); 
                        //Game.PrintChat("WKS " + enemy.Name);
                    }
                    else if (GetComboDamage(enemy, Menu.Item("UseQKS").GetValue<bool>(), false, Menu.Item("UseEKS").GetValue<bool>(), false, false) > enemy.Health && Player.Distance(enemy, true) <= Math.Pow(Q.Range + 25f, 2))
                    {
                        UseSpells(Menu.Item("UseQKS").GetValue<bool>(), //Q
                        false, //W
                        Menu.Item("UseEKS").GetValue<bool>(), //E
                        false, //R
                        false, //QE
                        true //fromKS
                        ); 
                        //Game.PrintChat("QEKSC " + enemy.Name);
                    }
                    else if (GetComboDamage(enemy, Menu.Item("UseQKS").GetValue<bool>(), Menu.Item("UseWKS").GetValue<bool>(), Menu.Item("UseEKS").GetValue<bool>(), Menu.Item("UseRKS").GetValue<bool>()) > enemy.Health && Player.Distance(enemy, true) <= Math.Pow(R.Range, 2))
                    {
                        UseSpells(Menu.Item("UseQKS").GetValue<bool>(), //Q
                        Menu.Item("UseWKS").GetValue<bool>(), //W
                        Menu.Item("UseEKS").GetValue<bool>(), //E
                        Menu.Item("UseRKS").GetValue<bool>(), //R
                        Menu.Item("UseQEKS").GetValue<bool>(), //QE
                        true //fromKS
                        ); 
                        //Game.PrintChat("QWERKS " + enemy.Name);
                    }
                    else if ((GetComboDamage(enemy, false, false, Menu.Item("UseEKS").GetValue<bool>(), Menu.Item("UseRKS").GetValue<bool>(), false) > enemy.Health || GetComboDamage(enemy, false, Menu.Item("UseWKS").GetValue<bool>(), Menu.Item("UseEKS").GetValue<bool>(), false, false) > enemy.Health) && Player.Distance(enemy, true) <= Math.Pow(QE.Range, 2))
                    {
                        UseSpells(false, //Q
                        false, //W
                        false, //E
                        false, //R
                        Menu.Item("UseQEKS").GetValue<bool>(), //QE
                        true //fromKS
                        ); 
                        //Game.PrintChat("QEKS " + enemy.Name);
                    }
                    //Flash Kill
                    bool UseFlash = Menu.Item("FKT" + enemy.BaseSkinName) != null && Menu.Item("FKT" + enemy.BaseSkinName).GetValue<bool>() == true;
                    PredictionOutput ePos = R.GetPrediction(enemy);
                    if ((FlashSlot != SpellSlot.Unknown || Player.SummonerSpellbook.CanUseSpell(FlashSlot) == SpellState.Ready) && UseFlash
                        && Player.Distance(ePos.UnitPosition, true) <= Math.Pow(Q.Range + 25f + 395, 2) && Player.Distance(ePos.UnitPosition, true) > Math.Pow(Q.Range + 25f + 200, 2))

                    if ((GetComboDamage(enemy, Menu.Item("UseQKS").GetValue<bool>(), false, Menu.Item("UseEKS").GetValue<bool>(), false, false) > enemy.Health && Menu.Item("UseFK1").GetValue<bool>())
                        || (GetComboDamage(enemy, false, false, false, Menu.Item("UseRKS").GetValue<bool>()) > enemy.Health && Menu.Item("UseFK2").GetValue<bool>() && Player.Distance(ePos.UnitPosition, true) <= Math.Pow(R.Range + 395, 2) && Player.Distance(ePos.UnitPosition, true) > Math.Pow(R.Range + 200, 2)))
                    {
                        var NearbyE = ePos.UnitPosition.CountEnemysInRange(1000);
                        if (NearbyE <= Menu.Item("MaxE").GetValue<Slider>().Value)
                        {
                            Vector3 FlashPos = Player.ServerPosition - Vector3.Normalize(Player.ServerPosition - ePos.UnitPosition) * 400;
                            Player.SummonerSpellbook.CastSpell(FlashSlot, FlashPos);
                            //Use Ult after flash if can't be killed by QE
                            if (GetComboDamage(enemy, Menu.Item("UseQKS").GetValue<bool>(), false, Menu.Item("UseEKS").GetValue<bool>(), false, false) < enemy.Health)
                                UseSpells(false, //Q
                                false, //W
                                false, //E
                                Menu.Item("UseRKS").GetValue<bool>(), //R
                                false, //QE
                                true //fromKS
                                );
                        }
                    }

                }
        }

        private static void UseSpells(bool UQ, bool UW, bool UE, bool UR, bool UQE, bool fromKS=false)
        {   
            //Set Target
            var QTarget = SimpleTs.GetTarget(Q.Range + 25f, SimpleTs.DamageType.Magical);
            var WTarget = SimpleTs.GetTarget(W.Range + W.Width, SimpleTs.DamageType.Magical);
            var RTarget = SimpleTs.GetTarget(R.Range, SimpleTs.DamageType.Magical);
            var QETarget = SimpleTs.GetTarget(QE.Range, SimpleTs.DamageType.Magical);
            bool UseR = false;
            var totmana = 0d;
            //Use DFG
            if (RTarget != null && GetComboDamage(RTarget, UQ, UW, UE, UR) + GetIgniteDamage(RTarget) > RTarget.Health)
            {
                //DFG
                if (DFG.IsReady() && Player.Distance(RTarget, true) <= Math.Pow(DFG.Range, 2) && GetComboDamage(RTarget, UQ, UW, UE, false, false) + GetIgniteDamage(QTarget) < RTarget.Health)
                    if((UR && R.IsReady()) || (UQ && Q.IsReady())) DFG.Cast(RTarget);
            }
            else if (RTarget != null && Menu.Item("HarassActiveT").GetValue<KeyBind>().Active && Player.Distance(RTarget, true) <= Math.Pow(R.Range, 2) && !fromKS)
            {
                    DFG.Cast(QTarget);
            }       

            //Use Q
            if (UQ && QTarget != null)
            {
                UseQ(QTarget);
            }

            //R, Ignite 
            foreach (var enemy in ObjectManager.Get<Obj_AI_Hero>().Where(enemy => enemy.Team != Player.Team))
                if (!enemy.HasBuff("UndyingRage") && !enemy.HasBuff("JudicatorIntervention") && enemy.IsValid && !enemy.IsDead)
                {
                    //R
                    UseR = Menu.Item("DontR" + enemy.BaseSkinName) != null && Menu.Item("DontR" + enemy.BaseSkinName).GetValue<bool>() == false && UR;
                    if (UseR && R.IsReady() && Player.Distance(enemy, true) <= Math.Pow(R.Range, 2) && (DFGBuff(enemy) ? GetRDamage(enemy) * 1.2 : GetRDamage(enemy)) > enemy.Health)
                    { // && Player.GetSpellDamage(RTarget, SpellSlot.Q) < RTarget.Health && Player.Spellbook.GetSpell(SpellSlot.W).Cooldown<2 
                        foreach (var spell in SpellList)
                        { // Total Combo Mana
                            totmana += Player.Spellbook.GetSpell(spell.Slot).ManaCost;
                        }
                        if ((totmana < Player.Mana || RCheck(enemy)))
                            if (!(Player.GetSpellDamage(RTarget, SpellSlot.Q) > RTarget.Health && Player.Spellbook.GetSpell(SpellSlot.Q).CooldownExpires - Game.Time < 2 && Player.Spellbook.GetSpell(SpellSlot.Q).CooldownExpires - Game.Time >= 0 && enemy.IsStunned))
                                R.CastOnUnit(enemy); 
                    }
                    else if (RTarget != null && Menu.Item("HarassActiveT").GetValue<KeyBind>().Active && Player.Distance(RTarget, true) <= Math.Pow(R.Range, 2) && UseR && !fromKS)
                    {
                        R.CastOnUnit(RTarget);
                    }
                    //Ignite
                    if (Player.Distance(enemy, true) <= 600 * 600 && GetIgniteDamage(enemy) > enemy.Health)
                        Player.SummonerSpellbook.CastSpell(IgniteSlot, enemy);
                }     

            //Use E
            if (UE && E.IsReady() && Environment.TickCount - W.LastCastAttemptT > Game.Ping + 150)
                foreach (var enemy in ObjectManager.Get<Obj_AI_Hero>().Where(enemy => enemy.Team != Player.Team))
                {
                    if(enemy.IsValid && !enemy.IsDead)
                        if (GetComboDamage(enemy, UQ, UW, UE, UR) > enemy.Health && Player.Distance(enemy, true) <= Math.Pow(E.Range, 2))
                            E.Cast(enemy);
                        else if (Player.Distance(enemy, true) <= Math.Pow(QE.Range, 2))
                            UseE(enemy);
                }
            
            //Use QE
            if (UQE && WTarget == null && QETarget != null && Q.IsReady() && E.IsReady())
            {
                UseQE(QETarget);
            }
                

            //Use W1
            if (UW && QETarget != null && W.IsReady() && Player.Spellbook.GetSpell(SpellSlot.W).ToggleState == 1 )
            {
                Vector3 gObjectPos = GetGrabableObjectPos(false);

                if (gObjectPos.To2D().IsValid() && Environment.TickCount - Q.LastCastAttemptT > 600 + Game.Ping && Environment.TickCount - E.LastCastAttemptT > 600 + Game.Ping && Environment.TickCount - W.LastCastAttemptT > 600 + Game.Ping)
                {
                    W.Cast(gObjectPos);
                }
            }

            //Use W2
            if (UW && W.IsReady() && Player.Spellbook.GetSpell(SpellSlot.W).ToggleState != 1 && QETarget != null && !(OrbManager.WObject(false).Name.ToLower() == "heimertblue"))
            {
                W.UpdateSourcePosition(OrbManager.WObject(false).ServerPosition);
                PredictionOutput Pos = W.GetPrediction(WTarget, true);
                if (Pos.Hitchance >= HitChance.High)
                    W.Cast(Pos.CastPosition);
            }
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
                Q.Cast(Pos.CastPosition);
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
                        E.Cast(orb);                
                }
        }

        private static void UseQE(Obj_AI_Hero Target)
        {
            if (!Q.IsReady() || !E.IsReady()) return;
            Vector3 SPos = Prediction.GetPrediction(Target, 0.53f).UnitPosition;
            if (Player.Distance(SPos, true) > Math.Pow(700, 2))
            {
                QE.Delay = 0.53f;
                QE.From = Player.ServerPosition + Vector3.Normalize(Target.ServerPosition - Player.ServerPosition) * 700;
                var TPos = QE.GetPrediction(Target);
                if (TPos.Hitchance >= HitChance.High)
                {
                    Vector3 Pos = Player.ServerPosition + Vector3.Normalize(TPos.UnitPosition - Player.ServerPosition) * 700;
                    UseQE2(Target, Pos);
                }
            }
            else
            {
                Q.Width = 60f;
                var Pos = Q.GetPrediction(Target);
                if (Pos.Hitchance >= HitChance.High)
                {
                   
                    UseQE2(Target, Pos.UnitPosition);
                }
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
                    Utility.DelayAction.Add(Math.Max(0, Delay), () => E.Cast(Pos));
                    QE.LastCastAttemptT = Environment.TickCount;
                    Q.Cast(Pos);
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
                    var combodamage = GetComboDamage(enemy, Menu.Item("UseQ").GetValue<bool>(), Menu.Item("UseW").GetValue<bool>(), Menu.Item("UseE").GetValue<bool>(), Menu.Item("UseR").GetValue<bool>());
                    var PercentHPleft = (enemy.Health-combodamage) / enemy.MaxHealth ;
                    if (PercentHPleft < 0) PercentHPleft = 0;
                    double comboXPos = hpBarPos.X - 36 + (107 * PercentHPleft);
                    var barcolor = Color.SeaShell;
                    if (combodamage + Player.GetSpellDamage(enemy, SpellSlot.Q) + Player.GetAutoAttackDamage(enemy)*2 > enemy.Health) barcolor = Color.SpringGreen;
                    Drawing.DrawLine((float)comboXPos, hpBarPos.Y, (float)comboXPos, (float)hpBarPos.Y + 5, 2, barcolor);
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
            if (Menu.Item("HarassActiveT").GetValue<KeyBind>().Active) Drawing.DrawText(Drawing.Width * 0.90f, Drawing.Height * 0.68f, System.Drawing.Color.DarkGreen, "Auto Harass : On");
            else Drawing.DrawText(Drawing.Width * 0.90f, Drawing.Height * 0.68f, System.Drawing.Color.DarkRed, "Auto Harass : Off");

            if (Menu.Item("AutoKST").GetValue<KeyBind>().Active) Drawing.DrawText(Drawing.Width * 0.90f, Drawing.Height * 0.66f, System.Drawing.Color.DarkGreen, "Auto KS : On");
            else Drawing.DrawText(Drawing.Width * 0.90f, Drawing.Height * 0.66f, System.Drawing.Color.DarkRed, "Auto KS : Off");
            
        }
    }
}
