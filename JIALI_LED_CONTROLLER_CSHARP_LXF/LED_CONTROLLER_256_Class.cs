using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;
using System.Threading;

namespace JIALI_LED_CONTROLLER_CSHARP_LXF
{

    public class LED_CONTROLLER_256_Class : LED_CONTROLLER
    {
        public SerialPort serialPort_1 = new SerialPort();
        public StringBuilder strRec=new StringBuilder();
        public StringBuilder strRend = new StringBuilder();


        public void PortDataReceive(object sender, SerialDataReceivedEventArgs e)
        {
            
            Thread.Sleep(20);
            try
            {
                int Readlen = serialPort_1.BytesToRead;
                if (Readlen==1)
                {   //设置亮度时，控制器接收到指令后返回的指令为$或&，因此为1个字节
                    byte[] DataRec = new byte[1];
                    serialPort_1.Read(DataRec, 0, 1);
                    string RecDataBuffer = Encoding.ASCII.GetString(DataRec);
                    strRec.Clear();
                    strRec = new StringBuilder(RecDataBuffer);

                }
                
                if (Readlen==8)
                {
                    //获取亮度时，新款控制器接收到指令后返回的指令为8个字节
                    byte[] DataRec = new byte[8];
                    serialPort_1.Read(DataRec, 0, 8);
                    string RecDataBuffer = Encoding.ASCII.GetString(DataRec);
                    strRec.Clear();
                    strRec = new StringBuilder(RecDataBuffer);
                }
                if (Readlen == 9)
                {
                    //获取亮度时，部分老款控制器接收到指令后返回的指令为9个字节，前置多一个"$"，因此删除
                    byte[] DataRec = new byte[9];
                    serialPort_1.Read(DataRec, 0, 9);
                    string RecDataBuffer = Encoding.ASCII.GetString(DataRec);
                    strRec.Clear();
                    RecDataBuffer=RecDataBuffer.Replace("$$","$");
                    strRec = new StringBuilder(RecDataBuffer);
                }

                serialPort_1.DiscardInBuffer();
               

                
               

            }
            catch (Exception)
            {

               
            }
        }
        public bool ConnectController(int BaudRate, string PortName, int DataBits)
        {
            serialPort_1.BaudRate = BaudRate;
            serialPort_1.PortName = PortName;
            serialPort_1.DataBits = DataBits;
            serialPort_1.Parity = Parity.None;
           
            try
            {
                serialPort_1.Open();
                serialPort_1.DataReceived += new SerialDataReceivedEventHandler(PortDataReceive);
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        public bool DisConnectController()
        {
           

            try
            {
                serialPort_1.DataReceived -= new SerialDataReceivedEventHandler(PortDataReceive);
                serialPort_1.Close();
                
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }
        public string CloseALLChannel()
        {


            return "关闭成功";
        }

        public string CloseChannel(string ch)
        {
            return "关闭成功";
        }

        public string CloseCh(string ch, string value = "255")
        {
            string value_str = Convert.ToInt32(value).ToString("X");
            if (value_str.Length == 1)
            {
                value_str = "00" + value_str;
            }

            switch (value_str.Length)
            {
                case 1:
                    value_str = "00" + value_str;
                    break;
                case 2:
                    value_str = "0" + value_str;
                    break;
                default:
                    break;
            }
            string msg = "$2" + ch + value_str;
            msg = msg + GetXorResualt(msg);
            serialPort_1.Write(msg);
            return "关闭成功";
        }

        //获取对应通道亮度值
        public string GetIntensity(string ch)
        {
            try
            {
                string dataRec = "$4" + ch + "000";
                dataRec = dataRec + GetXorResualt(dataRec);
                serialPort_1.Write(dataRec);
                Thread.Sleep(100);
                dataRec = strRec.ToString();
                dataRec = dataRec.Substring(3, 3);
                int convertTodec = Convert.ToInt32(dataRec, 16);
                return convertTodec.ToString();
            }
            catch (Exception)
            {

                return "0";
            }
        }

        //此功能暂时关闭
        public string GetONorOFF()
        {   
            return "";
        }

        public string GetXorResualt(string msg)
        {
            //获取字节数组
            byte[] b = Encoding.ASCII.GetBytes(msg);
            // xorResult 存放校验结注意：初值首元素值
            byte xorResult = b[0];
            // 求xor校验注意：XOR运算第二元素始
            for (int i = 1; i < b.Length; i++)
            {
                xorResult ^= b[i];
            }
            // 运算xorResultXOR校验结，^=为异或符号
            // MessageBox.Show();
            return xorResult.ToString("X");

        }

        public string OpenAllChannel()
        {
            throw new NotImplementedException();
        }

        public string OpenChannel(string ch)
        {
            throw new NotImplementedException();
        }





        public string SetIntensity(string ch, string value)
        {
           
            string value_str = Convert.ToInt32(value).ToString("X");
            if (value_str.Length==1)
            {
                value_str = "00" + value_str;
            }

            switch (value_str.Length)
            {
                case 1:
                    value_str = "00" + value_str;
                    break;
                case 2:
                    value_str = "0" + value_str;
                    break;
                default:
                    break;
            }
            string msg = "$3" + ch + value_str;
            msg = msg + GetXorResualt(msg);
            serialPort_1.Write(msg);
            Thread.Sleep(30);
            strRend = new StringBuilder(msg);
            return strRec.ToString();

        }
    }
}
