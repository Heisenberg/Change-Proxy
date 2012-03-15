using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Microsoft.Win32;
using System.Net;
using System.Runtime.InteropServices;
using System.Xml;
using System.Collections;

namespace Change
{
    public partial class FormMain : Form
    {
        private static Int32 HeightHalf;
        private static Int32 HeightFull;
        private static Color BtnColor;

        private static Icon IconOn;
        private static Icon IconOff;
        private static bool IconFlag;
        private static string IconStatus;

        public FormMain()
        {
            InitializeComponent();
        }

        [DllImport(@"wininet",
        SetLastError = true,
        CharSet = CharSet.Auto,
        EntryPoint = "InternetSetOption",
        CallingConvention = CallingConvention.StdCall)]

        public static extern bool InternetSetOption
        (
            int hInternet,
            int dmOption,
            IntPtr IpBuffer,
            int dwBufferLength
        );

        private void FormMain_Load(object sender, EventArgs e)
        {
            char[] sp = { ':' };
            IconOn = new Icon("ethernet-on.ico");
            IconOff = new Icon("ethernet-off.ico");

            BtnColor = this.btnConfig.BackColor;
            HeightHalf = this.labelIP.Height + this.cbGate.Height + this.tbAddress.Height + this.btnOK.Height + 100;
            HeightFull = this.labelIP.Height + this.cbGate.Height + this.tbAddress.Height + this.btnOK.Height + this.btnConfig.Height + this.lvProxy.Height + this.btnSave.Height + 160;
            //实现ListView点击排序功能
            this.lvProxy.ListViewItemSorter = new Common.ListViewColumnSorter();
            this.lvProxy.ColumnClick += new ColumnClickEventHandler(Common.ListViewHelper.ListView_ColumnClick);

            //将这项改为控件高度和的多项式，或外部变量\
            this.Height = HeightHalf;
            //打开注册表
            RegistryKey regKey = Registry.CurrentUser;
            string SubKeyPath = @"Software\Microsoft\Windows\CurrentVersion\Internet Settings";
            RegistryKey optionKey = regKey.OpenSubKey(SubKeyPath, true);
            if (optionKey.GetValue("ProxyEnable").ToString() == "1")
            {
                this.cbGate.Checked = true;
                string s = optionKey.GetValue("ProxyServer").ToString();
                string[] cs = optionKey.GetValue("ProxyServer").ToString().Split(sp, StringSplitOptions.None);
                this.tbAddress.Text = cs[0];
                this.tbPort.Text = cs[1];
                IconFlag = true;
            }
            else
            {
                this.cbGate.Checked = false;
                //this.tbAddress.Text = "10.3.161.44";
                //this.tbPort.Text = "808";
                this.tbAddress.Enabled = false;
                this.tbPort.Enabled = false;
                IconFlag = false;
            }

            if (IconOn != null && IconOff != null) //如果两个图标文件都被正确载入
            {
                if (IconFlag == true)//如果托盘图标为icon1，则更改为icon2
                {
                    this.notifyIcon1.Icon = IconOn;
                    IconStatus = "开启";
                }
                else//否则则将图标设置为icon1
                {
                    this.notifyIcon1.Icon = IconOff;
                    IconStatus = "关闭";
                }
            }
            this.notifyIcon1.Text = "代理修改助手" + "\n状态：" + IconStatus + "\n" + "designed by apie";

            //加载XML文件
            ReadXML("proxy.xml");
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            try
            {
                if (cbGate.Checked == true)
                {
                    //打开注册表
                    RegistryKey regKey = Registry.CurrentUser;
                    string SubKeyPath = @"Software\Microsoft\Windows\CurrentVersion\Internet Settings";
                    RegistryKey optionKey = regKey.OpenSubKey(SubKeyPath, true);
                    //更改健值，设置代理，
                    optionKey.SetValue("ProxyEnable", 1);
                    optionKey.SetValue("ProxyServer", this.tbAddress.Text.ToString() + ":" + this.tbPort.Text.ToString());
                    InternetSetOption(0, 39, IntPtr.Zero, 0);
                    InternetSetOption(0, 37, IntPtr.Zero, 0);
                    IconFlag = true;

                    MessageBox.Show("设置成功！！！");
                }
                else
                {
                    //打开注册表键
                    RegistryKey rk = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Internet Settings", true);
                    //设置代理不可用
                    rk.SetValue("ProxyEnable", 0);
                    InternetSetOption(0, 39, IntPtr.Zero, 0);
                    InternetSetOption(0, 37, IntPtr.Zero, 0);
                    rk.Close();
                    IconFlag = false;

                    MessageBox.Show("设置成功！！！");
                }

                if (IconOn != null && IconOff != null) //如果两个图标文件都被正确载入
                {
	                if (IconFlag == true)//如果托盘图标为icon1，则更改为icon2
	                {
                            this.notifyIcon1.Icon = IconOn;
                            IconStatus = "开启";
	                }
	                else//否则则将图标设置为icon1
	                {
		                this.notifyIcon1.Icon = IconOff;
                        IconStatus = "关闭";
	                }
                }
                this.notifyIcon1.Text = "代理修改助手" + "\n状态：" + IconStatus + "\n" + "designed by apie";
            }
            catch (System.IndexOutOfRangeException eg)
            {
                System.Console.WriteLine(eg.Message);
                //set IndexOutOfRangeException to the new exception's InnerException
                throw new System.ArgumentOutOfRangeException("index parameter is out of range", eg);
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            try
            {
                DialogResult MsgBoxResult;//设置对话框的返回值
                MsgBoxResult = MessageBox.Show("真的就这么走了",//对话框的显示内容 
                                                "提示",//对话框的标题 
                                                MessageBoxButtons.YesNo,//定义对话框的按钮，这里定义了YSE和NO两个按钮 
                                                MessageBoxIcon.Exclamation,//定义对话框内的图表式样，这里是一个黄色三角型内加一个感叹号 
                                                MessageBoxDefaultButton.Button2);//定义对话框的按钮式样
                if (MsgBoxResult == DialogResult.Yes)//如果对话框的返回值是YES（按"Y"按钮）
                {
                    //需要确定是否需要保存XML文件
                    this.notifyIcon1.Visible = false;  //托盘图标隐藏
                    this.Close();
                }
                if (MsgBoxResult == DialogResult.No)//如果对话框的返回值是NO（按"N"按钮）
                {
                    MessageBox.Show("幸亏及早发现啊");
                }

            }
            catch (System.IndexOutOfRangeException eg)
            {
                System.Console.WriteLine(eg.Message);
                //set IndexOutOfRangeException to the new exception's InnerException
                throw new System.ArgumentOutOfRangeException("index parameter is out of range", eg);
            }
        }

        private void cbGate_CheckedChanged(object sender, EventArgs e)
        {
            if (cbGate.Checked == false)
            {
                this.tbAddress.Enabled = false;
                this.tbPort.Enabled = false;
            }
            else
            {
                this.tbAddress.Enabled = true;
                this.tbPort.Enabled = true;
            }
        }

        private void btnConfig_Click(object sender, EventArgs e)
        {
            try
            {
                if (this.Height == HeightHalf)
                {
                    this.Height = HeightFull;
                    this.btnConfig.BackColor = Color.Gray;
                }
                else
                {
                    this.Height = HeightHalf;
                    this.btnConfig.BackColor = BtnColor;
                }
            }
            catch (System.IndexOutOfRangeException eg)
            {
                System.Console.WriteLine(eg.Message);
                //set IndexOutOfRangeException to the new exception's InnerException
                throw new System.ArgumentOutOfRangeException("index parameter is out of range", eg);
            }
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            try
            {
                DialogResult MsgBoxResult;//设置对话框的返回值
                MsgBoxResult = MessageBox.Show("真的要全部清空",//对话框的显示内容 
                                                "提示",//对话框的标题 
                                                MessageBoxButtons.YesNo,//定义对话框的按钮，这里定义了YSE和NO两个按钮 
                                                MessageBoxIcon.Exclamation,//定义对话框内的图表式样，这里是一个黄色三角型内加一个感叹号 
                                                MessageBoxDefaultButton.Button2);//定义对话框的按钮式样
                if (MsgBoxResult == DialogResult.Yes)//如果对话框的返回值是YES（按"Y"按钮）
                {
                    //保留项目名称，清空键值，需有提醒
                    this.lvProxy.Items.Clear();
                }
                if (MsgBoxResult == DialogResult.No)//如果对话框的返回值是NO（按"N"按钮）
                {
                    MessageBox.Show("幸亏及早发现啊");
                }
            }
            catch (System.IndexOutOfRangeException eg)
            {
                System.Console.WriteLine(eg.Message);
                //set IndexOutOfRangeException to the new exception's InnerException
                throw new System.ArgumentOutOfRangeException("index parameter is out of range", eg);
            }            
        }

        private void btnDown_Click(object sender, EventArgs e)
        {
            try
            {
                if (this.tbAddress.Text.Length != 0 && this.tbPort.Text.Length != 0)
                {
                    ListViewItem listItem;
                    listItem = new ListViewItem();
                    listItem.Text = this.tbAddress.Text.ToString();
                    listItem.SubItems.Add(this.tbPort.Text.ToString());
                    this.lvProxy.Items.Add(listItem);
                }
                else
                {
                    MessageBox.Show("请将信息填写完整！！");
                }
            }
            catch (System.IndexOutOfRangeException eg)
            {
                System.Console.WriteLine(eg.Message);
                //set IndexOutOfRangeException to the new exception's InnerException
                throw new System.ArgumentOutOfRangeException("index parameter is out of range", eg);
            }
        }

        private void btnUp_Click(object sender, EventArgs e)
        {
            try
            {
                if (this.lvProxy.SelectedItems.Count > 0)
                {
                    string s1 = string.Empty;
                    string s2 = string.Empty;

                    s1 = this.lvProxy.SelectedItems[0].Text.ToString();
                    s2 = this.lvProxy.SelectedItems[0].SubItems[1].Text.ToString();
                    this.tbAddress.Text = s1;
                    this.tbPort.Text = s2;
                }
                else
                {
                    MessageBox.Show("请选中一个再说");
                }
            }
            catch (System.IndexOutOfRangeException eg)
            {
                System.Console.WriteLine(eg.Message);
                //set IndexOutOfRangeException to the new exception's InnerException
                throw new System.ArgumentOutOfRangeException("index parameter is out of range", eg);
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            try
            {
                WriteXML("proxy.xml");
                MessageBox.Show("OK");
            }
            catch (System.IndexOutOfRangeException eg)
            {
                System.Console.WriteLine(eg.Message);
                //set IndexOutOfRangeException to the new exception's InnerException
                throw new System.ArgumentOutOfRangeException("index parameter is out of range", eg);
            }
        }

        private void btnDel_Click(object sender, EventArgs e)
        {
            try
            {
                //判断是否有项被选中
                if (this.lvProxy.SelectedItems.Count > 0 )
                {
                    DialogResult MsgBoxResult;//设置对话框的返回值
                    MsgBoxResult = MessageBox.Show("真的要删除这项",//对话框的显示内容 
                                                    "提示",//对话框的标题 
                                                    MessageBoxButtons.YesNo,//定义对话框的按钮，这里定义了YSE和NO两个按钮 
                                                    MessageBoxIcon.Exclamation,//定义对话框内的图表式样，这里是一个黄色三角型内加一个感叹号 
                                                    MessageBoxDefaultButton.Button2);//定义对话框的按钮式样
                    if (MsgBoxResult == DialogResult.Yes)//如果对话框的返回值是YES（按"Y"按钮）
                    {
                        this.lvProxy.Items.Remove(this.lvProxy.SelectedItems[0]);
                    }
                    if (MsgBoxResult == DialogResult.No)//如果对话框的返回值是NO（按"N"按钮）
                    {
                        MessageBox.Show("幸亏及早发现啊");
                    }
                }
                else
                {
                    MessageBox.Show("请选中一个再说");
                }
            }
            catch (System.IndexOutOfRangeException eg)
            {
                System.Console.WriteLine(eg.Message);
                //set IndexOutOfRangeException to the new exception's InnerException
                throw new System.ArgumentOutOfRangeException("index parameter is out of range", eg);
            }
        }

        //ReadXml 完成对Proxy的读取 
        //FileName 当前xml文件的存放位置////, string ProxyAddress, string ProxyPort
        //ProxyAddress 欲添加Proxy的Address
        //ProxyPort 欲添加Proxy的Port
        public void ReadXML(string FileName)
        {
            //初始化ListView
            this.lvProxy.View = View.Details;
            this.lvProxy.Columns.Clear();

            ColumnHeader columnHeader = new ColumnHeader();
            columnHeader.Text = "Address";
            columnHeader.Width = 150;
            this.lvProxy.Columns.Add(columnHeader);

            columnHeader = new ColumnHeader();
            columnHeader.Text = "Port";
            columnHeader.Width = 50;
            this.lvProxy.Columns.Add(columnHeader);

            //初始化XML文档
            XmlDocument doc = new XmlDocument();
            try
            {
                //加载XML文档
                doc.Load(FileName);

                XmlNodeList nodeList = doc.GetElementsByTagName("Connection");

                ListViewItem listItem;

                foreach (XmlNode xmlNode in nodeList)
                {
                    XmlNodeReader nodeReader = new XmlNodeReader(xmlNode);
                    listItem = new ListViewItem();
                    while (nodeReader.Read())
                    {
                        if (nodeReader.NodeType == XmlNodeType.Element && nodeReader.Name != "Connection")
                        {
                            if (nodeReader.Name.Equals("address"))
                            {
                                listItem.Text = nodeReader.ReadString();
                            }
                            else
                            {
                                listItem.SubItems.Add(nodeReader.ReadString());
                            }
                        }
                    }
                    lvProxy.Items.Add(listItem);
                }
            }
            catch
            {
                CreateXmlFile(FileName);
            }

        }

        //WriteXml 完成对Proxy的写入 
        //FileName 当前xml文件的存放位置////, string ProxyAddress, string ProxyPort
        //ProxyAddress 欲添加Proxy的Address
        //ProxyPort 欲添加Proxy的Port
        public void WriteXML(string FileName)
        {

            //初始化XML文档操作类
            XmlDocument myDoc = new XmlDocument();
            //加载XML文件
            myDoc.Load(FileName);

            System.Xml.XmlTextWriter writer = new System.Xml.XmlTextWriter(FileName, null);
            //使用自动缩进便于阅读
            writer.Formatting = Formatting.Indented;
 
            if(myDoc.FirstChild.NodeType != XmlNodeType.XmlDeclaration) 
            { 
                XmlDeclaration xmldecl = myDoc.CreateXmlDeclaration("1.0 ","utf-8 ", null); 
                XmlElement root = myDoc.DocumentElement; 
                myDoc.InsertBefore(xmldecl, root); 
            }

            //写入XML文件的头声明      
            //writer.WriteStartDocument();
            //writer.WriteWhitespace(System.Environment.NewLine);
            //写入根元素
            writer.WriteStartElement("Data");
            for (int i = 0; i < this.lvProxy.Items.Count; i++)
            {
                //写入根元素
                writer.WriteStartElement("Connection");
                //写入子元素
                writer.WriteElementString("address", this.lvProxy.Items[i].Text.ToString());
                writer.WriteElementString("port", this.lvProxy.Items[i].SubItems[1].Text.ToString());
                writer.WriteWhitespace("\n");
                writer.WriteEndElement();
            }
            writer.WriteFullEndElement();

            writer.Close();
        }

        /// <summary>
        /// 创建XML文件
        /// </summary>
        /// <param name="FileName"></param>
        public void CreateXmlFile(string FileName)
        {
            XmlDocument xmldoc = new XmlDocument();
            XmlNode node;
            System.Xml.XmlTextWriter writer = new System.Xml.XmlTextWriter(FileName, null);

            node = xmldoc.CreateXmlDeclaration("1.0", "utf-8", null);
            xmldoc.AppendChild(node);
            writer.WriteWhitespace(System.Environment.NewLine);
            XmlNode root = xmldoc.CreateElement("Data");
            xmldoc.AppendChild(root);
            CreateNode(xmldoc, root, "Address", "127.0.0.1");
            CreateNode(xmldoc, root, "Port", "8087");
            try
            {
                xmldoc.Save(FileName);
                writer.Close();
            }
            catch (Exception ex)
            {
               MessageBox.Show(ex.Message);
            }
        }

        /// <summary>
        /// 创建节点
        /// </summary>
        /// <param name="xmldoc"></param>
        /// <param name="parentnode"></param>
        /// <param name="name"></param>
        /// <param name="value"></param>
        public void CreateNode(XmlDocument xmldoc, XmlNode parentnode, string name, string value)
        {
            XmlNode node = xmldoc.CreateNode(XmlNodeType.Element, name, null);
            node.InnerText = value;
            parentnode.AppendChild(node);
        }

        //处理窗体最小化：窗口最小化，同时显示任务栏图标
        private void FormMain_Resize(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized)
            { 
                this.WindowState = FormWindowState.Minimized; 
                this.Visible = false;
                this.notifyIcon1.Visible = true;
            } 
        }

        //双击任务栏图标回复窗体，任务栏图标隐藏
        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
             if (this.WindowState == FormWindowState.Minimized)
             { 
                 this.Visible = true;
                 this.ShowInTaskbar = true;  //显示在系统任务栏
                 this.WindowState = FormWindowState.Normal;  //还原窗体 
                 this.notifyIcon1.Visible = false;  //托盘图标隐藏
             } 
        }
    }
}
