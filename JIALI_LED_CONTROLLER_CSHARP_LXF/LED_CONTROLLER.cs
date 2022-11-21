using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JIALI_LED_CONTROLLER_CSHARP_LXF
{
    interface LED_CONTROLLER
    {
        //设置对应通道强度
        string SetIntensity(String ch,string value);

        //打开所有通道亮度
        string OpenAllChannel();

        //打开某一通道亮度
        string OpenChannel(string ch);

        //关闭所有通道亮度
        string CloseALLChannel();

        //关闭某一通道亮度
        string CloseChannel(string ch);

        //获取当前通道亮度
        string GetIntensity(string ch);

        //获取当前通道打开关闭状态
        string GetONorOFF();

        //针对异或检验的控制器，根据前置位数据获取对应异或结果
        string GetXorResualt(String msg);

        
    }
}
