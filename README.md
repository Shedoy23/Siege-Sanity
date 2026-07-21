# Siege Sanity

Sieges that actually finish — plus fixes for two vanilla siege bugs.
Every patch can be toggled in MCM, so the mod can sit alongside other siege mods.

## What it does

| | |
|---|---|
| **Stubborn assault** (behaviour, **on** by default — the point of the mod) | Vanilla scales the retreat threshold by assault progress, so attackers who cannot get up the walls pull back at ~40% losses, regroup, and repeat for in-game weeks. Here the threshold is a flat low value: the assault runs to a conclusion. Attackers still break off when genuinely destroyed (default 0.25 = after losing ~75%); configurable. **This changes AI behaviour rather than fixing a bug** — it is stated first in the Workshop description and can be switched off. |
| **Army wipe on siege retreat** (fix, on by default) | Vanilla treats a failed assault as a total capture: the whole besieging army is taken prisoner or killed and its lords respawn with a single troop. With the fix, survivors retreat instead. A genuine wipe — no healthy survivors — is still a wipe. |
| **Siege-start crash** (guard, on by default) | A vanilla `NullReferenceException` in `MobileParty.OnPartyJoinedSiegeInternal` during the campaign tick hard-crashes the game. Caught so the game keeps running. **Trade-off:** that party does not join that siege. This hides the symptom rather than curing the cause — see the source comment. |

## Compatibility

Patches exactly three engine methods:

- `MapEvent.CalculateAndCommitMapEventResults`
- `MobileParty.OnPartyJoinedSiegeInternal`
- `TacticBreachWalls.ShouldRetreat`

No overlap with **SiegeFix** (which patches `PlayerEncounter.CheckIfBattleShouldContinueAfterBattleMission`
and `Settlement.OnPartyInteraction`). If something does conflict in your setup,
turn off the individual patch in MCM instead of uninstalling.

Patches use public API where possible and resolve their targets defensively — if
a game update renames a method, that patch is skipped instead of crashing.

Requirements: Bannerlord v1.3.15, Harmony, ButterLib, MCM (MBOptionScreen).

## Building

```
dotnet build src/SiegeSanity.csproj -c Release
```

Output goes to `bin/Win64_Shipping_Client/`. Copy the DLL into
`<Bannerlord>/Modules/Shedoy23.SiegeSanity/bin/Win64_Shipping_Client/` and
restart the game — there is no hot reload.

## Publishing to Steam Workshop

Two gotchas cost hours once; both are already handled in the XML files here, so
do not "tidy them up":

1. **No `<?xml ... ?>` declaration and no comment before `<Tasks>`.** The uploader
   reads `xmlDocument.FirstChild` and iterates its children. If the first node is
   the declaration (or a comment), the task list comes out empty and the tool
   prints `Starting... Finished...` having uploaded *nothing*, with no error.
2. **`ModuleFolder` must be an absolute path** — internally it calls
   `ModuleInfo.LoadWithFullPath`.

First publish (creates the item, **Private** so you can review it first):

```
cd /d "X:\SteamLibrary\steamapps\common\Mount & Blade II Bannerlord\bin\Win64_Shipping_Client"
TaleWorlds.MountAndBlade.SteamWorkshop.exe "X:\SteamLibrary\steamapps\common\Mount & Blade II Bannerlord\Modules\Shedoy23.SiegeSanity\workshop_publish.xml"
```

Success prints `Item created. Item ID is …` and `Uploading done!`. Silence
between `Starting...` and `Finished...` means nothing was uploaded.

Then: put the printed ID into `workshop_update.xml`, add a preview image on the
Workshop page, and flip visibility to Public when you are happy with it.

**Always verify on the Workshop page** that the update date changed. The tool
reports success far too easily.

## Credits

The siege-retreat idea comes from Bannerlord Legacy Tweaks; the implementation
here is independent.

## License

MIT — see `LICENSE.txt`.
