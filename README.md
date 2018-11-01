# CannonPhysicsGame2D
An implementation of 2D Physics in the form of a cannon mini-game in Unity and C#.

### Gameplay
The player controls a cannon in a 2D space. The player can move the cannon within boundaries ('A' and 'D') and adjust the cannon's shooting angle between 0 degrees to 90 degrees. ('W' and 'S'), as well as shoot cannonballs ('space'). The cannonballs collide with the environment including the randomly generated mountain in the middle of the scene and the tall wall at the end on the left. If a cannonball hits the ground or otherwise exits the scene it disappears. There is wind near the mountaintop and cannonballs are affected by it. Currently, there is no goal and thus the game continues indefinitely. This was intended to be a test project as opposed to a playable, winnable game.

### Implementation
All of the 2D physics were written from scratch or using online references, meaning no Components provided from Unity aside from renderers are used for the physics. Cannonballs have custom rigidbodies written for them where the movement is accomplished by using a form of Verlet Integration and impulse forces are applicable to these rigidbodies for external forces. 

Collision detection is primitively done using Axis-Aligned Bounding Boxes making use of the Separating Axis Theorem, checking if the current object is intersecting with any of the collidable objects in the scene (the mountain and the wall) every update. The Minimum Translation Vector is obtained upon detecting a collision and is used in collision resolution.

Collision resolution is done by using a coefficient of restitution and applying an impulse force on the rigidbody in the opposite x or y direction according to the Minimum Translation Vector that is obtained in the Collision Detection portion (If the y component of the MTV is greater than the X component we should apply an opposite force in the y direction). Before applying the impulse force, however, the object is displaced to be moved outside of the intersecting object using the Minimum Translation Vector to ensure that resolved collisions don't get re-resolved.

[This answer](https://gamedev.stackexchange.com/a/129450) on GameDev Stack Exchange makes up a bulk of the code for finding the penetration depth (or the Minimum Translation Vector) between two axis aligned bounding boxes and helped greatly in making the rest of the collision detection and resolution code function.

The mountaintop is randomly generated using 1D Perlin Noise, with the great help of [this Youtube tutorial from Arend Peter](https://www.youtube.com/watch?v=Exuz4OWP7t8). Only the outer blocks of the mountain are being checked for collision detection to lessen the amount of detection required. The rest of the mountain (edges and base) are generated accordingly and not randomly.

### Assets
All assets were made using the default sprites provided in Unity.
