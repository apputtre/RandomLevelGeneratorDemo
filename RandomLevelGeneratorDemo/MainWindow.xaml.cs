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
    private SteinerLevelGenerator levelGenerator;
    private TextBox seedTextBox;
    private bool manualSeeding = false;

    public static readonly DependencyProperty LevelWidthProperty = DependencyProperty.Register(
        name: "LevelWidth",
        propertyType: typeof(int),
        ownerType: typeof(MainWindow),
        typeMetadata: new FrameworkPropertyMetadata(
            propertyChangedCallback: new PropertyChangedCallback(OnLevelWidthChanged)
        ));

    public static readonly DependencyProperty LevelHeightProperty = DependencyProperty.Register(
        name: "LevelHeight",
        propertyType: typeof(int),
        ownerType: typeof(MainWindow),
        typeMetadata: new FrameworkPropertyMetadata(
            propertyChangedCallback: new PropertyChangedCallback(OnLevelHeightChanged)
        ));

    public static readonly DependencyProperty NumRoomsProperty = DependencyProperty.Register(
        name: "NumRooms",
        propertyType: typeof(int),
        ownerType: typeof(MainWindow)
        );


    public int LevelWidth
    {
        get => (int)GetValue(LevelWidthProperty);
        set => SetValue(LevelWidthProperty, value);
    }
    public int LevelHeight
    {
        get => (int)GetValue(LevelHeightProperty);
        set => SetValue(LevelHeightProperty, value);
    }
    public int NumRooms 
    {
        get => (int)GetValue(NumRoomsProperty);
        set => SetValue(NumRoomsProperty, value);
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

        LevelWidth = 25;
        LevelHeight = 25;
        NumRooms = 10;
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

            levelGenerator.SetParameters(new LevelParameters(LevelWidth, LevelHeight, NumRooms));

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
    private static void OnLevelWidthChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        MainWindow mainWindow = (MainWindow)d;

        mainWindow.levelViewer.LevelWidth = (int) e.NewValue;
        mainWindow.levelViewer.UpdateViewBorder();
    }
    private static void OnLevelHeightChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        MainWindow mainWindow = (MainWindow)d;

        mainWindow.levelViewer.LevelHeight = (int) e.NewValue;
        mainWindow.levelViewer.UpdateViewBorder();
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