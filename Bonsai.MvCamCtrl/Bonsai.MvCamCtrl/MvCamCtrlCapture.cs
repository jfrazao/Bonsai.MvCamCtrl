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

namespace Bonsai.MvCamCtrl
{
    [XmlType(Namespace = Constants.XmlNamespace)]
    [Description("Acquires a sequence of images from a HikRobot camera using the MvCamCtrl software.")]

    public class MvCamCtrlCapture : Source<MvCamCtrlDataFrame>
    {
        static readonly object cameraLock = new object();

        [Description("The optional index of the camera from which to acquire images.")]
        public int? Index { get; set; }

        //[TypeConverter(typeof(SerialNumberConverter))]
        [Description("The optional serial number of the camera from which to acquire images.")]
        public string SerialNumber { get; set; }
        public override IObservable<MvCamCtrlDataFrame> Generate()
        {
            return Generate(Observable.Return(Unit.Default));
        }
        //[Description("The method used to process bayer color images.")]
        //public ColorProcessingAlgorithm ColorProcessing { get; set; }
        public IObservable<MvCamCtrlDataFrame> Generate<TSource>(IObservable<TSource> start)
        {
            throw new NotImplementedException();

            /*
            return Observable.Create<MvCamCtrlDataFrame>((observer, cancellationToken) =>
            {
                return Task.Factory.StartNew(async () =>
                {
                    int nRet = CErrorDefine.MV_OK;
                    lock (cameraLock)
                    {
                        //var configFile = ParameterFile;
                        var camera = new CCamera();
                        // using pixelDataConverter?
                        try
                        {

                            var serialNumber = SerialNumber;
                            var ltDeviceList = new List<CCameraInfo>();
                            //var cinfo = new CUSBCameraInfo();
                            //cinfo.chSerialNumber = "blabla";
                            var cc = new CCameraInfo();
                            nRet = CSystem.EnumDevices(CSystem.MV_GIGE_DEVICE | CSystem.MV_USB_DEVICE, ref ltDeviceList);

                            if (!string.IsNullOrEmpty(serialNumber))
                            {

                                //nRet = camera.CreateHandle(ref cinfo);
                                if (camera == null)
                                {
                                    var message = string.Format("MvCamCtr camera with serial number {0} was not found.", serialNumber);
                                    throw new InvalidOperationException(message);
                                }
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
                                //camera = cameraList.GetByIndex((uint)index);
                            }
                            ltDeviceList.Clear();
                            //cameraList.Clear();

                        }
                        catch (Exception ex)
                        {
                            observer.OnError(ex);
                            throw;
                        }
                    }

                    try
                    {
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

                        //camera.BeginAcquisition();
                        nRet = camera.StartGrabbing();
                        if (CErrorDefine.MV_OK != nRet)
                        {
                            camera.DestroyHandle();
                            var message = string.Format("Start grabbing failed:{0:x8}", nRet);
                            throw new InvalidOperationException(message);
                        }

                        await start;

                        var imageFormat = default(PixelFormatEnums);
                        var converter = default(Func<IManagedImage, IplImage>);

                        using (var system = new ManagedSystem()) //(var cancellation = cancellationToken.Regi   ster(hikCamera.CloseDevice))
                        {
                            while (!cancellationToken.IsCancellationRequested)
                            {
                                using (var image = hikCamera.GetImageBuffer()
                                {
                                    if (image.IsIncomplete)
                                    {
                                        // drop incomplete frames
                                        continue;
                                    }

                                    if (converter == null || image.PixelFormat != imageFormat)
                                    {
                                        converter = GetConverter(image.PixelFormat, ColorProcessing);
                                        imageFormat = image.PixelFormat;
                                    }

                                    var output = converter(image);
                                    observer.OnNext(new MvCamCtrlDataFrame(output, image.ChunkData));
                                }
                            }
                        }
                    }
                    catch (Exception ex) { observer.OnError(ex); throw; }
                    finally
                    {
                        nRet = hikCamera.StopGrabbing();
                        if (CErrorDefine.MV_OK != nRet)
                        {
                            var message = string.Format("Stop grabbing failed:{0:x8}", nRet);
                            throw new InvalidOperationException(message);
                        }

                        // ch:关闭设备 | en:Close device
                        nRet = hikCamera.CloseDevice();
                        if (CErrorDefine.MV_OK != nRet)
                        {
                            var message = string.Format("Close device failed:{0:x8}", nRet);
                            throw new InvalidOperationException(message);
                        }

                        // ch:销毁设备 | en:Destroy device
                        nRet = hikCamera.DestroyHandle();
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
            });*/
        }
    }
}
