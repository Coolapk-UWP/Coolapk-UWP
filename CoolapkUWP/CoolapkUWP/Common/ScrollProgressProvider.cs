using System;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Composition;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Hosting;

namespace CoolapkUWP.Common
{
    public class ScrollProgressProvider : DependencyObject
    {
        private readonly CompositionPropertySet propSet;
        private readonly ExpressionAnimation progressBind;
        private readonly ExpressionAnimation thresholdBind;
        private double lastOffset;
        private bool readyToScroll;
        private double innerProgress;
        private CancellationTokenSource delayCancellationTokenSource;
        private CompositionPropertySet scrollPropertySet;

        public ScrollProgressProvider()
        {
            propSet = Window.Current.Compositor.CreatePropertySet();
            propSet.InsertScalar("progress", 0f);
            propSet.InsertScalar("threshold", 0f);
            propSet.InsertScalar("delayprogress", -1f);

            progressBind = Window.Current.Compositor.CreateExpressionAnimation("clamp(prop.progress, 0f, 1f)");
            progressBind.SetReferenceParameter("prop", propSet);

            thresholdBind = Window.Current.Compositor.CreateExpressionAnimation("max(prop.threshold, 0f)");
            thresholdBind.SetReferenceParameter("prop", propSet);
        }

        #region Dependency Properties

        // Using a DependencyProperty as the backing store for ScrollViewer.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ScrollViewerProperty =
            DependencyProperty.Register("ScrollViewer", typeof(ScrollViewer), typeof(ScrollProgressProvider), new PropertyMetadata(null, (s, a) =>
            {
                if (a.NewValue != a.OldValue)
                {
                    if (s is ScrollProgressProvider sender)
                    {
                        sender.ScrollViewerChanged(a.OldValue as ScrollViewer, a.NewValue as ScrollViewer);
                    }
                }
            }));


        /// <summary>
        /// 改变ScrollViewer
        /// </summary>
        /// <param name="oldSv"></param>
        /// <param name="newSv"></param>
        private async void ScrollViewerChanged(ScrollViewer oldSv, ScrollViewer newSv)
        {
            if (oldSv != null)
            {
                oldSv.ViewChanged -= ScrollViewer_ViewChanged;
                oldSv.Unloaded -= ScrollViewer_Unloaded;

                propSet.InsertScalar("progress", (float)innerProgress);
                propSet.InsertScalar("delayprogress", (float)innerProgress);
            }

            if (newSv != null)
            {
                readyToScroll = true;

                if (newSv.VerticalOffset == 0 && (oldSv == null || (oldSv != null && oldSv.VerticalOffset == 0)))
                {
                    StartScrollProgressAnimation(newSv, false);
                }
                else if (newSv.VerticalOffset > Threshold && lastOffset > Threshold) // 前后进度都为1时 增加延迟防止闪烁
                {
                    StartScrollProgressAnimation(newSv, true);
                }
                else if (newSv.VerticalOffset == lastOffset)
                {
                    StartScrollProgressAnimation(newSv, false);
                }
                else if (newSv.VerticalOffset < Threshold || lastOffset < Threshold) //前后状态不同时 先设置滚动条位置再绑定动画
                {
                    await SyncScrollView(newSv);
                    StartScrollProgressAnimation(newSv, true);
                }
                newSv.ViewChanged += ScrollViewer_ViewChanged;
                newSv.Unloaded += ScrollViewer_Unloaded;
            }
        }


        public static readonly DependencyProperty ThresholdProperty =
            DependencyProperty.Register("Threshold", typeof(double), typeof(ScrollProgressProvider), new PropertyMetadata(0d, (s, a) =>
            {
                if (a.NewValue != a.OldValue)
                {
                    if (s is ScrollProgressProvider sender)
                    {
                        double val = (double)a.NewValue;
                        if (val < 0)
                        {
                            throw new ArgumentException($"{nameof(Threshold)}不能小于0");
                        }

                        sender.propSet.InsertScalar("threshold", (float)val);
                        sender.OnProgressChanged();
                    }
                }
            }));

        // Using a DependencyProperty as the backing store for Progress.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ProgressProperty =
            DependencyProperty.Register("Progress", typeof(double), typeof(ScrollProgressProvider), new PropertyMetadata(0d, (s, a) =>
            {
                if (a.NewValue != a.OldValue)
                {
                    if ((double)a.NewValue > 1)
                    {
                        throw new ArgumentException($"{nameof(Progress)}不能大于1");
                    }

                    if ((double)a.NewValue < 0)
                    {
                        throw new ArgumentException($"{nameof(Progress)}不能小于0");
                    }

                    if (s is ScrollProgressProvider sender)
                    {
                        if (sender.innerProgress != (double)a.NewValue)
                        {
                            _ = sender.SyncScrollView(sender.ScrollViewer);
                        }
                        sender.OnProgressChanged();
                    }
                }
            }));

        #endregion Dependency Properties

        #region Properties

        public ScrollViewer ScrollViewer
        {
            get { return (ScrollViewer)GetValue(ScrollViewerProperty); }
            set { SetValue(ScrollViewerProperty, value); }
        }

        /// <summary>
        /// 阈值
        /// </summary>
        public double Threshold
        {
            get { return (double)GetValue(ThresholdProperty); }
            set { SetValue(ThresholdProperty, value); }
        }

        /// <summary>
        /// 进度
        /// </summary>
        public double Progress
        {
            get { return (double)GetValue(ProgressProperty); }
            set { SetValue(ProgressProperty, value); }
        }

        #endregion Properties

        #region Methods

        /// <summary>
        /// 将Progress绑定到ScrollViewer
        /// </summary>
        /// <param name="sv"></param>
        /// <param name="delay"></param>
        private async void StartScrollProgressAnimation(ScrollViewer sv, bool delay)
        {
            readyToScroll = false;
            if (delayCancellationTokenSource != null)
            {
                delayCancellationTokenSource.Cancel();
                delayCancellationTokenSource = null;
            }

            if (delay)
            {
                delayCancellationTokenSource = new CancellationTokenSource();
            }

            try
            {
                scrollPropertySet = ElementCompositionPreview.GetScrollViewerManipulationPropertySet(sv);

                ExpressionAnimation exp = Window.Current.Compositor.CreateExpressionAnimation($"(prop.delayprogress >= 0) ? prop.delayprogress : clamp(-sv.Translation.Y, 0f, prop.threshold) / prop.threshold");
                exp.SetReferenceParameter("sv", scrollPropertySet);
                exp.SetReferenceParameter("prop", propSet);

                propSet.StartAnimation("progress", exp);

                if (delay)
                {
                    await Task.Delay(150, delayCancellationTokenSource.Token);
                }

                propSet.InsertScalar("delayprogress", -1f);

            }
            catch
            {

            }
        }

        /// <summary>
        /// 同步ScrollOffset
        /// </summary>
        /// <param name="sv"></param>
        private async Task SyncScrollView(ScrollViewer sv)
        {
            if (sv == null)
            {
                return;
            }

            TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();

            sv.ViewChanged += Sv_ViewChanged;
            sv.ChangeView(null, Threshold * Progress, null, true);
            await tcs.Task;

            void Sv_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
            {
                if (!e.IsIntermediate)
                {
                    sv.ViewChanged -= Sv_ViewChanged;
                    tcs.SetResult(true);
                }
            }
        }


        public CompositionPropertySet GetProgressPropertySet()
        {
            CompositionPropertySet _propSet = Window.Current.Compositor.CreatePropertySet();
            _propSet.InsertScalar("progress", (float)innerProgress);
            _propSet.InsertScalar("threshold", (float)Threshold);
            _propSet.StartAnimation("progress", progressBind);
            _propSet.StartAnimation("threshold", thresholdBind);
            return _propSet;
        }

        #endregion Methods

        #region Event Callback Methods

        private void ScrollViewer_Unloaded(object sender, RoutedEventArgs e)
        {
            ((ScrollViewer)sender).Unloaded -= ScrollViewer_Unloaded;
            ((ScrollViewer)sender).ViewChanged -= ScrollViewer_ViewChanged;
        }

        private void ScrollViewer_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            lastOffset = ((ScrollViewer)sender).VerticalOffset;
            innerProgress = GetProgress(lastOffset, Threshold);
            Progress = innerProgress;

            if (readyToScroll)
            {
                readyToScroll = false;
                StartScrollProgressAnimation((ScrollViewer)sender, true);
            }
        }

        #endregion Event Callback Methods

        #region Events

        public event TypedEventHandler<object, double> ProgressChanged;
        protected void OnProgressChanged()
        {
            ProgressChanged?.Invoke(this, Progress);
        }

        #endregion Events

        #region Utilities

        private static double GetProgress(double offset, double threshold)
        {
            return threshold == 0 ? 0 : Math.Min(1, Math.Max(0, offset / threshold));
        }

        #endregion Utilities

    }
}
