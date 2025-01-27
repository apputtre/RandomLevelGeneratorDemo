using System.Text;
using System.Windows;
using System;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace RandomLevelGeneratorDemo;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        LevelBuilder builder = new();
        TestLevelGenerator generator = new(builder);
        generator.Generate();
        Level level = builder.Level;

        Console.WriteLine(level.Tiles[new Vec2i(0, 0)] == TileType.Wall);
        Console.WriteLine(level.Tiles[new Vec2i(1, 1)] == TileType.Wall);

        LevelViewer viewer = new(level);
    }

    public void OnLoad(object sender, EventArgs e)
    {
        Console.WriteLine("Loaded");
    }
}