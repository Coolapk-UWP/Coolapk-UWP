using Microsoft.Toolkit.Uwp.UI;
using System;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Hosting;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;

// The Templated Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234235

namespace CoolapkUWP.Controls
{
    public sealed class PivotHeader : ListBox
    {
        private CancellationTokenSource cts;

        #region Const Values

        private static readonly Vector2 c_frame1point1 = new Vector2(0.9f, 0.1f);
        private static readonly Vector2 c_frame1point2 = new Vector2(0.7f, 0.4f);
        private static readonly Vector2 c_frame2point1 = new Vector2(0.1f, 0.9f);
        private static readonly Vector2 c_frame2point2 = new Vector2(0.2f, 1f);

        #endregion Const Values

        public static readonly DependencyProperty PivotProperty =
            DependencyProperty.Register(
                nameof(Pivot),
                typeof(Pivot),
                typeof(PivotHeader),
                new PropertyMetadata(null, OnPivotPropertyChanged));

        public Pivot Pivot
        {
            get => (Pivot)GetValue(PivotProperty);
            set => SetValue(PivotProperty, value);
        }

        private static void OnPivotPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue != e.OldValue)
            {
                ((PivotHeader)d).SetPivot();
            }
        }

        public PivotHeader()
        {
            DefaultStyleKey = typeof(PivotHeader);

            SelectionChanged += ShyHeader_SelectionChanged;
        }

        private void SetPivot()
        {
            if (Pivot == null) { return; }
            SetBinding(SelectedIndexProperty, new Binding()
            {
                Source = Pivot,
                Mode = BindingMode.TwoWay,
                Path = new PropertyPath(nameof(SelectedIndex))
            });
            ItemCollection items = Pivot.Items;
            ItemsSource = (from item in items
                           where item is PivotItem
                           select ((PivotItem)item).Header ?? string.Empty).ToArray();
            HidePivotHeader();
        }

        private async void HidePivotHeader()
        {
            if (cts != null)
            {
                cts.Cancel();
            }

            cts = new CancellationTokenSource(TimeSpan.FromSeconds(20));
            PivotPanel PivotPanel = (PivotPanel)await WaitForLoaded(Pivot, () => Pivot?.FindDescendant<PivotPanel>() as FrameworkElement, c => c != null, cts.Token);
            Grid PivotLayoutElement = PivotPanel?.FindDescendant<Grid>();
            if (PivotLayoutElement != null)
            {
                PivotLayoutElement.RowDefinitions.First().Height = new GridLength(0, GridUnitType.Pixel);
            }
        }

        private void ShyHeader_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SelectionMode != SelectionMode.Single)
            {
                return;
            }

            Rectangle oldIndicator = GetIndicator(e.RemovedItems.FirstOrDefault());
            if (oldIndicator == null)
            {
                return;
            }

            Rectangle newIndicator = GetIndicator(e.AddedItems.FirstOrDefault());
            if (newIndicator == null)
            {
                return;
            }

            try { TryStartAnimationWithScale(newIndicator, oldIndicator); } catch { }
        }

        private Rectangle GetIndicator(object item)
        {
            if (item == null)
            {
                return null;
            }

            DependencyObject container = ContainerFromItem(item);
            if (container == null)
            {
                return null;
            }

            Grid grid = VisualTreeHelper.GetChild(container, 0) as Grid;
            return grid == null ? null : grid.FindName("Indicator") as Rectangle;
        }

        private async Task<T> WaitForLoaded<T>(FrameworkElement element, Func<T> func, Predicate<T> pre, CancellationToken cancellationToken)
        {
            TaskCompletionSource<T> tcs = null;
            try
            {
                tcs = new TaskCompletionSource<T>();
                cancellationToken.ThrowIfCancellationRequested();
                T result = func.Invoke();
                if (pre(result))
                {
                    return result;
                }

                element.Loaded += Element_Loaded;

                return await tcs.Task;

            }
            catch
            {
                element.Loaded -= Element_Loaded;
                T result = func.Invoke();
                if (pre(result))
                {
                    return result;
                }
            }

            return default;


            void Element_Loaded(object sender, RoutedEventArgs e)
            {
                if (tcs == null)
                {
                    return;
                }

                try
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    element.Loaded -= Element_Loaded;
                    T _result = func.Invoke();
                    if (pre(_result))
                    {
                        tcs.SetResult(_result);
                    }
                    else
                    {
                        tcs.SetCanceled();
                    }
                }
                catch
                {
                    System.Diagnostics.Debug.WriteLine("canceled");
                }
            }

        }

        private void TryStartAnimationWithScale(FrameworkElement newIndicator, FrameworkElement oldIndicator)
        {
            Windows.UI.Composition.Compositor compositor = Window.Current.Compositor;

            Windows.UI.Composition.Visual old_target = ElementCompositionPreview.GetElementVisual(oldIndicator);
            Windows.UI.Composition.Visual new_target = ElementCompositionPreview.GetElementVisual(newIndicator);

            old_target.Offset = Vector3.Zero;
            old_target.CenterPoint = Vector3.Zero;
            old_target.Scale = Vector3.One;

            Vector2 oldSize = new Vector2((float)oldIndicator.ActualWidth, (float)oldIndicator.ActualHeight);
            Vector2 newSize = new Vector2((float)newIndicator.ActualWidth, (float)newIndicator.ActualHeight);

            Vector2 oldScale = oldSize / newSize;
            if (oldScale.Y < 0)
            {
                return;
            }

            Vector2 oldOffset = newIndicator.TransformToVisual(oldIndicator).TransformPoint(new Windows.Foundation.Point(0, 0)).ToVector2();

            float startx = 0, endx = 0, starty = 0, endy = 0;

            if (oldOffset.X > 0)
            {
                endx = newSize.X;
            }
            else
            {
                startx = newSize.X;
                oldOffset.X = oldOffset.X + newSize.X - oldSize.X;
            }

            if (oldOffset.Y > 0)
            {
                endy = newSize.Y;
            }
            else
            {
                starty = newSize.Y;
                oldOffset.Y = oldOffset.Y + newSize.Y - oldSize.Y;
            }

            TimeSpan duration = TimeSpan.FromSeconds(0.6d);

            Windows.UI.Composition.CubicBezierEasingFunction standard = compositor.CreateCubicBezierEasingFunction(new Vector2(0.8f, 0.0f), new Vector2(0.2f, 1.0f));

            Windows.UI.Composition.StepEasingFunction singleStep = compositor.CreateStepEasingFunction();
            singleStep.IsFinalStepSingleFrame = true;

            Windows.UI.Composition.Vector3KeyFrameAnimation centerAnimation = compositor.CreateVector3KeyFrameAnimation();
            centerAnimation.InsertExpressionKeyFrame(0f, "Vector3(startx,starty,0f)", singleStep);
            centerAnimation.InsertExpressionKeyFrame(0.333f, "Vector3(endx,endy,0f)", singleStep);
            centerAnimation.SetScalarParameter("startx", startx);
            centerAnimation.SetScalarParameter("starty", starty);
            centerAnimation.SetScalarParameter("endx", endx);
            centerAnimation.SetScalarParameter("endy", endy);
            centerAnimation.Duration = duration;

            Windows.UI.Composition.Vector2KeyFrameAnimation offsetAnimation = compositor.CreateVector2KeyFrameAnimation();
            offsetAnimation.InsertExpressionKeyFrame(0f, "-oldOffset", singleStep);
            offsetAnimation.InsertExpressionKeyFrame(0.333f, "This.StartingValue", singleStep);
            offsetAnimation.SetVector2Parameter("oldOffset", oldOffset);
            offsetAnimation.Duration = duration;

            Windows.UI.Composition.Vector2KeyFrameAnimation scaleAnimation = compositor.CreateVector2KeyFrameAnimation();
            scaleAnimation.InsertExpressionKeyFrame(0f, "oldScale", standard);
            scaleAnimation.InsertExpressionKeyFrame(0.333f, "(target.Size + abs(oldOffset)) / target.Size",
                compositor.CreateCubicBezierEasingFunction(c_frame1point1, c_frame1point2));
            scaleAnimation.InsertExpressionKeyFrame(1f, "this.StartingValue",
                compositor.CreateCubicBezierEasingFunction(c_frame2point1, c_frame2point2));
            scaleAnimation.SetVector2Parameter("oldScale", oldScale);
            scaleAnimation.SetVector2Parameter("oldOffset", oldOffset);
            scaleAnimation.SetReferenceParameter("target", new_target);
            scaleAnimation.SetReferenceParameter("old", old_target);
            scaleAnimation.Duration = duration;

            new_target.StartAnimation("CenterPoint", centerAnimation);
            new_target.StartAnimation("Offset.XY", offsetAnimation);
            new_target.StartAnimation("Scale.XY", scaleAnimation);
        }
    }
}
