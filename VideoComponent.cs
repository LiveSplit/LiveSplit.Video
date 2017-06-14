using AxAXVLC;
using LiveSplit.Model;
using LiveSplit.UI.Components;
using System;
using System.Windows.Forms;
using System.Drawing;
using System.Xml;

namespace LiveSplit.Video
{
    public class VideoComponent : ControlComponent
    {
        public VideoSettings Settings { get; set; }
        public LiveSplitState State { get; set; }
        public System.Timers.Timer SynchronizeTimer { get; set; }

        private class VLCErrorException : Exception { }

        protected string OldMRL { get; set; }

        public override string ComponentName => "Video";

        public override float HorizontalWidth => Settings.Width;

        public override float MinimumHeight => 10;

        public override float VerticalHeight => Settings.Height;

        public override float MinimumWidth => 10;

        public AxVLCPlugin2 VLC { get; set; }
        public bool Initialized { get; set; }

        public VideoComponent(LiveSplitState state)
            : this(state, CreateVLCControl())
        { }

        public VideoComponent(LiveSplitState state, AxVLCPlugin2 vlc)
            : base(state, vlc, ex => ErrorCallback(state.Form, ex))
        {
            Settings = new VideoSettings();
            State = state;
            VLC = vlc;

            state.OnReset += state_OnReset;
            state.OnStart += state_OnStart;
            state.OnPause += state_OnPause;
            state.OnResume += state_OnResume;
        }

        static void ErrorCallback(Form form, Exception ex)
        {
            string requiredBits = Environment.Is64BitProcess ? "64" : "32";
            MessageBox.Show(form, "VLC Media Player 2.2.1 (" + requiredBits + "-bit) along with the ActiveX Plugin need to be installed for the Video Component to work.", "Video Component Could Not Be Loaded", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        void state_OnResume(object sender, EventArgs e)
        {
            InvokeIfNeeded(() =>
            {
                lock (VLC)
                {
                    VLC.playlist.play();
                }
            });
        }

        void state_OnPause(object sender, EventArgs e)
        {
            InvokeIfNeeded(() =>
            {
                lock (VLC)
                {
                    VLC.playlist.pause();
                }
            });
        }

        void state_OnStart(object sender, EventArgs e)
        {
            InvokeIfNeeded(() =>
            {
                lock (VLC)
                {
                    VLC.playlist.play();
                    if (activated)
                        Control.Visible = true;
                }
            });
            Synchronize();
        }

        public void Synchronize()
        {
            Synchronize(TimeSpan.Zero);
        }

        public void Synchronize(TimeSpan offset)
        {
            if (SynchronizeTimer != null && SynchronizeTimer.Enabled)
                SynchronizeTimer.Enabled = false;
            InvokeIfNeeded(() =>
            {
                lock (VLC)
                    VLC.input.Time = (State.CurrentTime[State.CurrentTimingMethod].Value + offset + Settings.Offset).TotalMilliseconds;
            });
            SynchronizeTimer = new System.Timers.Timer(1000);

            SynchronizeTimer.Elapsed += (s, ev) =>
            {
                InvokeIfNeeded(() =>
                {
                    lock (VLC)
                    {
                        if (VLC.input.state == 3)
                        {
                            var delta = VLC.input.Time - (State.CurrentTime[State.CurrentTimingMethod].Value + offset + Settings.Offset).TotalMilliseconds;
                            if (Math.Abs(delta) > 500)
                                VLC.input.Time = (State.CurrentTime[State.CurrentTimingMethod].Value + offset + Settings.Offset).TotalMilliseconds + Math.Max(0, -delta);
                            else
                                SynchronizeTimer.Enabled = false;
                        }
                        else if (VLC.input.state == 5)
                        {
                            SynchronizeTimer.Enabled = false;
                        }
                    }
                });
            };

            SynchronizeTimer.Enabled = true;
        }

        void state_OnReset(object sender, TimerPhase e)
        {
            InvokeIfNeeded(() =>
            {
                lock (VLC)
                {
                    VLC.playlist.stop();
                    if (activated)
                        Control.Visible = false;
                }
            });
        }

        private static AxVLCPlugin2 CreateVLCControl()
        {
            var vlc = new AxVLCPlugin2();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ComponentHostForm));
            ((System.ComponentModel.ISupportInitialize)(vlc)).BeginInit();
            vlc.Enabled = true;
            vlc.Name = "vlc";
            vlc.OcxState = ((AxHost.State)(resources.GetObject("axVLCPlugin21.OcxState")));
            ((System.ComponentModel.ISupportInitialize)(vlc)).EndInit();

            return vlc;
        }

        public override Control GetSettingsControl(UI.LayoutMode mode)
        {
            Settings.Mode = mode;
            return Settings;
        }

        public override XmlNode GetSettings(XmlDocument document)
        {
            return Settings.GetSettings(document);
        }

        public override void SetSettings(XmlNode settings)
        {
            Settings.SetSettings(settings);
        }

        private void DisposeIfError()
        {
            if (ErrorWithControl && !VLC.IsDisposed)
            {
                Dispose();
                throw new VLCErrorException();
            }
        }

        public override void DrawVertical(Graphics g, LiveSplitState state, float width, Region clipRegion)
        {
            base.DrawVertical(g, state, width, clipRegion);
            DisposeIfError();
        }

        public override void DrawHorizontal(Graphics g, LiveSplitState state, float height, Region clipRegion)
        {
            base.DrawHorizontal(g, state, height, clipRegion);
            DisposeIfError();
        }

        public override void Update(UI.IInvalidator invalidator, LiveSplitState state, float width, float height, UI.LayoutMode mode)
        {
            if (!VLC.IsDisposed)
            {
                base.Update(invalidator, state, width, height, mode);

                if (!Initialized)
                {
                    Control.Visible = !Control.Created;
                    Initialized = Control.Created;
                }
                else
                {
                    if (VLC != null && OldMRL != Settings.MRL && !string.IsNullOrEmpty(Settings.MRL))
                    {
                        InvokeIfNeeded(() =>
                        {
                            lock (VLC)
                            {
                                VLC.playlist.items.clear();
                                VLC.playlist.add(Settings.MRL);
                            }
                        });
                    }
                    OldMRL = Settings.MRL;

                    if (VLC != null)
                    {
                        InvokeIfNeeded(() =>
                        {
                            lock (VLC)
                            {
                                VLC.audio.mute = true;
                                VLC.Volume = 5;
                            }
                        });
                    }
                }
            }
        }

        public override void Dispose()
        {
            base.Dispose();

            State.OnReset -= state_OnReset;
            State.OnStart -= state_OnStart;
            State.OnPause -= state_OnPause;
            State.OnResume -= state_OnResume;
            if (SynchronizeTimer != null)
                SynchronizeTimer.Dispose();
        }

        public int GetSettingsHashCode() => Settings.GetSettingsHashCode();
    }
}
