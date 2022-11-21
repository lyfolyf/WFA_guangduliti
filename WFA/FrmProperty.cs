using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace WFA
{
    public partial class FrmProperty : Form
    {
        public FrmProperty()
        {
            InitializeComponent();
        }

        private void FrmProperty_Load(object sender, EventArgs e)
        {
            Property py = new Property();

            propertyGrid1.SelectedObject = py;
        }

        private void propertyGrid1_PropertyValueChanged(object s, PropertyValueChangedEventArgs e)
        {


            try
            {
                switch (e.ChangedItem.PropertyDescriptor.Category.ToString())
                {
                    case "串口通信":
                          SysConfig.INIConfig.IniWriteValue("SerPort", e.ChangedItem.PropertyDescriptor.Name, e.ChangedItem.Value.ToString());
                        break;

                    case "计算":
                        SysConfig.INIConfig.IniWriteValue("Calc", e.ChangedItem.PropertyDescriptor.Name, e.ChangedItem.Value.ToString());
                        break;

                    case "其他":
                        SysConfig.INIConfig.IniWriteValue("System", e.ChangedItem.PropertyDescriptor.Name, e.ChangedItem.Value.ToString());
                        break;

                    case "网络通信":
                        SysConfig.INIConfig.IniWriteValue("System", e.ChangedItem.PropertyDescriptor.Name, e.ChangedItem.Value.ToString());
                        break;

                    default:
                        break;
                }
            }
            catch (Exception ex)
            {

               
            }



        }
    }
}
