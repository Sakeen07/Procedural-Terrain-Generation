# Prototype

View the functionalities of the project here: [![GameAI](https://youtu.be/uPl7NP5Mmts)

This project presents a prototype for a game. The game features a terrain consisting of multiple mountains (some of which are crossable, while others are not) and water (which is
crossable). The game includes two types of NPCs, with three NPCs of each type, making a total of six. Each NPC has four different states, and there are six different types of items, with three instances of each type. Each item has unique characteristics. Players can use a pathfinding system to identify the paths to items and NPCs.

The terrain was used Perlin Noise combined with Fractal Brownian Motion (FBM). This technique generates realistic terrain by combining multiple layers (octaves) of Perlin Noise at
different frequencies and amplitudes.

There is two different NPCs that two different behaviour.

### NPC1 State
<p align = "center">
<img width="300" alt="image" src="https://github.com/user-attachments/assets/453fef5a-3393-4147-9316-2b9428255cc4" />
</p>

### NPC2 State
<p align = "center">
<img width="300" alt="image" src="https://github.com/user-attachments/assets/34e60101-bd95-49f4-b05f-8a6348821ce1" />
</p>

### NPC Additional Behaviours

* When the player picks up a coin object, all NPCs (NPC and NewNPC) immediately start chasing the player, bypassing the chase range.
* If an NPC's health drops to 0, it will be destroyed from the terrain, and its ID, health, and state will be visible to the player.
* When the player picks up a Magic item, both NPC models are unable to see the player for 5 seconds.
* When the player picks up Armor, NPCs cannot damage the player, even if they shoot at the player.
* When a NewNPC’s health is below or equal to 20, it seeks help from another NPC. Both NPCs then chase the player until they enter the attack range.
* When they managed to make the player health 0, the player will be reset to his spawn position.
