using PdfSharp.Xamarin.Forms.Delegates;
using Xamarin.Forms;

namespace PdfSharp.Xamarin.Forms
{
	public class PdfRendererAttributes : BindableObject
	{
		public static bool GetShouldRender(BindableObject view)
		{
			return (bool)view.GetValue(ShouldRenderProperty);
		}

		public static void SetShouldRender(BindableObject view, bool value)
        {
			view.SetValue(ShouldRenderProperty, value);
        }

		public PdfListViewRendererDelegate ListRendererDelegate
		{
			get => (PdfListViewRendererDelegate) GetValue(ListRendererDelegateProperty);
			set => SetValue(ListRendererDelegateProperty, value);
		}

		public static readonly BindableProperty ShouldRenderProperty =
			BindableProperty.CreateAttached("ShouldRender", typeof(bool), typeof(PdfRendererAttributes), true);

		public static readonly BindableProperty ListRendererDelegateProperty =
			BindableProperty.CreateAttached(nameof(ListRendererDelegate), typeof(PdfListViewRendererDelegate),
				typeof(PdfRendererAttributes), new PdfListViewRendererDelegate());

		public static bool ShouldRenderView(BindableObject bindable)
		{
			return (bool) bindable.GetValue(ShouldRenderProperty);
		}
	}
}
