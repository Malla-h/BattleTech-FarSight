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

**About the default (x1.69):** I set the default to x1.69 because that is what I found gave the most bang for your buck: a clearly longer view of
the vegetation for a modest frame-rate cost. It's only my preference, so feel free to change it in the window to suit your hardware.

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

## Related projects
[BattleTech-DLSS](https://github.com/Malla-h/BattleTech-DLSS) is a separate mod that renders the combat view at a lower resolution and upscales it with
NVIDIA DLSS. The two mods work together.

## License
Released under the [MIT License](LICENSE).

## Building
`dotnet build -c Release`. It references the game's `Managed` folder. Point it at your game with the `BATTLETECH_DIR` environment variable,
`-p:BTRoot=...`, or a git-ignored `local.props` file (see the comment in `FarSight.csproj`); without any of these it looks in the default
Steam location. The output is `bin\Release\net472\FarSight.dll`.
