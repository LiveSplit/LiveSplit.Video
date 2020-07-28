using LiveSplit.Model;
using LiveSplit.UI.Components;
using System;
using System.Windows.Forms;
using System.Drawing;
using System.Xml;
using LibVLCSharp.Shared;
using LibVLCSharp.WinForms;
using System.Diagnostics;
using System.Linq;
using System.ComponentModel;

namespace LiveSplit.Video
{
    public class VideoComponent : ControlComponent
    {
        public VideoSettings Settings { get; set; }
        public LiveSplitState State { get; set; }
        public System.Timers.Timer SynchronizeTimer { get; set; }

        private class VLCErrorException : Exception { }

        protected string OldPath { get; set; }

        public override string ComponentName => "Video";

        public override float HorizontalWidth => Settings.Width;

        public override float MinimumHeight => 10;

        public override float VerticalHeight => Settings.Height;

        public override float MinimumWidth => 10;

        private static LibVLC libVLC;

        public MediaPlayer mediaPlayer => ((VideoView)Control).MediaPlayer;

        static VideoComponent()
        {
            Core.Initialize();
            libVLC = new LibVLC(new string[] { });
        }

        public VideoComponent(LiveSplitState state) : this(state, CreateVLCControl())
        {
        }

        public VideoComponent(LiveSplitState state, VideoView vview) : base(state, vview, ex => ErrorCallback(state.Form, ex))
        {
            Settings = new VideoSettings();
            State = state;

            var mediaplayer = new MediaPlayer(libVLC)
            {
                EnableKeyInput = false,
                EnableMouseInput = false,
                EnableHardwareDecoding = false
            };
            vview.MediaPlayer = mediaplayer;

            state.OnReset += state_OnReset;
            state.OnStart += state_OnStart;
            state.OnPause += state_OnPause;
            state.OnResume += state_OnResume;
        }

        public delegate void InvokeAction(ISynchronizeInvoke control, Action action);

        public static void Lock(ISynchronizeInvoke control, Action action)
        {
            lock (control)
            {
                action();
            }
        }

        public new void InvokeIfNeeded(Action x)
        {
            if (Control != null && Control.InvokeRequired)
                Control.Invoke((InvokeAction)Lock, Control, x);
            else
                Lock(Control, x);
        }

        static void ErrorCallback(Form form, Exception ex)
        {
            MessageBox.Show(form, ex.Message, "Error Loading Video Component", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        void state_OnResume(object sender, EventArgs e)
        {
            InvokeIfNeeded(delegate
            {
                mediaPlayer.Play();
            });
        }

        void state_OnPause(object sender, EventArgs e)
        {
            InvokeIfNeeded(delegate
            {
                mediaPlayer.Pause();

            });
        }

        void state_OnStart(object sender, EventArgs e)
        {
            InvokeIfNeeded(delegate
            {
                mediaPlayer.Play();
                Control.Visible = true;
                Synchronize();
            });
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
            InvokeIfNeeded(delegate
            {
                mediaPlayer.Time = (long)(GetCurrentTime() + offset + Settings.Offset).TotalMilliseconds;
            });
            SynchronizeTimer = new System.Timers.Timer(1000);

            SynchronizeTimer.Elapsed += (s, ev) =>
            {
                InvokeIfNeeded(delegate
                {
                    if (mediaPlayer.State == VLCState.Playing)
                    {
                        var currentTime = GetCurrentTime();
                        long delta = mediaPlayer.Time - (long)(currentTime + offset + Settings.Offset).TotalMilliseconds;
                        if (Math.Abs(delta) > 500)
                            mediaPlayer.Time = (long)(currentTime + offset + Settings.Offset).TotalMilliseconds + Math.Max(0, -delta);
                        else
                            SynchronizeTimer.Enabled = false;
                    }
                    else if (mediaPlayer.State == VLCState.Stopped)
                    {
                        SynchronizeTimer.Enabled = false;
                    }
                });
            };

            SynchronizeTimer.Enabled = true;
        }

        void state_OnReset(object sender, TimerPhase e)
        {
            InvokeIfNeeded(delegate
            {
                mediaPlayer.Stop();
                Control.Visible = false;
            });
        }

        private static VideoView CreateVLCControl()
        {
            if (libVLC == null)
            {
                libVLC = new LibVLC(new string[] { });
            }

            VideoView vv = new VideoView()
            {
                Name = "vlc",
                AllowDrop = false,
            };
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

                throw new VideoComponent.VLCErrorException();
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
            State = state;
            if (!Control.IsDisposed && !state.Form.IsDisposed)
            {
                base.Update(invalidator, state, width, height, mode);

                if (Control.Created)
                {
                    if (mediaPlayer != null && OldPath != Settings.VideoPath && !string.IsNullOrEmpty(Settings.VideoPath))
                    {
                        InvokeIfNeeded(delegate
                        {
                            mediaPlayer.Media = new Media(libVLC, Settings.VideoPath, FromType.FromPath); ;
                            mediaPlayer.Volume = 0;
                            Debug.WriteLine("Video Component media changed.");
                        });
                    }
                    OldPath = Settings.VideoPath;
                }
            }
        }

        public int ComponentCount()
        {
            return State.Layout.LayoutComponents.Count((component) => component.Component is VideoComponent);
        }

        public override void Dispose()
        {
            State.Form.Invoke((Action)delegate
            {
                lock (Control)
                {
                    mediaPlayer.Media = null;
                    var mp = mediaPlayer;
                    ((VideoView)Control).MediaPlayer = null;
                    mp?.Dispose();
                }
                base.Dispose();
            });

            int instances = ComponentCount();
            Debug.WriteLine($"{instances} instance(s) of {ComponentName} are still running.");
            if (instances == 0)
            {
                LibVLC t = libVLC;
                libVLC = null;
                t?.Dispose();
            }

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
