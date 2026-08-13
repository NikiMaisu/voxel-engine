# voxel-engine

Voxel renderer written in Unity for a 3D graphics course.

## What it does

Chunked world, meshed at runtime. Faces between two solid blocks are skipped,
including across chunk borders, so nothing hidden gets sent to the GPU. The
cross-chunk case needed neighbour data available at mesh time and a re-mesh
when a neighbour changed.

## Shaders

Written in HLSL:

- water, Blinn-Phong lighting
- obsidian
- portal

## Running it

Open in Unity 6000.3.9f1 and press play.
