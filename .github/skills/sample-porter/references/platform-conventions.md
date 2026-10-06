# .NET sample conventions

Read the shared conventions for every port and the UI sections relevant to the
sample. Confirm API details against the SDK version being targeted.

## Layout and discovery

| Platform | Sample directory | Namespace |
| --- | --- | --- |
| WPF | `src\WPF\WPF.Viewer\Samples\<Category>\<FormalName>` | `ArcGIS.WPF.Samples.<FormalName>` |
| WinUI | `src\WinUI\ArcGIS.WinUI.Viewer\Samples\<Category>\<FormalName>` | `ArcGIS.WinUI.Samples.<FormalName>` |
| MAUI | `src\MAUI\Maui.Samples\Samples\<Category>\<FormalName>` | `ArcGIS.Samples.<FormalName>` |

Use the same PascalCase formal name for the directory, class, XAML, and
code-behind. Samples are discovered through `[Sample]` by
`src\Samples.CatalogGenerator`; do not edit generated catalogs or add per-sample
project entries.

Use named `[Sample]` arguments: `name:`, `category:`, `description:`,
`instructions:`, and `tags:` when present. Metadata processing regenerates this
attribute from the README, so do not edit descriptions or tags only in code.

Keep business logic and SDK operation order equivalent across platforms.
Differences should have a concrete platform reason, not arise from three
independent implementations.

## README and screenshots

Edit the canonical .NET README in the WPF sample folder only. Synchronization
copies it to WinUI and MAUI, adapts platform links and mobile interaction wording,
and lowercases the MAUI image filename.

The metadata parser requires the first three blank-line-separated blocks to be:

```markdown
# Canonical title

Canonical one-line description.

![Description of the sample](FormalName.jpg)
```

Preserve the source README's wording, sections, step count, and nested workflow
structure as closely as the .NET port permits. Do not invent an "Additional
information" or "About the data" section. Translate API names and source-specific
UI wording, remove source-language code blocks, and explain any genuine workflow
change. Do not automatically replace "tap" with "click".

Use verified .NET identifiers: for example, `MapPoint` rather than a source
language's `Point`, and `Async` only when that .NET method actually has the suffix.
Translate the Relevant API list as well as the numbered workflow.

The screenshot filename, README image reference, and metadata `images` entry
must agree. Preserve the generator's `<FormalName>.jpg` for WPF/WinUI and
`<formalname>.jpg` for MAUI. The placeholders are intentionally left for a human
to replace; never substitute source-design screenshots or claim they show the
finished sample.

Review semantics, not just matching strings: the documented initial state,
controls, layer sources, renderer/filter types, constants, update/removal logic,
and reset behavior must match the code on all platforms.

## Geo-view selection and XAML

| Platform | XAML root | ArcGIS XML namespace |
| --- | --- | --- |
| WPF | `UserControl` | `xmlns:esri="http://schemas.esri.com/arcgis/runtime/2013"` |
| WinUI | `UserControl` | `xmlns:esriUI="using:Esri.ArcGISRuntime.UI.Controls"` |
| MAUI | `ContentPage` | `xmlns:esriUI="clr-namespace:Esri.ArcGISRuntime.Maui;assembly=Esri.ArcGISRuntime.Maui"` |

MAUI uses `xmlns="http://schemas.microsoft.com/dotnet/2021/maui"`.

Choose `MapView` for 2D, `SceneView` for global 3D, and `LocalSceneView` for local
3D where supported by the target SDK. Preserve both the source's view type and
scene mode: `new Scene(SceneViewingMode.Local, ...)` alone does not turn a
`SceneView` into a `LocalSceneView`. Verify required layer support as well.

If defaults render but custom renderers, filters, or runtime updates do not,
check the geo-view and scene-mode pairing before blaming event handlers.

## Controls and layout

WPF and WinUI control panels normally use
`<Border Style="{StaticResource BorderStyle}">`. The shared style supplies a
375-wide panel; override it only when content requires a different size.

MAUI uses these shared styles:

```xml
<Grid Style="{DynamicResource EsriSampleContainer}">
    <esriUI:MapView x:Name="MyMapView"
                   Style="{DynamicResource EsriSampleGeoView}" />
    <Border Style="{DynamicResource EsriSampleControlPanel}">
        <!-- Sample controls -->
    </Border>
</Grid>
```

Use the chosen geo-view instead of `MapView` for scene samples. Wrap unusually
tall MAUI controls in a `ScrollView`; give it a finite height constraint when the
surrounding layout does not. Nearby samples use `MaximumHeightRequest="500"`.
Do not add scrolling or loading controls when they are unnecessary to the lesson.

WinUI panels such as `Grid` and `StackPanel` do not have `IsEnabled`; enable or
disable their individual controls. Invalid panel properties can appear as
generic XAML compiler failures.

For indeterminate loading, use a WPF/WinUI `ProgressBar` with
`IsIndeterminate="True"` and `Visibility="Collapsed"` at rest. MAUI uses an
`ActivityIndicator` with `IsRunning="True"` and toggled `IsVisible`, not an
indeterminate `ProgressBar`.

## Events, dialogs, and colors

| Concern | WPF | WinUI | MAUI |
| --- | --- | --- | --- |
| Radio selection event | `Checked` | `Checked` | `CheckedChanged` |
| Radio event args | `RoutedEventArgs` | `RoutedEventArgs` | `CheckedChangedEventArgs` |
| Radio checked state | `bool?` | `bool?` | `bool` |
| Error dialog | `MessageBox.Show(message, title)` | `await new MessageDialog2(message, title).ShowAsync()` | `await Application.Current.Windows[0].Page.DisplayAlertAsync(title, message, "OK")` |

Use `IsChecked == true` on WPF/WinUI. MAUI raises the event for both selection
and deselection; perform selection-only work after checking `e.Value`.
Initialization in XAML can raise events during `InitializeComponent()`, before
SDK fields assigned later are ready. Guard handlers accordingly.

Use a `TextBlock` child for wrapping WPF/WinUI radio labels. Set MAUI
`RadioButton.Content` to text rather than a `Label` object, which can render as a
type name on Android. Use `GroupName` when grouping controls across parents.

Use `async void` only for event handlers that need to await work; keep other
asynchronous operations task-returning. Prevent overlapping WinUI error dialogs
when repeated events can fire: `MessageDialog2` rejects a second open dialog.
Follow the current sample's lifecycle and cleanup patterns for event subscriptions
and long-running operations.

In MAUI, qualify `System.Drawing.Color` instead of importing `System.Drawing`;
MAUI's global usings also introduce `Microsoft.Maui.Graphics.Color`.
