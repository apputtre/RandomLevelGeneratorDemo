using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using System.Drawing;
using System.Windows.Media.Imaging;
using System.Windows.Controls;

using Size = System.Windows.Size;
using Point = System.Drawing.Point;
using System.Data;
using System.Security.Policy;

namespace RandomLevelGeneratorDemo;

public class LevelViewer : Canvas
{
    private Level? _level;
    private long _lastInputUpdateTicks;
    private double _cameraVelocity = 300;
    private BitmapImage _tileset;
    private List<CroppedBitmap> _wallTiles = new();
    private List<CroppedBitmap> _floorTiles = new();
    private Camera _camera;
    private DispatcherTimer _inputTimer = new DispatcherTimer();
    private System.Windows.Point _dragStart = new(int.MinValue, int.MinValue);

    private ViewBorder _viewBorder = new();
    private Image _levelRender = new();
    private Grid _levelView = new();

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

    private class Camera
    {
        private double _zoomFactor = 1.0;
        private double _minZoom = 0;
        private double _maxZoom = int.MaxValue;
        private double _posX;
        private double _posY;
        private Rect _bounds = new Rect(double.MinValue, double.MinValue, double.MaxValue, double.MaxValue);
        private FrameworkElement _subject;

        public double PosX
        {
            get => _posX;
            set
            {
                _posX = value;
                UpdatePosition();
            }
        }

        public double PosY
        {
            get => _posY;
            set
            {
                _posY = value;
                UpdatePosition();
            }
        }

        public double ZoomFactor => _subject.RenderTransform.Value.M11;

        public double MaxZoom { get => _maxZoom; set => _maxZoom = value; }
        public double MinZoom { get => _minZoom; set => _minZoom = value; }
        public Rect Bounds { get => _bounds; set { _bounds = value; EnforceBounds(); } }

        public Camera(FrameworkElement subject)
        {
            _subject = subject;
            _subject.RenderTransform = Transform.Identity;
        }

        public void ZoomAt(double posX, double posY, double factor)
        {
            Matrix tMat = _subject.RenderTransform.Value;

            tMat.ScaleAt(factor, factor, posX, posY);

            if (tMat.M11 > _maxZoom && factor > 1)
                return;

            if (tMat.M11 < _minZoom && factor < 1)
                return;

            _subject.RenderTransform = new MatrixTransform(tMat);

            _posX = -tMat.OffsetX;
            _posY = -tMat.OffsetY;

            UpdatePosition();
        }

        private void UpdatePosition()
        {
            EnforceBounds();

            Matrix tMat = _subject.RenderTransform.Value;
            tMat.Translate(-_posX - tMat.OffsetX, -_posY - tMat.OffsetY);
            _subject.RenderTransform = new MatrixTransform(tMat);
        }
        private void EnforceBounds()
        {
            if (_posX > _bounds.Right)
                _posX = _bounds.Right;
            if (_posX < _bounds.Left)
                _posX = _bounds.Left;
            if (_posY > _bounds.Bottom)
                _posY = _bounds.Bottom;
            if (_posY < _bounds.Top)
                _posY = _bounds.Top;
        }
    }

    public LevelViewer(Level? level)
    {
        Background = Brushes.Black;
        VerticalAlignment = VerticalAlignment.Stretch;
        HorizontalAlignment = HorizontalAlignment.Stretch;
        Focusable = true;

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

        _camera = new(_levelView);
        _camera.MaxZoom = 10;
        _camera.MinZoom = 0.5;

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

        _inputTimer.Tick += new EventHandler(CheckInput);
        _inputTimer.Interval = new TimeSpan(0, 0, 0, 0, 1000 / 60);
        _lastInputUpdateTicks = DateTime.Now.Ticks;
        _inputTimer.Start();
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
        _camera.PosX = -ActualWidth / 2 + LevelWidth * TileWidth * _camera.ZoomFactor / 2;
        _camera.PosY = -ActualHeight / 2 + LevelHeight * TileHeight * _camera.ZoomFactor / 2;
    }

    public void UpdateViewBorder()
    {
        _viewBorder.LevelWidth = LevelWidth;
        _viewBorder.LevelHeight = LevelHeight;
        _viewBorder.InvalidateVisual();
    }
    private void UpdateCameraBounds()
    {
        _camera.Bounds = new Rect(-RenderSize.Width * 2, -RenderSize.Height * 2, RenderSize.Width * 4, RenderSize.Height * 4);
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

    protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
    {
        UpdateCameraBounds();
    }

    protected override void OnMouseWheel(MouseWheelEventArgs e)
    {
        double zoom = 0;

        if (e.Delta > 0)
            zoom = 0.1;
        else if (e.Delta < 0)
            zoom = -0.1;

        _camera.ZoomAt(e.GetPosition(this).X, e.GetPosition(this).Y, 1.0 + zoom);
    }

    protected override void OnMouseDown(MouseButtonEventArgs e)
    {
        Focus();
        _dragStart = e.GetPosition(this);
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);

        if (e.LeftButton == MouseButtonState.Pressed)
        {
            PanCamera(-(e.GetPosition(this).X - _dragStart.X), -(e.GetPosition(this).Y - _dragStart.Y));
            _dragStart = e.GetPosition(this);
        }
    }

    private void PanCamera(double dx, double dy)
    {
        _camera.PosX += dx;
        _camera.PosY += dy;
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
