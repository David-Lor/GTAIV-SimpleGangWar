# GTA IV SimpleGangWar script

Grand Theft Auto IV script to create a basic battle between two teams.

## Background

This is a port of [GTA V SimpleGangWar script](https://github.com/David-Lor/GTAV-SimpleGangWar) and [RDR2 SimpleGangWar script](https://github.com/David-Lor/RDR2-SimpleGangWar). Since most functions and methods are the same between GTA5, RDR2 and GTA4, and the language used is the same (C#), almost all functionalities were ported to GTA IV.

## Installing

**Prerrequisites**: GTA IV ScriptHookDotNet and its requirements. I recommend downloading [LCPD:FR](https://www.lcpdfr.com/downloads/gta4mods/g17media/4607-lcpd-first-response-legacy-edition/), which bundles all of the requirements. Tested on GTA IV version 1.2.0.43 (Steam+Rockstar version) in October 2021.

**Installing**: download the latest version (zip file) from [Releases](https://github.com/David-Lor/GTAIV-SimpleGangWar/releases). Extract and put SimpleGangWar.cs and SimpleGangWar.ini into the `Grand Theft Auto IV/scripts` folder

## Usage

The key `F9` ("Hotkey") is used to navigate through all the steps of the script. In-game help popups will describe what to do, but these are the different stages you will find:

1. The script will ask you to move to where the enemies will spawn
2. After pressing the hotkey, you must do the same to define where the allies will spawn
3. Right after defining both spawnpoints, peds from both teams will spawn on their respective spawnpoints, and fight each other
4. Press the hotkey once to enter the "exit mode" (it will ask for confirmation to stop the battle)
5. Pressing the hotkey again will inmediately stop the battle and remove all alive & dead peds from the map

An additional hotkey `F8` ("SpawnHotkey") is used to pause/resume the ped spawning in both teams.

### Battle functionality

- When the battle starts, peds from both teams will spawn on their respective spawnpoints, and start fighting their adversaries.
- As peds get killed¹, new peds are spawned on the respective team spawnpoint as replacements.
- The settings "MaxPeds"/"MaxPedsPerTeam" are used to determine how many peds can be fighting on each team at the same time, regulating the respawn rate of reinforcements.

¹ _In this GTA IV implementation, peds that are "Wounded In Action" are treated as killed - this means peds that are still alive, but hurt and bleeding on the floor, unable to keep fighting.
Notice that in GTAV and RDR2, wounded peds remain alive for a small time, while in GTA IV they can remain for longer (forever?)._

#### Ending a battle

- Different mechanisms are provided for making a battle end naturally. This is done by making one ped run out of peds, by disabling the respawning (on that team or both teams) and having all their members killed.
- The battle can be paused and resumed any time with the "SpawnHotkey" - the existing peds will keep fighting, but no more peds will spawn on any of the teams.
- The setting "MaxSpawnPeds" can be used for limiting the amount of replacements that will spawn for a team. Please notice that this will NOT spawn any more peds on the team that reaches this limit for the current battle, even if pausing/resuming it with the "SpawnHotkey".
- When the battle is definitively ended (using the main hotkey to navigate between the different stages of the script), all the peds (both alive and killed/downed) are instantly removed from the game.

## Settings

Settings can be defined on the `SimpleGangWar.ini` file, being the following:

### ALLIED_TEAM & ENEMY_TEAM

_All lists of items (models & weapons) are separated by comma (`,`) or semi-colon (`;`). Spaces and case ignored. **If any item name does not exist, SimpleGangWar will crash!**_

- Ped models and weapons: each ped will spawn with a random Ped Model and Weapon, chosen from the configured Models and Weapons lists.
  - `Models`: list of ped models ([Reference](docs/PedModels.md))
  - `Weapons`: list of ped weapons ([Reference](docs/Weapons.md))
- `Health`: health for peds (should not be less than 100; if -1, not changed)
- `Armor`: armor for peds (greater/equal to 0; if -1, not changed)
- `Accuracy`: accuracy for peds (greater/equal to 0; if -1, not changed)
- `MaxPeds`: maximum alive peds on the team at the same time (if not specified, the MaxPedsPerTeam setting will be used) (greater/equal to 0)
- `MaxSpawnPeds`: limit of peds that will spawn on the team. When the limit is reached, no more peds on the team will spawn on the current battle for that team (greater/equal to 0; if -1, not set)

### SETTINGS

- `Hotkey`: the single hotkey used to iterate over the script stages ([Reference](https://docs.microsoft.com/en-us/dotnet/api/system.windows.input.key?view=netcore-3.1#fields))
- `SpawnHotkey`: hotkey used to pause/resume ped spawn in both teams ([Reference](https://docs.microsoft.com/en-us/dotnet/api/system.windows.input.key?view=netcore-3.1#fields))
- `MaxPedsPerTeam`: maximum alive peds on each team - teams with the setting MaxPeds will ignore this option (greater/equal to 0)
- `EnemiesRespectOtherGroups`: experimental quickfix that should allow integrating SimpleGangWar during missions involving fighting enemies (making SimpleGangWar enemies cooperate with mission enemies).
  Without it, SimpleGangWar enemy peds would fight mission enemies. This setting enabled will make the enemy team RelationshipGroup respect all the other Groups of the game except for the Player and Cops groups (true/false)
- `SpawnpointFloodLimitPeds`: limit how many peds can be near its spawnpoint. If more than this quantity of peds are near the spawnpoint, no more peds on the team will spawn (greater than 0; if 0, not set)
- `SpawnpointFloodLimitDistance`: in-game distance from a team spawnpoint to keep track of the SpawnpointFloodLimitPeds. Can be integer or decimal (if using decimals, use dot or comma depending on your system regional settings)
- `ShowBlipsOnPeds`: if true, each spawned ped will have a blip on the map (true/false)
- `RemoveDeadPeds`: if true, mark dead peds as no longer needed, making the game handle their cleanup; recommended to keep it enabled for long fights, as many persistent bodies may crash the game (true/false)
- `RunToSpawnpoint`: if true, the peds task will be to run to their enemies' spawnpoint; if false, will be to fight hated targets on the area (true/false) (currently not working, fine, recommended to leave on false or remove).
- Intervals: delay between loop runs. Lower values will have more precise results but also use more system resources. Default values are recommended. Two values are available, depending on the battle stage:
  - `IdleInterval`: when battle is not running
  - `BattleInterval`: when battle is running

## Known bugs

This is a list of known issues that are currently not being tracked, or are unfixable. Other bugs that may get fixed in the future are listed on [Issues](https://github.com/David-Lor/GTAIV-SimpleGangWar/issues).

- RunToSpawnpoint=true is not working fine; peds usually keep stuttering forever, or slow-walk into the enemies. For now it's recommended to keep this setting disabled, as peds seem to fight well with the defaul behaviour.
- Peds may not fight if spawnpoints are too far.
- Usage during missions may cause different problems:
  - Allies may not follow the player (this may happen when setting the battle before the mission, but setting it during the mission may work - tested on "Deconstruction for Beginners")
  - Mission and SimpleGangWar enemies may fight each other (this may be fixed when implementing the ProcessOtherRelationshipGroups feature)

## Changelog

- 0.1.2
  - fix: wounded (no longer fighting) peds are now counted as killed (so reinforcements can respawn)
  - fix: peds blips not being deleted on ped kill
  - fix: script crash when processing spawned peds that no longer exist
  - refactor: new Fighter class that holds info about each spawned ped (Ped object, Blip object, team); this allows fixing some of the bugs solved on this version
  - style: refactor code style
  - docs(README): describe how the battle works; describe difference between Known Bugs and Github Issues
- 0.1.1
  - Support for .ini config file, and multiple configurations from SimpleGangWar integrated
- 0.0.1
  - Initial ported functional release; missing some features and support for config file
