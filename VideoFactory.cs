using LiveSplit.Model;
using LiveSplit.Video;
using System;

namespace LiveSplit.UI.Components
{
    public class VideoFactory : IComponentFactory
    {
        public string ComponentName => "Video";

        public string Description => "Shows a PB or WR video that is synced up to the current run time.";

        public ComponentCategory Category => ComponentCategory.Media;

        public IComponent Create(LiveSplitState state) => new VideoComponent(state);

        public string UpdateName => ComponentName;

        public string XMLURL => "http://livesplit.org/update/Components/update.LiveSplit.Video.xml";

        public string UpdateURL => "http://livesplit.org/update/";

        public Version Version => Version.Parse("1.7.5");
    }
}
