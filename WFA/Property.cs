using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms.Design;

namespace WFA
{
   public class Property
    {

        [Category("网络通信"), DisplayName("地址")]
        public string HostIP
        {
            get { return SysConfig.mHostIP; }
            set { SysConfig.mHostIP = value; }
        }
        [Category("网络通信"), DisplayName("端口")]
        public string HPort
        {
            get { return SysConfig.mPort; }
            set { SysConfig.mPort = value; }
        }

        [Category("串口通信"), DisplayName("串口号")]
        public string PortName
        {
            get { return SysConfig.PortName; }
            set { SysConfig.PortName = value; }
        }
        [Category("串口通信"), DisplayName("波特率")]
        public int BaudRate
        {
            get { return SysConfig.BaudRate; }
            set { SysConfig.BaudRate = value; }
        }
        [Category("串口通信"), DisplayName("数据位")]
        public int DataBits
        {
            get { return SysConfig.DataBits; }
            set { SysConfig.DataBits = value; }
        }


        [Category("其他"), DisplayName("光源")]
        public string ComClass
        {
            get { return SysConfig.comClass; }
            set { SysConfig.comClass = value; }
        }

        [Category("其他"), DisplayName("图片保存")]
        public bool ImageSave
        {
            get { return SysConfig.ImageSave; }
            set { SysConfig.ImageSave = value; }
        }

        [System.ComponentModel.Editor(typeof(FolderNameEditor), typeof(UITypeEditor))]
        [Category("其他"), DisplayName("图片保存路径")]
        public string ImageSavePath
        {
            get { return SysConfig.ImageSavePath; }
            set { SysConfig.ImageSavePath = value; }
        }


        [Category("计算"), DisplayName("斑点MIN")]
        public int BlobMin
        {
            get { return SysConfig.BlobMin; }
            set { SysConfig.BlobMin = value; }
        }

        [Category("计算"), DisplayName("斑点MAX")]
        public int BlobMax
        {
            get { return SysConfig.BlobMax; }
            set { SysConfig.BlobMax = value; }
        }

    }
}
