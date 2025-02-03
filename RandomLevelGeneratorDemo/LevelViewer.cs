using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using System;
using System.Drawing;
using System.IO;
using System.Windows.Media.Imaging;
using System.Windows.Controls;
using System.Windows.Data;
using System.Globalization;

namespace RandomLevelGeneratorDemo;

public class LevelViewer : FrameworkElement
{
    private VisualCollection _children;
    private Level? _level;
    private long _lastInputUpdateTicks;
    private double _cameraVelocity = 200;
    private BitmapImage _tileset;
    private DrawingVisual levelView = new();
    private DrawingVisual viewBorder = new();
    private DrawingVisual background = new();
    private List<CroppedBitmap> _wallTiles = new();
    private List<CroppedBitmap> _floorTiles = new();

    public const int TileHeight = 8;
    public const int TileWidth = 8;
    public Level? Level { get => _level; set { _level = value; } }
    public int LevelWidth { get; set; }
    public int LevelHeight { get; set; }

    public LevelViewer(Level? level)
    {
        _children = new(this);
        _children.Add(background);
        _children.Add(levelView);
        _children.Add(viewBorder);

        viewBorder.Transform = new MatrixTransform(Matrix.Identity);
        levelView.Transform = new MatrixTransform(Matrix.Identity);

        if (level == null)
            _level = new();
        else
            _level = level;

        Uri tilesetUri = new Uri("C:\\Users\\Revch\\src\\RandomLevelGeneratorDemo\\RandomLevelGeneratorDemo\\assets\\tileset.png");
        _tileset = new BitmapImage(tilesetUri);

        for (int i = 0; i < 6; ++i)
        {
            _wallTiles.Add(new (_tileset, new Int32Rect(
                32 * i,
                0,
                32,
                32
             )));

            _floorTiles.Add(new (_tileset, new Int32Rect(
                32 * i,
                32,
                32,
                32
             )));
        }

        RenderOptions.SetBitmapScalingMode(_tileset, BitmapScalingMode.NearestNeighbor);

        DispatcherTimer inputTimer = new DispatcherTimer();
        inputTimer.Tick += new EventHandler(CheckInput);
        inputTimer.Interval = new TimeSpan(0, 0, 0, 0, 1000 / 60);
        _lastInputUpdateTicks = DateTime.Now.Ticks;
        inputTimer.Start();
    }
    public LevelViewer() : this(null) {}

    public void UpdateLevelView()
    {
        // draw the background
        DrawingContext context = background.RenderOpen();
        context.DrawRectangle(Brushes.Black, null, new Rect(0, 0, ActualWidth, ActualHeight));
        context.Close();

        // draw the level view
        context = levelView.RenderOpen();
        foreach (Vec2i tile in _level.Tiles.Keys)
        {
            Rect r = TransformTile(tile);

            if (_level.Tiles[tile] == TileType.Wall)
                DrawWall(context, new Vec2d(r.X, r.Y));
            else if (_level.Tiles[tile] == TileType.Floor)
                DrawFloor(context, new Vec2d(r.X, r.Y));
        }
        context.Close();
    }

    public void CenterCamera()
    {
        // n.b. This only works properly if the camera has not been zoomed yet
        PanCamera(-ActualWidth / 2 + LevelWidth / 2 * TileWidth, -ActualHeight / 2 + LevelHeight / 2 * TileHeight);
    }

    public void UpdateViewBorder()
    {
        DrawingContext context = viewBorder.RenderOpen();

        double borderThickness = 4;
        context.DrawLine(new Pen(Brushes.Red, borderThickness), new(0 - borderThickness/2, 0 - borderThickness/2), new(0 - borderThickness/2, LevelHeight * TileHeight + borderThickness/2));
        context.DrawLine(new Pen(Brushes.Red, borderThickness), new(0 - borderThickness, LevelHeight * TileHeight + borderThickness/2), new(LevelWidth * TileWidth + borderThickness, LevelHeight * TileHeight + borderThickness/2));
        context.DrawLine(new Pen(Brushes.Red, borderThickness), new(LevelWidth * TileWidth + borderThickness/2, LevelHeight * TileHeight + borderThickness/2), new(LevelWidth * TileWidth + borderThickness/2, 0 - borderThickness));
        context.DrawLine(new Pen(Brushes.Red, borderThickness), new(LevelWidth * TileWidth + borderThickness/2, 0 - borderThickness/2), new(0 - borderThickness, 0 - borderThickness/2));

        context.Close();
    }

    private void CheckInput(object? sender, EventArgs e)
    {
        long currentTicks = DateTime.Now.Ticks;
        double dt = (double) (currentTicks - _lastInputUpdateTicks) / TimeSpan.TicksPerSecond;

        double dx = 0;
        double dy = 0;

        if (Keyboard.IsKeyDown(Key.W))
        {
            dy = -_cameraVelocity * dt;
        }
        if (Keyboard.IsKeyDown(Key.S))
        {
            dy = _cameraVelocity * dt;
        }
        if (Keyboard.IsKeyDown(Key.D))
        {
            dx = _cameraVelocity * dt;
        }
        if (Keyboard.IsKeyDown(Key.A))
        {
            dx = -_cameraVelocity * dt;
        }

        PanCamera(dx, dy);

        _lastInputUpdateTicks = currentTicks;
    }

    protected override void OnMouseWheel(MouseWheelEventArgs e)
    {
        Matrix mat = levelView.Transform.Value;

        double zoom = 0;

        if (e.Delta > 0)
            zoom = 0.1;
        else if (e.Delta < 0)
            zoom = -0.1;

        mat.ScaleAt(1.0 + zoom, 1.0 + zoom, e.GetPosition(this).X, e.GetPosition(this).Y);

        levelView.Transform = new MatrixTransform(mat);
        viewBorder.Transform = new MatrixTransform(mat);
    }

    protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
    {
        base.OnRenderSizeChanged(sizeInfo);

        UpdateLevelView();
    }

    private Rect TransformTile(Vec2i tile)
    {
        return new Rect(tile.X * TileWidth, tile.Y * TileHeight, TileWidth, TileHeight);
    }
    private void PanCamera(double dx, double dy)
    {
        Matrix mat = levelView.Transform.Value;

        mat.OffsetX -= dx;
        mat.OffsetY -= dy;

        levelView.Transform = new MatrixTransform(mat);
        viewBorder.Transform = new MatrixTransform(mat);
    }

    private void DrawWall(DrawingContext context, Vec2d pos)
    {
        Random rand = new();
        Rect r = new(pos.X, pos.Y, TileWidth, TileHeight);
        ImageBrush brush = new(_wallTiles[rand.Next(6)]);
        context.DrawRectangle(brush, null, r);
        /*
        Rect r = new(pos.X, pos.Y, TileWidth, TileHeight);
        context.DrawRectangle(Brushes.DarkGray, null, r);
        */

        /*
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

        context.DrawRectangle(brush, null, new(pos.X, pos.Y, TileWidth, TileHeight));
        */

        /*
        CroppedBitmap bitmapImage = new(_tileset,new Int32Rect(0, 0, 32, 32));

        //brush.Viewbox = _wallTiles[rand.Next(6)];

        //context.DrawRectangle(brush, null, r);
        Random rand = new();
        int idx = rand.Next(6);
        context.DrawImage(_wallTiles[idx], r);
        */
    }
    private void DrawFloor(DrawingContext context, Vec2d pos)
    {
        Rect r = new(pos.X, pos.Y, TileWidth, TileHeight);
        context.DrawRectangle(Brushes.LightGray, null, r);

        /*
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
        */
    }

    protected override int VisualChildrenCount => _children.Count;
    protected override Visual GetVisualChild(int index) => _children[index];
}
