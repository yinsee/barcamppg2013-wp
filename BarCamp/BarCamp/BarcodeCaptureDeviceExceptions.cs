using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace BarCamp
{
    public class ReportExceptionMessageEventArgs
    {
        public string Message { get; set; }
        public Exception Exception { get; set; }
        public bool Handled { get; set; }

        public ReportExceptionMessageEventArgs(string msg, Exception ex)
        {
            Message = msg;
            Exception = ex;
        }
    }

    public class BarcodeCaptureDeviceExceptions
    {
        public static bool DisableUnhandledExceptionProcessing { get; set; }

        public static event EventHandler<ReportExceptionMessageEventArgs> ReportExceptionMessage;

        public static bool ProcessUnhandledException(Exception exception)
        {
            if (DisableUnhandledExceptionProcessing)
                return false;

            bool handled = true;

            try
            {
                throw exception;
            }
            catch (CriticalCameraNotSupportedException)
            {
                
            }
            catch (CardCaptureDeviceDisposedException)
            {
                
            }
            catch (InitCameraFailedException)
            {
                
            }
            catch (CaptureFailedException)
            {
                
            }
            catch (DataTooBigException)
            {
                
            }
            catch (GenerateBarcodeException)
            {
                
            }
            catch
            {
                handled = false;
            }

            return handled;
        }

        private static void ReportMessageInternal(string message, Exception exception)
        {
            bool handled = false;
            if (ReportExceptionMessage != null)
            {
                var args = new ReportExceptionMessageEventArgs(message, exception);
                ReportExceptionMessage(null, args);
                handled = args.Handled;
            }
            
            if (!handled)
                MessageBox.Show(message);
        }
    }

    public class CriticalCameraNotSupportedException : Exception
    {

    }

    public class InitCameraFailedException : Exception
    { 
    
    }

    public class CardCaptureDeviceDisposedException : ObjectDisposedException
    {
        public CardCaptureDeviceDisposedException()
            : base("CardCaptureDevice")
        {

        }
    }

    public class CaptureFailedException : Exception
    {
        public CaptureFailedException() { }
        public CaptureFailedException(string message) : base(message) { }
        public CaptureFailedException(string message, Exception inner) : base(message, inner) { }
    }

    public class GenerateBarcodeException : Exception
    {
        public GenerateBarcodeException() { }
        public GenerateBarcodeException(string message) : base(message) { }
        public GenerateBarcodeException(string message, Exception inner) : base(message, inner) { }
    }

    public class DataTooBigException : GenerateBarcodeException
    {

    }

}
