# Item Reroll

RimWorld 1.6 mod. Adds a **Reroll** button to items in development mode. It re-rolls an item's random properties
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

`build.cmd` builds `ItemReroll\Assemblies\ItemReroll.dll` with MSBuild (Visual Studio 2022) and copies the mod into
the RimWorld `Mods` folder. Set the paths at the top of `build.cmd` and `RimWorldManaged` in
`Source\ItemReroll\ItemReroll.csproj` to your RimWorld install.

## Credits

Dice icon: Board Game Icons by [Kenney](https://kenney.nl), CC0.
