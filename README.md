# Unity Tile Mesh Paint

Tile Mesh Paint is a Unity Editor Tool which lets you paint 2D tiles on 3D meshes. 

Download and Import the package here.

### How to Set Up a Mesh for Painting

Place an Image with your Tiles in your Unity project

Set the Image to use a `Texture Type` of `Sprite` and a `Sprite Mode` of `Multiple` 

Use the `Sprite Editor` to splice the image into individual tiles

Create a Material and set your Tiles image as the Texture

Create a Cube (or any other mesh you want to paint)

Drop the Material onto the Cube

Add a Mesh Collider to your Cube (you’ll need to remove the Box Collider first)

Open the Tile Mesh Paint Window  
`Window` > `Tile Mesh Paint`

Click on the Mesh to paint a tile
