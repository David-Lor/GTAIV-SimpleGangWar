using GTA;
using System;
using System.Windows.Forms;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace SimpleGangWar
{
    public class SimpleGangWarScript : Script
    {
        // Settings defined on script variables serve as fallback for settings not defined (or invalid) on .ini config file

        // Models: see docs/PedModels.md | https://gtamods.com/wiki/List_of_models_hashes
        // Weapons: see docs/Weapons.md

        private static string[] pedsAllies = {"M_O_GRUS_HI_01", "M_Y_GRUS_LO_01", "M_Y_GRUS_LO_02", "M_Y_GRUS_HI_02", "M_M_GRU2_HI_01", "M_M_GRU2_HI_02", "M_M_GRU2_LO_02", "M_Y_GRU2_LO_01"};
        private static string[] weaponsAllies = {"HANDGUN_GLOCK", "HANDGUN_DESERTEAGLE", "RIFLE_AK47", "SMG_UZI"};
        private static string[] pedsEnemies = {"M_M_GJAM_HI_01", "M_M_GJAM_HI_02", "M_M_GJAM_HI_03", "M_Y_GJAM_LO_01", "M_Y_GJAM_LO_02"};
        private static string[] weaponsEnemies = {"HANDGUN_GLOCK", "RIFLE_M4", "SMG_MP5"};

        private static readonly char[] StringSeparators = {',', ';'};

        private static int healthAllies = 120;
        private static int armorAllies = 0;
        private static int healthEnemies = 120;
        private static int armorEnemies = 0;
        private static int accuracyAllies = 5;
        private static int accuracyEnemies = 5;

        private static int maxPedsPerTeam = 10;
        private static Keys hotkey = Keys.F9;
        private static Keys spawnHotkey = Keys.F8;
        private static bool showBlipsOnPeds = true;
        private static bool removeDeadPeds = true;
        private static bool runToSpawnpoint = false;
        private static int spawnpointFloodLimitPeds = 10;
        private static float spawnpointFloodLimitDistance = 8.0f;
        private static int idleInterval = 500;
        private static int battleInterval = 100;
        private static int maxPedsAllies = 10;
        private static int maxPedsEnemies = 10;
        private static int maxSpawnPedsAllies = -1;
        private static int maxSpawnPedsEnemies = -1;

        // Settings that can be changed, but not supported on config file

        private static readonly BlipColor allyBlipColor = BlipColor.Cyan;
        private static readonly BlipColor enemyBlipColor = BlipColor.Orange;
        private static readonly BlipColor disabledBlipColor = BlipColor.Grey;
        
        // From here, internal script variables - do not change!

        private static readonly RelationshipGroup relationshipGroupEnemies = RelationshipGroup.NetworkPlayer_32;

        private int spawnedAlliesCounter;
        private int spawnedEnemiesCounter;

        private readonly List<Fighter> spawnedAllies = new List<Fighter>();
        private readonly List<Fighter> spawnedEnemies = new List<Fighter>();
        private readonly List<Fighter> deadPeds = new List<Fighter>();
        private readonly List<Fighter> pedsRemove = new List<Fighter>();

        private bool spawnEnabled = true;
        private Stage stage = Stage.Initial;

        private Vector3 spawnpointAllies;
        private Vector3 spawnpointEnemies;
        private float spawnpointsDistance;

        private Blip spawnpointBlipAllies;
        private Blip spawnpointBlipEnemies;

        private static Random random;

        private enum Stage {
            Initial = 0,
            DefiningEnemySpawnpoint = 1,
            EnemySpawnpointDefined = 2,
            Running = 3,
            StopKeyPressed = 4
        }

        private class SettingsHeader {
            public static readonly string Allies = "ALLIED_TEAM";
            public static readonly string Enemies = "ENEMY_TEAM";
            public static readonly string General = "SETTINGS";
        }


        public SimpleGangWarScript() {
            Tick += MainLoop;
            KeyUp += OnKeyUp;
            Interval = idleInterval;

            IniFile config = new IniFile("scripts\\SimpleGangWar.ini");

            string configString = config.GetValue(SettingsHeader.General, "Hotkey", "");
            hotkey = EnumParse(configString, hotkey);
            configString = config.GetValue(SettingsHeader.General, "SpawnHotkey", "");
            spawnHotkey = EnumParse(configString, spawnHotkey);

            configString = config.GetValue(SettingsHeader.Allies, "Models", "");
            pedsAllies = ArrayParse(configString, pedsAllies);
            configString = config.GetValue(SettingsHeader.Enemies, "Models", "");
            pedsEnemies = ArrayParse(configString, pedsEnemies);

            configString = config.GetValue(SettingsHeader.Allies, "Weapons", "");
            weaponsAllies = ArrayParse(configString, weaponsAllies);
            configString = config.GetValue(SettingsHeader.Enemies, "Weapons", "");
            weaponsEnemies = ArrayParse(configString, weaponsEnemies);

            healthAllies = config.GetValue(SettingsHeader.Allies, "Health", healthAllies);
            healthEnemies = config.GetValue(SettingsHeader.Enemies, "Health", healthEnemies);

            armorAllies = config.GetValue(SettingsHeader.Allies, "Armor", armorAllies);
            armorEnemies = config.GetValue(SettingsHeader.Enemies, "Armor", armorEnemies);

            accuracyAllies = config.GetValue(SettingsHeader.Allies, "Accuracy", accuracyAllies);
            accuracyEnemies = config.GetValue(SettingsHeader.Enemies, "Accuracy", accuracyEnemies);

            maxPedsPerTeam = config.GetValue(SettingsHeader.General, "MaxPedsPerTeam", maxPedsPerTeam);
            maxPedsAllies = config.GetValue(SettingsHeader.Allies, "MaxPeds", maxPedsPerTeam);
            maxPedsEnemies = config.GetValue(SettingsHeader.Enemies, "MaxPeds", maxPedsPerTeam);

            maxSpawnPedsAllies = config.GetValue(SettingsHeader.Allies, "MaxSpawnPeds", maxSpawnPedsAllies);
            maxSpawnPedsEnemies = config.GetValue(SettingsHeader.Enemies, "MaxSpawnPeds", maxSpawnPedsEnemies);

            spawnpointFloodLimitPeds = config.GetValue(SettingsHeader.General, "SpawnpointFloodLimitPeds",
                spawnpointFloodLimitPeds);
            spawnpointFloodLimitDistance = config.GetValue(SettingsHeader.General, "SpawnpointFloodLimitDistance",
                spawnpointFloodLimitDistance);

            removeDeadPeds = config.GetValue(SettingsHeader.General, "RemoveDeadPeds", removeDeadPeds);
            showBlipsOnPeds = config.GetValue(SettingsHeader.General, "ShowBlipsOnPeds", showBlipsOnPeds);
            runToSpawnpoint = config.GetValue(SettingsHeader.General, "RunToSpawnpoint", runToSpawnpoint);
            idleInterval = config.GetValue(SettingsHeader.General, "IdleInterval", idleInterval);
            battleInterval = config.GetValue(SettingsHeader.General, "BattleInterval", battleInterval);

            World.SetGroupRelationship(RelationshipGroup.Player, Relationship.Respect, RelationshipGroup.Player);
            World.SetGroupRelationship(RelationshipGroup.Player, Relationship.Hate, relationshipGroupEnemies);
            World.SetGroupRelationship(relationshipGroupEnemies, Relationship.Respect, relationshipGroupEnemies);
            World.SetGroupRelationship(relationshipGroupEnemies, Relationship.Hate, RelationshipGroup.Player);

            random = new Random();

            Game.DisplayText("SimpleGangWar loaded");
        }


        /// <summary>
        /// The main script loop runs at the frequency delimited by the Interval, which varies depending if the battle is running or not.
        /// The loop only spawn peds and processes them as the battle is running. Any other actions that happen outside a battle are processed by Key event handlers.
        /// </summary>
        private void MainLoop(object sender, EventArgs e) {
            if (stage >= Stage.Running) {
                try {
                    SpawnPeds(true);
                    SpawnPeds(false);

                    ProcessSpawnedPeds(true);
                    ProcessSpawnedPeds(false);
                } catch (FormatException exception) {
                    Game.DisplayText("(SimpleGangWar) Error! " + exception.Message);
                }
            }
        }


        /// <summary>
        /// Key event handler for key releases.
        /// </summary>
        private void OnKeyUp(object sender, GTA.KeyEventArgs e) {
            if (e.Key == hotkey) {
                switch (stage) {
                    case Stage.Initial:
                        Game.DisplayText(
                            "Welcome to SimpleGangWar!\nGo to the enemy spawnpoint and press the hotkey again to define it.",
                            180000);
                        stage = Stage.DefiningEnemySpawnpoint;
                        break;
                    case Stage.DefiningEnemySpawnpoint:
                        DefineSpawnpoint(false);
                        Game.DisplayText(
                            "Enemy spawnpoint defined! Now go to the allied spawnpoint and press the hotkey again to define it.",
                            180000);
                        stage = Stage.EnemySpawnpointDefined;
                        break;
                    case Stage.EnemySpawnpointDefined:
                        DefineSpawnpoint(true);
                        SetupBattle();
                        Game.DisplayText("The battle begins NOW!", 5000);
                        stage = Stage.Running;
                        break;
                    case Stage.Running:
                        Game.DisplayText("Do you really want to stop the battle? Press the hotkey again to confirm.",
                            7000);
                        stage = Stage.StopKeyPressed;
                        break;
                    case Stage.StopKeyPressed:
                        Game.DisplayText("The battle has ended!", 5000);
                        stage = Stage.Initial;
                        Teardown();
                        break;
                }
            } else if (e.Key == spawnHotkey) {
                spawnEnabled = !spawnEnabled;
                BlinkSpawnpoint(true);
                BlinkSpawnpoint(false);
            }
        }


        /// <summary>
        /// After the spawnpoints are defined, some tweaks are required just before the battle begins.
        /// </summary>
        private void SetupBattle() {
            Interval = battleInterval;
            spawnpointsDistance = spawnpointEnemies.DistanceTo(spawnpointAllies);
            spawnedAlliesCounter = 0;
            spawnedEnemiesCounter = 0;
        }

        /// <summary>
        /// Spawn peds on the given team, until the ped limit for that team is reached.
        /// </summary>
        /// <param name="alliedTeam">true=ally team / false=enemy team</param>
        private void SpawnPeds(bool alliedTeam) {
            while (spawnEnabled && CanPedsSpawn(alliedTeam)) {
                SpawnRandomPed(alliedTeam);
            }
        }

        /// <summary>
        /// Determine if peds on the given team should spawn or not.
        /// </summary>
        /// <param name="alliedTeam">true=ally team / false=enemy team</param>
        private bool CanPedsSpawn(bool alliedTeam) {
            List<Fighter> spawnedPedsList = alliedTeam ? spawnedAllies : spawnedEnemies;
            int maxPeds = alliedTeam ? maxPedsAllies : maxPedsEnemies;
            int maxSpawnPeds = alliedTeam ? maxSpawnPedsAllies : maxSpawnPedsEnemies;
            int totalSpawnedPeds = alliedTeam ? spawnedAlliesCounter : spawnedEnemiesCounter;

            // by MaxPeds in the team
            if (spawnedPedsList.Count >= maxPeds) return false;

            // by MaxSpawnPeds limit
            if (maxSpawnPeds >= 0 && totalSpawnedPeds > maxSpawnPeds) return false;

            // by SpawnpointFlood limit
            if (spawnpointFloodLimitPeds < 1) return true;
            
            Vector3 spawnpointPosition = alliedTeam ? spawnpointAllies : spawnpointEnemies;
            Ped[] pedsNearSpawnpoint = World.GetPeds(spawnpointPosition, spawnpointFloodLimitDistance);
            Dictionary<Ped, Fighter> fightersPeds = FightersToPeds(spawnedPedsList);

            int pedsNearSpawnpointCount = 0;
            foreach (Ped ped in pedsNearSpawnpoint) {
                Fighter fighter;
                if (!fightersPeds.TryGetValue(ped, out fighter)) continue;
                if (fighter.CanFight() && spawnedPedsList.Contains(fighter)) pedsNearSpawnpointCount++;
            }

            return pedsNearSpawnpointCount < spawnpointFloodLimitPeds;
        }

        /// <summary>
        /// Spawns a ped on the given team, ready to fight.
        /// </summary>
        /// <param name="alliedTeam">true=ally team / false=enemy team</param>
        /// <returns>The spawned ped</returns>
        private Ped SpawnRandomPed(bool alliedTeam) {
            Vector3 pedPosition = alliedTeam ? spawnpointAllies : spawnpointEnemies;
            string pedName = RandomChoice(alliedTeam ? pedsAllies : pedsEnemies);
            string weaponName = RandomChoice(alliedTeam ? weaponsAllies : weaponsEnemies);
            Weapon weaponGive;

            // TODO Verify names from arrays on script startup
            // TODO Invalid ped models not warning as custom exception, but NullPointerException (seems that cannot assert pedModel == null)
            Model pedModel = Model.FromString(pedName);
            if (!Enum.TryParse(weaponName, true, out weaponGive)) {
                throw new FormatException("Weapon name " + weaponName + " does not exist!");
            }

            Ped ped = World.CreatePed(pedModel, pedPosition);
            ped.Weapons.Select(weaponGive);
            ped.Weapons.Current.Ammo = Int32.MaxValue;

            int health = ped.MaxHealth = alliedTeam ? healthAllies : healthEnemies;
            int armor = alliedTeam ? armorAllies : armorEnemies;
            int accuracy = ped.Accuracy = alliedTeam ? accuracyAllies : accuracyEnemies;
            if (health >= 0) ped.Health = health;
            if (armor >= 0) ped.Armor = armor;
            if (accuracy >= 0) ped.Accuracy = accuracy;

            ped.Money = 0;
            ped.RelationshipGroup = alliedTeam ? RelationshipGroup.Player : relationshipGroupEnemies;

            Fighter fighter = new Fighter(ped, alliedTeam);

            if (showBlipsOnPeds) {
                fighter.SetBlip();
            }

            ped.Task.ClearAllImmediately();
            ped.Task.AlwaysKeepTask = true;
            if (runToSpawnpoint) ped.Task.RunTo(alliedTeam ? spawnpointEnemies : spawnpointAllies);
            else ped.Task.FightAgainstHatedTargets(spawnpointsDistance);

            if (alliedTeam) {
                spawnedAllies.Add(fighter);
                spawnedAlliesCounter++;
            } else {
                spawnedEnemies.Add(fighter);
                spawnedEnemiesCounter++;
            }

            return ped;
        }

        /// <summary>
        /// Processes the spawned peds of the given team. This includes making sure they fight and process their removal as they are killed in action.
        /// </summary>
        /// <param name="alliedTeam">true=ally team / false=enemy team</param>
        private void ProcessSpawnedPeds(bool alliedTeam) {
            List<Fighter> pedList = alliedTeam ? spawnedAllies : spawnedEnemies;

            foreach (Fighter fighter in pedList) {
                Ped ped = fighter.ped;
                if (!fighter.CanFight()) {
                    fighter.DeleteBlip();
                    pedsRemove.Add(fighter);
                    deadPeds.Add(fighter);
                    if (removeDeadPeds) ped.NoLongerNeeded();
                } else if (ped.isIdle) {
                    // TODO this check can make peds stutter forever if runToSpawnpoint=true:
                    if (runToSpawnpoint) ped.Task.RunTo(alliedTeam ? spawnpointEnemies : spawnpointAllies);
                    else ped.Task.FightAgainstHatedTargets(spawnpointsDistance);
                }
            }

            foreach (Fighter fighter in pedsRemove) {
                pedList.Remove(fighter);
            }

            pedsRemove.Clear();
        }

        /// <summary>
        /// Set the spawnpoint for the given team on the position where the player is at.
        /// </summary>
        /// <param name="alliedTeam">true=ally team / false=enemy team</param>
        private void DefineSpawnpoint(bool alliedTeam) {
            Vector3 position = Player.Character.Position;
            Blip blip = Blip.AddBlip(position);

            if (alliedTeam) {
                spawnpointAllies = position;
                spawnpointBlipAllies = blip;
                blip.Icon = BlipIcon.Building_Garage;
                blip.Color = allyBlipColor;
                blip.Display = BlipDisplay.ArrowAndMap;
                blip.Name = "Ally spawnpoint";
            } else {
                spawnpointEnemies = position;
                spawnpointBlipEnemies = blip;
                blip.Icon = BlipIcon.Activity_Darts;
                blip.Color = enemyBlipColor;
                blip.Display = BlipDisplay.ArrowAndMap;
                blip.Name = "Enemy spawnpoint";
            }

            BlinkSpawnpoint(alliedTeam);
        }

        /// <summary>
        /// Blink or stop blinking the spawnpoint blip of the given team, depending on if the spawn is disabled (blink) or not (stop blinking).
        /// The current effect is changing the color of the blips (greyed out would be blinking; original color would be static).
        /// The method name was kept for having the same language as other SimpleGangWar scripts.
        /// </summary>
        /// <param name="alliedTeam">true=ally team / false=enemy team</param>
        private void BlinkSpawnpoint(bool alliedTeam) {
            Blip blip = alliedTeam ? spawnpointBlipAllies : spawnpointBlipEnemies;
            if (blip == null) return;

            if (spawnEnabled) {
                blip.Color = alliedTeam ? allyBlipColor : enemyBlipColor;
            } else {
                blip.Color = disabledBlipColor;
            }
        }

        /// <summary>
        /// Physically delete the peds from the given list from the game world.
        /// </summary>
        /// <param name="pedList">List of peds to teardown</param>
        private void TeardownPeds(List<Fighter> pedList) {
            foreach (Fighter fighter in pedList) {
                if (fighter.ped.Exists()) fighter.ped.Delete();
            }
        }

        /// <summary>
        /// Manage the battle teardown on user requests. This brings the game to an initial state, before battle start and spawnpoint definition.
        /// </summary>
        private void Teardown() {
            Interval = idleInterval;
            spawnpointBlipAllies.Delete();
            spawnpointBlipEnemies.Delete();

            TeardownPeds(spawnedAllies);
            TeardownPeds(spawnedEnemies);
            TeardownPeds(deadPeds);

            spawnedAllies.Clear();
            spawnedEnemies.Clear();
            deadPeds.Clear();
            pedsRemove.Clear();
        }

        /// <summary>
        /// Choose a random item from a given array, containing objects of type T
        /// </summary>
        /// <typeparam name="T">Type of objects in the array</typeparam>
        /// <param name="array">Array to choose from</param>
        /// <returns>A random item from the array</returns>
        private T RandomChoice<T>(T[] array) {
            return array[random.Next(0, array.Length)];
        }

        /// <summary>
        /// Given a string key from an enum, return the referenced enum object.
        /// </summary>
        /// <typeparam name="EnumType">The whole enum object, to choose an option from</typeparam>
        /// <param name="enumKey">The enum key as string</param>
        /// <param name="defaultValue">What enum option to return if the referenced enum key does not exist in the enum</param>
        /// <returns>The chosen enum option</returns>
        private EnumType EnumParse<EnumType>(string enumKey, EnumType defaultValue) where EnumType : struct {
            EnumType returnValue;
            if (!Enum.TryParse(enumKey, true, out returnValue)) returnValue = defaultValue;
            return returnValue;
        }

        /// <summary>
        /// Given a string of words to be split, split them and return a string array.
        /// </summary>
        /// <param name="stringInput">Input string</param>
        /// <param name="defaultArray">Array to return if the input string contains no items</param>
        /// <returns>A string array</returns>
        private string[] ArrayParse(string stringInput, string[] defaultArray) {
            string[] resultArray = stringInput.Replace(" ", string.Empty)
                .Split(StringSeparators, StringSplitOptions.RemoveEmptyEntries);
            if (resultArray.Length == 0) resultArray = defaultArray;
            return resultArray;
        }

        /// <summary>
        /// Given a List of Fighters, return a Dictionary associating Ped:Fighter.
        /// </summary>
        /// <returns>Dictionary Ped:Fighter</returns>
        private Dictionary<Ped, Fighter> FightersToPeds(List<Fighter> fighters) {
            Dictionary<Ped, Fighter> dict = new Dictionary<Ped, Fighter>();
            foreach (Fighter fighter in fighters) {
                dict.Add(fighter.ped, fighter);
            }

            return dict;
        }
    }

    /// <summary>
    /// Spawned ped
    /// </summary>
    class Fighter
    {
        public readonly bool allied;
        public readonly Ped ped;
        public Blip blip;

        public Fighter(Ped ped, bool allied) {
            this.ped = ped;
            this.allied = allied;
        }

        public void SetBlip() {
            blip = ped.AttachBlip();
            blip.Color = allied ? BlipColor.Cyan : BlipColor.Orange;
            blip.Name = allied ? "Ally team member" : "Enemy team member";
            blip.Display = BlipDisplay.MapOnly;
            blip.Scale = 0.5f;
        }

        public void DeleteBlip() {
            if (blip != null) {
                blip.Delete();
                blip = null;
            }
        }

        public bool CanFight() {
            return ped.Exists() && ped.isAliveAndWell;
        }
    }
    
    // Source: https://stackoverflow.com/a/14906422
    class IniFile
    {
        private readonly string _path;

        [DllImport("kernel32", CharSet = CharSet.Unicode)]
        private static extern int GetPrivateProfileString(string section, string key, string defaultValue, StringBuilder retVal, int size, string filePath);

        public IniFile(string iniPath) {
            _path = new FileInfo(iniPath).FullName;
        }

        public T GetValue<T>(string section, string key, T defaultValue) {
            var readRaw = new StringBuilder(255);
            GetPrivateProfileString(section, key, "", readRaw, 255, _path);
            string readString = readRaw.ToString();
            if (readString.Length == 0) return defaultValue;

            var converter = TypeDescriptor.GetConverter(typeof(T));
            return (T) converter.ConvertFromInvariantString(readString);
        }
    }
}
