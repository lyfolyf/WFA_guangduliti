using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WFA
{
    class SysConfig
    {

        /// <summary>
        /// 文件配置
        /// </summary>
        public static INI INIConfig = null;

        /// <summary>
        /// 通信ip地址 与端口
        /// </summary>
        public static string mPort = ""; //主机IP
        public static string mHostIP = "";

        /// <summary>
        /// 相机序列号   曝光    增益
        /// </summary>
        public static string mCam1SerNum = ""; //相机序列号
        public static double Exposure1 = 0;
        public static double Exposure2 = 0;
        public static double Gain1 = 0;
        public static string comClass = ""; //光源型号

        public static string PortName = "COM1";
        public static int BaudRate = 9600;
        public static int DataBits = 8;


        public static bool ImageSave = false; //图片保存
        public static string ImageSavePath = ""; //图片保存

        public static int BlobMin = 30;
        public static int BlobMax = 30;
        public static bool IsDebug = false;
        public static string DefaultJob = "JOB1";
        public static void SysLoadConfig()
        {
            try
            {
                INIConfig = new INI(AppDomain.CurrentDomain.BaseDirectory + "Config\\Config.ini");

                mHostIP = INIConfig.IniReadValue("System", "HostIP");
                mPort = INIConfig.IniReadValue("System", "HPort");

                comClass = INIConfig.IniReadValue("System", "ComClass");

                ImageSave = Convert.ToBoolean(INIConfig.IniReadValue("System", "ImageSave"));

                ImageSavePath = INIConfig.IniReadValue("System", "ImageSavePath");


                DefaultJob = INIConfig.IniReadValue("System", "DefaultJob");

                PortName = INIConfig.IniReadValue("SerPort", "PortName");
                int.TryParse(INIConfig.IniReadValue("SerPort", "BaudRate"),out BaudRate);
                int.TryParse(INIConfig.IniReadValue("SerPort", "DataBits"), out DataBits);

                mCam1SerNum = INIConfig.IniReadValue("Cam1", "CamSerNum");
                double.TryParse(INIConfig.IniReadValue("Cam1", "Exposure"), out Exposure1);
                double.TryParse(INIConfig.IniReadValue("Cam1", "Exposure2"), out Exposure2);
                double.TryParse(INIConfig.IniReadValue("Cam1", "Gain"), out Gain1);

                int.TryParse(INIConfig.IniReadValue("Calc", "BlobMin"), out BlobMin);
                int.TryParse(INIConfig.IniReadValue("Calc", "BlobMax"), out BlobMax);

                bool.TryParse(INIConfig.IniReadValue("System", "Debug"),out IsDebug);

            }
            catch (Exception ex)
            {
                ErrLog.WriteLogEx(ex.ToString());
            }

        }

    }
}
