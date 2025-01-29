using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using System;
using System.Drawing;
using System.IO;
using System.Windows.Media.Imaging;
using System.Windows.Controls;

namespace RandomLevelGeneratorDemo;

public class LevelViewer : FrameworkElement
{
    private VisualCollection _children;
    private Level? _level;
    private Vec2d _cameraPos;
    private double _zoom = 1f;
    private double _zoomVelocity = 0.1f;
    private long _lastInputUpdateTicks;
    private double _cameraVelocity = 200;
    private BitmapImage _tileset;

    public const int TileHeight = 8;
    public const int TileWidth = 8;
    public Vec2d CameraPos { get => _cameraPos; set { _cameraPos = value; } }
    public Level? Level { get => _level; set { _level = value; } }
    public int LevelWidth { get; set; }
    public int LevelHeight { get; set; }

    public LevelViewer(Level? level)
    {
        _children = new VisualCollection(this);
        _level = level;
        _tileset = new BitmapImage(new Uri("C:\\Users\\Revch\\src\\RandomLevelGeneratorDemo\\RandomLevelGeneratorDemo\\assets\\tileset.png", UriKind.Relative));

        DispatcherTimer inputTimer = new DispatcherTimer();
        inputTimer.Tick += new EventHandler(CheckInput);
        inputTimer.Interval = new TimeSpan(0, 0, 0, 0, 1000 / 60);
        _lastInputUpdateTicks = DateTime.Now.Ticks;
        inputTimer.Start();
    }
    public LevelViewer() : this(null)
    {
    }

    public void Update()
    {
        _children.Clear();

        DrawingVisual visual = new();
        DrawingContext context = visual.RenderOpen();

        foreach (Vec2i tile in _level.Tiles.Keys)
        {
            Rect r = TransformTile(tile);

            if (_level.Tiles[tile] == TileType.Empty)
                context.DrawRectangle(Brushes.Black, null, r);
            else if (_level.Tiles[tile] == TileType.Wall)
                DrawWall(context, new Vec2d(r.X, r.Y));
            else if (_level.Tiles[tile] == TileType.Floor)
                DrawFloor(context, new Vec2d(r.X, r.Y));
        }

        double borderThickness = 4;
        context.DrawLine(new Pen(Brushes.Red, borderThickness), new(0 - borderThickness/2, 0 - borderThickness/2), new(0 - borderThickness/2, LevelHeight * TileHeight + borderThickness/2));
        context.DrawLine(new Pen(Brushes.Red, borderThickness), new(0 - borderThickness, LevelHeight * TileHeight + borderThickness/2), new(LevelWidth * TileWidth + borderThickness/2, LevelHeight * TileHeight + borderThickness/2));
        context.DrawLine(new Pen(Brushes.Red, borderThickness), new(LevelWidth * TileWidth + borderThickness/2, LevelHeight * TileHeight + borderThickness/2), new(LevelWidth * TileWidth + borderThickness/2, 0 - borderThickness));
        context.DrawLine(new Pen(Brushes.Red, borderThickness), new(LevelWidth * TileWidth + borderThickness/2, 0 - borderThickness/2), new(0 - borderThickness, 0 - borderThickness/2));

        context.Close();
        _children.Add(visual);
    }
    private void CheckInput(object? sender, EventArgs e)
    {
        long currentTicks = DateTime.Now.Ticks;
        double dt = (double) (currentTicks - _lastInputUpdateTicks) / TimeSpan.TicksPerSecond;

        if (Keyboard.IsKeyDown(Key.W))
        {
            _cameraPos = new Vec2d(_cameraPos.X, _cameraPos.Y - _cameraVelocity * dt);
        }
        if (Keyboard.IsKeyDown(Key.S))
        {
            _cameraPos = new Vec2d(_cameraPos.X, _cameraPos.Y + _cameraVelocity * dt);
        }
        if (Keyboard.IsKeyDown(Key.D))
        {
            _cameraPos = new Vec2d(_cameraPos.X + _cameraVelocity * dt, _cameraPos.Y);
        }
        if (Keyboard.IsKeyDown(Key.A))
        {
            _cameraPos = new Vec2d(_cameraPos.X - _cameraVelocity * dt, _cameraPos.Y);
        }

        _lastInputUpdateTicks = currentTicks;
        UpdateCameraPos();
    }

    protected override void OnMouseWheel(MouseWheelEventArgs e)
    {
        if (e.Delta > 0)
        {
            _zoom += _zoom * 0.05;
        }
        else if (e.Delta < 0)
        {
            _zoom -= _zoom * 0.05;
        }
    }

    private Rect TransformTile(Vec2i tile)
    {
        return new Rect(tile.X * TileWidth, tile.Y * TileHeight, TileWidth, TileHeight);
    }
    private void UpdateCameraPos()
    {
        TranslateTransform translate = new TranslateTransform(-_cameraPos.X, -_cameraPos.Y);
        ScaleTransform scale = new ScaleTransform(_zoom, _zoom);

        TransformGroup group = new();
        group.Children.Add(translate);
        group.Children.Add(scale);

        RenderTransform = group;
    }

    private void DrawWall(DrawingContext context, Vec2d pos)
    {
        Rect r = new(pos.X, pos.Y, TileWidth, TileHeight);

        ImageBrush brush = new(_tileset);
        brush.ViewboxUnits = BrushMappingMode.Absolute;

        Random rand = new();

        double pxX = rand.NextInt64(0, 6) * 32;
        double pxY = 0;

        double dipX = pxX / _tileset.DpiX * 96.0;
        double dipY = pxY / _tileset.DpiY * 96.0;
        double dipW = 32 / _tileset.DpiX * 96;
        double dipH = 32 / _tileset.DpiY * 96;

        brush.Viewbox = new Rect((int) dipX, (int) dipY, (int) dipW, (int) dipH);

        context.DrawRectangle(brush, null, r);
    }
    private void DrawFloor(DrawingContext context, Vec2d pos)
    {
        Rect r = new(pos.X, pos.Y, TileWidth, TileHeight);

        ImageBrush brush = new(_tileset);
        brush.ViewboxUnits = BrushMappingMode.Absolute;

        Random rand = new();

        double pxX = rand.NextInt64(0, 6) * 32;
        double pxY = 32;

        double dipX = pxX / _tileset.DpiX * 96.0;
        double dipY = pxY / _tileset.DpiY * 96.0;
        double dipW = 32 / _tileset.DpiX * 96;
        double dipH = 32 / _tileset.DpiY * 96;

        brush.Viewbox = new Rect(dipX, dipY, dipW, dipH);

        context.DrawRectangle(brush, null, r);
    }

    protected override int VisualChildrenCount => _children.Count;
    protected override Visual GetVisualChild(int index) => _children[index];
}
