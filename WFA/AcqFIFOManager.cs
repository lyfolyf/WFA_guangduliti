using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Drawing;
using System.Windows.Forms;
using PylonC.NET;
using IHalconHikvision;
using HalconDotNet;

namespace WFA
{
    //delegate void ErrMsgHandler(string msg);
    delegate void ImageReadyHandler(uint camNo, HObject ho_Image);

    public class AcqFIFOManager
    {
        //internal event ErrMsgHandler OnErrMsg = null;
        internal event ImageReadyHandler OnImageReady = null;
        /// <summary>
        /// 1号相机
        /// </summary>
        public ImageProvider AcqFifo1
        {
            get { return mAcqFifo1; }
        }
        private ImageProvider mAcqFifo1 = null;
        private bool mConnection1 = false;
        /// <summary>
        /// 一号相机链接状态
        /// </summary>
        public bool Connection1
        {
            get { return mConnection1; }
            set { mConnection1=value; }
        }

        /// <summary>
        /// 初始化必要数据
        /// </summary>
        public void InitAcqFIFOManager()
        {
            try
            {
                List<DeviceEnumerator.Device> mDevice = DeviceEnumerator.EnumerateDevices();
                foreach (DeviceEnumerator.Device device in mDevice)
                {
                    if (device.SerialNumber == SysConfig.mCam1SerNum)
                    {
                        mAcqFifo1 = new ImageProvider();
                        if (!mAcqFifo1.IsOpen)
                        {
                            mAcqFifo1.Open(device.Index);
                        }
                    }
                }
                if (mAcqFifo1 != null)
                {
                    mAcqFifo1.ImageReadyEvent += new ImageProvider.ImageReadyEventHandler(mAcqFifo1_Complete);
                    mConnection1 = true;
                    mAcqFifo1.LineDebouncerTime(100);
                }
                else
                {
                    ErrLog.WriteLogEx("相机丢失!");
                }
            }
            catch (System.Exception ex)
            {
                ErrLog.WriteLogEx("相机通信故障!---"+ex.ToString());
            }
        }

        #region Complete event

        void mAcqFifo1_Complete()
        {
            try
            {
                HObject ho_Image;
                HOperatorSet.GenEmptyObj(out ho_Image);
                mAcqFifo1.GetCurrentImage (ref ho_Image);
                if (ho_Image != null)
                {
                    ImageReady(1, ho_Image);
                }
            }
            catch (Exception ex)
            {
                ImageReady(1, null);
                ErrLog.WriteLogEx("1号相机" + ex.Message);
            }
        }

        #endregion
        /// <summary>
        /// 启动获取图像
        /// </summary>
        /// <param name="camNo">相机编号</param>
        public void StartAcquire(uint camNo)
        {
            switch (camNo)
            {
                case 1:
                    if (mAcqFifo1 != null)
                    {
                        mAcqFifo1.Start();
                    }
                    break;
                default: break;
            }
        }
        /// <summary>
        /// 获取图像
        /// </summary>
        /// <param name="camNo">相机编号</param>
        public void GainAcquire(uint camNo)
        {
            switch (camNo)
            {
                case 1:
                    mAcqFifo1.Trigger(false);
                    mAcqFifo1.OneShot();
                    break;
                default: break;
            }
        }
        /// <summary>
        /// 停止获取图像
        /// </summary>
        /// <param name="camNo">相机编号</param>
        public void StopAcquire(uint camNo)
        {
            switch (camNo)
            {
                case 1:
                    if (mAcqFifo1 != null)
                    {
                        mAcqFifo1.Stop();
                    }
                    break;
                default: break;
            }
        }
        /// <summary>
        /// 设置出发模式
        /// </summary>
        /// <param name="camNo">相机编号</param>
        /// <param name="triggerModel">触发模式</param>
        public void SetTriggerModel(uint camNo, bool triggerModel)
        {
            switch (camNo)
            {
                case 1:
                    if (mAcqFifo1 != null)
                        mAcqFifo1.Trigger(triggerModel);
                    break;

                default: break;
            }
        }
        /// <summary>
        /// 设置曝光时间mSecs
        /// </summary>
        /// <param name="camNo">相机编号</param>
        /// <param name="exposure">曝光时间mSecs</param>
        public void SetExposure(uint camNo, double exposure)
        {
            uint exp = Convert.ToUInt32(exposure * 1000);
            switch (camNo)
            {
                case 1:
                    if (mAcqFifo1 != null)
                        mAcqFifo1.ExposureTime(exp); //mSecs
                    break;

                default: break;
            }
        }

        public void SetGain(uint camNo, float gainvalue)
        {
            switch (camNo)
            {
                case 1:
                    if (mAcqFifo1 != null)
                        mAcqFifo1.Gain(gainvalue);
                    break;

                default: break;
            }
        }


        /// <summary>
        /// 事件
        /// </summary>
        /// <param name="camNo"></param>
        /// <param name="ho_Image"></param>
        private void ImageReady(uint camNo, HObject ho_Image)
        {
            if (OnImageReady != null)
                OnImageReady(camNo, ho_Image);
        }
        /// <summary>
        /// 释放资源，关闭相机
        /// </summary>
        public void AcqFIFOManagerFree()
        {
            if (mAcqFifo1 != null)
                mAcqFifo1.Close();
        }
    }
}
