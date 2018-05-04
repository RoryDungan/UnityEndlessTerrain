# Unity Endless Terrain

Terrain displacement shader using perlin noise.

Based on code I wrote for a game jam that generated terrain meshes procedurally on the CPU. Procedurally building geometry in Unity can be quite slow so I ported the code to HLSL. It can run either as a vertex displacement shader (using the "Terrain" shader), or output to a render texture which is useful for if you need the data in CPU memory as well (say, for generating collision meshes).

## Future plans
 - Upgrade the shader to work with proper pixel lighting instead of being vertex-lit.
 - Support blending between multiple textures based on height or introduce a concept of "biomes".
 - Write a system for loading/unloading chunks around the camera to simulate infinite terrain.
    - Should recycle chunk objects to avoid overhead of spawning/destroying objects
 - Use tesselation shader to blend smoothly between LODs.
 - Support deferred rendering as well as forward.