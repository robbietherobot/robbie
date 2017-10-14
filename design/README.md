# Robbie the Sitecore robot - design files #

These are the design files and the part listing of the hardware that we've used to build Robbie! This way, it's very easy to build your own, but of course, you can give it any appearance you like.

* The 'robbie-frame-laser.dwg' file is a laser cut file for 3 mm multiplex. You can very easily assemble the feet and pan tilt mechanism this way. 
* The 'robbie-head-*.stl' files are 3D printed via Shapeways, using the Strong & Flexible White plastic material. Some minor sanding or filing may be required to fit your hardware, especially for the eyes and camera, depending on your soldering connections. Lastly, we have just painted the eyes black with acrylic paint. If you use other 3D printing techniques, you could choose to print in multiple colours too.

About the hardware used:
* The speaker cones are removed from the casing. Keeping all the electronics in tact. There is a very thin metal square plate around the cone, which you can easily cut of using either metal cutters or a pair of scissors.
* The webcam is removed from it's casing. You can do that by loosening two tiny screws, then the feet, and lastly the back, which might require cutting the casing.
* The fact that we chose analog servos is quite important, because digital servos couldn't cope with the heavy leverage of the geometry of the head (digital servos give feedback about their position, and when they go slightly of their intended position, they correct by a small movement back, creating an oscillating head with this geometry, even when using very strong servos (20kg i.e.)). To get a more steady movement, we should redesign the pan tilt mechanism and refrain from direct drive on the servo axle.
* If you use other hardware, like another webcam, other speakers, other servos etc. You probably need to alter the design files accordingly.

Any questions on the hardware? Send Rob a message via Twitter [@rhabraken](https://twitter.com/rhabraken)