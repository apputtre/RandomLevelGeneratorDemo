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
    }

    public void OnLoad(object sender, EventArgs e)
    {
        Vec2i v1 = new(5, 5);
        Vec2i v2 = new(v1);

        LevelBuilder builder = new();
        RandomLevelGenerator generator = new(builder);
        generator.Generate();
        Level level = builder.Level;
        LevelViewer viewer = new(level);

        Canvas mainCanvas = (Canvas)FindName("MainCanvas");

        mainCanvas.Children.Add(viewer);
        viewer.Update();
    }
}