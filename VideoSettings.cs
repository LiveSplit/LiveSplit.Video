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
            Offset = TimeSpan.Zero;

            txtVideoPath.DataBindings.Add("Text", this, "VideoPath", false, DataSourceUpdateMode.OnPropertyChanged);
            txtOffset.DataBindings.Add("Text", this, "OffsetString");
        }

        public void SetSettings(XmlNode node)
        {
            var element = (XmlElement)node;
            VideoPath = SettingsHelper.ParseString(element["VideoPath"]);
            OffsetString = SettingsHelper.ParseString(element["Offset"]);
            Height = SettingsHelper.ParseFloat(element["Height"]);
            Width = SettingsHelper.ParseFloat(element["Width"]);
        }

        public XmlNode GetSettings(XmlDocument document)
        {
            var parent = document.CreateElement("Settings");
            parent.AppendChild(SettingsHelper.ToElement(document, "Version", "1.4"));
            parent.AppendChild(SettingsHelper.ToElement(document, "VideoPath", VideoPath));
            parent.AppendChild(SettingsHelper.ToElement(document, "Offset", OffsetString));
            parent.AppendChild(SettingsHelper.ToElement(document, "Height", Height));
            parent.AppendChild(SettingsHelper.ToElement(document, "Width", Height));
            return parent;
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
