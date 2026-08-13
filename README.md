# voxel-engine

Voxel renderer written in Unity for a 3D graphics course.

![Idle](Idle.gif)

## What it does

Chunked world, meshed at runtime. Faces between two solid blocks are skipped,
including across chunk borders, so nothing hidden gets sent to the GPU. The
cross-chunk case needed neighbour data available at mesh time and a re-mesh
when a neighbour changed.

## Chunk loading

Chunks are prioritised by camera facing, not just distance. Anything close in
gets loaded regardless of where you're looking, but past that inner radius
only chunks roughly in front of the camera load in; the ones behind you are
the first to unload when the loaded count hits the cap.

![Chunk culling](ChunkCulling.gif)

## Shaders

Written in HLSL:

- water, Blinn-Phong lighting
- obsidian
- portal

## Running it

Open in Unity 6000.3.9f1 and press play.
