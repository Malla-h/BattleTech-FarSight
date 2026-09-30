# FarSight: longer shrub and tree draw distance for BattleTech

BattleTech draws its shrubs and trees with its own system (`BattleTech.Rendering.Trees.RenderTrees`), and stops drawing them beyond a
fixed distance. When you zoom the camera out, shrubs vanish entirely and trees switch to flat imposters early. FarSight scales that
distance so the vegetation stays visible further out.

It changes one number, `RenderTrees.distanceRange`, and only as a **multiplier of the game's own value** (1.0 = vanilla). It does not
touch game files.

## Install
Copy the `FarSight` folder (`FarSight.dll` and `mod.json`) into `BATTLETECH\Mods\`. It loads through ModTek or the game's mod loader
(a plain DLL entry point).

## Use
Press **Ctrl+F11** to open the FarSight window: an FPS readout, an **Enabled** toggle (off = vanilla), the range slider (x1 to x4), **Save**
and **Close**. Save writes `FarSight.user.json` in the mod folder, which overrides `mod.json` on the next launch. Delete it to reset.

## Settings (`mod.json`)
| Setting | Meaning | Default |
|---|---|---|
| `enabled` | Apply the multiplier (false = vanilla) | true |
| `treeRangeMult` | Multiplier of the game's own draw distance | 1.69 |
| `dumpInfo` | Log the vanilla value FarSight found to `FarSight.log` | true |
| `toggleKey` | Window hotkey, a Unity key name with optional modifiers, e.g. `Ctrl+F11` | Ctrl+F11 |

## Notes
- Frame rate cost grows with the multiplier, since more vegetation is drawn. Pick a value that suits your hardware.
- It has no effect on urban maps: the game's tree renderer returns early there.
- The game's terrain `treeDistance`, `QualitySettings.lodBias` and detail distance were tried and are not what limits this vegetation, so
  FarSight leaves them alone.

## Building
`dotnet build -c Release` (references the game's `Managed` folder; adjust `BTManaged` in `FarSight.csproj` if the game is elsewhere).
The output is `bin\Release\net472\FarSight.dll`.
