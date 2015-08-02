using LiveSplit.Model;
using LiveSplit.Video;
using System;

namespace LiveSplit.UI.Components
{
    public class VideoFactory : IComponentFactory
    {
        public string ComponentName
        {
            get { return "Video"; }
        }

        public string Description
        {
            get { return "Shows a PB or WR video that is synced up to the current run time."; }
        }

        public ComponentCategory Category
        {
            get { return ComponentCategory.Media; }
        }

        public IComponent Create(LiveSplitState state)
        {
            return new VideoComponent(state);
        }

        public string UpdateName
        {
            get { return ComponentName; }
        }

        public string XMLURL
        {
#if RELEASE_CANDIDATE
            get { return "http://livesplit.org/update_rc_sdhjdop/Components/update.LiveSplit.Video.xml"; }
#else
            get { return "http://livesplit.org/update/Components/update.LiveSplit.Video.xml"; }
#endif
        }

        public string UpdateURL
        {
#if RELEASE_CANDIDATE
            get { return "http://livesplit.org/update_rc_sdhjdop/"; }
#else
            get { return "http://livesplit.org/update/"; }
#endif
        }

        public Version Version
        {
            get { return Version.Parse("1.1.0"); }
        }
    }
}
