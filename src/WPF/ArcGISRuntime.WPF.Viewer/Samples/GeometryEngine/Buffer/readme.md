# Buffer

This sample demonstrates how to use GeometryEngine to create planar and geodesic buffer polygons from a map location and buffer distance. It illustrates the difference between planar and geodesic results both visually on the map and by comparing their areas.

<img src="Buffer.jpg" width="350"/>

## Instructions

1. Tap on the map.    
2. A planar and a geodesic buffer will be created at the tap location using the distance (miles) specified in the text box.    
3. Continue tapping to create additional buffers. Notice that buffers closer to the equator are similar in size. As you move north or south from the equator, however, the geodesic polygons appear larger. Geodesic polygons are in fact a better representation of the true shape and size of the buffer. While the planar polygons appear consistent across the map, the area they represent is less accurate as you move towards the poles.    
4. The total area of planar and geodesic buffer polygons will update in the text boxes along with the percent difference between them.    
5. Click `Clear` to remove all buffers and start again.
