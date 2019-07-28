# Unity Tile Mesh Paint

A Unity Editor Tool which lets you paint 2D tiles on 3D meshes. 

Download and Import the package [here](https://github.com/antonpantev/unity-tile-mesh-paint/raw/master/unity-tile-mesh-paint.unitypackage).

<img src="https://github.com/antonpantev/unity-tile-mesh-paint/raw/master/PreviewImages/ScreenShot.png">

### How to Set Up a Mesh for Painting

* Add an Image with your Tiles to your Unity project

* Set the Image to use a `Texture Type` of `Sprite` and a `Sprite Mode` of `Multiple` 

* Use the `Sprite Editor` to splice the image into individual tiles

* Create a Material and set your Tiles image as the Texture

* Create a Cube (or any other mesh you want to paint)

* Drop the Material onto the Cube

* Add a Mesh Collider to your Cube (you’ll need to remove any other preexisting colliders like the Box Collider first)

* Open the Tile Mesh Paint Window  
`Window` > `Tile Mesh Paint`

* Click on the Mesh to paint a tile

#### Attribution
Example Tiles were created by Kicked in Teeth:  
https://kicked-in-teeth.itch.io/pico-8-tiles
