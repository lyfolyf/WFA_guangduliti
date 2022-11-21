using System;
using System.Collections.Generic;
using System.Text;
using MvCamCtrl.NET;
using System.Runtime.InteropServices;

namespace IHalconHikvision
{
    public class DeviceEnumerator
    {
       public static MyCamera.MV_CC_DEVICE_INFO_LIST m_pDeviceList = new MyCamera.MV_CC_DEVICE_INFO_LIST();
        /* Data class used for holding device data. */
        public class Device
        {
            public string Name; /* The friendly name of the device. */
            public uint Index; /* The index of the device. */
            public string SerialNumber;
            public string Versions;
            public string ModelNumber;
        }
        /* Queries the number of available devices and creates a list with device data. */
        public static List<Device> EnumerateDevices()
        {
            /* Create a list for the device data. */
            //ch:创建设备列表 | en:Create Device List
            System.GC.Collect();
            List<Device> list = new List<Device>();
            int nRet = MyCamera.MV_CC_EnumDevices_NET(MyCamera.MV_GIGE_DEVICE | MyCamera.MV_USB_DEVICE, ref m_pDeviceList);
            if (0 != nRet)
            {
                throw new Exception("Enumerate devices fail!:"+nRet.ToString());
            }
            for (uint i = 0; i < m_pDeviceList.nDeviceNum; i++)
            {
                /* Create a new data packet. */
                Device device = new Device();
                MyCamera.MV_CC_DEVICE_INFO mdevice = (MyCamera.MV_CC_DEVICE_INFO)Marshal.PtrToStructure(m_pDeviceList.pDeviceInfo[i], typeof(MyCamera.MV_CC_DEVICE_INFO));
                if (mdevice.nTLayerType == MyCamera.MV_GIGE_DEVICE)
                {
                    IntPtr buffer = Marshal.UnsafeAddrOfPinnedArrayElement(mdevice.SpecialInfo.stGigEInfo, 0);
                    MyCamera.MV_GIGE_DEVICE_INFO gigeInfo = (MyCamera.MV_GIGE_DEVICE_INFO)Marshal.PtrToStructure(buffer, typeof(MyCamera.MV_GIGE_DEVICE_INFO));
                    device.Index = i;
                    device.ModelNumber = gigeInfo.chModelName;
                    device.Name = "GigE: " + gigeInfo.chManufacturerName;
                    device.SerialNumber = gigeInfo.chSerialNumber;
                    device.Versions = gigeInfo.chDeviceVersion;
                }
                else if (mdevice.nTLayerType == MyCamera.MV_USB_DEVICE)
                {
                    IntPtr buffer = Marshal.UnsafeAddrOfPinnedArrayElement(mdevice.SpecialInfo.stUsb3VInfo, 0);
                    MyCamera.MV_USB3_DEVICE_INFO usbInfo = (MyCamera.MV_USB3_DEVICE_INFO)Marshal.PtrToStructure(buffer, typeof(MyCamera.MV_USB3_DEVICE_INFO));
                    device.Index = i;
                    device.ModelNumber = usbInfo.chModelName;
                    device.Name = "USB: " + usbInfo.chManufacturerName;
                    device.SerialNumber = usbInfo.chSerialNumber;
                    device.Versions = usbInfo.chDeviceVersion;
                }
                list.Add(device);
            }
            return list;
        }
    }
}
