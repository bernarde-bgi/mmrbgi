==========================================================
Voxwell: A Quick Voxel Toolkit for Unity
==========================================================

+Introduction
+Getting started
+Keyboard Shortcuts and Mouse Interactions New!
+Voxel Structure Settings
  -The Mouse Action Dropdowns
  -Dimensions
  -Selectively Enable/Disable Faces
  -Generate Secondary UV Set - New!
  -The Buttons
+Voxel Templates 	!New!
+Voxel Pallette 	!New!
  -Atlas Material
  -Voxel Templates[]
+The Material & Texture
+Version History
  -0.1 - February 10th Current
  -0.2 - Coming Soon!
+Roadmap to da future
  -“Chunking”
  -Procedural Generation Tools
  -Slopes/Stairs/Half Blocks
  -Better UI/UX
+More Resources
+Contact me
+Thanks!



----------------------------------------------------------
Introduction
----------------------------------------------------------
Voxwell is a small tool for Unity3D, aimed at enabling rapid game design and development. It uses voxels and a simple point and click interface to get you making game assets as quickly as possible. Other tools might result in more complicated, better looking, or more realistic assets, but Voxwell will be faster to learn and use, and will have a much smaller footprint on your project and workflow.

It is not intended to be an all in one package. 
It won’t force you to change the way you structure your code.
It won’t take you hours to figure out.

It is quick to setup, learn, and use.
It requires no technical knowledge, just the ability to point and click.
It will increase the speed that you design levels.
It will allow you to quickly test out ideas and refine them as soon as inspiration strikes.

I’ve included some additional resources (youtube tutorial/demo, contact info, link to the Unity Asset store) at the end of this document.
----------------------------------------------------------




----------------------------------------------------------
Getting started
----------------------------------------------------------
Once you’ve imported the Voxwell package, you can either open a brand new scene, or start with one of the demo scenes provided in /Assets/Voxwell/Demo Scenes/

For the purpose of this document, we’ll open the Empty Demo Scene, which just contains a directional light (with shadows) and a camera with a basic movement component.

Locate the “Basic Voxel Structure” prefab located in /Assets/Voxwell/ and drag that into your scene.

This prefab is simply a gameobject with the VoxelStructure.cs component. This component requires a Mesh Renderer, Mesh Collider, and Mesh Filter. It uses a Transparent/Cutout/Diffuse shader, and has the VoxelAtlas.psd texture applied to it.

To get started, go ahead and click the “Room with Back” button in the inspector. Now try left clicking and right clicking on the generated mesh. That’s really all there is to it, but read on to learn what else there is.

The two Scripts that do all of the work are /Voxwell/Scripts/VoxelStructure.cs and /Voxwell/Editor/VoxelStructureEditor.cs - If you want to customize this to suit your needs, most of the work will be done in VoxelStructure.cs, but exposing those functions to the editor will be in VoxelStructureEditor.cs
----------------------------------------------------------




----------------------------------------------------------
Keyboard Shortcuts and Mouse Interactions !New!
----------------------------------------------------------
If you hold down the shift key while dragging with the LMB or RMB, the current Action will continuously ‘fire.’ This allows for quicker addition or removal of voxels. 

You can use the numbers 1-5 to quickly select the desired action for the LMB.
1: Add
2: Max Pull
3: Paint
4: Erase
5: Flood
----------------------------------------------------------







----------------------------------------------------------
Voxel Structure Settings
----------------------------------------------------------

In the inspector, you should see some dropdown menus, input fields, checkboxes, and a whole bunch of buttons.
The Mouse Action Dropdowns
The first dropdown, is Current Voxel Type which determines which voxel type to use when performing any action. You’ll probably change this more frequently than anything else. In the future, I’ll include a visualization of the current voxel type as well.

Next are the LMB Action and RMB Action which determines what to do with you click on the voxel structure with either the Left Mouse Button or Right Mouse button. Both dropdowns contain the same options, and are there to allow you to configure your mouse clicks however you’d like.

Add adds a new voxel wherever you click
Erase removes the voxel that you click
MaxPull creates a column/row of blocks from where you clicked until it finds another occupied cell
Flood takes the Y value of wherever you clicked, and sets every cell at that height to the current voxel type.
None is there in case you don’t want any actions interfering with the normal mouse clicks.

In the future, I will be adding more actions like surface extrusion and marquee selection.


Dimensions
-------------------------------
Below those dropdowns, you’ll see a place to specify the Width, Height, and Depth of your voxel structure. The current max is 32x32x32 - though in the future, there will be no upper limit, and new gameobjects will be spawned automatically whenever the limit is reached.


Selectively Enable/Disable Faces
-------------------------------
Next, we see some checkboxes. These determine whether or not it will generate a certain face for each voxel. 

For example, if you are creating a sidescroller, and are certain that the camera will never see the back of your level, simple uncheck Draw Back, hit the Draw button (below) and those faces won’t be created, saving precious verts and tris.
Generate Secondary UV Set - New!
Because the voxel structures are already UV-mapped, if we want lightmapping to work, we must generate a secondary UV Set for the lightmap to use. Enabling this will slow down the time it takes to redraw a mesh, but will allow for lightmapping.


The Buttons 
-------------------------------
Most of these buttons are simply a quick way to easily call utility functions from the inspector. As with the LMB and RMB actions, these buttons (with the exception of the Draw and Export Mesh buttons) use the Current Voxel Type field to determine what blocks to place. 

-Solid Cube sets every cell in the Voxel Structure to the current voxel type.

-Room with back gives you all walls except for the front

-Room Without Back gives you all walls except the font and back

-Left Wall, Right Wall, Floor, Ceiling, Front Wall, and Back Wall all fill the specified wall with the current voxel type.

-Set All Non-Empties is kind of weird, but can be useful at times. It basically looks for every voxel that is not empty, and sets them all to the current voxel type. Good if you want to test out a few options for the ‘main’ bits of your level - ie should this room be made of wood, or stone?

-Grow Grass looks for every dirt voxel, checks to see if the voxel above it is empty, and if it is, it turns into a ‘dirt with grass’ voxel.

-Pillars Front Eight is intended to be a starting point/example for you to create your own buttons. Search through the VoxelStructure.cs and VoxelStructureEditor.cs files to see how it works, and modify to suit your needs.

-The Draw button just tells the Voxel Structure mesh to redraw itself. This is useful if you’ve somehow changed the Voxel Structure (for instance by enabling or disabling one of the ‘draw face’ checkboxes) and you want the mesh to be redrawn.

-The Clear All button set’s every cell in the Voxel Structure to empty.

-And finally, the Export Mesh button, which could use some explaining: whenever we generate a mesh in the editor, it doesn’t exist anywhere but in that scene. So if we were to take one of our voxel structures and drag it into the project panel to create a prefab, and then drag it back into our scene, we’d notice that the mesh was gone! If this happens, you can simply hit the Draw button again (assuming you didn’t remove the VoxelStructure.cs component from your prefab) and get your mesh back.

-To avoid this problem, click on the Export Mesh button, and select a location inside your assets folder. Now when you create a prefab from the object, it will have the exported mesh referenced. Keep in mind, however, that if you drag a bunch of instances of these out onto the stage, and then edit the mesh, all instances will be updated, since you are really making changes to the serialized mesh, which all instances will reference.
----------------------------------------------------------





----------------------------------------------------------
Voxel Templates New!
----------------------------------------------------------
A Voxel Template is a collection of data that determines how each voxel is drawn. You can define UV-mapping, vertex colors, toggle which faces to draw, determine whether or not to draw the faces in the center of the voxel, and determine the display order in Voxel selection menus.

To create a new Voxel Template, go to  Assets > Create > Voxel Template or right click in the Project view and select Create > Voxel Template.

Defining your Voxel Template: First, select the template that you wish to edit. In the inspector you’ll see everything you need to customize your voxels.

Display Order - This will determine the position of this Voxel Template in any voxel selection menus. You can use it to make sure that all of your ‘wood types’ are next to eachother, or ensure that the empty voxel is always at the top of your list.

Color - Use this to set the vertex color for your voxel. Whichever color you choose will be multiplied with the texture colors. NOTE: You must use a shader that supports vertex colors for this to work. Use the CutoutVertexColor.shader provided or make your own. The default Unity Shaders do not support vertex colors.

Draw Faces in Center - Normally, all of the faces of the voxel will be drawn on the ‘outside of the cube.’ However, if you wanted to create something like a fence or flower, you can enable this option. If enabled, each of the faces will be drawn in the center of the voxel, and neighboring voxels will draw adjacent faces.

Use Front UVs for All Faces - Most of the time, you’ll only want one texture for each side of your voxel. Enabling this option means you will only need to configure the UVs for the front face, which will be copied to all other faces.

Atlas Scale - This is the size of each tile in the Texture Atlas. For an atlas of 16x16 textures, this value would be 0.0625 (1/16) for an atlas of 8x8 textures, it would be 0.125 (1/8).

UVOffsets - This is where you define which piece of the Texture Atlas is shown on each face of the voxel. The value corresponds to the bottom left position of the desired texture, and can be any value between 0 and 1. 0 being the absolute bottom or left, 1 being absolute rop or right, and .5 being in the center.

Should Draw - This just toggles whether or not to draw the voxel. If enable, the voxel will be drawn. If disabled, the voxel will not be drawn, and neighboring voxels will draw adjacent faces.

Draw [x] Face - This toggles whether or not to draw a specific face. This is useful when creating ‘voxels’ that are only 2 dimensional, perhaps a window or fence, where you only want to draw the front and back of the voxel. If you want to disable a specific face for every voxel, that is better handled by toggling this option on the Voxel Structure.
----------------------------------------------------------





----------------------------------------------------------
Voxel Pallette New!
----------------------------------------------------------

Voxel Pallettes are collections of Voxel Templates. 

Each Voxel Structure references a Voxel Pallette, and uses the information in it’s Voxel Templates to determine what to draw.

To create a new Voxel Pallette, go to Assets > Create > Voxel Pallette or right click in the Project view and select Create > Voxel Pallette.

In the inspector, you will see two fields:

Atlas Material
-------------------------------
Because the Voxel Templates rely on the material used by the Voxel Structure to properly render, the Voxel Pallette must provide the material to the Voxel Structure when it is drawn. Whatever material is selected here will be applied to the Mesh Renderer of the Voxel Structure whenever it is drawn.

Read more about the Material and Texture below.

Voxel Templates[]
-------------------------------
This is where we keep references to all of the Voxel Templates that we want to use. Whenever you place a voxel in your structure, what you are really doing is setting an integer value. That integer value corresponds to the Voxel Template with that index in the Voxel Pallette.

For example, if you place a stone voxel somewhere, you are really setting that voxel value to 1. When the voxel structure is drawn, it finds the Voxel Template at index 1 in the Voxel Pallette and uses that data to figure out what to draw.
----------------------------------------------------------






----------------------------------------------------------
The Material & Texture
----------------------------------------------------------

In ../Voxwell/Materials & Textures/ you will find a material named Atlas,  atlas_psd.psd, and atlas_png.png. The Atlas material has the atlas_psd.psd texture applied. It uses the CutoutVertexColor shader, which is similar to the Transparent/Cutout/Diffuse shader, but multiplies vertex colors with the diffuse texture.

IMPORTANT: Playing around with the import settings on your texture can have unexpected results, if things are rendering oddly, check to make sure your import settings match mine:

Texture Type: Advanced
Generate Cubemap: None
Read/Write Enabled: false
Import Type: Default
Alpha From Grayscale: false
Alpha From Transparency: true
Bypass sRGB Sampling: false
Generate Mip Maps: false, but you should feel free to play around with this depending on your scene/needs
Wrap Mode: Clamp
Filter Mode: Point
Max Size: [The size of the texture]
Format: Automatic Compressed
----------------------------------------------------------




----------------------------------------------------------
Version History
----------------------------------------------------------
+0.1 - February 10th Current
  -Initial Release. 
+0.2 - Coming Soon! 
  -Introduced Voxel Pallettes and Voxel Templates
  -Added support for Vertex colors
  -“Spray Paint” Mouse Actions
----------------------------------------------------------






----------------------------------------------------------
Roadmap
----------------------------------------------------------
The following items are features that I plan on adding in the future. They are roughly ordered based on feedback from users, but the order that they are implemented in are subject to change.

“Chunking”
Instead of needing to create a new Voxel Structure manually, and needing many structures to build something large, each structure will automatically break itself down into “chunks” to allow infinitely large structures.

Procedural Generation Tools
Create procedurally generated dungeons and terrains with the push of a button.

Slopes/Stairs/Half Blocks
Better UI/UX
----------------------------------------------------------







----------------------------------------------------------
More Resources
----------------------------------------------------------
Unity Asset Store: https://www.assetstore.unity3d.com/#/content/14989
YouTube demo/tutorial: http://www.youtube.com/watch?v=Z-9Q3GPFbkI
Google Doc Readme: https://docs.google.com/document/d/1UK2bAgDDR6KcsIxA70QVJEAVc65ZXrntmeYfAOgDfhM
----------------------------------------------------------







----------------------------------------------------------
Contact Me
----------------------------------------------------------
E-mail: DarkMeatGames@gmail.com
Twitter: https://twitter.com/DarkMeatGames
----------------------------------------------------------







----------------------------------------------------------
Thanks!
----------------------------------------------------------
Thanks to reddit and all of their help and words of encouragement.

Extra special thanks to /u/Rancarable for the name Voxwell!
----------------------------------------------------------






