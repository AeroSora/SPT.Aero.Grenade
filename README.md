# Aero.Grenade

Faster, more deliberate grenade handling for SPT: hold one key to draw, pull the pin and ready a grenade in a
single motion, tap it to pick a grenade from a menu, and cook grenades before you throw them.

## Requirements

- SPT 4.1.x (developed and tested on 4.1.6; client only, nothing is installed on the server)
- Optional: Fika (tested with Fika.Core 2.4.4) for cook sync in multiplayer

## Installation

Copy the `BepInEx` folder from the release archive into your SPT folder, so that the mod ends up in:

```
BepInEx/plugins/Aero.Grenade/Aero.Grenade.dll
```

If you play with Fika, also copy the `BepInEx` folder inside `Fika Compat Patch` into your SPT folder. It adds the
Fika sync next to the mod:

```
BepInEx/plugins/Aero.Grenade/Aero.Grenade.Fika.dll
```

Without Fika, leave the patch out.

## Multiplayer (Fika)

Cook sync needs the mod, including `Aero.Grenade.Fika.dll`, on the host (or the headless client) as well as on the
throwing player's game.

## License

MIT, see [LICENSE](LICENSE).
