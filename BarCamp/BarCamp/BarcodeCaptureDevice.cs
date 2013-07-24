using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using Windows.Phone.Media.Capture;
using Microsoft.Devices;
using System.Windows.Threading;
using System.Threading;
using System.Runtime.CompilerServices;
using ZXing;
using System.Windows.Media.Imaging;
using System.IO.IsolatedStorage;
using Microsoft.Phone;
using ZXing.QrCode;
using System.Windows.Controls;

namespace BarCamp
{
    public class BarcodeCaptureDevice : IDisposable
    {
        #region Fields

        public static readonly double AutoFocusIntervalInSecondsFrist = 2;
        public static readonly double AutoFocusIntervalInSeconds = 4;
        public static readonly double BarcodeDetectIntervalInSeconds = 0.5; //0.020;

        public static readonly int DefaultReaderSwitchCount = 20;

        private bool _autoFocus;
        private bool _autoDetectBarcode;
        internal PhotoCaptureDevice _device;
        private DispatcherTimer _focusTimer;

        private DispatcherTimer _barcodeTimer;
        private IBarcodeReader _qrcodeReader;
        private IBarcodeReader _barcodeReader;
        private int _readerSwitchCount = DefaultReaderSwitchCount;

        private bool _barcodeDecoding;

        private Windows.Foundation.Size? _previewSize;
        private bool _inited;
        private bool _capturing;
        private bool _focusing;
        private bool? _isFocusIlluminationModeSupported;
        private Windows.Foundation.Size _initRes;

        #endregion // Constructors

        #region Properties

        public bool AutoFocus
        {
            get { return _autoFocus; }
            set
            {
                if (_autoFocus != value)
                {
                    _autoFocus = value;
                    OnAutoFocusPropertyChanged();
                }
            }
        }

        public bool AutoDetectBarcode
        {
            get { return _autoDetectBarcode; }
            set
            {
                if (_autoDetectBarcode != value)
                {
                    _autoDetectBarcode = value;
                    OnAutoDetectBarcodePropertyChanged();
                }
            }
        }

        public Size Resolution
        {
            get
            {
                if (!_inited)
                    throw new InvalidOperationException();
                return new Size(_initRes.Width, _initRes.Height);
            }
        }

        public Size PreviewResolution
        {
            get
            {
                if (!_inited || !_previewSize.HasValue)
                    throw new InvalidOperationException();
                return new Size(_previewSize.Value.Width, _previewSize.Value.Height);
            }
        }

        #endregion // Properties

        #region Events

        public event EventHandler<BarcodeDetectedEventArgs> BarcodeDetected;

        #endregion // Events

        #region Constructor

        static BarcodeCaptureDevice()
        {
            if (Application.Current == null)
                return;

            Application.Current.UnhandledException += Current_UnhandledException;
        }

        static void Current_UnhandledException(object sender, ApplicationUnhandledExceptionEventArgs e)
        {
            if (!e.Handled)
            {
                e.Handled = BarcodeCaptureDeviceExceptions.ProcessUnhandledException(e.ExceptionObject);
            }
        }

        public BarcodeCaptureDevice()
        {
            _autoFocus = true;
        }

        #endregion // Constructor

        #region Init

        public async Task InitAsync()
        {
            ThrowIfDisposed();

            try
            {
                if (_inited)
                    throw new InvalidOperationException("already inited");

                // choose front or back
                CameraSensorLocation cameraLocation;
                if (PhotoCaptureDevice.AvailableSensorLocations.Contains(CameraSensorLocation.Back))
                {
                    var supportedResolutions = PhotoCaptureDevice.GetAvailableCaptureResolutions(CameraSensorLocation.Back);
                    _initRes = GetBestResolution(supportedResolutions);
                    cameraLocation = CameraSensorLocation.Back;
                }
                else if (PhotoCaptureDevice.AvailableSensorLocations.Contains(CameraSensorLocation.Front))
                {
                    var supportedResolutions = PhotoCaptureDevice.GetAvailableCaptureResolutions(CameraSensorLocation.Front);
                    _initRes = GetBestResolution(supportedResolutions);
                    cameraLocation = CameraSensorLocation.Front;
                }
                else
                {
                    throw new CriticalCameraNotSupportedException();
                }

                // open camera
                _device = await PhotoCaptureDevice.OpenAsync(cameraLocation, _initRes);

                #region retry when failed hack
                if (_device == null)
                {
                    try
                    {
                        await Task.Delay(200);
                        _device = await PhotoCaptureDevice.OpenAsync(cameraLocation, _initRes);
                    }
                    catch { }
                    if (_device == null)
                    {
                        await Task.Delay(500);
                        _device = await PhotoCaptureDevice.OpenAsync(cameraLocation, _initRes);
                    }
                }
                #endregion // retry when failed

                ThrowIfDisposed();

                // set flash mode, focus range...
                SetDeviceProperties(cameraLocation);

                var list = PhotoCaptureDevice.GetAvailablePreviewResolutions(cameraLocation);
                Windows.Foundation.Size resPreview = GetBestPreviewResolution(list, _initRes);

                await _device.SetPreviewResolutionAsync(resPreview);
                ThrowIfDisposed();

                _previewSize = resPreview;

                // init auto focus timer
                OnAutoFocusPropertyChanged();

                // init barcode reader
                OnAutoDetectBarcodePropertyChanged();

                // init frame report and barcode detect timer
                StartFrameTimer();

                _inited = true;
            }
            catch (ObjectDisposedException)
            {
                Dispose();         
                //throw new CardCaptureDeviceDisposedException();
            }
            catch (CriticalCameraNotSupportedException notSupportEx)
            {
                ThrowExceptionOrDisposed(notSupportEx, true);
            }
            catch
            {
                ThrowExceptionOrDisposed(new InitCameraFailedException(), true);
            }

            ThrowIfDisposed();

        }

        private void ThrowExceptionOrDisposed(Exception ex, bool dispose)
        {
            bool flag = this.IsDisposed;
            if (dispose)
                Dispose();
            if (flag)
                throw new CardCaptureDeviceDisposedException();
            else
                throw ex;
        }

        private void SetDeviceProperties(CameraSensorLocation camera)
        {
            // try set Flash Mode to Off
            try
            {
                _device.SetProperty(KnownCameraPhotoProperties.FlashMode, FlashState.Off);
            }
            catch
            {
                Debug.Assert(false, "Off FlashMode not supported");
            }

            // scene mode
            try
            {
                _device.SetProperty(KnownCameraPhotoProperties.SceneMode, CameraSceneMode.Macro);
            }
            catch
            {
                Debug.Assert(false, "Macro SceneMode not supported");
            }
        }

        // get highest capture resolution
        private Windows.Foundation.Size GetBestResolution(IReadOnlyList<Windows.Foundation.Size> supportedResolutions)
        {
            if (supportedResolutions.Count == 0)
                throw new CriticalCameraNotSupportedException();

            Windows.Foundation.Size maxSize = Windows.Foundation.Size.Empty;
            foreach (var size in supportedResolutions)
            {
                // width first
                if (size.Width > maxSize.Width)
                    maxSize = size;
                else if (size.Width == maxSize.Width && size.Height > maxSize.Height)
                    maxSize = size;
            }
            if (maxSize == Windows.Foundation.Size.Empty)
                throw new CriticalCameraNotSupportedException();

            return maxSize;
        }

        // get highest preview resolution
        private Windows.Foundation.Size GetBestPreviewResolution(IReadOnlyList<Windows.Foundation.Size> supportedResolutions, Windows.Foundation.Size initResolution)
        {
            if (supportedResolutions.Count == 0)
                throw new CriticalCameraNotSupportedException();

            Windows.Foundation.Size maxSize = Windows.Foundation.Size.Empty;
            double initRatio = initResolution.Width / initResolution.Height;
            foreach (var size in supportedResolutions)
            {
                if (!IsSameRatio(initRatio, size))
                    continue;

                if (size.Width > maxSize.Width)
                    maxSize = size;
                else if (size.Width == maxSize.Width && size.Height > maxSize.Height)
                    maxSize = size;
            }
            if (maxSize == Windows.Foundation.Size.Empty)
                throw new CriticalCameraNotSupportedException();
            return maxSize;
        }

        #endregion // Init

        #region Capture

        public async Task<Stream> CaptureAsync(bool focusBeforeCapture = true)
        {
            ThrowIfDisposed();

            if (!_inited)
                throw new InvalidOperationException("not inited");

            if (_capturing)
                throw new InvalidOperationException("is already capturing");

            MemoryStream stream = null;
            _capturing = true;
            try
            {
                CameraCaptureSequence seq = _device.CreateCaptureSequence(1);
                stream = new MemoryStream();
                seq.Frames[0].CaptureStream = stream.AsOutputStream();

                await _device.PrepareCaptureSequenceAsync(seq);
                ThrowIfDisposed();

                if (focusBeforeCapture)
                {
                    await FocusAsyncInternal();         // focus before capture
                }
                ThrowIfDisposed();

                #region Wait for auto focus hack
                if (_focusing)
                {
                    await AwaitWhenNotFocusing();
                    ThrowIfDisposed();
                }
                #endregion // Wait for auto focus hack

                await seq.StartCaptureAsync();
            }
            catch (ObjectDisposedException)
            {
                throw new CardCaptureDeviceDisposedException();
            }
            catch (Exception ex)
            {
                ThrowExceptionOrDisposed(new CaptureFailedException(ex.Message), false);
            }
            finally
            {
                _capturing = false;
            }


            ThrowIfDisposed();
            return stream;
        }

        private async Task AwaitWhenNotFocusing()
        {
            while (_focusing)
            {
                await Task.Delay(50).ConfigureAwait(false);
                ThrowIfDisposed();
            }
        }

        #endregion // Capture

        #region Focus

        private bool IsFocusIlluminationModeSupported
        {
            get
            {
                if (_isFocusIlluminationModeSupported == null)
                {
                    var query = PhotoCaptureDevice.GetSupportedPropertyValues(_device.SensorLocation, KnownCameraPhotoProperties.FocusIlluminationMode);
                    _isFocusIlluminationModeSupported = query != null && query.Count() > 0;
                }
                return _isFocusIlluminationModeSupported.Value;
            }
        }

        private void OnAutoFocusPropertyChanged()
        {
            //return;
            if (_autoFocus)
            {
                if (_focusTimer == null)
                {
                    _focusTimer = new DispatcherTimer();
                    _focusTimer.Interval = TimeSpan.FromSeconds(AutoFocusIntervalInSecondsFrist);
                    _focusTimer.Tick += AutoFocusTimer_Tick;
                }

                if (!_focusTimer.IsEnabled)
                    _focusTimer.Start();
            }
            else
            {
                if (_focusTimer != null && _focusTimer.IsEnabled)
                    _focusTimer.Stop();
            }
        }

        private async void AutoFocusTimer_Tick(object sender, EventArgs e)
        {
            ThrowIfDisposed();

            if (!_inited)
                return;

            if (_focusTimer.Interval == TimeSpan.FromSeconds(AutoFocusIntervalInSecondsFrist))
                _focusTimer.Interval = TimeSpan.FromSeconds(AutoFocusIntervalInSeconds);

            if (!_capturing && !_focusing)
            {
                // disable flash when auto focus
                object oldFlashMode = null;
                try
                {
                    if (IsFocusIlluminationModeSupported)
                    {
                        oldFlashMode = _device.GetProperty(KnownCameraPhotoProperties.FocusIlluminationMode);
                        _device.SetProperty(KnownCameraPhotoProperties.FocusIlluminationMode, FocusIlluminationMode.Off);
                    }
                    await FocusAsyncInternal();
                    ThrowIfDisposed();
                }
                catch
                {
                    if (this.IsDisposed)
                        throw new CardCaptureDeviceDisposedException();
                    else
                    {
                        Debug.Assert(false);
                    }
                }
                finally
                {
                    if (!IsDisposed && oldFlashMode != null)
                    {
                        _device.SetProperty(KnownCameraPhotoProperties.FocusIlluminationMode, oldFlashMode);
                    }
                }
            }
        }

        public async Task FocusAsync()
        {
            if (_focusing)                           // wait until not focusing
            {
                await AwaitWhenNotFocusing();
                ThrowIfDisposed();
                return;
            }
            await FocusAsyncInternal();
        }

        private async Task FocusAsyncInternal()
        {
            if (_inited)
            {
                if (!_focusing && PhotoCaptureDevice.IsFocusSupported(_device.SensorLocation))
                {
                    _focusing = true;
                    try
                    {
                        await _device.FocusAsync();
                    }
                    finally
                    {
                        _focusing = false;
                    }
                }
            }
        }

        #endregion // Auto Focus

        #region Barcode Detect

        private async void BarcodeTimer_Tick(object sender, EventArgs e)
        {
            ThrowIfDisposed();
            if (!_inited)
                return;

            if (!_previewSize.HasValue) return;
            int width = (int)_previewSize.Value.Width;
            int height = (int)_previewSize.Value.Height;

            // for barcode
            if (AutoDetectBarcode)
            {
                if (_barcodeDecoding == true)
                    return;
                _barcodeDecoding = true;
                try
                {
                    var result = await DetectBarcodeAsync(width, height);

                    if (result != null)
                    {
                        // raise BarcodeDetected
                        BarcodeDetectedEventArgs args = new BarcodeDetectedEventArgs(result);
                        args.HostImageSize = new Size(width, height);
                        if (args.IsQRCode && result.ResultMetadata.ContainsKey(ResultMetadataType.QRCODE_DIMENSION))
                        {
                            int dimension = (int)result.ResultMetadata[ResultMetadataType.QRCODE_DIMENSION];
                            Debug.Assert(result.ResultPoints.Length >= 3);
                            if (result.ResultPoints.Length >= 3)
                            {
                                var p1 = new Point(result.ResultPoints[0].X, result.ResultPoints[0].Y);
                                var p2 = new Point(result.ResultPoints[1].X, result.ResultPoints[1].Y);
                                var p3 = new Point(result.ResultPoints[2].X, result.ResultPoints[2].Y);
                                args.BarcodePosition = new BarcodePosition(p1, p2, p3, dimension, width, height);
                            }
                        }

                        if (BarcodeDetected != null)
                        {
                            BarcodeDetected(this, args);

                        }
                    }
                }
                catch (ObjectDisposedException)
                {
                    throw new CardCaptureDeviceDisposedException();
                }
                catch
                {
                    
                }
                finally
                {
                    _barcodeDecoding = false;
                }
            }

            ThrowIfDisposed();
        }

        private Task<Result> DetectBarcodeAsync(int width, int height)
        {
            return Task.Run(() =>
            {
                return DetectBarcode(width, height);
            });
        }

        private Result DetectBarcode(int width, int height)
        {
            var start = DateTime.Now;
            int bufferSize = width * height;

            #region Reader Switch Hack
            IBarcodeReader reader;
            if (_readerSwitchCount == 0)
            {
                _readerSwitchCount = DefaultReaderSwitchCount;
                reader = _barcodeReader;
            }
            else
            {
                _readerSwitchCount--;
                reader = _qrcodeReader;
            }
            #endregion // Reader Switch Hack

            byte[] buffer = new byte[bufferSize];
            _device.GetPreviewBufferY(buffer);

            var decodeStart = DateTime.Now;
            Result result = reader.Decode(buffer, width, height, RGBLuminanceSource.BitmapFormat.Gray8);

            var span = DateTime.Now - decodeStart;
            var total = DateTime.Now - start;
            //Debug.WriteLine("Barcode Decoded: " + span.TotalMilliseconds + "\tTotal: " + total.TotalMilliseconds);

            return result;
        }

        private void OnAutoDetectBarcodePropertyChanged()
        {
            if (_autoDetectBarcode)
            {
                if (_qrcodeReader == null)
                {
                    _qrcodeReader = new BarcodeReader(new QRCodeReader(), null, null);
                }

                if (_barcodeReader == null)
                {
                    _barcodeReader = new BarcodeReader();
                }
            }
            else
            {
                // do nothing
            }
        }

        private static bool IsSameRatio(double ratio, Windows.Foundation.Size size)
        {
            var right = size.Width / size.Height;
            return Math.Abs(ratio - right) < 0.01;
        }

        private void StartFrameTimer()
        {
            if (_barcodeTimer == null)
            {
                _barcodeTimer = new DispatcherTimer();
                _barcodeTimer.Interval = TimeSpan.FromSeconds(BarcodeDetectIntervalInSeconds);
                _barcodeTimer.Tick += BarcodeTimer_Tick;
            }

            if (!_barcodeTimer.IsEnabled)
                _barcodeTimer.Start();

            // won't stop until dispose
        }

        #endregion // Barcode Detect

        #region Barcode Generate

        public static WriteableBitmap GenerateBarcode(string text, int size = 400)
        {
            try
            {
                var writer = new BarcodeWriter();
                writer.Options.Width = writer.Options.Height = size;
                writer.Format = BarcodeFormat.QR_CODE;
                writer.Options.Hints.Add(EncodeHintType.CHARACTER_SET, "UTF-8");
                var bitmap = writer.Write(text);
                return bitmap;
            }
            catch (WriterException writerEx)
            {
                if (writerEx.Message == "Data too big")
                {
                    throw new DataTooBigException();
                }
                else
                {
                    throw new GenerateBarcodeException("generate barcode failed", writerEx);
                }
            }
            catch (Exception ex)
            {
                throw new GenerateBarcodeException("generate barcode failed", ex);
            }
        }

        #endregion // Barcode Generate

        #region Dispose

        private void ThrowIfDisposed()
        {
            return;
            if (IsDisposed)  
                throw new CardCaptureDeviceDisposedException();
        }

        public bool IsDisposed { get; private set; }

        /// <summary>
        /// dispose capture device，disable auto focus and barcode detect
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~BarcodeCaptureDevice()
        {
            this.Dispose(false);
        }

        protected virtual void Dispose(bool disposing)
        {
            IsDisposed = true;

            if (disposing)
            {
                // safe to access member object

                // make sure we dispose device, otherwise other app will not be able to use it
                if (_device != null /*&& !_capturing && !_focusing && !_barcodeDecoding*/)
                {
                    _device.Dispose();
                    _device = null;
                }

                // stop focus timer
                if (_focusTimer != null)
                {
                    if (_focusTimer.IsEnabled) _focusTimer.Stop();
                    _focusTimer = null;
                }

                // stop barcode timer
                if (_barcodeTimer != null)
                {
                    if (_barcodeTimer.IsEnabled) _barcodeTimer.Stop();
                    _barcodeTimer = null;
                }

                // events
                BarcodeDetected = null;
            }
        }

        #endregion // Dispose

        #region Video Brush Support

        public void BindVideoBrush(VideoBrush brush)
        {
            if (_device == null)
                throw new InvalidOperationException("device is null");

            brush.SetSource(_device);
        }

        #endregion
    }

}
