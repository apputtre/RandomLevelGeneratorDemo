# Overview
This is an application which generates and displays levels similar to those found in turn-based dungeon crawler games, such as Nethack and Rogue.

# Screenshots
![random_level_generator_screenshot_1](https://github.com/user-attachments/assets/bc070502-b054-4278-b2e7-7734b5555c2c)
![random_level_generator_screenshot_2](https://github.com/user-attachments/assets/a5052338-0014-492f-9b42-138cef527369)

# Video Demonstration
The video demo can be viewed [here](https://drive.google.com/file/d/1IiW03ssC12GnllOtC7P_yNqxsbx4VSTg/view?usp=sharing).

# How to Run
This is a Windows Presentation Forms (WPF) project created using the .NET 8 SDK. A Visual Studio 2022 solution file is included in the repository. Open this in Visual Studio and build. You will be required to install the .NET 8 SDK if it is not already installed.

# Usage
Adjust the generation parameters and click the "Generate" button to display the result. The view can be panned with WASD, the arrow keys, or by clicking and dragging. The view can be zoomed in and out using the mouse wheel.

The layout of the level is determined by a number which is used to seed the random number generator. There are two modes which can be selected: in "auto" mode, each seed is generated using the previous seed. In "manual" mode, the user may input a string of digits which will be used as the seed for the next level. Levels generated using the same seed will have identical layouts.

# The Algorithm
First, a graph is created which represents the grid of tiles which make up the level. Each vertex in the graph represents a tile in the level. For example, a 100 x 100 level will be represented by a graph with 10,000 vertices. In the graph, each vertex is connected to each of its four cardinal neighbors; thus, vertices in the center of the graph have 4 neighbors, vertices on the edges have 3, and corner vertices have only 2. Next, a number of rectangular rooms of varying sizes are created and placed at random. Each room has a "room leader" vertex which represents the entire room. Then, the shortest path between every pair of leader vertices is computed. These values are used to create a weighted graph which includes every room leader vertex. In this new graph, each room leader is connected to every other leader by an edge whose weight is equal to the length of the shortest path between them in the original graph. Next, a minimum spanning tree is generated to find the set of edges which connects every room leader with the shortest total length of hallways. Finally, each hallway which was chosen to be included in the minimum spanning tree is carved out in the level.

# Attributions
* The graphical assets used are taken from the Dungeon Crawl Stone Soup tileset, which is licensed under CC0. The tileset can be found [here](https://code.google.com/archive/p/crawl-tiles/).

* The algorithm used to generate the halls is based on the so-called 2-approximation algorithm discussed in [this lecture](https://courses.cs.duke.edu/spring15/compsci590.1/scribing/cps590_lec16-17.pdf).
