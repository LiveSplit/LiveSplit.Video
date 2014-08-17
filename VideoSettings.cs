using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using LiveSplit.TimeFormatters;
using System.Text.RegularExpressions;
using LiveSplit.Options;
using System.Globalization;
using LiveSplit.Model;
using LiveSplit.UI;
using System.Web;

namespace LiveSplit.Video
{
    public partial class VideoSettings : UserControl
    {
        public String MRL { get { return HttpUtility.UrlPathEncode("file:///" + VideoPath.Replace('\\', '/').Replace("%", "%25")); } }
        public String VideoPath { get; set; }
        //public bool PerSegmentVideo { get; set; }
        public TimeSpan Offset { get; set; }
        public float Height { get; set; }
        public float Width { get; set; }
        public LayoutMode Mode { get; set; }

        protected ITimeFormatter TimeFormatter { get; set; }

        public String OffsetString
        {
            get
            {
                return TimeFormatter.Format(Offset);
            }
            set
            {
                if (Regex.IsMatch(value, "[^0-9:.,-]"))
                    return;

                try { Offset = TimeSpanParser.Parse(value); }
                catch (Exception ex)
                {
                    Log.Error(ex);
                }
            }
        }

        public VideoSettings()
        {
            InitializeComponent();

            TimeFormatter = new ShortTimeFormatter();

            VideoPath = "";
            Width = 200;
            Height = 200;
            //PerSegmentVideo = false;
            Offset = TimeSpan.Zero;

            //chkPerSegment.DataBindings.Add("Checked", this, "PerSegmentVideo", false, DataSourceUpdateMode.OnPropertyChanged);
            txtVideoPath.DataBindings.Add("Text", this, "VideoPath", false, DataSourceUpdateMode.OnPropertyChanged);
            txtOffset.DataBindings.Add("Text", this, "OffsetString");
        }

        public void SetSettings(XmlNode node)
        {
            var element = (XmlElement)node;
            Version version;
            if (element["Version"] != null)
                version = Version.Parse(element["Version"].InnerText);
            else
                version = new Version(1, 0, 0, 0);
            VideoPath = element["VideoPath"].InnerText;
            OffsetString = element["Offset"].InnerText;
            Height = Single.Parse(element["Height"].InnerText, CultureInfo.InvariantCulture);
            Width = Single.Parse(element["Width"].InnerText, CultureInfo.InvariantCulture);
            //PerSegmentVideo = Boolean.Parse(element["PerSegmentVideo"].InnerText);
        }

        public XmlNode GetSettings(XmlDocument document)
        {
            var parent = document.CreateElement("Settings");
            parent.AppendChild(ToElement(document, "Version", "1.4"));
            parent.AppendChild(ToElement(document, "VideoPath", VideoPath));
            parent.AppendChild(ToElement(document, "Offset", OffsetString));
            parent.AppendChild(ToElement(document, "Height", Height));
            parent.AppendChild(ToElement(document, "Width", Height));
            //parent.AppendChild(ToElement(document, "PerSegmentVideo", PerSegmentVideo));
            return parent;
        }

        private XmlElement ToElement<T>(XmlDocument document, String name, T value)
        {
            var element = document.CreateElement(name);
            element.InnerText = value.ToString();
            return element;
        }

        private XmlElement ToElement(XmlDocument document, String name, float value)
        {
            var element = document.CreateElement(name);
            element.InnerText = value.ToString(CultureInfo.InvariantCulture);
            return element;
        }

        private void btnSelectFile_Click(object sender, EventArgs e)
        {
            var dialog = new OpenFileDialog()
            {
                FileName = VideoPath,
                Filter = "Video Files|*.avi;*.mpeg;*.mpg;*.mp4;*.mov;*.wmv;*.m4v;*.flv;*.mkv;*.ogg|All Files (*.*)|*.*"
            };
            var result = dialog.ShowDialog();
            if (result == DialogResult.OK)
                VideoPath = txtVideoPath.Text = dialog.FileName;
        }

        private void VideoSettings_Load(object sender, EventArgs e)
        {
            if (Mode == LayoutMode.Horizontal)
            {
                trkHeightWidth.DataBindings.Clear();
                trkHeightWidth.Minimum = 100;
                trkHeightWidth.Maximum = 400;
                trkHeightWidth.DataBindings.Add("Value", this, "Width", false, DataSourceUpdateMode.OnPropertyChanged);
                lblHeightWidth.Text = "Width:";
            }
            else
            {
                trkHeightWidth.DataBindings.Clear();
                trkHeightWidth.Minimum = 100;
                trkHeightWidth.Maximum = 300;
                trkHeightWidth.DataBindings.Add("Value", this, "Height", false, DataSourceUpdateMode.OnPropertyChanged);
                lblHeightWidth.Text = "Height:";
            }
        }
    }
}
