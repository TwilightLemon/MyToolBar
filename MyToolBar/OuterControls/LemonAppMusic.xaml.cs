using MyToolBar.Func;
using System;
using System.Text.Json.Nodes;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace MyToolBar.OuterControls
{
    /// <summary>
    /// LemonAppMusic.xaml 的交互逻辑
    /// </summary>
    public partial class LemonAppMusic : OuterControlBase
    {
        public LemonAppMusic()
        {
            InitializeComponent();
            Loaded += LemonAppMusic_Loaded;
        }
        private MsgHelper ms = new MsgHelper();
        private void LemonAppMusic_Loaded(object sender, RoutedEventArgs e)
        {
            MaxStyleAct= maxStyleAct;
            ms.Start();
            ms.MsgReceived += Ms_MsgReceived;
        }
        private void maxStyleAct(bool max,Brush foreColor) {
            if(foreColor!=null)
                LyricTb.Foreground = foreColor;
            else LyricTb.SetResourceReference(ForegroundProperty, "ForeColor");
            if (max)
            {
                LyricTb.FontWeight = FontWeights.Normal;
            }
            else
            {
                LyricTb.FontWeight = FontWeights.Bold;
            }
        }
        private void Ms_MsgReceived(string str)
        {
            Dispatcher.Invoke(() => {
                var obj = JsonNode.Parse(str);
                if (str.Contains("LemonAppLyricData"))
                {
                    if (str.Contains("Handle"))
                        MsgHelper.ConnectedWindowHandle = int.Parse(obj["Handle"].ToString());
                    string data = obj["Data"].ToString() + " 🎵";
                    LyricTb.Text = data;
                    IsShown = true;
                }
                else if (str.Contains("LemonAppOrd"))
                {
                    string data = obj["Data"].ToString();
                    if (data == "Start")
                    {
                        IsShown = true;
                        if (str.Contains("Handle"))
                            MsgHelper.ConnectedWindowHandle = int.Parse(obj["Handle"].ToString());
                    }
                    else if (data == "Exit")
                    {
                        MsgHelper.ConnectedWindowHandle = 0;
                        IsShown = false;
                    }
                }
            });
        }

        private void Func_Left_TouchDown(object sender, TouchEventArgs e)
        {
            MsgHelper.SendMsg(MsgHelper.SEND_LAST, MsgHelper.ConnectedWindowHandle);
        }

        private void Func_Center_TouchDown(object sender, TouchEventArgs e)
        {
            MsgHelper.SendMsg(MsgHelper.SEND_PAUSE, MsgHelper.ConnectedWindowHandle);
        }

        private void Func_Right_TouchDown(object sender, TouchEventArgs e)
        {
            MsgHelper.SendMsg(MsgHelper.SEND_NEXT, MsgHelper.ConnectedWindowHandle);
        }
    }
}
