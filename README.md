# Immersive Mixed Reality Training Concept for Mastering Surgical Knot-tying - Prototype Implementation

![Screenshot 2024-12-06 162308](https://github.com/user-attachments/assets/bf0a3be1-4691-48de-bc59-fb930c439c96)

This repository contains a prototype implemtation of our mixed reality training concept designed to enhance medical students’ acquisition of surgical knot-tying skills. Utilizing a virtual reality headset with video passthrough functionality, the system provides adaptive visual instructions tailored to the user’s hand movements during the knot-tying process. This prototype is implemented in Unity3D Version 2022.3 LTS fpr the Varjo XR-3 headset. The prototype can be adapted to other headsets as well, but these systems need to be equipped with a Ultraleap Handtracking sensor unit.

# Installation

## Requirements 
- [Git](https://git-scm.com/) with [Git LTS](https://git-lfs.com/) : 
- [Volumetric Video sequences](https://zenodo.org/records/14705931) downloaded and unzipped : 
- Latest [Unity 2022 LTS](https://unity.com/de/releases/2022-lts) version
- Varjo XR-3 Headset
- [Varjo Base](https://varjo.com/downloads/) installed and hand tracking enabled
- A thread for tying the knots
- Ideally, a knot tying bench, or a similar object which you can use to tye the thread around
- Best case: [The Digital knot tying bench](https://github.com/ExperimentalSurgery/Digital-Knot-Trainer)

## Setup
- Clone this repository with Git
- Put the folder "PointClouds" from the unzipped volumetric video sequences into the "Assets/Streamingassets" folder of the Unity Project
- Open the project with Unity 2022 LTS
- Open the scene "Assets/Scenes/BMBF_KnotbAR_INTRO.unity"
- Click play! Make sure to have audio enabled, as the voiceover contains important instructions

# Citation
Coming sooon

# License

Licensed under the [GNU General Public License Version 3](https://github.com/ExperimentalSurgery/Knot-Tying-Trainer-Prototype/blob/main/LICENSE)
