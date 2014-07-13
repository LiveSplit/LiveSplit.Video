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

namespace LiveSplit.Video
{
    public partial class VideoSettings : UserControl
    {
        public String MRL { get; set; }
        public bool PerSegmentVideo { get; set; }
        public TimeSpan Offset { get; set; }

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

                try { Offset = parseTime(value); }
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

            MRL = "";
            PerSegmentVideo = false;
            Offset = TimeSpan.Zero;

            chkPerSegment.DataBindings.Add("Checked", this, "PerSegmentVideo", false, DataSourceUpdateMode.OnPropertyChanged);
            txtMRL.DataBindings.Add("Text", this, "MRL", false, DataSourceUpdateMode.OnPropertyChanged);
            txtOffset.DataBindings.Add("Text", this, "OffsetString");
        }

        TimeSpan parseTime(String timeString)
        {
            double num = 0.0;
            var factor = 1;
            if (timeString.StartsWith("-"))
            {
                factor = -1;
                timeString = timeString.Substring(1);
            }

            string[] array = timeString.Split(new char[]
	                                    {
		                                    ':'
	                                    });
            for (int i = 0; i < array.Length; i++)
            {
                string s = array[i];
                double num2;
                if (double.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out num2))
                {
                    num = num * 60.0 + num2;
                }
            }

            if (factor * num > 864000)
                throw new Exception();

            return new TimeSpan((long)(factor * num * 10000000));
        }

        public void SetSettings(XmlNode node)
        {
            var element = (XmlElement)node;
            Version version;
            if (element["Version"] != null)
                version = Version.Parse(element["Version"].InnerText);
            else
                version = new Version(1, 0, 0, 0);
            MRL = element["MRL"].InnerText;
            OffsetString = element["Offset"].InnerText;
            PerSegmentVideo = Boolean.Parse(element["PerSegmentVideo"].InnerText);
        }

        public XmlNode GetSettings(XmlDocument document)
        {
            var parent = document.CreateElement("Settings");
            parent.AppendChild(ToElement(document, "Version", "1.3"));
            parent.AppendChild(ToElement(document, "MRL", MRL));
            parent.AppendChild(ToElement(document, "Offset", OffsetString));
            parent.AppendChild(ToElement(document, "PerSegmentVideo", PerSegmentVideo));
            return parent;
        }

        private XmlElement ToElement<T>(XmlDocument document, String name, T value)
        {
            var element = document.CreateElement(name);
            element.InnerText = value.ToString();
            return element;
        }
    }
}
