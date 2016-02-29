using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace METRO.Controls
{
    public sealed class BusyMessage
    {
        private Popup _popup = new Popup();
        private Border _border = new Border();
        private TextBlock _messageTextBlock = new TextBlock();
        private Storyboard _openStoryBoard;
        private Storyboard _closeStoryBoard;

        public BusyMessage()
        {
            //DefaultStyleKey = this.GetType();
            _border.Height = 17;
            _border.Width = 480;
            _border.Background = new SolidColorBrush(Colors.Black) { Opacity = 0.2 };
            _border.IsHitTestVisible = false;
            // system tray space
            _border.RenderTransform = new TranslateTransform() { Y = 0 };

            _border.Child = _messageTextBlock;
            _messageTextBlock.Text = "Recherche...";
            _messageTextBlock.Margin = new Thickness(5, 0, 0, 0);
            _messageTextBlock.FontSize = 16;
            _messageTextBlock.Foreground = new SolidColorBrush(Colors.White);
            _messageTextBlock.IsHitTestVisible = false;

            _popup.Child = _border;
            _popup.IsHitTestVisible = false;
            _popup.IsOpen = false;
            _openStoryBoard = new Storyboard();
            CreateOpenAnimation(_openStoryBoard, new TimeSpan(0), _border, _popup);
            _closeStoryBoard = new Storyboard();
            CreateCloseAnimation(_closeStoryBoard, new TimeSpan(0), _border, _popup);
        }


        public string Message
        {
            get { return _messageTextBlock.Text; }
            set
            {
                if (_messageTextBlock.Text == value)
                    return;
                _messageTextBlock.Text = value;
            }
        }
        public bool IsOpen
        {
            get { return _popup.IsOpen; }
            set
            {
                if (value)
                {
                    _openStoryBoard.Stop();
                    _openStoryBoard.Begin();
                }
                else
                {
                    _closeStoryBoard.Stop();
                    _closeStoryBoard.Begin();
                }
            }
        }
        private void CreateOpenAnimation(Storyboard sb, TimeSpan beginTime, UIElement target, Popup popup)
        {
            var animation = new DoubleAnimationUsingKeyFrames() { BeginTime = beginTime };
            Storyboard.SetTargetProperty(animation, new PropertyPath("(TranslateTransform.Y)"));
            Storyboard.SetTarget(animation, target.RenderTransform);
            animation.KeyFrames.Add(new EasingDoubleKeyFrame()
            {
                KeyTime = KeyTime.FromTimeSpan(new TimeSpan(0, 0, 0, 0, 400)),
                Value = 32,
                EasingFunction = new CircleEase { EasingMode = EasingMode.EaseOut }
            });
            sb.Children.Add(animation);

            var animation2 = new ObjectAnimationUsingKeyFrames();
            Storyboard.SetTargetProperty(animation2, new PropertyPath("IsOpen"));
            Storyboard.SetTarget(animation2, popup);
            animation2.KeyFrames.Add(new DiscreteObjectKeyFrame()
            {
                KeyTime = KeyTime.FromTimeSpan(new TimeSpan(0)),
                Value = true,
            });
            sb.Children.Add(animation2);



        }
        private void CreateCloseAnimation(Storyboard sb, TimeSpan beginTime, UIElement target, Popup popup)
        {
            var animation = new DoubleAnimationUsingKeyFrames() { BeginTime = beginTime };
            Storyboard.SetTargetProperty(animation, new PropertyPath("(TranslateTransform.Y)"));
            Storyboard.SetTarget(animation, target.RenderTransform);
            animation.KeyFrames.Add(new EasingDoubleKeyFrame()
            {
                KeyTime = KeyTime.FromTimeSpan(new TimeSpan(0, 0, 0, 0, 400)),
                Value = 0,
                EasingFunction = new CircleEase { EasingMode = EasingMode.EaseIn }
            });
            sb.Children.Add(animation);

            var animation2 = new ObjectAnimationUsingKeyFrames();
            Storyboard.SetTargetProperty(animation2, new PropertyPath("IsOpen"));
            Storyboard.SetTarget(animation2, popup);
            animation2.KeyFrames.Add(new DiscreteObjectKeyFrame()
            {
                KeyTime = KeyTime.FromTimeSpan(new TimeSpan(0, 0, 0, 0, 400)),
                Value = false,
            });
            sb.Children.Add(animation2);
        }


    }
}
