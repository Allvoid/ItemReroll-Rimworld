# Ar: Item Reroll

RimWorld 1.6 and 1.7 mod. Adds a **Reroll** button to items in development mode. It re-rolls an item's random properties
within the same range the game uses when it creates the item.

[Steam Workshop](https://steamcommunity.com/sharedfiles/filedetails/?id=3824069594)

## What it re-rolls

Chosen in the mod settings:

- weapon traits of unique weapons (Odyssey) and persona weapons (Royalty) — on by default;
- name and color of unique weapons — on by default;
- quality of unique weapons;
- material;
- art description.

Relics keep their weapon type, material, Legendary quality and name; only their traits change.

## Notes

- The button appears only in development mode, on items lying on the map. An equipped weapon has no button.
- No Harmony. The mod adds a component to item definitions at startup and stores nothing in the save, so it can be
  removed at any time.

## Building

`build.cmd` builds `ItemReroll.dll` twice and copies the mod into the RimWorld `Mods` folders (the Steam folder and,
if it exists, the 1.6 copy `W:\SteamLibrary\RimWorld-1.6\Mods`):

- `ItemReroll\1.6\Assemblies` for RimWorld 1.6: MSBuild (Visual Studio 2022), .NET Framework 4.7.2,
  `Source\ItemReroll\ItemReroll.csproj` (`RimWorldManaged`: the 1.6 copy of the game);
- `ItemReroll\1.7\Assemblies` for RimWorld 1.7 (the Waylith engine): `dotnet build`, .NET 10,
  `Source\ItemReroll.1.7\ItemReroll.1.7.csproj` (the same sources; `RimWorld17`: the game's Steam folder).

`LoadFolders.xml` makes each game load its own dll.

## Credits

Dice icon: Board Game Icons by [Kenney](https://kenney.nl), CC0.
