using OpenCV.Net;
using Bonsai;
using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Xml.Serialization;
using System.Threading.Tasks;
using MvCamCtrl.NET;
using System.Reactive;
using Bonsai.Expressions;
using System.Xml.Schema;
using MvCamCtrl.NET.CameraParams;
using System.Drawing.Imaging;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Diagnostics.Eventing.Reader;
using static System.Net.Mime.MediaTypeNames;
using Bonsai.Reactive;

namespace Bonsai.MvCamCtrl
{
    [XmlType(Namespace = Constants.XmlNamespace)]
    [Description("Acquires a sequence of images from a HikRobot camera using the MvCamCtrl api.")]

    public class MvCamCtrlCapture : Source<IplImage>
    {
        static readonly object cameraLock = new object();

        [Description("The optional index of the camera from which to acquire images.")]
        public int? Index { get; set; }

        //[TypeConverter(typeof(SerialNumberConverter))]
        [Description("The optional serial number of the camera from which to acquire images.")]
        public string SerialNumber { get; set; }
        public override IObservable<IplImage> Generate()
        {
            return Generate(Observable.Return(Unit.Default));
        }
        //[Description("The method used to process bayer color images.")]
        //public ColorProcessingAlgorithm ColorProcessing { get; set; }
        public IObservable<IplImage> Generate<TSource>(IObservable<TSource> start)
        {
            //throw new NotImplementedException();


            return Observable.Create<IplImage>((observer, cancellationToken) =>
            {
                return Task.Factory.StartNew(async () =>
                {
                    int nRet = CErrorDefine.MV_OK;
                    var camera = new CCamera();
                    lock (cameraLock)
                    {
                        //var configFile = ParameterFile;
                        // using pixelDataConverter?
                        try
                        {

                            var serialNumber = SerialNumber;
                            var ltDeviceList = new List<CCameraInfo>();
                            var cc = new CCameraInfo();
                            nRet = CSystem.EnumDevices(CSystem.MV_GIGE_DEVICE | CSystem.MV_USB_DEVICE, ref ltDeviceList);

                            if (!string.IsNullOrEmpty(serialNumber))
                            {
                                var myDeviceInfo = ltDeviceList.Find(value => value.nTLayerType == CSystem.MV_USB_DEVICE && (value as CUSBCameraInfo).chSerialNumber == serialNumber);

                                nRet = camera.CreateHandle(ref myDeviceInfo);
                                if (camera == null)
                                {
                                    var message = string.Format("MvCamCtr camera with serial number {0} was not found.", serialNumber);
                                    throw new InvalidOperationException(message);
                                }
                                // en:Open device
                                nRet = camera.OpenDevice();
                                if (CErrorDefine.MV_OK != nRet)
                                {
                                    camera.DestroyHandle();
                                    var message = string.Format("Open device MVcamCtr failed with serial number {0}.", serialNumber);
                                    throw new InvalidOperationException(message);
                                }
                                //camera = cameraList.GetByIndex((uint)index);
                            }
                            else
                            {
                                var index = Index.GetValueOrDefault(0);
                                if (index < 0 || index >= ltDeviceList.Count)
                                {
                                    var message = string.Format("No MvCamCtr camera was found at index {0}.", index);
                                    throw new InvalidOperationException(message);
                                }

                                CCameraInfo stDevice = ltDeviceList[index];

                                // en:Create device
                                nRet = camera.CreateHandle(ref stDevice);
                                if (CErrorDefine.MV_OK != nRet)
                                {
                                    var message = string.Format("No MvCamCtr camera was found at index {0}.", index);
                                    throw new InvalidOperationException(message);
                                }

                                // en:Open device
                                nRet = camera.OpenDevice();
                                if (CErrorDefine.MV_OK != nRet)
                                {
                                    camera.DestroyHandle();
                                    var message = string.Format("Open device MVcamCtr failed at index {0}.", index);
                                    throw new InvalidOperationException(message);
                                }
                            }
                            ltDeviceList.Clear();
                        }
                        catch (Exception ex)
                        {
                            observer.OnError(ex);
                            throw;
                        }
                    }

                    try
                    {
                        //TODO: REMOVE THE FOLLOWING CODE 
                        //camera.Init();
                        camera.SetEnumValue("AcquisitionMode", (uint)MV_CAM_ACQUISITION_MODE.MV_ACQ_MODE_CONTINUOUS);
                        // en error handling

                        // en:set trigger mode as off
                        nRet = camera.SetEnumValue("TriggerMode", (uint)MV_CAM_TRIGGER_MODE.MV_TRIGGER_MODE_OFF);
                        if (CErrorDefine.MV_OK != nRet)
                        {
                            camera.DestroyHandle();
                            var message = string.Format("Set TriggerMode failed:{0:x8}", nRet);
                            throw new InvalidOperationException(message);
                        }
                        //Configure(camera);
                        nRet = camera.StartGrabbing();

                        if (CErrorDefine.MV_OK != nRet)
                        {
                            camera.DestroyHandle();
                            var message = string.Format("Start grabbing failed:{0:x8}", nRet);
                            throw new InvalidOperationException(message);
                        }

                        await start;

                        //var imageFormat = default(PixelFormatEnums);
                        //var converter = default(Func<IManagedImage, IplImage>);
                        CFrameout pcFrameInfo = new CFrameout();
                        //CPixelConvertParam pcConvertParam = new CPixelConvertParam();
                        using (var cancellation = cancellationToken.Register(() => camera.CloseDevice()))
                        {
                            while (!cancellationToken.IsCancellationRequested)
                            {
                                IplImage result = null;
                                nRet = camera.GetImageBuffer(ref pcFrameInfo, 1000);
                                lock (cameraLock)
                                {

                                    // BitmapData m_pcBitmapData = m_pcBitmap.LockBits(new Rectangle(0, 0, pcConvertParam.InImage.Width, pcConvertParam.InImage.Height), ImageLockMode.ReadWrite, m_pcBitmap.PixelFormat);
                                    //Marshal.Copy(pcConvertParam.OutImage.ImageData, 0, m_pcBitmapData.Scan0, (Int32)pcConvertParam.OutImage.ImageData.Length);
                                    //var pcImgForDriver = pcFrameInfo.Image.Clone() as CImage;
                                    var pcImgForDriver = pcFrameInfo.Image;

                                    var pcImgSpecInfo = pcFrameInfo.FrameSpec;
  
                                    IntPtr unmanagedPointer = Marshal.AllocHGlobal(pcImgForDriver.ImageData.Length);
                                    Marshal.Copy(pcImgForDriver.ImageData, 0, unmanagedPointer, pcImgForDriver.ImageData.Length);
                                    if (pcImgForDriver.PixelType == MvGvspPixelType.PixelType_Gvsp_Mono8)
                                    {
                                        using (var bitmapHeader = new IplImage(new OpenCV.Net.Size(pcImgForDriver.Width, pcImgForDriver.Height), IplDepth.U8, 1, unmanagedPointer))
                                        {
                                            result = new IplImage(bitmapHeader.Size, bitmapHeader.Depth, bitmapHeader.Channels);
                                            CV.Copy(bitmapHeader, result);
                                        }
                                    }
                                    else if (pcImgForDriver.PixelType == MvGvspPixelType.PixelType_Gvsp_BGR8_Packed)
                                    {
                                        using (var bitmapHeader = new IplImage(new OpenCV.Net.Size(pcImgForDriver.Width, pcImgForDriver.Height), IplDepth.U8, 3, unmanagedPointer))
                                        {
                                            result = new IplImage(bitmapHeader.Size, bitmapHeader.Depth, bitmapHeader.Channels);
                                            CV.Copy(bitmapHeader, result);
                                        }
                                    }
                                    Marshal.FreeHGlobal(unmanagedPointer);
                                    camera.FreeImageBuffer(ref pcFrameInfo);
                                }
                                observer.OnNext(result);
                                //camera.DisplayOneFrame(ref pcDisplayInfo);

                                //camera.FreeImageBuffer(ref pcFrameInfo);
                            }
                        }
                    }
                    catch (Exception ex) { observer.OnError(ex); throw; }
                    finally
                    {
                        nRet = camera.StopGrabbing();
                        if (CErrorDefine.MV_OK != nRet)
                        {
                            var message = string.Format("Stop grabbing failed:{0:x8}", nRet);
                            throw new InvalidOperationException(message);
                        }

                        // ch:关闭设备 | en:Close device
                        nRet = camera.CloseDevice();
                        if (CErrorDefine.MV_OK != nRet)
                        {
                            var message = string.Format("Close device failed:{0:x8}", nRet);
                            throw new InvalidOperationException(message);
                        }

                        // ch:销毁设备 | en:Destroy device
                        nRet = camera.DestroyHandle();
                        if (CErrorDefine.MV_OK != nRet)
                        {
                            var message = string.Format("Destroy device failed:{0:x8}", nRet);
                            throw new InvalidOperationException(message);
                        }
                        //camera.DeInit();
                        //camera.Dispose();
                    }
                },
                cancellationToken,
                TaskCreationOptions.LongRunning,
                TaskScheduler.Default);
            });
        }
    }
}
