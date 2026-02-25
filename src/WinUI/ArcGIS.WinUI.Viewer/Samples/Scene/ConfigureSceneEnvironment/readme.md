# Configure scene environment

Configure the environment settings in a local scene to change the lighting conditions and background appearance.

![Image of Configure Scene Environment](ConfigureSceneEnvironment.jpg)

## Use case

The scene environment defines the appearance of the local scene. This includes sky and background color settings, whether objects in the local scene cast shadows, and if virtual lighting or simulated sunlight is used to light the scene.

For an exploration scenario, like a city planning app, a developer might decide to enable sun lighting and direct shadows so the user can see how buildings and trees cast shadows at a given date and time.

For an analytic scenario, like examining a building scene layer, the developer may choose to use a virtual light source with shadows disabled and a solid background color instead of the atmosphere.

## How to use the sample

At start-up, you will see a local scene with a set of scene environment controls. Adjusting the controls will change the scene's environment altering the presentation of the scene. Toggle the "Stars" and "Atmosphere" check boxes to enable or disable those features. Select a color from the dropdown to set a solid background color; selecting a new background color will disable the stars and atmosphere so you can see the new color. Switch between "Sun" and "Virtual" lighting, toggle "Direct Shadows", and adjust the hour slider to change the sun position.

## How it works

1. Create a `Scene` from an online resource and add it to the `LocalSceneView`. The sample's controls are updated to reflect the web scene's initial environment.
2. Changes to the sky and background color settings will set values directly on the `SceneEnvironment` object.
    * `IsAtmosphereEnabled` and `AreStarsEnabled` are boolean properties dictating whether the atmosphere and star field are visible.
    * Colors selected from the dropdown are set to the `BackgroundColor`.
3. Changes to the settings in the lighting controls manipulate the `SceneLighting` object in the `SceneEnvironment.Lighting` property.
    * Switching between "Sun" and "Virtual" lighting assigns a new `SunLighting` or `VirtualLighting` object to the lighting property.
        * Sun lighting simulates the position of the sun based on a given date and time. This includes lighting conditions for day, twilight, and night.
        * For virtual lighting, the light source is always on and is slightly offset from the camera. As the scene rotated or panned, the light source stays in the same position relative to the camera.
    * The "Direct Shadows" button sets the `AreDirectShadowsEnabled` boolean property on the lighting object. This toggles the shadows cast by objects in the scene.
    * If `SunLighting` is active, manipulating the slider changes the hour of the `SimulatedDate` property on the lighting object. `VirtualLighting` does not have a slider because the lighting is always the same regardless of time.

## Relevant API

* LocalSceneView
* Scene
* SceneEnvironment
* SceneLighting
* SunLighting
* VirtualLighting

## About the data

The [WebM Open 3D Esri Local Scene](https://www.arcgis.com/home/item.html?id=fcebd77958634ac3874bbc0e6b0677a4) used for this sample contains a clipped local scene consisting of a surface and 3D objects representing the buildings. The scene is located in Santa Fe, New Mexico, USA.

## Tags

3D, environment, lighting, scene
