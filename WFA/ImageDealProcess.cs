using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HalconDotNet;

namespace WFA
{
    public class ImageDealProcess
    {
        //静态实例初始化
        public static ImageDealProcess I_P = new ImageDealProcess();

        HDevEngine hengine;  //定义调用hdev程序的引擎       
        public static HDevProcedureCall hprocall_A1; //定义hdev程序执行实例 
        public static HDevProcedureCall hprocall_B1; //定义hdev程序执行实例 
        public static HDevProcedureCall hprocall_L; //定义hdev程序执行实例 
        public static HDevProcedureCall hprocall_R; //定义hdev程序执行实例 

        public void LoadHdevFile()
        {
            try
            {
                string exePath = string.Format(AppDomain.CurrentDomain.BaseDirectory + "HDEV\\{0}",SysConfig.DefaultJob);
                //string exePath = AppDomain.CurrentDomain.BaseDirectory + "HDEV\\"; 


                //设置hdev程序所在路径
                hengine = new HDevEngine();    //新建对应实例
                hengine.SetProcedurePath(exePath);
                var Program = new HDevProcedure("Inspect_ASF_A1");
                hprocall_A1 = new HDevProcedureCall(Program);

                var Programb = new HDevProcedure("Inspect_ASF_B1");
                hprocall_B1 = new HDevProcedureCall(Programb);

                var ProgramL = new HDevProcedure("Inspect_HAF_L");
                hprocall_L = new HDevProcedureCall(ProgramL);

                var ProgramR = new HDevProcedure("Inspect_HAF_R");
                hprocall_R = new HDevProcedureCall(ProgramR);


            }
            catch (Exception ex)
            {
                ErrLog.WriteLogEx(ex.ToString());
            }
        }


        public bool RunASF_A1(HObject ImageAll)
        {
       
            try
            {               
                hprocall_A1.SetInputIconicParamObject("Images", ImageAll);
                hprocall_A1.SetInputCtrlParamTuple("minArea", SysConfig.BlobMin);
                hprocall_A1.SetInputCtrlParamTuple("maxArea", SysConfig.BlobMax);
                hprocall_A1.Execute();
                return true;
            }
            catch (HDevEngineException ex)
            {
               
                ErrLog.WriteLogEx(ex.ToString());
                return false;
            }
          
        }

        public bool RunASF_B1(HObject ImageAll)
        {

            try
            {
                hprocall_B1.SetInputIconicParamObject("Images", ImageAll);
                hprocall_B1.SetInputCtrlParamTuple("minArea", SysConfig.BlobMin);
                hprocall_B1.SetInputCtrlParamTuple("maxArea", SysConfig.BlobMax);
                hprocall_B1.Execute();
                return true;
            }
            catch (HDevEngineException ex)
            {

                ErrLog.WriteLogEx(ex.ToString());
                return false;
            }

        }

        public bool Run_HAF_L(HObject ImageAll)
        {

            try
            {
                hprocall_L.SetInputIconicParamObject("Images", ImageAll);
                hprocall_L.SetInputCtrlParamTuple("minArea", SysConfig.BlobMin);
                hprocall_L.SetInputCtrlParamTuple("maxArea", SysConfig.BlobMax);
                hprocall_L.Execute();
                return true;
            }
            catch (HDevEngineException ex)
            {

                ErrLog.WriteLogEx(ex.ToString());
                return false;
            }

        }


        public bool Run_HAF_R(HObject ImageAll)
        {

            try
            {
                hprocall_R.SetInputIconicParamObject("Images", ImageAll);
                hprocall_R.SetInputCtrlParamTuple("minArea", SysConfig.BlobMin);
                hprocall_R.SetInputCtrlParamTuple("maxArea", SysConfig.BlobMax);
                hprocall_R.Execute();
                return true;
            }
            catch (HDevEngineException ex)
            {

                ErrLog.WriteLogEx(ex.ToString());
                return false;
            }

        }



    }
}
