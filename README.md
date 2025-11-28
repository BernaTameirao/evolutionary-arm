# Evolutionary Arm

Interactive simulation of a robotic manipulator developed in Unity.\
This project served as the basis for the article accepted, published and presented at **IEEE CEC 2025**.

![2025-11-2813-07-32-ezgif com-optimize](https://github.com/user-attachments/assets/ab9cf1fb-a246-4409-b104-34bf38eeb9dd)

## Description

3D simulation of an articulated robotic arm that combines evolutionary computation and the A* pathfinding algorithm to reach target positions while avoiding obstacles.\
The system can dynamically avoid randomly placed obstacles, navigate through narrow tunnels, and operate under fog-of-war conditions.

Main features:

- Evolutionary computation–based inverse kinematics, enabling efficient solutions to high-dimensional and non-linear joint configurations.
- Interactive joint control with real-time visualization.
- Evolutionary algorithm parameters fully configurable by the user.
- Data logging and export for benchmarking, optimization studies, and further analysis.

## Article (IEEE CEC 2025)

**Title:** *Evolutionary Computation Applied to the Control of a Robotic Manipulator with Obstacle Avoidance*  
**Link IEEE Xplore:** https://ieeexplore.ieee.org/document/11043104

## Repository Structure

    evolutionary-arm/
    ├── Assets/
    ├── Packages/
    ├── ProjectSettings/
    ├── .gitignore
    └── README.md

## How to Run

1.  Clone:

``` bash
git clone https://github.com/BernaTameirao/evolutionary-arm.git
```
2.  Install and open the Unity Hub.
3.  Log on your Unity Hub account.
4.  Install the required Unity version (as specified in ProjectSettings/ProjectVersion.txt).
5.  Add this repository as a project.
6.  Open the main scene and press Play.

## Authors

-   **Bernardo Rodrigues Tameirão Santos**
-   **Lázaro Pereira Vinaud Neto**
-   **Nicolas Carreiro Rodrigues**
-   **Eduardo do Valle Simões**
