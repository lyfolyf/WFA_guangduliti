using System;
using System.Collections.Generic;
using System.Text;
using MvCamCtrl.NET;
using System.Runtime.InteropServices;
using System.Threading;
using HalconDotNet;
using System.Drawing;
namespace IHalconHikvision
{
    public class ImageProvider
    {
        public class Image
        {
            internal  int mWidth; /* The width of the image. */
            public int Width
            {
                get
                {
                    return mWidth;
                }
            }
            internal int mHeight; /* The height of the image. */
            public int Height
            {
                get
                {
                    return mHeight;
                }
            }
            internal Byte[] mBuffer; /* The raw image data. */
            public Byte[] Buffer
            {
                get
                {
                    return mBuffer;
                }
            }
            internal bool mColor; /* If false the buffer contains a Mono8 image. Otherwise, RGBA8packed is provided. */
            public bool Color
            {
                get
                {
                    return mColor;
                }
            }
        }
        public delegate void ImageReadyEventHandler();
        public event ImageReadyEventHandler ImageReadyEvent;
        protected Object m_lockObject;
        protected MyCamera m_pMyCamera;           
        protected bool m_open = false;
        protected List<Image> m_grabbedBuffers; /* List of grab results already grabbed. */
        private MyCamera.cbOutputdelegate ImageCallback;
        private void ImageCallbackFunc(IntPtr pData, ref MyCamera.MV_FRAME_OUT_INFO stFrameInfo, IntPtr pUser)
        {
            int nRet;
            Image mImage = new Image();
            mImage.mHeight = stFrameInfo.nHeight;
            mImage.mWidth = stFrameInfo.nWidth;
            MyCamera.MvGvspPixelType enDstPixelType;
            if (stFrameInfo.enPixelType == MyCamera.MvGvspPixelType.PixelType_Gvsp_Mono8)
            {
                enDstPixelType = MyCamera.MvGvspPixelType.PixelType_Gvsp_Mono8;
                mImage.mColor = false;
            }
            else
            {
                mImage.mColor = true;
                enDstPixelType = MyCamera.MvGvspPixelType.PixelType_Gvsp_RGB8_Packed;
            }
            mImage.mBuffer = new byte[3*stFrameInfo.nFrameLen+2048];
            IntPtr pImage = Marshal.UnsafeAddrOfPinnedArrayElement(mImage.mBuffer, 0);
            MyCamera.MV_PIXEL_CONVERT_PARAM stSaveParam = new MyCamera.MV_PIXEL_CONVERT_PARAM();
            stSaveParam.nWidth = stFrameInfo.nWidth;
            stSaveParam.nHeight = stFrameInfo.nHeight;
            stSaveParam.pSrcData = pData;
            stSaveParam.nSrcDataLen = stFrameInfo.nFrameLen;
            stSaveParam.enSrcPixelType = stFrameInfo.enPixelType;
            stSaveParam.enDstPixelType = enDstPixelType;
            stSaveParam.pDstBuffer = pImage;
            stSaveParam.nDstBufferSize =(uint)mImage.mBuffer.Length;
            nRet = m_pMyCamera.MV_CC_ConvertPixelType_NET(ref stSaveParam);
            lock (m_lockObject) /* Lock the grab result queue to avoid that two threads modify the same data. */
            {
                m_grabbedBuffers.Add(mImage); /* Add the new grab result to the queue. */
            }
            OnImageReadyEvent();
        }
        public ImageProvider()
        {
            m_lockObject = new Object();
            m_pMyCamera = new MyCamera();
            m_grabbedBuffers = new List<Image>();
        }
        /* Indicates that ImageProvider and device are open. */
        public bool IsOpen
        {
            get { return m_open; }
        }
        public void Open(uint index)
        {
            
            //ch:获取选择的设备信息 | en:Get Used Device Info
            MyCamera.MV_CC_DEVICE_INFO device =
               (MyCamera.MV_CC_DEVICE_INFO)Marshal.PtrToStructure(DeviceEnumerator.m_pDeviceList.pDeviceInfo[index],
                                                             typeof(MyCamera.MV_CC_DEVICE_INFO));
            //ch:打开设备 | en:Open device
            //ch:打开设备 | en:Open Device
            int nRet = m_pMyCamera.MV_CC_CreateDevice_NET(ref device);
            if (MyCamera.MV_OK != nRet)
            {
                m_pMyCamera.MV_CC_CloseDevice_NET();
                throw new Exception("Create Camera failed!:" + nRet.ToString());
            }
            nRet = m_pMyCamera.MV_CC_OpenDevice_NET();
            if (MyCamera.MV_OK != nRet)
            {
                m_pMyCamera.MV_CC_CloseDevice_NET();
                throw new Exception("Device open fail!:" + nRet.ToString());
            }
            // ch:注册回调函数 | en:Register image callback
            ImageCallback = new MyCamera.cbOutputdelegate(ImageCallbackFunc);
            nRet = m_pMyCamera.MV_CC_RegisterImageCallBack_NET(ImageCallback, IntPtr.Zero);
            if (MyCamera.MV_OK != nRet)
            {
                throw new Exception("Register image callback failed!");
            }
            //ch:设置采集连续模式 | en:Set Continues Aquisition Mode
            m_pMyCamera.MV_CC_SetEnumValue_NET("AcquisitionMode", 2);
            m_pMyCamera.MV_CC_SetEnumValue_NET("TriggerMode", 0);
            // ch:触发源选择:0 - Line0; | en:Trigger source select:0 - Line0;
            //           1 - Line1;
            //           2 - Line2;
            //           3 - Line3;
            //           4 - Counter;
            //           7 - Software;
            m_pMyCamera.MV_CC_SetEnumValue_NET("TriggerSource", 0);
            m_open = true;
        }
        /* Close the device */
        public void Close()
        {
            //ch:关闭设备 | en:Close Device 
            if (m_open)
            {
                m_open = false;
                int nRet;

                nRet = m_pMyCamera.MV_CC_CloseDevice_NET();
                if (MyCamera.MV_OK != nRet)
                {
                    return;
                }
                nRet = m_pMyCamera.MV_CC_DestroyDevice_NET();
                if (MyCamera.MV_OK != nRet)
                {
                    return;
                }
            }
        }
        /* Stops the grabbing of images. */
        public void Stop()
        {
            if (m_open) /* Only start when open and grabbing. */
            {
                int nRet = -1;
                //ch:停止采集 | en:Stop Grabbing
                nRet = m_pMyCamera.MV_CC_StopGrabbing_NET();
                if (nRet != MyCamera.MV_OK)
                {
                    throw new Exception("MV_CC_StopGrabbing_NET Failed！:" + nRet.ToString());
                }
            }
        }
        /* Starts the grabbing of images. */
        public void Start()
        {
            Flush();
            if (m_open) /* Only start when open and grabbing. */
            {
                int nRet;
                //ch:开始采集 | en:Start Grabbing
                nRet = m_pMyCamera.MV_CC_StartGrabbing_NET();
                if (MyCamera.MV_OK != nRet)
                {
                    throw new Exception("MV_CC_StartGrabbing_NET Failed！:"+nRet.ToString());
                }
            }
        }
        public bool ExposureTime(uint time)
        {
            bool result = false;
            if (m_open)
            {
                try
                {
                    if (time < 0)
                    {
                        throw new Exception("曝光时间不能小于0");
                    }
                    int nRet;
                   // bool bSuccess = true;
                    m_pMyCamera.MV_CC_SetEnumValue_NET("ExposureAuto", 0);
                    nRet = m_pMyCamera.MV_CC_SetFloatValue_NET("ExposureTime", time);
                    if (nRet != MyCamera.MV_OK)
                    {
                        throw new Exception("Device Set Exposure Time Failed!");
                    }
                    result = true;
                }
                catch (Exception ex)
                {
                    throw new Exception(ex.Message);
                }
            }
            return result;
        }
        public bool SetFrameRate(int value)
        {
            bool result = false;
            if (m_open)
            {
                try
                {
                    int nRet = m_pMyCamera.MV_CC_SetFloatValue_NET("AcquisitionFrameRate", value);
                    if (nRet != MyCamera.MV_OK)
                    {
                        throw new Exception("Set Frame Rate Fail!");
                    }
                    result = true;
                }
                catch (Exception ex)
                {
                    throw new Exception(ex.Message);
                }
            }
            return result;
        }
        public bool Gain(float gain)
        {
            bool result = false;
            if (m_open)
            {
                try
                {
                    if (gain < 0)
                    {
                        throw new Exception("增益不能小于0");
                    }
                    m_pMyCamera.MV_CC_SetEnumValue_NET("GainAuto", 0);
                    int nRet = m_pMyCamera.MV_CC_SetFloatValue_NET("Gain", gain);
                    if (nRet != MyCamera.MV_OK)
                    {
                        throw new Exception("Device Set Gain Failed!");
                    }
                    result = true;
                }
                catch (Exception ex)
                {
                    throw new Exception(ex.Message);
                }
            }
            return result;
        }
        public bool LineDebouncerTime(uint Time)
        {
            bool result = false;
            if (m_open)
            {
                try
                {
                    MyCamera.MVCC_INTVALUE DSD = new MyCamera.MVCC_INTVALUE();
                    m_pMyCamera.MV_CC_GetIntValue_NET("LineDebouncerTime", ref DSD);
                    if (Time >= DSD.nMin && Time <= DSD.nMax)
                    {
                        m_pMyCamera.MV_CC_SetIntValue_NET("LineDebouncerTime", Time);
                    }
                    else
                    {
                        throw new Exception("去抖设置参数必须在0-1000000之间");
                    }
                }
                catch(Exception ex)
                {
                    throw new Exception(ex.Message);
                }
            }
            return result;
        }
        /*set AutoWB*/
        public void CfgAutoWB(out uint r, out uint g, out  uint b)
        {
            try
            {
                if (m_open)
                {
                    MyCamera.MVCC_INTVALUE Red = new MyCamera.MVCC_INTVALUE();
                    MyCamera.MVCC_INTVALUE Green = new MyCamera.MVCC_INTVALUE();
                    MyCamera.MVCC_INTVALUE Blue = new MyCamera.MVCC_INTVALUE();
                    m_pMyCamera.MV_CC_SetEnumValue_NET("BalanceWhiteAuto", 2);
                    m_pMyCamera.MV_CC_SetEnumValue_NET("BalanceWhiteAuto", 0);
                    m_pMyCamera.MV_CC_GetBalanceRatioRed_NET(ref Red);
                    m_pMyCamera.MV_CC_GetBalanceRatioGreen_NET(ref Green);
                    m_pMyCamera.MV_CC_GetBalanceRatioBlue_NET(ref Blue);
                    r = Red.nCurValue;
                    g = Green.nCurValue;
                    b = Blue.nCurValue;
                }
                else
                {
                    throw new Exception("相机没有打开");
                }
            }
            catch (System.Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        /*set WB*/
        public void CfgSetWB(uint r, uint g, uint b)
        {
            try
            {
                if (m_open)
                {
                    m_pMyCamera.MV_CC_SetBalanceRatioBlue_NET(b);
                    m_pMyCamera.MV_CC_SetBalanceRatioGreen_NET(g);
                    m_pMyCamera.MV_CC_SetBalanceRatioRed_NET(r);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        /*set ROIXYWidthHeight*/
        public void SetROIXYWidthHeight(uint x, uint y, uint w, uint h)
        {
            try
            {
                if (m_open)
                {
                    m_pMyCamera.MV_CC_SetIntValue_NET("OffsetX", x);
                    m_pMyCamera.MV_CC_SetIntValue_NET("OffsetY", y);
                    m_pMyCamera.MV_CC_SetIntValue_NET("Width", w);
                    m_pMyCamera.MV_CC_SetIntValue_NET("Height", h);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        /*set */
        public void SetPacketSize(uint size)
        {
            try
            {
                if (m_open)
                {
                    m_pMyCamera.MV_GIGE_SetGevSCPSPacketSize_NET(size);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public bool GetTrigger()
        {
            try
            {
                if (m_open)
                {

                    MyCamera.MVCC_ENUMVALUE _MVCC_ENUMVALUE = new MyCamera.MVCC_ENUMVALUE();
                    m_pMyCamera.MV_CC_GetTriggerMode_NET(ref _MVCC_ENUMVALUE);
                    if (_MVCC_ENUMVALUE.nCurValue == 0)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {

                return false;
            }
        }


        /*set trigger mode*/
        public void Trigger(bool trigger)
        {
            try
            {
                if (m_open)
                {
                    int triggerMode = trigger ? 1 : 0;
                    /* Disable frame start trigger if available. */
                    m_pMyCamera.MV_CC_SetEnumValue_NET("TriggerMode", (uint)triggerMode);

                    // MV_CC_GetTriggerMode_NET

                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        /// <summary>
        ///  // ch:触发源选择:0 - Line0; | en:Trigger source select:0 - Line0;
        ///           1 - Line1;
        ///          2 - Line2;
        ///          3 - Line3;
        ///          4 - Counter;
        ///           7 - Software;
        /// </summary>
        /// <param name="NUM"></param>
        public void TriggerSource(uint NUM)
        {
            if(NUM>=0&& NUM<=7)
            m_pMyCamera.MV_CC_SetEnumValue_NET("TriggerSource", NUM);
            else
                throw new Exception("触发源必须在0-7之间!");
        }
        /// <summary>
        /// 触发一次
        /// </summary>
        public void OneShot()
        {
            if (m_open)
            {
                int nRet;
                //ch:触发命令 | en:Trigger Command
                nRet = m_pMyCamera.MV_CC_SetCommandValue_NET("TriggerSoftware");
                if (MyCamera.MV_OK != nRet)
                {
                    throw new Exception("MV_CC_SetCommandValue_NET Failed!");
                }
            }
            else
            {
                throw new Exception("相机没有打开!");
            }
        }
        protected void OnImageReadyEvent()
        {
            if (ImageReadyEvent != null)
            {
                ImageReadyEvent();
            }
        }
        public void GetCurrentImage(ref HObject ho_Image)
        {
            lock (m_lockObject) /* Lock the grab result queue to avoid that two threads modify the same data. */
            {
                if (m_grabbedBuffers.Count > 0) /* If images available. */
                {
                    Ho_ImageFactory.Createho_Image(out ho_Image, m_grabbedBuffers[0].Buffer, m_grabbedBuffers[0].Width, m_grabbedBuffers[0].Height, m_grabbedBuffers[0].Color);
                    ReleaseImage();
                    return;
                }
            }
            return; /* No image available. */
        }
        public void GetCurrentImage(ref byte[] buf, ref int Width, ref int Height)
        {
            //m_bitmap = null; /* The bitmap is used for displaying the image. */
            lock (m_lockObject) /* Lock the grab result queue to avoid that two threads modify the same data. */
            {
                if (m_grabbedBuffers.Count > 0)
                {
                    Width = m_grabbedBuffers[0].Width;
                    Height = m_grabbedBuffers[0].Height;
                    buf = new byte[Width * Height];
                    buf = m_grabbedBuffers[0].Buffer;
                    /* The processing of the image is done. Release the image buffer. */
                    ReleaseImage();
                    return;
                }
            }
            // ho_Image = null;
            return; /* No image available. */
        }
        public void GetCurrentImage(ref Bitmap ho_Image)
        {
            lock (m_lockObject) /* Lock the grab result queue to avoid that two threads modify the same data. */
            {
                if (m_grabbedBuffers.Count > 0) /* If images available. */
                {
                    Ho_ImageFactory.Createho_Image(out ho_Image, m_grabbedBuffers[0].Buffer, m_grabbedBuffers[0].Width, m_grabbedBuffers[0].Height, m_grabbedBuffers[0].Color);
                    ReleaseImage();
                    return;
                }
            }
            return; /* No image available. */
        }
        public bool ReleaseImage()
        {
            lock (m_lockObject) /* Lock the grab result queue to avoid that two threads modify the same data. */
            {
                if (m_grabbedBuffers.Count > 0) /* If images are available and grabbing is in progress.*/
                {
                    /* Remove it from the grab result queue. */
                    m_grabbedBuffers.RemoveAt(0);
                    GC.Collect();
                    return true;
                }
            }
            return false;
        }
        public bool Flush()
        {
            try
            {
                lock (m_lockObject) /* Lock the grab result queue to avoid that two threads modify the same data. */
                {
                    m_grabbedBuffers.Clear();
                }
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
