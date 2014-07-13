//using CefSharp;
//using CefSharp.WinForms;
//using System;
//using System.Collections.Generic;
//using System.ComponentModel;
//using System.Data;
//using System.Drawing;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using System.Windows.Forms;

//namespace LiveSplit.Video
//{
//    public partial class Form2 : Form, LiveSplit.UI.Components.IComponent
//    {
//        private Form1 form1;
//        int i = 0;
//        private WebView web_view;

//        public Form2()
//        {
//            InitializeComponent();
//            try
//            {
//                web_view = new WebView("https://github.com/perlun/CefSharp", new BrowserSettings());
//                web_view.Dock = DockStyle.Fill;
//                this.Controls.Add(web_view);
//            }
//            catch { }

//            form1 = new Form1();
//            form1.Show();
//        }

//        private void Form2_Paint(object sender, PaintEventArgs e)
//        {
//            var b = new Bitmap(web_view.Width, web_view.Height);
//            web_view.DrawToBitmap(b, new Rectangle(0, 0, b.Width, b.Height));
//            b.Save("D:\\unity\\" + (i++) + ".png");
//            form1.RefreshImage();
//            if (pictureBox1.Image != null)
//                pictureBox1.Image.Dispose();
//            pictureBox1.Image = form1.GetImage();
//        }

//        private void Form2_Load(object sender, EventArgs e)
//        {
//            form1.Show();

//            new Timer() { Interval = 16, Enabled = true }.Tick += (s, ev) => this.Invalidate();
//        }

//        public string ComponentName
//        {
//            get { return "Video"; }
//        }

//        public Control GetSettingsControl(UI.LayoutMode mode)
//        {
//            return null;
//        }

//        public System.Xml.XmlNode GetSettings(System.Xml.XmlDocument document)
//        {
//            throw new NotImplementedException();
//        }

//        public void SetSettings(System.Xml.XmlNode settings)
//        {
//            throw new NotImplementedException();
//        }

//        public float HorizontalWidth
//        {
//            get { throw new NotImplementedException(); }
//        }

//        public float MinimumHeight
//        {
//            get { throw new NotImplementedException(); }
//        }

//        public void DrawHorizontal(Graphics g, Model.LiveSplitState state, float height)
//        {
//            throw new NotImplementedException();
//        }

//        public float VerticalHeight
//        {
//            get { return 150; }
//        }

//        public float MinimumWidth { get; set; }

//        public void DrawVertical(Graphics g, Model.LiveSplitState state, float width)
//        {
//            form1.RefreshImage();
//            using (var image = form1.GetImage())
//            {
//                var aspectWidth = image.Width / (float)image.Height * VerticalHeight;
//                g.DrawImage(image, 0.5f * (width - aspectWidth), 0, aspectWidth, VerticalHeight);
//                MinimumWidth = aspectWidth;
//            }
//        }
//    }
//}
