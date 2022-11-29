// Copyright 2021 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using ArcGIS.Samples.Managers;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Location;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Navigation;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.Tasks.NetworkAnalysis;
using Esri.ArcGISRuntime.UI;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Speech.Synthesis;
using System.Threading.Tasks;
using System.Windows;

namespace ArcGIS.WPF.Samples.NavigateRouteRerouting
{
    [ArcGIS.Samples.Shared.Attributes.Sample(
        name: "Navigate route with rerouting",
        category: "Network analysis",
        description: "Navigate between two points and dynamically recalculate an alternate route when the original route is unavailable.",
        instructions: "Click 'Navigate' to simulate traveling and to receive directions from a preset starting point to a preset destination. Observe how the route is recalculated when the simulation does not follow the suggested route. Click 'Recenter' to refocus on the location display.",
        tags: new[] { "directions", "maneuver", "navigation", "route", "turn-by-turn", "voice" })]
    [ArcGIS.Samples.Shared.Attributes.OfflineData("567e14f3420d40c5a206e5c0284cf8fc")]
    public partial class NavigateRouteRerouting
    {
        // Variables for tracking the navigation route.
        private RouteTracker _tracker;
        private RouteResult _routeResult;
        private Route _route;
        private RouteTask _routeTask;
        private RouteParameters _routeParams;

        // List of driving directions for the route.
        private IReadOnlyList<DirectionManeuver> _directionsList;

        // Speech synthesizer to play voice guidance audio.
        private SpeechSynthesizer _speechSynthesizer = new SpeechSynthesizer();

        // Graphics to show progress along the route.
        private Graphic _routeAheadGraphic;
        private Graphic _routeTraveledGraphic;

        // San Diego Convention Center.
        private readonly MapPoint _conventionCenter = new MapPoint(-117.160386727066026, 32.706608204740171, SpatialReferences.Wgs84);

        // RH Fleet Aerospace Museum.
        private readonly MapPoint _aerospaceMuseum = new MapPoint(-117.146679999797271, 32.730351428946854, SpatialReferences.Wgs84);

        // Simulated route to follow.
        private readonly string _tourPolylineJSON = "{\"paths\":[[[ -117.160386727066026, 32.706608204740171 ], [ -117.160205336548898, 32.706508733108137 ], [ -117.16002394693011, 32.706409261365152 ], [ -117.159842556412997, 32.706309788755419 ], [ -117.159651667110055, 32.706216667017223 ], [ -117.159454444684357, 32.706127778166461 ], [ -117.159257222258688, 32.706038889227095 ], [ -117.159059999833019, 32.705950000199223 ], [ -117.158902856642044, 32.705835713852132 ], [ -117.158739999267553, 32.705720000231665 ], [ -117.158576363053726, 32.705604545733003 ], [ -117.158417272315205, 32.70549090901352 ], [ -117.158256250198832, 32.705376874547866 ], [ -117.158087499876075, 32.70526125028416 ], [ -117.157918750451643, 32.70514562511471 ], [ -117.1577500001289, 32.705029999795485 ], [ -117.157600000341063, 32.704940000101111 ], [ -117.157425000438906, 32.704823332883613 ], [ -117.157250000536706, 32.704706666269544 ], [ -117.15707875020253, 32.70459250003033 ], [ -117.156909999879787, 32.704480000413547 ], [ -117.156741250455326, 32.704367499899007 ], [ -117.156569643489959, 32.704254999998568 ], [ -117.156393750662403, 32.704142500712138 ], [ -117.156217857834818, 32.70403000128394 ], [ -117.156041965007248, 32.703917501713846 ], [ -117.155866072179691, 32.703805002001992 ], [ -117.155690179352078, 32.703692502148236 ], [ -117.155514286524522, 32.703580002152705 ], [ -117.155338393696923, 32.7034675020153 ], [ -117.155162499971055, 32.703355001736114 ], [ -117.154986607143471, 32.703242501315046 ], [ -117.154810714315886, 32.703130000752203 ], [ -117.154635517682649, 32.703016896826178 ], [ -117.154463104030171, 32.702901379868429 ], [ -117.154290690377692, 32.702785862761147 ], [ -117.154118275826903, 32.702670345504323 ], [ -117.153945862174425, 32.702554828097959 ], [ -117.153773448521946, 32.702439310542076 ], [ -117.153599428681744, 32.702323429238 ], [ -117.153423000458261, 32.702207001250933 ], [ -117.153246571336439, 32.702090573867849 ], [ -117.15307014311297, 32.701974145576926 ], [ -117.152893714889458, 32.701857717134175 ], [ -117.152717285767665, 32.701741289295413 ], [ -117.152540857544182, 32.701624860548854 ], [ -117.152364428422359, 32.701508432406285 ], [ -117.152188000198905, 32.701392003355934 ], [ -117.152011571077097, 32.701275574909566 ], [ -117.151835142853614, 32.701159145555422 ], [ -117.15165871463013, 32.701042716049322 ], [ -117.151482285508337, 32.70092628714734 ], [ -117.151305857284839, 32.700809857337454 ], [ -117.151133333139583, 32.700694999722828 ], [ -117.150966666009978, 32.700582500484039 ], [ -117.150799999778684, 32.700470000347529 ], [ -117.150666666434319, 32.700606666276805 ], [ -117.150550000431735, 32.700739999832187 ], [ -117.150413637069931, 32.700887727693875 ], [ -117.150277272809774, 32.701035455311022 ], [ -117.150140909447984, 32.701183182683579 ], [ -117.150004546086151, 32.70133090905572 ], [ -117.149866667266465, 32.701474445066133 ], [ -117.149727777842074, 32.701615186178799 ], [ -117.149588889315993, 32.701755927069449 ], [ -117.149449999891601, 32.701896667738154 ], [ -117.14931111046721, 32.702037408184886 ], [ -117.149171785359911, 32.702179286074646 ], [ -117.149030713927701, 32.702325715147047 ], [ -117.148889642495448, 32.702472143979179 ], [ -117.148748571063251, 32.702618572571104 ], [ -117.148607499631026, 32.702765000922703 ], [ -117.148466428198816, 32.702911428278099 ], [ -117.148324285076455, 32.703055714638857 ], [ -117.148181427793475, 32.703198572087189 ], [ -117.148038571408776, 32.703341429306768 ], [ -117.147895714125781, 32.703484287053563 ], [ -117.147752856842757, 32.703627143815801 ], [ -117.147609999559762, 32.70377000034928 ], [ -117.147775789035336, 32.703888421897091 ], [ -117.147941578510924, 32.704006842531832 ], [ -117.148107367986498, 32.704125263765263 ], [ -117.148270999708757, 32.704241999926992 ], [ -117.148426000417771, 32.704352000065626 ], [ -117.148584000601517, 32.704464000191109 ], [ -117.148754000480821, 32.704584000655537 ], [ -117.148924000360154, 32.704704000958593 ], [ -117.149094000239444, 32.704824001100256 ], [ -117.149264000118777, 32.704944000324637 ], [ -117.149407499697205, 32.705072500228539 ], [ -117.149343333036455, 32.705169047379755 ], [ -117.14912666657645, 32.705166666300379 ], [ -117.14891000011643, 32.705164285976849 ], [ -117.148693333656425, 32.705161904897288 ], [ -117.148478888730111, 32.70516111120412 ], [ -117.148273333531876, 32.705166666300379 ], [ -117.148064000019531, 32.705171999918136 ], [ -117.147848999934354, 32.705177000184506 ], [ -117.147633999849205, 32.705181999694666 ], [ -117.147418999764071, 32.705186999960468 ], [ -117.147200769337161, 32.705185384606324 ], [ -117.146977692499263, 32.705173846577395 ], [ -117.146754615661365, 32.70516230779107 ], [ -117.146708461120397, 32.705286154077442 ], [ -117.146700768846614, 32.705466923690707 ], [ -117.146693076572859, 32.705647692937767 ], [ -117.146683999995219, 32.705830000815943 ], [ -117.146673999949485, 32.706013334065815 ], [ -117.146663999903751, 32.706196666939007 ], [ -117.146664285567994, 32.70638142881964 ], [ -117.146671428072821, 32.706567143490375 ], [ -117.146678571475945, 32.706752857018657 ], [ -117.146740000071702, 32.706929999956024 ], [ -117.146814999516465, 32.707105000372742 ], [ -117.146889999859525, 32.707279999690257 ], [ -117.146924210400499, 32.70746421055955 ], [ -117.146958420941473, 32.707648421804457 ], [ -117.146992631482419, 32.707832631913085 ], [ -117.147028333226785, 32.708011666927277 ], [ -117.14706999978462, 32.708169999740647 ], [ -117.147123125252193, 32.708315313142343 ], [ -117.147188749878651, 32.708491876248765 ], [ -117.147254375403406, 32.70866843900572 ], [ -117.147320000029879, 32.708845002169127 ], [ -117.14738562465628, 32.709021564227307 ], [ -117.147451250181092, 32.709198125936041 ], [ -117.147516874807508, 32.709374687295316 ], [ -117.14752833371729, 32.709551667052253 ], [ -117.147520000046384, 32.709710000156058 ], [ -117.147520000046384, 32.709893333747829 ], [ -117.147520000046384, 32.710076666962848 ], [ -117.147520000046384, 32.71026200054947 ], [ -117.147520000046384, 32.710448667076598 ], [ -117.147520000046384, 32.710635333213204 ], [ -117.147515909118582, 32.710823182620821 ], [ -117.147509090905601, 32.71101181922716 ], [ -117.147502272692577, 32.711200455434678 ], [ -117.147495454479568, 32.711389091999187 ], [ -117.147485142718423, 32.71157742885061 ], [ -117.14746085676471, 32.711764573341057 ], [ -117.147436571709335, 32.711951716683039 ], [ -117.147412285755621, 32.712138859632439 ], [ -117.147387999801936, 32.712326002189215 ], [ -117.147363713848208, 32.712513145109241 ], [ -117.147339428792819, 32.712700286880825 ], [ -117.147316521753055, 32.712886522016262 ], [ -117.147299130369177, 32.713069131040328 ], [ -117.147281738985257, 32.71325173969052 ], [ -117.147264347601379, 32.713434348722721 ], [ -117.147246956217472, 32.713616956625302 ], [ -117.147232058556796, 32.713802941936294 ], [ -117.147218823677719, 32.713991177701793 ], [ -117.147205587900316, 32.714179413825953 ], [ -117.147192353021239, 32.71436764955282 ], [ -117.147179117243851, 32.714555884126625 ], [ -117.147165882364774, 32.714744119059084 ], [ -117.147152647485697, 32.714932352838417 ], [ -117.147153636530803, 32.715120909708922 ], [ -117.147158182006137, 32.715309546296893 ], [ -117.147162727481486, 32.715498182485916 ], [ -117.147167272956835, 32.715686818275991 ], [ -117.147173404856957, 32.715875319889243 ], [ -117.147181915495963, 32.716063619303895 ], [ -117.147190425236644, 32.716251918321021 ], [ -117.14719893587565, 32.716440216940612 ], [ -117.147207446514642, 32.716628515162668 ], [ -117.147215957153648, 32.716816812987197 ], [ -117.147224467792654, 32.717005110414192 ], [ -117.147232978431632, 32.717193407443645 ], [ -117.147241489070623, 32.717381704075564 ], [ -117.147249999709629, 32.717570000309962 ], [ -117.147249999709629, 32.717738750619532 ], [ -117.147332500087032, 32.717837500042386 ], [ -117.147538750581333, 32.717831250407393 ], [ -117.147756000047934, 32.717830000329201 ], [ -117.147976000155978, 32.717833999974772 ], [ -117.148195999365697, 32.717839000287256 ], [ -117.148399999580192, 32.717848889054309 ], [ -117.148599999596726, 32.717859999933836 ], [ -117.148792857108432, 32.717859999933836 ], [ -117.149002857170714, 32.717855714600553 ], [ -117.149221110953832, 32.717848889054309 ], [ -117.149426667050406, 32.717843333236125 ], [ -117.149629999816653, 32.717840000198528 ], [ -117.149834794141881, 32.717839725846488 ], [ -117.150058766600068, 32.717838356353404 ], [ -117.150282739058298, 32.717836986104565 ], [ -117.150506712414838, 32.717835616611495 ], [ -117.150730684873039, 32.717834246362614 ], [ -117.150954657331255, 32.717832876869451 ], [ -117.151178629789484, 32.71783150662052 ], [ -117.151402602247686, 32.717830137127372 ], [ -117.151626575604226, 32.717828766878398 ], [ -117.151850548062413, 32.717827397385207 ], [ -117.152074520520642, 32.717826027136198 ], [ -117.152298492978844, 32.717824657642915 ], [ -117.152522466335384, 32.717823287393841 ], [ -117.152746438793599, 32.717821917900579 ], [ -117.1529704112518, 32.717820547651478 ], [ -117.153186000631806, 32.717819999702947 ], [ -117.153393000321032, 32.717819999702947 ], [ -117.153598000360432, 32.717819999702947 ], [ -117.153680000376198, 32.717931250560419 ], [ -117.153680000376198, 32.718116667591957 ], [ -117.153680000376198, 32.71830208423809 ], [ -117.153680000376198, 32.718487500498668 ], [ -117.153680000376198, 32.71867291637377 ], [ -117.153680000376198, 32.718853999719613 ], [ -117.153691764713159, 32.719040589315924 ], [ -117.153706471032677, 32.719228825364496 ], [ -117.153721176453885, 32.719417061015754 ], [ -117.153735882773404, 32.719605296269705 ], [ -117.153750588194583, 32.719793531126342 ], [ -117.153765294514116, 32.719981765585658 ], [ -117.153779999935324, 32.720169999647716 ], [ -117.153724444524883, 32.720347777996373 ], [ -117.15365916664814, 32.720522916848374 ], [ -117.153554999804427, 32.720687501074849 ], [ -117.153450833859011, 32.720852084241734 ], [ -117.153346667015299, 32.721016667104912 ], [ -117.153242500171572, 32.721181250420067 ], [ -117.153129999656983, 32.721337142933685 ], [ -117.153004999983494, 32.721480000582972 ], [ -117.152880000310034, 32.721622857247624 ], [ -117.152759999761116, 32.72175500006324 ], [ -117.152636666462485, 32.721883333669965 ], [ -117.152503333118148, 32.722016666414483 ], [ -117.152369999773768, 32.722149999715342 ], [ -117.152266296460738, 32.722315741775319 ], [ -117.152162592249397, 32.722481484282923 ], [ -117.152058888936367, 32.722647225726703 ], [ -117.15195518472504, 32.722812966862485 ], [ -117.151851481411995, 32.722978708445886 ], [ -117.151747778098965, 32.723144448965456 ], [ -117.151644073887638, 32.723310189177035 ], [ -117.151540370574594, 32.723475929836226 ], [ -117.151436666363239, 32.723641669431601 ], [ -117.151332963050237, 32.723807408718947 ], [ -117.151234999971862, 32.72397250002998 ], [ -117.151160000527099, 32.724135000367234 ], [ -117.151080000161144, 32.724297500408319 ], [ -117.150996667045533, 32.724460000153258 ], [ -117.150913333929878, 32.724623889395701 ], [ -117.150829999915885, 32.724793333722438 ], [ -117.150746666800273, 32.724962778482904 ], [ -117.150663333684619, 32.725132222165605 ], [ -117.150569142632122, 32.725297714582261 ], [ -117.150467713853416, 32.725460572230027 ], [ -117.150366285972964, 32.725623430335979 ], [ -117.150264857194259, 32.7257862873888 ], [ -117.150163428415524, 32.725949144144138 ], [ -117.150062000535129, 32.726112000602008 ], [ -117.149960571756395, 32.726274857518128 ], [ -117.149810286304302, 32.726402571732081 ], [ -117.149627428838329, 32.726506857771184 ], [ -117.149444571372356, 32.726611143688253 ], [ -117.149261713906355, 32.726715429483335 ], [ -117.149078857338722, 32.726819715156495 ], [ -117.148895999872721, 32.726924000707605 ], [ -117.148713142406748, 32.727028286136743 ], [ -117.148527500163411, 32.727107499935954 ], [ -117.148341999853869, 32.727179999922392 ], [ -117.148156999905936, 32.727254999741994 ], [ -117.147970000308177, 32.727333332937185 ], [ -117.147782499450514, 32.727412499611347 ], [ -117.147604782144612, 32.727511304068443 ], [ -117.147441738818856, 32.727639565155108 ], [ -117.1472786954931, 32.727767826057253 ], [ -117.147115652167315, 32.727896086774876 ], [ -117.146952608841573, 32.728024347307972 ], [ -117.14683238101891, 32.728179524297857 ], [ -117.146722856622844, 32.728341429031481 ], [ -117.14661333312506, 32.728503334226737 ], [ -117.146503809627319, 32.728665238372294 ], [ -117.146433750018303, 32.728838750495896 ], [ -117.146389999369021, 32.729020001365825 ], [ -117.146346249618063, 32.72920125111154 ], [ -117.146302499867119, 32.729382501244444 ], [ -117.146258749217793, 32.729563750253142 ], [ -117.146400000313093, 32.729589999948736 ], [ -117.146600000329613, 32.729668571401881 ], [ -117.146679999797271, 32.729808571287677 ], [ -117.146679999797271, 32.729989523956149 ], [ -117.146679999797271, 32.730170477013012 ], [ -117.146679999797271, 32.730351428946854 ]]],\"spatialReference\":{\"wkid\":4326,\"latestWkid\":4326}}";

        // Path to the network geodatabase for San Diego.
        private readonly string _networkGeodatabasePath = DataManager.GetDataFolder("567e14f3420d40c5a206e5c0284cf8fc", "sandiego.geodatabase");

        public NavigateRouteRerouting()
        {
            InitializeComponent();
            _ = Initialize();
        }

        private async Task Initialize()
        {
            try
            {
                // Add event handler for when this sample is unloaded.
                Unloaded += SampleUnloaded;

                // Create the map view.
                MyMapView.Map = new Map(BasemapStyle.ArcGISNavigation);

                // Create the route task, using the routing service.
                _routeTask = await RouteTask.CreateAsync(_networkGeodatabasePath, "Streets_ND");

                // Get the default route parameters.
                _routeParams = await _routeTask.CreateDefaultParametersAsync();

                // Explicitly set values for parameters.
                _routeParams.ReturnDirections = true;
                _routeParams.ReturnStops = true;
                _routeParams.ReturnRoutes = true;
                _routeParams.OutputSpatialReference = SpatialReferences.Wgs84;

                // Create stops for each location.
                Stop stop1 = new Stop(_conventionCenter) { Name = "San Diego Convention Center" };
                Stop stop2 = new Stop(_aerospaceMuseum) { Name = "RH Fleet Aerospace Museum" };

                // Assign the stops to the route parameters.
                List<Stop> stopPoints = new List<Stop> { stop1, stop2 };
                _routeParams.SetStops(stopPoints);

                // Get the route results.
                _routeResult = await _routeTask.SolveRouteAsync(_routeParams);
                _route = _routeResult.Routes[0];

                // Add a graphics overlay for the route graphics.
                MyMapView.GraphicsOverlays.Add(new GraphicsOverlay());

                // Add graphics for the stops.
                SimpleMarkerSymbol stopSymbol = new SimpleMarkerSymbol(SimpleMarkerSymbolStyle.Diamond, Color.OrangeRed, 20);
                MyMapView.GraphicsOverlays[0].Graphics.Add(new Graphic(_conventionCenter, stopSymbol));
                MyMapView.GraphicsOverlays[0].Graphics.Add(new Graphic(_aerospaceMuseum, stopSymbol));

                // Create a graphic (with a dashed line symbol) to represent the route.
                _routeAheadGraphic = new Graphic(_route.RouteGeometry) { Symbol = new SimpleLineSymbol(SimpleLineSymbolStyle.Solid, Color.BlueViolet, 5) };

                // Create a graphic (solid) to represent the route that's been traveled (initially empty).
                _routeTraveledGraphic = new Graphic { Symbol = new SimpleLineSymbol(SimpleLineSymbolStyle.Solid, Color.LightBlue, 3) };

                // Add the route graphics to the map view.
                MyMapView.GraphicsOverlays[0].Graphics.Add(_routeAheadGraphic);
                MyMapView.GraphicsOverlays[0].Graphics.Add(_routeTraveledGraphic);

                // Set the map viewpoint to show the entire route.
                await MyMapView.SetViewpointGeometryAsync(_route.RouteGeometry, 100);

                // Enable the navigation button.
                StartNavigationButton.IsEnabled = true;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "Error");
            }
        }

        private async void StartNavigation(object sender, RoutedEventArgs e)
        {
            try
            {
                // Disable the start navigation button.
                StartNavigationButton.IsEnabled = false;

                // Get the directions for the route.
                _directionsList = _route.DirectionManeuvers;

                // Create a route tracker.
                _tracker = new RouteTracker(_routeResult, 0, true);
                _tracker.NewVoiceGuidance += SpeakDirection;

                // Handle route tracking status changes.
                _tracker.TrackingStatusChanged += TrackingStatusUpdated;

                // Check if this route task supports rerouting.
                if (_routeTask.RouteTaskInfo.SupportsRerouting)
                {
                    // Enable automatic re-routing.
                    await _tracker.EnableReroutingAsync(new ReroutingParameters(_routeTask, _routeParams) { Strategy = ReroutingStrategy.ToNextWaypoint, VisitFirstStopOnStart = false });

                    // Handle re-routing completion to display updated route graphic and report new status.
                    _tracker.RerouteStarted += RerouteStarted;
                    _tracker.RerouteCompleted += RerouteCompleted;
                }

                // Turn on navigation mode for the map view.
                MyMapView.LocationDisplay.AutoPanMode = LocationDisplayAutoPanMode.Navigation;
                MyMapView.LocationDisplay.AutoPanModeChanged += AutoPanModeChanged;

                // Add a data source for the location display.
                var simulatedSource = new SimulatedLocationDataSource();
                simulatedSource.LocationChanged += SimulatedLocationChanged;

                // Create SimulationParameters starting at the current time, velocity, and horizontal and vertical accuracy.
                SimulationParameters simulationParameters = new SimulationParameters(DateTime.Now, 40.0);
                simulatedSource.SetLocationsWithPolyline(Geometry.FromJson(_tourPolylineJSON) as Polyline, simulationParameters);

                // Set up the location display.
                MyMapView.LocationDisplay.DataSource = new RouteTrackerLocationDataSource(_tracker, simulatedSource);

                // Use this instead if you want real location:
                // MyMapView.LocationDisplay.DataSource = new RouteTrackerLocationDataSource(_tracker, new SystemLocationDataSource());

                // Enable the location display (this will start the location data source).
                MyMapView.LocationDisplay.IsEnabled = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error");
            }
        }

        private void SimulatedLocationChanged(object sender, Location e)
        {
            if (sender is SimulatedLocationDataSource simSource && simSource.CurrentLocationIndex + 1 == simSource.Locations.Count)
            {
                // Prevent simulated source from looping.
                simSource.StopAsync();
            }
        }

        private void RerouteStarted(object sender, EventArgs e)
        {
            // Remove the event listeners for tracking status changes while the route tracker recalculates.
            _tracker.NewVoiceGuidance -= SpeakDirection;
            _tracker.TrackingStatusChanged -= TrackingStatusUpdated;
        }

        private void RerouteCompleted(object sender, RouteTrackerRerouteCompletedEventArgs e)
        {
            // Get the new directions.
            _directionsList = e.TrackingStatus.RouteResult.Routes[0].DirectionManeuvers;

            // Re-add the event listeners for tracking status changes.
            _tracker.NewVoiceGuidance += SpeakDirection;
            _tracker.TrackingStatusChanged += TrackingStatusUpdated;
        }

        private void TrackingStatusUpdated(object sender, RouteTrackerTrackingStatusChangedEventArgs e)
        {
            TrackingStatus status = e.TrackingStatus;

            // Start building a status message for the UI.
            System.Text.StringBuilder statusMessageBuilder = new System.Text.StringBuilder("Route Status:\n");

            // Check if navigation is on route.
            if (status.IsOnRoute && !status.IsRouteCalculating)
            {
                // Check the destination status.
                if (status.DestinationStatus == DestinationStatus.NotReached || status.DestinationStatus == DestinationStatus.Approaching)
                {
                    statusMessageBuilder.AppendLine("Distance remaining: " +
                                                status.RouteProgress.RemainingDistance.DisplayText + " " +
                                                status.RouteProgress.RemainingDistance.DisplayTextUnits.PluralDisplayName);

                    statusMessageBuilder.AppendLine("Time remaining: " +
                                                    status.RouteProgress.RemainingTime.ToString(@"hh\:mm\:ss"));

                    if (status.CurrentManeuverIndex + 1 < _directionsList.Count)
                    {
                        statusMessageBuilder.AppendLine("Next direction: " + _directionsList[status.CurrentManeuverIndex + 1].DirectionText);
                    }

                    // Set geometries for progress and the remaining route.
                    _routeAheadGraphic.Geometry = status.RouteProgress.RemainingGeometry;
                    _routeTraveledGraphic.Geometry = status.RouteProgress.TraversedGeometry;
                }
                else if (status.DestinationStatus == DestinationStatus.Reached)
                {
                    statusMessageBuilder.AppendLine("Destination reached.");

                    // Set the route geometries to reflect the completed route.
                    _routeAheadGraphic.Geometry = null;
                    _routeTraveledGraphic.Geometry = status.RouteResult.Routes[0].RouteGeometry;
                }
            }
            else
            {
                statusMessageBuilder.AppendLine("Off route!");
            }

            Dispatcher.BeginInvoke((Action)delegate ()
            {
                // Show the status information in the UI.
                MessagesTextBlock.Text = statusMessageBuilder.ToString();
            });
        }

        private void SpeakDirection(object sender, RouteTrackerNewVoiceGuidanceEventArgs e)
        {
            // Say the direction using voice synthesis.
            _speechSynthesizer.SpeakAsyncCancelAll();
            _speechSynthesizer.SpeakAsync(e.VoiceGuidance.Text);
        }

        private void AutoPanModeChanged(object sender, LocationDisplayAutoPanMode e)
        {
            // Turn the recenter button on or off when the location display changes to or from navigation mode.
            RecenterButton.IsEnabled = e != LocationDisplayAutoPanMode.Navigation;
        }

        private void RecenterButton_Click(object sender, RoutedEventArgs e)
        {
            // Change the mapview to use navigation mode.
            MyMapView.LocationDisplay.AutoPanMode = LocationDisplayAutoPanMode.Navigation;
        }

        private void SampleUnloaded(object sender, RoutedEventArgs e)
        {
            // Stop the speech synthesizer.
            _speechSynthesizer.SpeakAsyncCancelAll();
            _speechSynthesizer.Dispose();

            // Stop the tracker.
            if (_tracker != null)
            {
                _tracker.TrackingStatusChanged -= TrackingStatusUpdated;
                _tracker.NewVoiceGuidance -= SpeakDirection;
                _tracker.RerouteStarted -= RerouteStarted;
                _tracker.RerouteCompleted -= RerouteCompleted;
                _tracker = null;
            }

            // Stop the location data source.
            MyMapView.LocationDisplay?.DataSource?.StopAsync();
        }
    }
}