Hello!

This implementation of infinite depth rendering, kinda works.
Basic idea:
2 cameras
- camera in back of scene
- player's camera in front
- back camera follows position/rotation of front player camera with some offset

render back camera to texture
project that texture onto a portal plane in the front -from the perspective of the player camera.
let the magic of a hall of mirrors - effect take over.

the problems are
1. the plane the texture is projected onto has physical location, meaning the projection of the texture gets very small when the player camera gets close, like a flashlight close to a wall.
- Solution(KINDA) fake moving the projection plane forward and back in relation to the camera to minimize destortion (search for y=mx+b in the infinite view script to adjust)
- better yet, don't let player  get close to the view plane by teleporting early or somthin

2. sides clip off in farther away areas with sharp viewing angles
- SOLUTION: can be fixed by 1: making the fov of the back camera large, 2: making the render texture larger to compensate, 3: make the perspective projection matrix for the front projection use the fov of the back camera (see docs for Matrix4x4.perspective() but Pass each param of back camera, but position of player camera))

3. the wiggles!!
- IDK. DON'T USE THIS, or just have infinite frame rate!