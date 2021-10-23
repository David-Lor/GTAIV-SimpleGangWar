# GTA IV SimpleGangWar script

Grand Theft Auto IV script to create a basic battle between two teams.

## Background

This is a port of [GTA V SimpleGangWar script](https://github.com/David-Lor/GTAV-SimpleGangWar) and [RDR2 SimpleGangWar script](https://github.com/David-Lor/RDR2-SimpleGangWar). Since most functions and methods are the same between GTA5, RDR2 and GTA4, and the language used is the same (C#), almost all functionalities were ported to GTA IV.

## Installing

**Prerrequisites**: ScriptHookDotNet and its requirements. I recommend downloading [LSPD:FR](https://www.lcpdfr.com/downloads/gta4mods/g17media/4607-lcpd-first-response-legacy-edition/), which bundles all the requirements. Tested on GTA IV version 1.2.0.43 (Steam+Rockstar version) on October 2021.

**Installing**: put SimpleGangWar.cs and SimpleGangWar.ini into the `Grand Theft Auto IV/scripts` folder

## Usage

The key `F9` ("Hotkey") is used to navigate through all the steps of the script. In-game help popups will describe what to do, but these are the different stages you will find:

1. The script will ask you to move to where the enemies will spawn
2. After pressing the hotkey, you must do the same to define where the allies will spawn
3. Right after defining both spawnpoints, peds from both teams will spawn on their respective spawnpoints, and fight each other
4. Press the hotkey once to enter the "exit mode" (it will ask for confirmation to stop the battle)
5. Pressing the hotkey again will inmediately stop the battle and remove all alive & dead peds from the map

An additional hotkey `F8` ("SpawnHotkey") is used to pause/resume the ped spawning in both teams.

## Settings

Settings can be defined on the `SimpleGangWar.ini` file, being the following:

### ALLIED_TEAM & ENEMY_TEAM

_All lists of items (models & weapons) are separated by comma (`,`) or semi-colon (`;`). Spaces and case ignored. **If any item name does not exist, SimpleGangWar will crash!**_

- `Models`: list of ped models ([Reference](docs/PedModels.md))
- `Weapons`: list of ped weapons ([Reference](docs/Weapons.md))
- `Health`: health for peds (should not be less than 100; if -1, not changed)
- `Armor`: armor for peds (greater/equal to 0; if -1, not changed)
- `Accuracy`: accuracy for peds (greater/equal to 0; if -1, not changed)
- `MaxPeds`: maximum alive peds on the team at the same time (if not specified, the MaxPedsPerTeam setting will be used) (greater/equal to 0)

### SETTINGS

- `Hotkey`: the single hotkey used to iterate over the script stages ([Reference](https://docs.microsoft.com/en-us/dotnet/api/system.windows.input.key?view=netcore-3.1#fields))
- `SpawnHotkey`: hotkey used to pause/resume ped spawn in both teams ([Reference](https://docs.microsoft.com/en-us/dotnet/api/system.windows.input.key?view=netcore-3.1#fields))
- `MaxPedsPerTeam`: maximum alive peds on each team - teams with the setting MaxPeds will ignore this option (greater/equal to 0)

## Known bugs

- First spawned peds may stand still and not fight. A possible workaround is to bump into them, or kill them.

## TODO

- Support .ini config file
- Make spawnpoint blips blink (or change in any other way) when spawning is paused.
- Implement other missing configuration features...

## Changelog

- 0.0.1
  - Initial ported functional release; missing some features and support for config file
