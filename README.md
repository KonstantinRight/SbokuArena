# Sboku Arena
> Sboku, sboku zahodi!

Sboku arena is an **FPS arena game mode** for *S&Box*, built on top of [Simple Weapon Base](https://github.com/timmybo5/simple-weapon-base). It demonstrates the capabilities of **SbokuBot**, an AI framework for shooter NPCs.

![image](https://cdn.sbox.game/org/righty/sbokuarena/screenshots/d99f6d64-af4f-4602-b5da-e832f5e9c712.jpg)

## Features
- Built-in support for Simple Weapon Base 
- Survive finite waves of enemies
- Upgrade your character and create your own build 

# Sboku Bot
**SbokuBot** is a flexible framework for creating shooter NPCs in _S&Box_. You can use the default AI or extend it with your own logic. The files are located in `Libraries/SbokuBot`.  

* Intended usage: Inherit from `SbokuBase` and create adapter classes for your game mode.
* Easy way: If you use SWB, copy the existing bot prefab from Sboku Arena and tweak as needed.
* Hard way: Build everything from scratch using the provided interfaces.

# License
This repository contains three different license files:
1. The original [SWB license](https://github.com/KonstantinRight/SbokuArena/blob/master/LICENSE) (found in the repository root).
2. The [Sboku Arena license](https://github.com/KonstantinRight/SbokuArena/blob/master/code/Sboku/LICENSE) (located in the `code/Arena` folder).
3. The [Sboku Bot license](https://github.com/KonstantinRight/SbokuArena/blob/master/Libraries/SbokuBot/LICENSE) (found in the `Library/SbokuBot` folder).
