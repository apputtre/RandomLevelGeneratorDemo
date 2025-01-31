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
using System.Text.RegularExpressions;
using System.Diagnostics;

namespace RandomLevelGeneratorDemo;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private LevelViewer levelViewer;
    private LevelBuilder levelBuilder;
    private RandomLevelGenerator levelGenerator;
    private TextBox seedTextBox;
    private bool manualSeeding = false;

    private int _levelWidth;
    private int _levelHeight;
    private int _numRooms;

    public int LevelWidth
    {
        get => _levelWidth;
        set
        {
            _levelWidth = value;
            levelViewer.LevelWidth = _levelWidth;
            levelViewer.UpdateViewBorder();
        }
    }
    public int LevelHeight
    {
        get => _levelHeight;
        set
        {
            _levelHeight = value;
            levelViewer.LevelHeight = _levelHeight;
            levelViewer.UpdateViewBorder();
        }
    }
    public int NumRooms
    {
        get => _numRooms;
        set
        {
            _numRooms = value;
        }
    }

    public class TestContext
    {
        public int X { get; set; }
    }

    public MainWindow()
    {
        InitializeComponent();

        levelBuilder = new();
        levelGenerator = new(levelBuilder);

        DataContext = new TestContext();
    }

    public void OnLoad(object sender, EventArgs e)
    {
        levelViewer = (LevelViewer)FindName("Viewer");
        seedTextBox = (TextBox)FindName("SeedTextBox");

        ((Slider)FindName("LevelWidthSlider")).Value = 25;
        ((Slider)FindName("LevelHeightSlider")).Value = 25;
        ((Slider)FindName("NumRoomsSlider")).Value = 10;
    }

    private void GenerateButton_Click(object sender, RoutedEventArgs e)
    {
        if (manualSeeding)
            if (seedTextBox.Text == string.Empty)
            {
                MessageBox.Show("Please enter a seed or select auto seeding mode.");
                return;
            }

        GenerateLevel();
    }

    private async void GenerateLevel()
    {
        Label statusLabel = (Label)FindName("StatusLabel");
        statusLabel.Content = "Generating...";
        Button generateButton = (Button)FindName("GenerateButton");
        generateButton.IsEnabled = false;
        try
        {
            if (manualSeeding)
                levelGenerator.NextSeed = int.Parse(seedTextBox.Text);

            levelGenerator.SetParameters(new LevelParameters(_levelWidth, _levelHeight, _numRooms));

            Stopwatch timer = new();
            timer.Start();
            await Task.Run(() => levelGenerator.Generate());

            levelViewer.Level = levelBuilder.Level;
            seedTextBox.Text = levelGenerator.Seed.ToString();
            levelViewer.UpdateLevelView();
            timer.Stop();
            statusLabel.Content = $"Done ({Math.Round(timer.Elapsed.TotalMicroseconds / 1000.0, 2)} ms)";
        }
        catch(Exception)
        {
            MessageBox.Show("A level generation error has occurred.");
            statusLabel.Content = "Done";
        }

        generateButton.IsEnabled = true;
    }

    private void SeedTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (!IsLoaded)
            return;

        string text = seedTextBox.Text;
        string strippedText = Regex.Replace(text, "[^0-9]", "");

        if (text != strippedText)
        {
            seedTextBox.Text = strippedText;
            MessageBox.Show("Seeds must include only numeric characters.");
        }
    }

    private void AutoSeedingSelected(object sender, RoutedEventArgs e)
    {
        if (!IsLoaded)
            return;

        manualSeeding = false;
        seedTextBox.IsReadOnly = true;
    }

    private void ManualSeedingSelected(object sender, RoutedEventArgs e)
    {
        if (!IsLoaded)
            return;

        manualSeeding = true;
        seedTextBox.IsReadOnly = false;
    }
}