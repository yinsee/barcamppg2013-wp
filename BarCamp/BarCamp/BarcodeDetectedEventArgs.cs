using System;
using System.Windows;
using System.Windows.Media;
using ZXing;
using System.Windows.Controls;

namespace BarCamp
{
    public class BarcodeDetectedEventArgs
    {
        private string _rawContent;

        private Border _barcodeBorder;
        private PathFigure _barcodePath;
        private PolyLineSegment _barcodePolyLine;

        private Border _vufBarcodeBorder;
        private PathFigure _vufBarcodePath;
        private PolyLineSegment _vufBarcodePolyLine;

        private Func<double, double> _pointMappingX;
        private Func<double, double> _pointMappingY;

        public bool IsQRCode { get; private set; }

        /// <summary>
        /// raw text
        /// </summary>
        public string Text { get { return _rawContent; } }

        /// <summary>
        /// size
        /// </summary>
        public Size HostImageSize { get; set; }

        /// <summary>
        /// position for QRCode
        /// </summary>
        public BarcodePosition BarcodePosition { get; set; }

        /// <summary>
        /// Result object from zxing
        /// </summary>
        public Result Result { get; set; }

        internal BarcodeDetectedEventArgs(Result result)
        {
            this.Result = result;
            _rawContent = result.Text;
            IsQRCode = result.BarcodeFormat == BarcodeFormat.QR_CODE;
        }

        public Border GetBarcodeBorderUIVideoUniformFill(double width, double height)
        {
            try
            {
                if (IsQRCode)
                {
                    CreateBarcodeBorder(width, height);
                    UpdateBarcodeBorder(width, height);

                    return _vufBarcodeBorder;
                }
            }
            catch { }

            return null;
        }

        private void UpdateBarcodeBorder(double width, double height)
        {
            InitPointMapping(HostImageSize.Width, HostImageSize.Height, width, height);
            _vufBarcodePath.StartPoint = ImagePoint2UIPoint(BarcodePosition.BottomLeft);
            _vufBarcodePolyLine.Points.Clear();
            _vufBarcodePolyLine.Points.Add(ImagePoint2UIPoint(BarcodePosition.TopLeft));
            _vufBarcodePolyLine.Points.Add(ImagePoint2UIPoint(BarcodePosition.TopRight));
            _vufBarcodePolyLine.Points.Add(ImagePoint2UIPoint(BarcodePosition.BottomRight));
        }

        private void CreateBarcodeBorder(double width, double height, Color? borderColor = null, Brush backBrush = null)
        {
            if (_vufBarcodeBorder == null)
            {
                _vufBarcodeBorder = new Border();
                _vufBarcodeBorder.Width = width;
                _vufBarcodeBorder.Height = height;

                Deployment.Current.Dispatcher.CheckAccess();

                Canvas canvas = new Canvas();
                _vufBarcodeBorder.Child = canvas;

                canvas.HorizontalAlignment = HorizontalAlignment.Left;
                canvas.VerticalAlignment = VerticalAlignment.Top;

                System.Windows.Shapes.Path path = new System.Windows.Shapes.Path();
                path.StrokeThickness = 4;
                canvas.Children.Add(path);

                if (borderColor == null) borderColor = Colors.White;
                path.Stroke = new SolidColorBrush(borderColor.Value);
                path.Fill = backBrush;

                PathGeometry pg = new PathGeometry();
                _vufBarcodePolyLine = new PolyLineSegment();
                _vufBarcodePath = new PathFigure();
                path.Data = pg;
                pg.Figures.Add(_vufBarcodePath);
                _vufBarcodePath.Segments.Add(_vufBarcodePolyLine);
                _vufBarcodePath.IsClosed = true;
            }
        }

        private void InitPointMapping(double sourceWidth, double sourceHeight, double targetWidth, double targetHeight)
        {
            double w = sourceWidth;
            double h = sourceHeight;
            double w1 = targetWidth;
            double h1 = targetHeight;

            double r = w / h;
            double r1 = w1 / h1;
            if (r1 < r)
            {
                double n = h1 / h;
                double offset = (w1 - h1 * r) / 2;
                _pointMappingX = x => n * x + offset;
                _pointMappingY = y => n * y;
            }
            else
            {
                double n = w1 / w;
                double offset = (h1 - w1 / r) / 2;
                _pointMappingX = x => n * x;
                _pointMappingY = y => n * y + offset;
            }
        }

        private Point ImagePoint2UIPoint(Point inPoint)
        {
            double x = _pointMappingX(inPoint.X);
            double y = _pointMappingY(inPoint.Y);
            return new Point(x, y);
        }
    }

}
