//using AxAXVLC;
using LiveSplit.Model;
using LiveSplit.UI.Components;
using System;
using System.Windows.Forms;
using System.Drawing;
using System.Xml;
using LibVLCSharp.Shared;
using LibVLCSharp.WinForms;
using System.Diagnostics;

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

        //public AxVLCPlugin2 VLC { get; set; }

        public bool Initialized { get; set; }

        private static readonly LibVLC libVLC;

        static VideoComponent()
        {
            Core.Initialize();
            libVLC = new LibVLC(new string[] { });
        }

        public VideoComponent(LiveSplitState state) : this(state, CreateVLCControl())
        {
        }

        public VideoComponent(LiveSplitState state, VideoView vview) : base(state,vview, ex => ErrorCallback(state.Form,ex))
        {
            Settings = new VideoSettings();
            State = state;
            vview.MediaPlayer = new MediaPlayer(libVLC)
            {
                EnableKeyInput = false,
                EnableMouseInput = false
            };
            vview.Height = (int)Settings.Height;
            vview.Width = (int)Settings.Width;
            vview.ResumeLayout();
            vview.Show();

            state.OnReset += state_OnReset;
            state.OnStart += state_OnStart;
            state.OnPause += state_OnPause;
            state.OnResume += state_OnResume;
        }

        public new void InvokeIfNeeded(Action x)
        {
            if (Control != null && Control.InvokeRequired)
                Control.Invoke(x);
            else
                x();
        }

        static void ErrorCallback(Form form, Exception ex)
        {
            //string requiredBits = Environment.Is64BitProcess ? "64" : "32";
            //MessageBox.Show(form, "VLC Media Player 2.2.1 (" + requiredBits + "-bit) along with the ActiveX Plugin need to be installed for the Video Component to work.", "Video Component Could Not Be Loaded", MessageBoxButtons.OK, MessageBoxIcon.Error);
            MessageBox.Show(form, ex.Message, "Error Loading Video Component", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        void state_OnResume(object sender, EventArgs e)
        {
            InvokeIfNeeded(() =>
            {
                lock (Control)
                {
                    ((VideoView)Control).MediaPlayer.Play();
                }
            });
        }

        void state_OnPause(object sender, EventArgs e)
        {
            InvokeIfNeeded(() =>
            {
                lock (Control)
                {
                    ((VideoView)Control).MediaPlayer.Pause();
                }
            });
        }

        void state_OnStart(object sender, EventArgs e)
        {
            InvokeIfNeeded(() =>
            {
                lock (Control)
                {
                    ((VideoView)Control).MediaPlayer.Play();
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

        private TimeSpan GetCurrentTime()
        {
            return State.CurrentTime[TimingMethod.RealTime].Value;
        }

        public void Synchronize(TimeSpan offset)
        {
            if (SynchronizeTimer != null && SynchronizeTimer.Enabled)
                SynchronizeTimer.Enabled = false;
            InvokeIfNeeded(() =>
            {
                lock (Control)
                    //VLC.input.Time = (GetCurrentTime() + offset + Settings.Offset).TotalMilliseconds;
                    ((VideoView)Control).MediaPlayer.Time = (GetCurrentTime() + offset + Settings.Offset).Milliseconds;
            });
            SynchronizeTimer = new System.Timers.Timer(1000);

            SynchronizeTimer.Elapsed += (s, ev) =>
            {
                InvokeIfNeeded(() =>
                {
                    lock (Control)
                    {
                        //if (VLC.input.state == 3)
                        if (((VideoView)Control).MediaPlayer.State == VLCState.Playing)
                        {
                            var currentTime = GetCurrentTime();
                            //var delta = VLC.input.Time - (currentTime + offset + Settings.Offset).TotalMilliseconds;
                            var delta = ((VideoView)Control).MediaPlayer.Time - (currentTime + offset + Settings.Offset).Milliseconds;
                            if (Math.Abs(delta) > 500)
                                //VLC.input.Time = (currentTime + offset + Settings.Offset).TotalMilliseconds + Math.Max(0, -delta);
                                ((VideoView)Control).MediaPlayer.Time = (currentTime + offset + Settings.Offset).Milliseconds + Math.Max(0, -delta);
                            else
                                SynchronizeTimer.Enabled = false;
                        }
                        //else if (VLC.state == 5)
                        else if (((VideoView)Control).MediaPlayer.State == VLCState.Stopped)
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
                lock (Control)
                {
                    //VLC.playlist.stop();
                    ((VideoView)Control).MediaPlayer.Stop();
                    if (activated)
                        Control.Visible = false;
                }
            });
        }

        private static VideoView CreateVLCControl()
        {
            VideoView vv = new VideoView()
            {
                //System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ComponentHostForm));
                //((System.ComponentModel.ISupportInitialize)(vlc)).BeginInit();
                Name = "vlc",
                AllowDrop = false,
                Size = new Size(320, 340)
            };
            //vlc.OcxState = ((AxHost.State)(resources.GetObject("axVLCPlugin21.OcxState")));
            //((System.ComponentModel.ISupportInitialize)(vlc)).EndInit();
            return vv;
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
            if (ErrorWithControl)
            {
                base.Dispose();

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
            //if (!VLC.IsDisposed)
            if(!Control.IsDisposed && !state.Form.IsDisposed)
            {
                base.Update(invalidator, state, width, height, mode);

                if (!Initialized)
                {
                    InvokeIfNeeded(() =>
                    {
                        lock (Control)
                        {
                            Control.Visible = !Control.Created;
                            Initialized = Control.Created;
                        }
                    });
                }
                else
                {
                    if (((VideoView)Control).MediaPlayer != null && OldMRL != Settings.MRL && !string.IsNullOrEmpty(Settings.MRL))
                    {
                        InvokeIfNeeded(() =>
                        {
                            lock (Control)
                            {
                                //VLC.playlist.add(Settings.MRL);
                                ((VideoView)Control).MediaPlayer.Media = new Media(libVLC, Settings.MRL);
                                Debug.WriteLine("Video Component media changed.");
                            }
                        });
                    }
                    OldMRL = Settings.MRL;

                    if (((VideoView)Control).MediaPlayer != null)
                    {
                        InvokeIfNeeded(() =>
                        {
                            lock (Control)
                            {
                                //VLC.Mute = true;
                                ((VideoView)Control).MediaPlayer.Mute = true;
                                ((VideoView)Control).MediaPlayer.Volume = 5;
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
