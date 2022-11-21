using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using HalconDotNet;
using Bing.Tcp;
using System.Net;
using System.Timers;
using DataGridViewTools;
using System.Threading;
using System.Net.Sockets;
using System.IO;
using JIALI_LED_CONTROLLER_CSHARP_LXF;
namespace WFA
{
    public partial class MainForm : Form
    {


        private  LED_CONTROLLER_256_Class ControllerA = new LED_CONTROLLER_256_Class(); //光源控制
        private AcqFIFOManager mAcqFIFOManager = null;
        private RecordDisplay mRecordDisplay = null;
        private HObject[]  ho_image; //相机触发图片
        private HObject img;
        private int _indexImage = 0;
        private double curExpose = 0;
 
        HTuple count;

        private bool IsVedio = false;

        HTuple BubbleArea ;
        HTuple BubbleWidth;
        HTuple BubbleHeight;

        private Server server = null;//视觉作为服务端 

        private Action actLoadHdev = null;

        private TcpClient mTcpClient = null; //我作为服务器时  连上的客户端信息
        private int nSum = 0;
        private int nOK = 0;
        private string strRece = "";
     
        private string strCommand = "";
        private System.Timers.Timer tme = new System.Timers.Timer();
        public MainForm()
        {
            InitializeComponent();
            DataGridViewToolsClass dgt = new DataGridViewToolsClass();
            dgt.NoShanSHuo(dgv);
            //serPort = new SerPort();
             curExpose = SysConfig.Exposure1;
           InitAcqFIFOManager();
            ho_image = new HObject[4];
            for (int i = 0; i < 4; i++)
            {
                HOperatorSet.GenEmptyObj(out ho_image[i]);
            }
            HOperatorSet.GenEmptyObj(out img);

            actLoadHdev = ImageDealProcess.I_P.LoadHdevFile;

            ControllerA.ConnectController(SysConfig.BaudRate, SysConfig.PortName, 8);
            InitTcp();


        }
        /// <summary>
        /// 膜 图片运算与显示结果
        /// </summary>
        /// <returns></returns>
        private bool RunAndDisResult()
        {
            try
            {
                HOperatorSet.CountObj(img, out count);
                bool status = false;
                if (count.D == 4)
                {

                    switch (strCommand)
                    {
                        case "AFC,ASF_A1":
                            status = ImageDealProcess.I_P.RunASF_A1(img);
                            break;
                        case "AFC,HAF_A1":
                            status = ImageDealProcess.I_P.Run_HAF_L(img);
                            break;
                        case "AFC,HAF_A2":
                            status = ImageDealProcess.I_P.Run_HAF_R(img);
                            break;
                        case "AFC,ASF_B1":
                            status = ImageDealProcess.I_P.RunASF_B1(img);
                            break;
                        case "AFC,HAF_B1":
                            status = ImageDealProcess.I_P.Run_HAF_L(img);
                            break;
                        case "AFC,HAF_B2":
                            status = ImageDealProcess.I_P.Run_HAF_R(img);
                            break;

                        default:
                            break;
                    }

                    if (status)
                    {

                        HRegion region = new HRegion();
                        switch (strCommand)
                        {
                            case "AFC,ASF_A1":
                                region = ImageDealProcess.hprocall_A1.GetOutputIconicParamRegion("DetectBubbles");
                                BubbleArea = ImageDealProcess.hprocall_A1.GetOutputCtrlParamTuple("Area");
                                BubbleWidth = ImageDealProcess.hprocall_A1.GetOutputCtrlParamTuple("bubbleWidth");
                                BubbleHeight = ImageDealProcess.hprocall_A1.GetOutputCtrlParamTuple("bubbleHeight");
                                break;
                            case "AFC,HAF_A1":
                                region = ImageDealProcess.hprocall_L.GetOutputIconicParamRegion("DetectBubbles");
                                BubbleArea = ImageDealProcess.hprocall_L.GetOutputCtrlParamTuple("Area");
                                BubbleWidth = ImageDealProcess.hprocall_L.GetOutputCtrlParamTuple("bubbleWidth");
                                BubbleHeight = ImageDealProcess.hprocall_L.GetOutputCtrlParamTuple("bubbleHeight");
                                break;
                            case "AFC,HAF_A2":
                                region = ImageDealProcess.hprocall_R.GetOutputIconicParamRegion("DetectBubbles");
                                BubbleArea = ImageDealProcess.hprocall_R.GetOutputCtrlParamTuple("Area");
                                BubbleWidth = ImageDealProcess.hprocall_R.GetOutputCtrlParamTuple("bubbleWidth");
                                BubbleHeight = ImageDealProcess.hprocall_R.GetOutputCtrlParamTuple("bubbleHeight");
                                break;
                            case "AFC,ASF_B1":
                                region = ImageDealProcess.hprocall_B1.GetOutputIconicParamRegion("DetectBubbles");
                                BubbleArea = ImageDealProcess.hprocall_B1.GetOutputCtrlParamTuple("Area");
                                BubbleWidth = ImageDealProcess.hprocall_B1.GetOutputCtrlParamTuple("bubbleWidth");
                                BubbleHeight = ImageDealProcess.hprocall_B1.GetOutputCtrlParamTuple("bubbleHeight");
                                break;
                            case "AFC,HAF_B1":
                                region = ImageDealProcess.hprocall_L.GetOutputIconicParamRegion("DetectBubbles");
                                BubbleArea = ImageDealProcess.hprocall_L.GetOutputCtrlParamTuple("Area");
                                BubbleWidth = ImageDealProcess.hprocall_L.GetOutputCtrlParamTuple("bubbleWidth");
                                BubbleHeight = ImageDealProcess.hprocall_L.GetOutputCtrlParamTuple("bubbleHeight");
                                break;
                            case "AFC,HAF_B2":
                                region = ImageDealProcess.hprocall_R.GetOutputIconicParamRegion("DetectBubbles");
                                BubbleArea = ImageDealProcess.hprocall_R.GetOutputCtrlParamTuple("Area");
                                BubbleWidth = ImageDealProcess.hprocall_R.GetOutputCtrlParamTuple("bubbleWidth");
                                BubbleHeight = ImageDealProcess.hprocall_R.GetOutputCtrlParamTuple("bubbleHeight");
                                break;

                            default:
                                break;
                        }

                        if (BubbleArea.Length > 0 && region != null)
                        {
                            mRecordDisplay.DisplayRegion(region, FontColor.red);
                        }

                        if (BubbleArea != null && BubbleArea.Length > 0)
                        {
                            for (int i = 0; i < BubbleArea.Length; i++)
                            {
                                dgv.Rows.Add(new object[] { nSum.ToString(), BubbleArea[i].D.ToString("F2"), BubbleWidth[i].D.ToString("F2"), BubbleHeight[i].D.ToString("F2") });
                       
                                if (dgv.Rows.Count > 25)
                                {
                                    dgv.Rows.RemoveAt(1);
                                }

                                mRecordDisplay.Message(BubbleArea[i].D.ToString("F2"), Position.image, BubbleHeight[i].D, BubbleWidth[i].D, FontColor.green, false, "40");
                            }
                            mRecordDisplay.Message("NG!", Position.image, 200, 200, FontColor.red, false, "40");
                            SendMsgResult("0");
                            //发送结果给上位机  
                        }
                        else
                        {
                            mRecordDisplay.Message("OK!", Position.image, 200, 200, FontColor.green, false, "40");
                            SendMsgResult("1");
                            //发送结果给上位机  OK
                        }
                        for (int i = 0; i < 4; i++)
                        {
                            ho_image[i].Dispose();
                        }
                        img.Dispose();
                        region.Dispose();
                        BubbleArea.Dispose();
                        BubbleWidth.Dispose();
                        BubbleHeight.Dispose();
                        count.Dispose();

                        for (int i = 0; i < 4; i++)
                        {
                            HOperatorSet.GenEmptyObj(out ho_image[i]);
                        }
                        HOperatorSet.GenEmptyObj(out img);
                    }
                    else
                    {
                        for (int i = 0; i < 4; i++)
                        {
                            ho_image[i].Dispose();
                        }
                        img.Dispose();
                        BubbleArea.Dispose();
                        BubbleWidth.Dispose();
                        BubbleHeight.Dispose();
                        count.Dispose();

                        for (int i = 0; i < 4; i++)
                        {
                            HOperatorSet.GenEmptyObj(out ho_image[i]);
                        }
                        HOperatorSet.GenEmptyObj(out img);
                        SendMsgResult("0");
                        mRecordDisplay.Message("NG!", Position.window, 200, 200, FontColor.red, false, "40");
                        //发送结果给上位机
                        return false;//检测失败
                    }
                }
                else
                {
                    for (int i = 0; i < 4; i++)
                    {
                        ho_image[i].Dispose();
                    }
                    img.Dispose();
                    BubbleArea.Dispose();
                    BubbleWidth.Dispose();
                    BubbleHeight.Dispose();
                    count.Dispose();

                    for (int i = 0; i < 4; i++)
                    {
                        HOperatorSet.GenEmptyObj(out ho_image[i]);
                    }
                    HOperatorSet.GenEmptyObj(out img);
                    mRecordDisplay.Message("NG!", Position.window, 200, 200, FontColor.red, false, "40");
                    SendMsgResult("0");
                    //发送结果给上位机
                    return false;//检测失败
                }

    
                return true;

            }
            catch (Exception ex)
            {
                count.Dispose();
                for (int i = 0; i < 4; i++)
                {
                    ho_image[i].Dispose();
                }
                img.Dispose();
                for (int i = 0; i < 4; i++)
                {
                    HOperatorSet.GenEmptyObj(out ho_image[i]);
                }
                HOperatorSet.GenEmptyObj(out img);
                mRecordDisplay.Message("NG!", Position.window, 200, 200, FontColor.red, false, "40");
                SendMsgResult("0");
                //发送结果给上位机 NG
                ErrLog.WriteLogEx(ex.ToString());
                return false;
            }
        }

        /// <summary>
        /// 开启对应光源与设置亮度
        /// </summary>
        /// <param name="seq"></param>
        private void OpenLightAndSoftTrig(string seq,double exposetime)
        {
            try
            {

                ControllerA.SetIntensity(seq, "254");
                Thread.Sleep(20);
                if (mAcqFIFOManager != null && mAcqFIFOManager.AcqFifo1 != null)
                {
                    UpdateLog("拍摄第---"+seq+"---张图片",Color.Lime);
                    mAcqFIFOManager.SetExposure(1, exposetime);
                    mAcqFIFOManager.AcqFifo1.OneShot();

                }
                Thread.Sleep(20);
                ControllerA.SetIntensity(seq, "0");
            }
            catch (Exception ex)
            {
                ErrLog.WriteLogEx(ex.ToString());
             
            }
        }

        private string[] GetAllJobs()
        {
            return Directory.GetDirectories(Application.StartupPath+ "\\HDEV");
        }

        private void InitTime()
        {

            tme.AutoReset = true;
            tme.Interval = 500;
            tme.Elapsed += Tme_Elapsed;
            tme.Enabled = true;
        }

        private void Tme_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new ElapsedEventHandler(Tme_Elapsed), sender, e);
                return;
            }
            try
            {
                tsslTime.Text = DateTime.Now.ToString("MM-dd HH:mm:ss");
            }
            catch (Exception ex)
            {

            
            }
        }

        #region  TCP通信与注册事件

        private void InitTcp()
        {
            try
            {
                server = new Server(IPAddress.Parse(SysConfig.mHostIP), Convert.ToInt32(SysConfig.mPort));
                server.TcpConnected += Server_TcpConnected;
                server.TcpDateReceived += Server_TcpDateReceived;
                server.TcpDisConnected += Server_TcpDisConnected;
                server.TcpDateSend += Server_TcpDateSend;
                server.Start();
            }
            catch (Exception ex)
            {
                ErrLog.WriteLogEx(ex.ToString());
            }
        }
        /// <summary>
        /// 发送结果
        /// </summary>
        /// <param name="ok_ng  1表示ok   0表示ng "></param>
        private void SendMsgResult(string ok_ng)
        {
            try
            {
                if (mTcpClient != null)
                {
                    string result = "";
                    if (ok_ng == "1")
                    {
                        nOK++;
                    }
                    this.BeginInvoke(new MethodInvoker(delegate {
                        labTitle.BackColor = (ok_ng == "1") ? Color.Lime : Color.Red;
                        labSum.Text = nSum.ToString();
                        labOK.Text = nOK.ToString();
                    }));


                    result = string.Format("{0},{1},1",strCommand,ok_ng);
                    server.Send(mTcpClient, result);
                }
 
            }
            catch (Exception ex)
            {
                ErrLog.WriteLogEx(ex.ToString());

            }
        }


        private void Server_TcpDateSend(object sender, TcpDateEventArgs e)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new TcpDateSendEventHandler(Server_TcpDateSend), sender, e);
                return;
            }
            UpdateLog("发送通信指令:"+e.Message,Color.Lime);
        }

        private void Server_TcpDisConnected(object sender, TcpDateEventArgs e)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new TcpDisConnectedEventHandler(Server_TcpDisConnected), sender, e);
                return;
            }
            tsslSocket.BackColor = Color.Red;
        }

        private void Server_TcpDateReceived(object sender, TcpDateEventArgs e)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new TcpDateReceivedEventHandler(Server_TcpDateReceived), sender, e);
                return;
            }
            try
            {
                strRece = e.Message;
                UpdateLog("收到通信指令:"+strRece,Color.Lime);
                if (e.Message.StartsWith("AFC,"))
                {
                    //AFC,21,1,1,1
                    nSum++;
                    strCommand = e.Message.ToString().Trim().Substring(0,10);
                    Interlocked.Exchange(ref _indexImage, 0);
                    switch (strCommand)
                    {
                        case "AFC,ASF_A1":
                            curExpose = SysConfig.Exposure1;
                            OpenLightAndSoftTrig("1", curExpose);//拍摄载具上左边物料ASF膜
                            break;
                        case "AFC,HAF_A1":
                            curExpose = SysConfig.Exposure2;
                            OpenLightAndSoftTrig("1", curExpose);//拍摄载具上左边物料HAF膜的靠近头部部分
                            break;
                        case "AFC,HAF_A2":
                            curExpose = SysConfig.Exposure2;
                            OpenLightAndSoftTrig("1", curExpose);//拍摄载具上左边物料HAF膜的靠近尾部部分
                            break;

                        case "AFC,ASF_B1":
                            curExpose = SysConfig.Exposure1;
                            OpenLightAndSoftTrig("1", curExpose);//载具上右边物料
                            break;
                        case "AFC,HAF_B1":
                            curExpose = SysConfig.Exposure2;
                            OpenLightAndSoftTrig("1", curExpose);//
                            break;
                        case "AFC,HAF_B2":
                            curExpose = SysConfig.Exposure2;
                            OpenLightAndSoftTrig("1", curExpose);//
                            break;

                        default:
                            break;
                    }
                    
   
                }
            }
            catch (Exception ex)
            {

                ErrLog.WriteLogEx(ex.ToString());
            }
        }

        private void Server_TcpConnected(object sender, TcpDateEventArgs e)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new TcpConnectedEventHandler(Server_TcpConnected),sender,e);
                return;
            }

            mTcpClient = e.Client;
            tsslSocket.BackColor = Color.Lime;
            UpdateLog("客户端已连上!",Color.Lime);
        }

        #endregion

        #region 相机初始化与 图片获取事件

        private void InitAcqFIFOManager()
        {
            try
            {
                mAcqFIFOManager = new AcqFIFOManager();
                mAcqFIFOManager.InitAcqFIFOManager();  // 初始化相机
                mAcqFIFOManager.OnImageReady += MAcqFIFOManager_OnImageReady;
                mAcqFIFOManager.SetTriggerModel(1, true);
                mAcqFIFOManager.AcqFifo1.TriggerSource(7); //设置触发方式为软件触发
                mAcqFIFOManager.SetExposure(1,SysConfig.Exposure1);
                mAcqFIFOManager.SetGain(1,Convert.ToSingle(SysConfig.Gain1));
                mAcqFIFOManager.StartAcquire(1);
            }
            catch (Exception ex)
            {
                ErrLog.WriteLogEx(ex.ToString());
            }
        }

        private void MAcqFIFOManager_OnImageReady(uint camNo, HObject  _Image)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new ImageReadyHandler(MAcqFIFOManager_OnImageReady), camNo, _Image);
                return;
            }
            try
            {
                Interlocked.Increment(ref _indexImage);
                if (_Image != null)
                {
        
                    if (!IsVedio)
                    {
                        HOperatorSet.CopyImage(_Image, out ho_image[_indexImage - 1]);
                        if (_indexImage == 4)
                        {
                            mRecordDisplay.Clear();
                            mRecordDisplay.Image = _Image;
                            HOperatorSet.ConcatObj(ho_image[0], ho_image[1], out img);
                            HOperatorSet.ConcatObj(img, ho_image[2], out img);
                            HOperatorSet.ConcatObj(img, ho_image[3], out img);
                       
                            if (!SysConfig.IsDebug)
                            {
                                //非调试模式
                                RunAndDisResult();
                            }
                            else
                            {
                                //调试模式,忽略运算,直接给结果
                                Thread.Sleep(2000);
                                SendMsgResult("1");
                                for (int i = 0; i < 4; i++)
                                {
                                    ho_image[i].Dispose();
                                }
                                img.Dispose();
                      
                                for (int i = 0; i < 4; i++)
                                {
                                    HOperatorSet.GenEmptyObj(out ho_image[i]);
                                }
                                HOperatorSet.GenEmptyObj(out img);
                              
                            }
                            UpdateLog("运算结束,输出结果!", Color.Lime);
                        }
                        else
                        {
                            OpenLightAndSoftTrig((_indexImage+1).ToString(),curExpose);//触发信号
                        }
                    }
                }

                 if (SysConfig.ImageSave && !IsVedio)
                 {
                        string ss = SysConfig.ImageSavePath + "//" + DateTime.Now.ToString("yyyy-MM-dd");
                        if (!Directory.Exists(ss))
                        {
                            Directory.CreateDirectory(ss);
                        }


                       HObject img_save;
                       HOperatorSet.GenEmptyObj(out img_save);
                       img_save = _Image.Clone();
                       ThreadPool.QueueUserWorkItem(new WaitCallback((o)=> {
                        HOperatorSet.WriteImage(img_save, "bmp", 0, ss + "//" + DateTime.Now.ToString("HH-mm-sss-fff") + ".bmp");
                           img_save.Dispose();
                       }));
                       
                 }
               
                _Image.Dispose();
            }
            catch (Exception ex)
            {
                ErrLog.WriteLogEx(ex.ToString());
            }
        }

        #endregion

        /// <summary>
        /// 运行日志实时显示
        /// </summary>
        /// <param name="log"></param>
        /// <param name="color"></param>
        public void UpdateLog(string log, Color color)
        {
            rtbLog.BeginInvoke(new Action(delegate {
                log = DateTime.Now.ToString("HH-mm-ss-fff") + " : " + log + "\n";
                rtbLog.AppendText(log);
                rtbLog.SelectionStart = rtbLog.TextLength - log.Length;
                rtbLog.SelectionLength = log.Length;
                rtbLog.SelectionColor = color;
                rtbLog.SelectionStart = rtbLog.TextLength - 1;
                rtbLog.SelectionLength = 0;
                rtbLog.ScrollToCaret();
                if (rtbLog.TextLength > 10000)
                {
                    rtbLog.Clear();
                }
            }));
        }

        #region 控件相关

        /// <summary>
        /// 手动测试
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                switch (comboBox1.Text)
                {

                    case "ASF_A1测试":
                        strCommand = "AFC,ASF_A1";
                        break;

                    case "ASF_B1测试":
                        strCommand = "AFC,ASF_B1";
                        break;

                    case "HAF_L测试":
                        strCommand = "AFC,HAF_A1";
                        break;

                    case "HAF_R测试":
                        strCommand = "AFC,HAF_A2";
                        break;

                    default:
                        break;
                }
                RunAndDisResult();
            }
            catch (Exception ex)
            {

                ErrLog.WriteLogEx(ex.ToString());
            }
        
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                ErrLog.WriteLogEx("软件退出!");
                for (int i = 1; i < 5; i++)
                {
                    if (ControllerA != null && ControllerA.serialPort_1 != null && ControllerA.serialPort_1.IsOpen)
                    {
                        ControllerA.SetIntensity(i.ToString(), "0");
                        Thread.Sleep(20);
                    }
                }
                if (mAcqFIFOManager !=null)
                {
                    mAcqFIFOManager.AcqFIFOManagerFree();
                }

                if (ControllerA != null)
                {
                    ControllerA.DisConnectController();
                }

                if (server != null)
                {
                    server.TcpConnected -= Server_TcpConnected;
                    server.TcpDateReceived -= Server_TcpDateReceived;
                    server.TcpDisConnected -= Server_TcpDisConnected;
                    server.TcpDateSend -= Server_TcpDateSend;
                    server.Stop();
                }

                mRecordDisplay.Dispose();

            }
            catch (Exception ex)
            {
                ErrLog.WriteLogEx(ex.ToString());

            }
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            try
            {
                for (int i = 0; i < 4; i++)
                {
                    HOperatorSet.GenEmptyObj(out ho_image[i]);
                }

                HOperatorSet.GenEmptyObj(out img);
                mRecordDisplay = new RecordDisplay(hW);
                UpdateLog("软件启动!",Color.Red);
                ErrLog.WriteLogEx("软件启动!");
                InitTime();
                if (mAcqFIFOManager != null)
                {
                    tsslCam.BackColor = mAcqFIFOManager.Connection1 ? Color.Lime : Color.Red ;
                    if (!mAcqFIFOManager.Connection1)
                    {
                        UpdateLog("相机链接失败!", Color.Red);
                    }

                }
                else
                {
                    UpdateLog("相机未初始化!", Color.Red);
                }
                if (ControllerA != null)
                {
                    tsslSerPort.BackColor = ControllerA.serialPort_1.IsOpen ? Color.Lime : Color.Red;
                    if (ControllerA.serialPort_1.IsOpen)
                    {
                        for (int i = 1; i < 5; i++)
                        {
                            ControllerA.SetIntensity(i.ToString(), "0");
                            Thread.Sleep(20);
                        }
                    }
                }
                else
                {
                    UpdateLog("串口未初始化!", Color.Red);
                }

                actLoadHdev.BeginInvoke(( o)=> {
                
                    UpdateLog("算法加载完成!",Color.Lime);


                },null);

                if (mAcqFIFOManager.Connection1)
                {
                    numExpose.Value = Convert.ToDecimal(SysConfig.Exposure1);
                    numGain.Value = Convert.ToDecimal(SysConfig.Gain1);
                }
                tableLayoutPanel2.Enabled = false;
                string[] jobs = GetAllJobs();
            }
            catch (Exception ex)
            {
                ErrLog.WriteLogEx(ex.ToString());
            }

        }

        private void numExpose_ValueChanged(object sender, EventArgs e)
        {
            if (mAcqFIFOManager != null)
            {
                mAcqFIFOManager.SetExposure(1,Convert.ToDouble(numExpose.Value));
                SysConfig.INIConfig.IniWriteValue("Cam1", "Exposure", numExpose.Value.ToString());
            }
        }

        private void numGain_ValueChanged(object sender, EventArgs e)
        {
            if (mAcqFIFOManager != null)
            {
                mAcqFIFOManager.SetGain(1, Convert.ToSingle(numGain.Value));
                SysConfig.INIConfig.IniWriteValue("Cam1", "Gain", numGain.Value.ToString());
            }
        }

        private void btnLoadLocalImage_Click(object sender, EventArgs e)
        {
            try
            {
                for (int i = 0; i < 4; i++)
                {
                    HOperatorSet.GenEmptyObj(out ho_image[i]);
                }
               
                HOperatorSet.GenEmptyObj(out img);
                OpenFileDialog openFile = new OpenFileDialog();
                    openFile.Filter = "图片|*.jpg;*.png;*.jpeg;*.bmp";
                    openFile.Multiselect = true;
                    if (openFile.ShowDialog() == DialogResult.OK)
                    {
                        string[] file = openFile.FileNames;
                        for (int i = 0; i < 4; i++)
                        {
                            HOperatorSet.ReadImage(out ho_image[i], file[i]);
                        
                        }
                        mRecordDisplay.Image = ho_image[3];
                        HOperatorSet.ConcatObj(ho_image[0], ho_image[1], out img);
                        HOperatorSet.ConcatObj(img, ho_image[2], out img);
                        HOperatorSet.ConcatObj(img, ho_image[3], out img);
                    }

            }
            catch (Exception ex)
            {

                ErrLog.WriteLogEx(ex.ToString());
            }
        }

        private void btnRealVedio_Click(object sender, EventArgs e)
        {
            try
            {
                if (btnRealVedio.Text == "视频模式")
                {
               
                    if (mAcqFIFOManager != null)
                    {
                        btnRealVedio.Text = "视频中";
                        mAcqFIFOManager.StopAcquire(1);
                        Thread.Sleep(100);
                        mAcqFIFOManager.SetTriggerModel(1, false);
                        Thread.Sleep(100);
                        mAcqFIFOManager.StartAcquire(1);

                        IsVedio =  mAcqFIFOManager.AcqFifo1.GetTrigger();
                    }
                }
                else
                {
                    btnRealVedio.Text = "视频模式";
                    mAcqFIFOManager.StopAcquire(1);
                    Thread.Sleep(100);
                    mAcqFIFOManager.SetTriggerModel(1, true);
                    Thread.Sleep(100);
                    mAcqFIFOManager.StartAcquire(1);
                    IsVedio = mAcqFIFOManager.AcqFifo1.GetTrigger();
                }
            }
            catch (Exception ex)
            {

                ErrLog.WriteLogEx(ex.ToString());
            }
        }

        private void btnConfig_Click(object sender, EventArgs e)
        {
            FrmProperty frm = new FrmProperty();
            frm.ShowDialog();
       
        }
        /// <summary>
        /// 相机软触发一次
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSoftTrig_Click(object sender, EventArgs e)
        {
            try
            {
                if (mAcqFIFOManager != null && mAcqFIFOManager.Connection1)
                {
                    Interlocked.Exchange(ref _indexImage, 0);
                    OpenLightAndSoftTrig("1",curExpose);//触发
                }
            }
            catch (Exception ex)
            {
                ErrLog.WriteLogEx(ex.ToString());
            }
        }

        #endregion

        private void labTitle_Click(object sender, EventArgs e)
        {
            tableLayoutPanel2.Enabled = !tableLayoutPanel2.Enabled;
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
    }
}
