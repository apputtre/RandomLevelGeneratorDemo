using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RandomLevelGeneratorDemo;

public struct LevelParameters
{
    public LevelParameters(int width, int height, int numRooms)
    {
        Width = width;
        Height = height;
        NumRooms = numRooms;
    }

    public int Width { get; set; }
    public int Height { get; set; }
    public int NumRooms { get; set; }
}
