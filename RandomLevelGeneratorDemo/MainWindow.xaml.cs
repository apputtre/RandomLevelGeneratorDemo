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
        LevelBuilder builder = new();
        TestLevelGenerator generator = new(builder);
        generator.Generate();
        Level level = builder.Level;
        LevelViewer viewer = new(level);

        Canvas mainCanvas = (Canvas)FindName("MainCanvas");

        mainCanvas.Children.Add(viewer);
        viewer.Update();
    }
}