using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using System.Drawing;
using System.Windows.Media.Imaging;
using System.Windows.Controls;

using Size = System.Windows.Size;
using System.Security.Cryptography.X509Certificates;

namespace RandomLevelGeneratorDemo;

public class LevelViewer : Canvas
{
    private Level? _level;
    private long _lastInputUpdateTicks;
    private double _cameraVelocity = 300;
    private BitmapImage _tileset;
    private List<CroppedBitmap> _wallTiles = new();
    private List<CroppedBitmap> _floorTiles = new();

    private ViewBorder _viewBorder = new();
    private Image _levelRender = new();
    private Grid _levelView = new();
    private Canvas _borderRect = new();

    public const int TileHeight = 8;
    public const int TileWidth = 8;
    public Level? Level { get => _level; set { _level = value; } }
    public int LevelWidth { get; set; }
    public int LevelHeight { get; set; }

    private class ViewBorder : FrameworkElement
    {
        private double borderThickness;

        public int LevelHeight { get; set; }
        public int LevelWidth { get; set; }
        public double BorderThickness { get => borderThickness; set => borderThickness = value; }


        protected override void OnRender(DrawingContext context)
        {
            context.DrawLine(new Pen(Brushes.Red, borderThickness), new(0 - borderThickness/2, 0 - borderThickness/2), new(0 - borderThickness/2, LevelHeight * TileHeight + borderThickness/2));
            context.DrawLine(new Pen(Brushes.Red, borderThickness), new(0 - borderThickness, LevelHeight * TileHeight + borderThickness/2), new(LevelWidth * TileWidth + borderThickness, LevelHeight * TileHeight + borderThickness/2));
            context.DrawLine(new Pen(Brushes.Red, borderThickness), new(LevelWidth * TileWidth + borderThickness/2, LevelHeight * TileHeight + borderThickness/2), new(LevelWidth * TileWidth + borderThickness/2, 0 - borderThickness));
            context.DrawLine(new Pen(Brushes.Red, borderThickness), new(LevelWidth * TileWidth + borderThickness/2, 0 - borderThickness/2), new(0 - borderThickness, 0 - borderThickness/2));
        }
    }

    public LevelViewer(Level? level)
    {
        Background = Brushes.Black;
        VerticalAlignment = VerticalAlignment.Stretch;
        HorizontalAlignment = HorizontalAlignment.Stretch;

        RenderOptions.SetBitmapScalingMode(_levelRender, BitmapScalingMode.HighQuality);
        RenderOptions.SetEdgeMode(_levelRender, EdgeMode.Aliased);

        _viewBorder.RenderTransform = new MatrixTransform(Matrix.Identity);
        _levelRender.RenderTransform = new MatrixTransform(Matrix.Identity);

        _viewBorder.BorderThickness = TileWidth * 1.5;

        _levelRender.HorizontalAlignment = HorizontalAlignment.Left;
        _levelRender.VerticalAlignment = VerticalAlignment.Top;

        _levelView.Children.Add(_levelRender);
        _levelView.Children.Add(_viewBorder);

        Children.Add(_levelView);
        Children.Add(new ViewBorder());

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

        DispatcherTimer inputTimer = new DispatcherTimer();
        inputTimer.Tick += new EventHandler(CheckInput);
        inputTimer.Interval = new TimeSpan(0, 0, 0, 0, 1000 / 60);
        _lastInputUpdateTicks = DateTime.Now.Ticks;
        inputTimer.Start();
    }
    public LevelViewer() : this(null) {}

    public void UpdateLevelView()
    {
        if (LevelWidth == 0 || LevelHeight == 0)
            return;

        WriteableBitmap bmp = new(
            LevelWidth*32,
            LevelHeight*32,
            _tileset.DpiX,
            _tileset.DpiY,
            _tileset.Format,
            null
        );

        // draw the level view
        foreach (Vec2i tile in _level.Tiles.Keys)
        {
            if (_level.Tiles[tile] == TileType.Wall)
                DrawWall(bmp, new Vec2i(tile.X, tile.Y));
            else if (_level.Tiles[tile] == TileType.Floor)
                DrawFloor(bmp, new Vec2i(tile.X, tile.Y));
        }

        _levelRender.Source = bmp;
        _levelRender.Width = TileWidth * LevelWidth;
        _levelRender.Height = TileHeight * LevelHeight;
    }

    public void CenterCamera()
    {
        // n.b. This only works properly if the camera has not been zoomed yet
        PanCamera(-ActualWidth / 2 + LevelWidth / 2 * TileWidth, -ActualHeight / 2 + LevelHeight / 2 * TileHeight);
    }

    public void UpdateViewBorder()
    {
        _viewBorder.LevelWidth = LevelWidth;
        _viewBorder.LevelHeight = LevelHeight;
        _viewBorder.InvalidateVisual();
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

        if (dx != 0 || dy != 0)
            PanCamera(dx, dy);

        _lastInputUpdateTicks = currentTicks;
    }

    protected override void OnMouseWheel(MouseWheelEventArgs e)
    {
        Matrix mat = _levelView.RenderTransform.Value;

        double zoom = 0;

        if (e.Delta > 0)
            zoom = 0.1;
        else if (e.Delta < 0)
            zoom = -0.1;

        mat.ScaleAt(1.0 + zoom, 1.0 + zoom, e.GetPosition(this).X, e.GetPosition(this).Y);

        _levelView.RenderTransform = new MatrixTransform(mat);
    }

    /*
    protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
    {
        base.OnRenderSizeChanged(sizeInfo);
    }
    */

    private void PanCamera(double dx, double dy)
    {
        Matrix mat = _levelView.RenderTransform.Value;

        mat.OffsetX -= dx;
        mat.OffsetY -= dy;

        _levelView.RenderTransform = new MatrixTransform(mat);
        //_viewBorder.RenderTransform = new MatrixTransform(mat);
    }

    private void DrawWall(WriteableBitmap wBmp, Vec2i pos)
    {
        Random rand = new();
        CopyRegion(_wallTiles[rand.Next(6)], wBmp, new Vec2i(pos.X*32, pos.Y*32));
    }
    private void DrawFloor(WriteableBitmap wBmp, Vec2i pos)
    {
        Random rand = new();
        CopyRegion(_floorTiles[rand.Next(6)], wBmp, new Vec2i(pos.X*32, pos.Y*32));
    }

    public static void CopyRegion(BitmapSource from, WriteableBitmap to, Vec2i at)
    {
        // bytes per pixel
        int fromBpp = from.Format.BitsPerPixel / 8;
        int fromStride = (from.PixelWidth * from.Format.BitsPerPixel + 7) / 8;

        byte[] pixelData = new byte[from.PixelHeight * fromStride];
        from.CopyPixels(pixelData, fromStride, 0);

        to.Lock();

        for (int row = 0; row < from.PixelHeight; ++row)
        {
            for (int col = 0; col < from.PixelWidth; ++col)
            {
                unsafe
                {
                    int colorData = pixelData[col * fromBpp + row * fromStride] << 0;
                    colorData |= pixelData[col * fromBpp + row * fromStride + 1] << 8;
                    colorData |= pixelData[col * fromBpp + row * fromStride + 2] << 16;
                    colorData |= pixelData[col * fromBpp + row * fromStride + 3] << 24;

                    IntPtr pBuffer = to.BackBuffer;
                    pBuffer += (col + (int) at.X) * to.Format.BitsPerPixel / 8 + (row + (int) at.Y) * to.BackBufferStride;
                    *((int*)pBuffer) = colorData;
                }
            }
        }

        to.AddDirtyRect(new((int) at.X, (int) at.Y, from.PixelWidth, from.PixelHeight));
        to.Unlock();
    }
}
