using System.Linq;
using System.Windows;
using System.Windows.Controls;
using METRO.Helpers;
using Microsoft.Phone.Controls;

namespace METRO.Controls
{
    [TemplatePart(Name = PartWatermark, Type = typeof (ContentControl))]
    public class WatermarkAutoCompleteBox : AutoCompleteBox
    {
        private const string PartWatermark = "Watermark";


        public WatermarkAutoCompleteBox()
        {
            DefaultStyleKey = typeof(WatermarkAutoCompleteBox);
            GotFocus += AutoCompleteComboBoxGotFocus;
            LostFocus += AutoCompleteComboBoxLostFocus;
            SelectionChanged += AutoCompleteComboBoxSelectionChanged;
        }

        private TextBox _textBox;
        public TextBox TextBox
        {
            get
            {
                return _textBox ?? (_textBox = TreeHelper.GetControlsDecendant<TextBox>(this).FirstOrDefault());
            }
        }

        private void AutoCompleteComboBoxSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            VisualStateManager.GoToState(this, "NoWatermarkState", false);
        }


        private void AutoCompleteComboBoxGotFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(Text))
                VisualStateManager.GoToState(this, "NoWatermarkState", false);
        }

        private void AutoCompleteComboBoxLostFocus(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(Text)) return;
            Text = "";
            VisualStateManager.GoToState(this, "WatermarkState", false);
        }

        public override void OnApplyTemplate()
        {

            base.OnApplyTemplate();
            WatermarkControl = GetTemplateChild(PartWatermark) as ContentControl;
            if (WatermarkControl == null) return;

            WatermarkControl.Content = Watermark;
            VisualStateManager.GoToState(this, "WatermarkState", false);

        }

        #region Watermark

        public ContentControl WatermarkControl { get; set; }

        /// <summary>
        /// Gets or sets the Watermark content.
        /// </summary>
        /// <value>The watermark.</value>
        public new object Watermark
        {
            get { return (object) GetValue(WatermarkProperty); }
            set { SetValue(WatermarkProperty, value); }
        }

        /// <summary>
        /// Watermark DP.
        /// </summary>
        public new static readonly DependencyProperty WatermarkProperty =
            DependencyProperty.Register(
                "Watermark",
                typeof (object),
                typeof (WatermarkAutoCompleteBox),
                new PropertyMetadata("Veuillez saisir du texte..."));

        #endregion
    }
}