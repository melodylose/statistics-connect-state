using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace StatisticsConnectStateApp.Extensions
{
    public static class ControlExtension
    {
        public static void ControlSetText(this Control ctrl, string text = "") {
            if (ctrl.InvokeRequired) {
                ctrl.Invoke(new Action(() => ctrl.ControlSetText(text)));
            }
            else {
                ctrl.Text = text;
            }
        }
    }
}
