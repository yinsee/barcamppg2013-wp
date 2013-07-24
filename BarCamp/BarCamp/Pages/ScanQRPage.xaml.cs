using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using ZXing;

namespace BarCamp.Pages
{
    public partial class ScanQRPage : PhoneApplicationPage
    {
        private BarcodeCaptureDevice _device;

        private bool _cameraFlag;
        public ScanQRPage()
        {
            InitializeComponent();
            callCamera();
        }
        private async void callCamera()
        {
            _cameraFlag = true;
            await StartCamera();
        }
        private async System.Threading.Tasks.Task StartCamera()
        {
            if (_device == null)
            {
                _device = new BarcodeCaptureDevice();
                _device.AutoFocus = true;
                _device.AutoDetectBarcode = true;
                await _device.InitAsync();
                previewTransform.Rotation = _device._device.SensorRotationInDegrees;
                barcodeUITransform.Rotation = _device._device.SensorRotationInDegrees;
                _device.BindVideoBrush(previewVideo);

                _device.BarcodeDetected += BarcodeDetected;

            }
        }
        private void BarcodeDetected(object sender, BarcodeDetectedEventArgs e)
        {
            _device.AutoDetectBarcode = false;
            barCodeBorder.Child = e.GetBarcodeBorderUIVideoUniformFill(barCodeBorder.ActualWidth, barCodeBorder.ActualHeight);
            DisplayResult(e.Result);

            _device.Dispose();
            _device = null;
            NavigationService.GoBack();
        }
        private void DisplayResult(Result result)
        {

            string msg = "";
            if (result != null)
            {
                //BarcodeType.SelectedItem = result.BarcodeFormat;
                //BarcodeContent.Text = result.Text;
                msg = "Fmt: " + result.BarcodeFormat.ToString() + "\nTxt: " + result.Text;
                //NavigationService.GoBack(); //go back first, but camera not yet stop
                //StopCamera();
            }
            else
            {
                //BarcodeContent.Text = "No barcode found.";
                msg = "No barcode found.";
            }
            //MessageBox.Show(msg);
            App.StringGetFromScanner = msg;
        }
    }
}