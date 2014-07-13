using AxAXVLC;
using LiveSplit.Model;
using LiveSplit.UI;
using LiveSplit.UI.Components;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

namespace LiveSplit.Video
{
    public class VideoComponent : IComponent
    {
        #region Properties

        public VideoSettings Settings { get; set; }

        public string ComponentName
        {
            get { return "Video"; }
        }

        public float HorizontalWidth
        {
            get { return 100; }
        }

        public float MinimumHeight
        {
            get { return 10; }
        }

        public float VerticalHeight
        {
            get { return 200; }
        }

        public float MinimumWidth
        {
            get { return 10; }
        }

        public float PaddingTop
        {
            get { return 0; }
        }

        public float PaddingBottom
        {
            get { return 0; }
        }

        public float PaddingLeft
        {
            get { return 0; }
        }

        public float PaddingRight
        {
            get { return 0; }
        }

        public IDictionary<string, Action> ContextMenuControls { get; set; }

        public Control Form { get; set; }
        public AxVLCPlugin2 VLC { get; set; }
        public LiveSplitState State { get; set; }
        public System.Timers.Timer SynchronizeTimer { get; set; }

        #endregion

        #region Constructors

        public VideoComponent()
        {
            Settings = new VideoSettings();
            Settings.txtMRL.TextChanged += txtMRL_TextChanged;
            ContextMenuControls = new Dictionary<String, Action>();
        }

        #endregion

        #region Event Handlers

        void txtMRL_TextChanged(object sender, EventArgs e)
        {
            if (VLC != null && !String.IsNullOrEmpty(Settings.txtMRL.Text))
            {
                lock (VLC)
                {
                    VLC.playlist.items.clear();
                    VLC.playlist.add(Settings.txtMRL.Text);
                }
            }
        }

        #endregion

        #region Methods

        public void InvokeIfNeeded(Action x)
        {
            if (Form != null && Form.InvokeRequired)
                Form.Invoke(x);
            else
                x();
        }

        public void Update(IInvalidator invalidator, LiveSplitState state, float width, float height, LayoutMode mode)
        {
            if (Form == null)
            {
                if (invalidator != null)
                    invalidator.Invalidate(0, 0, width, height);
            }
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

        protected void DrawGeneral(Graphics g, LiveSplitState state, float width, float height)
        {
            if (Form == null)
            {
                State = state;
                Form = Application.OpenForms.OfType<LiveSplit.View.TimerForm>().First(x => x.Layout.Components.Contains(this));

                System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
                VLC = new AxAXVLC.AxVLCPlugin2();
                InvokeIfNeeded(() =>
                {
                    lock (VLC)
                    {
                        ((System.ComponentModel.ISupportInitialize)(VLC)).BeginInit();
                        Form.SuspendLayout();

                        VLC.Enabled = true;
                        VLC.Name = "vlc";
                        VLC.OcxState = ((System.Windows.Forms.AxHost.State)(resources.GetObject("axVLCPlugin21.OcxState")));

                        Form.Controls.Add(VLC);

                        ((System.ComponentModel.ISupportInitialize)(VLC)).EndInit();
                        Form.ResumeLayout(false);

                        if (!String.IsNullOrEmpty(Settings.MRL))
                        {
                            VLC.playlist.items.clear();
                            VLC.playlist.add(Settings.MRL);
                        }
                    }
                });

                state.OnReset += state_OnReset;
                state.OnStart += state_OnStart;
                state.OnSplit += state_OnSplit;
                state.OnUndoSplit += state_OnUndoSplit;
                state.OnSkipSplit += state_OnSkipSplit;
                state.OnPause += state_OnPause;
                state.OnResume += state_OnResume;
            }
            Reposition(width, height, g);
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
            SynchronizeWithSplit();
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

        void state_OnSkipSplit(object sender, EventArgs e)
        {
            SynchronizeWithSplit();
        }

        void state_OnUndoSplit(object sender, EventArgs e)
        {
            SynchronizeWithSplit();
        }

        void state_OnSplit(object sender, EventArgs e)
        {
            SynchronizeWithSplit();
        }

        void state_OnStart(object sender, EventArgs e)
        {
            InvokeIfNeeded(() =>
            {
                lock (VLC)
                {
                    VLC.playlist.play();
                }
            });
            Synchronize();
        }

        void state_OnReset(object sender, EventArgs e)
        {
            InvokeIfNeeded(() =>
            {
                lock (VLC)
                {
                    VLC.playlist.stop();
                }
            });
        }

        public void SynchronizeWithSplit()
        {
            if (Settings.PerSegmentVideo)
            {
                var time =
                    State.CurrentSplitIndex - 1 >= 0
                    ? State.Run[State.CurrentSplitIndex - 1].PersonalBestSplitTime[State.CurrentTimingMethod]
                    : State.CurrentTime[State.CurrentTimingMethod].Value;
                if (time.HasValue)
                    Synchronize(time.Value - State.CurrentTime[State.CurrentTimingMethod].Value);
            }
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
                        System.Diagnostics.Debug.WriteLine("Synchronizing: " + (VLC.input.Time - (State.CurrentTime[State.CurrentTimingMethod].Value + offset + Settings.Offset).TotalMilliseconds) + " State: " + VLC.input.state);
                        if (VLC.input.state == 3)
                        {
                            var delta = VLC.input.Time - (State.CurrentTime[State.CurrentTimingMethod].Value + offset + Settings.Offset).TotalMilliseconds;
                            if (Math.Abs(delta) > 160)
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


        public void Reposition(float width, float height, Graphics g)
        {
            var points = new PointF[]
            {
                new PointF(0, 0),
                new PointF(width, height)
            };
            g.Transform.TransformPoints(points);

            InvokeIfNeeded(() =>
            {
                lock (VLC)
                {
                    VLC.Location = new System.Drawing.Point((int)(points[0].X + 0.5f) + 1, (int)(points[0].Y + 0.5f) + 1);
                    VLC.Size = new System.Drawing.Size((int)(points[1].X - points[0].X + 0.5f) - 2, (int)(points[1].Y - points[0].Y + 0.5f) - 2);
                }
            });
        }

        public void DrawHorizontal(Graphics g, LiveSplitState state, float height, Region clipRegion)
        {
            DrawGeneral(g, state, HorizontalWidth, height);
        }

        public void DrawVertical(Graphics g, LiveSplitState state, float width, Region clipRegion)
        {
            DrawGeneral(g, state, width, VerticalHeight);
        }

        public Control GetSettingsControl(LayoutMode mode)
        {
            return Settings;
        }

        public XmlNode GetSettings(XmlDocument document)
        {
            return Settings.GetSettings(document);
        }

        public void SetSettings(XmlNode settings)
        {
            var oldMRL = Settings.MRL;
            Settings.SetSettings(settings);
            if (VLC != null && oldMRL != Settings.MRL && !String.IsNullOrEmpty(Settings.MRL))
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
        }

        public void RenameComparison(string oldName, string newName)
        {
        }

        #endregion

        #region Unsafe Native Methods

        [DllImport("user32.dll", SetLastError = true, ExactSpelling = true)]
        public static extern IntPtr WindowFromDC(HandleRef hDC);

        #endregion
    }
}
