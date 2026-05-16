# Blish HUD API Notes

Quirks and non-obvious behaviors discovered during development. Not domain terms (those go in CONTEXT.md) — these are implementation gotchas.

## TabbedWindow2

### DoBuild receives the window, not a Panel

The `Container` passed to `IView.DoBuild(Container)` is the `TabbedWindow2` instance itself, **not** a child `Panel`. You must create your own `Panel` as a child:

```csharp
public void DoBuild(Container buildPanel)
{
    var panel = new Panel
    {
        Parent = buildPanel,
        Size = buildPanel.Size,
    };
    // Build controls on panel, not buildPanel
}
```

Casting to `Panel` throws `InvalidCastException`.

### Content region is clip-only, not a sub-container

The `contentRegion` parameter (`new Rectangle(82, 30, 467, 600)`) only sets a scissor/clip rectangle. The container passed to `DoBuild` is still at `(0, 0)` with the full window size (e.g., `545 × 670`). Controls positioned at `x < 82` or `y < 30` are **clipped away** and invisible.

Use starting offsets of roughly `x ≥ 95`, `y ≥ 40` for content to appear in the visible area.

### Tab icon must be non-null

`new Tab(null, ...)` causes a `NullReferenceException` crash in `Tab.Draw()`. Provide at least a solid-color placeholder via `new AsyncTexture2D(texture2D)`.

## AsyncTexture2D

### Loading GW2 dat textures

Use `AsyncTexture2D.FromAssetId(int)` to load textures from GW2's dat asset cache by their asset ID:

```csharp
var bg = Blish_HUD.Content.AsyncTexture2D.FromAssetId(155997);
```

`Content.GetTexture("controls/window/155997")` as shown in some docs does not compile — there is no static `Content` class with that method.

## Dropdown

### ValueChanged doesn't fire for programmatic selection during detached handler

When you detach the `ValueChanged` handler, set `SelectedItem`, then reattach — the old `SelectedItem` assignment **does not retroactively fire**. This is relevant for initial population flows:

```csharp
_deviceDropdown.ValueChanged -= handler;
// populate items...
_deviceDropdown.SelectedItem = someItem; // handler NOT fired here
_deviceDropdown.ValueChanged += handler;
// Must manually call handler for the auto-selected item:
handler(null, EventArgs.Empty);
```

## IView

### DoLoad returns Task\<bool\>

```csharp
public Task<bool> DoLoad(IProgress<string> progress)
{
    progress.Report("Loading...");
    return Task.FromResult(true);
}
```

Not `void`. The `DoBuild` returns `void`.

## DefineSetting (1.3.0+)

The non-obsolete overload uses `Func<string>` lambdas instead of raw strings:

```csharp
// Deprecated:
settings.DefineSetting("key", defaultValue, "Display Name", "Description");

// Current:
settings.DefineSetting("key", defaultValue, () => "Display Name", () => "Description");
```
