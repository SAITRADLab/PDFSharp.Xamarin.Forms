using PdfSharp.Xamarin.Forms.Attributes;
using PdfSharp.Xamarin.Forms.Extensions;
using PdfSharpCore.Drawing;
using Xamarin.Forms;
using System;

namespace PdfSharp.Xamarin.Forms.Renderers
{
	[PdfRenderer(ViewType = typeof(Image))]
	public class PdfImageRenderer : PdfRendererBase<Image>
	{
		public override async void CreatePDFLayout(XGraphics page, Image image, XRect bounds, double scaleFactor)
		{
			
			Console.WriteLine($"++++ The image source: {image.Source}");
            if (image.BackgroundColor != default)
                page.DrawRectangle(image.BackgroundColor.ToXBrush(), bounds);

            if (image.Source == null)
				return;

			XImage img = null;
			Console.WriteLine($"+++++ THE TYPE OF IMAGE SOURCE IS: {image.Source.GetType()}");
			switch (image.Source)
			{ 
				case FileImageSource fileImageSource:
					img = XImage.FromFile(fileImageSource.File);
					break;
				case UriImageSource uriImageSource:
					img = XImage.FromFile(uriImageSource.Uri.AbsolutePath);
					break;
				case StreamImageSource streamImageSource:
				{
					var stream = await streamImageSource.Stream.Invoke(new System.Threading.CancellationToken());
					img = XImage.FromStream(() => stream);
					break;
				}
			}

			XRect desiredBounds = bounds;
			switch (image.Aspect)
			{
				case Aspect.Fill:
					desiredBounds = bounds;
					break;
				case Aspect.AspectFit:
				{
					double aspectRatio = ((double) img.PixelWidth) / img.PixelHeight;
					if (aspectRatio > (bounds.Width / bounds.Height))
						desiredBounds.Height = desiredBounds.Width * aspectRatio;
					else
						desiredBounds.Width = desiredBounds.Height * aspectRatio;
				}
					break;
				//PdfSharp does not support drawing a portion pf image, its not supported
				case Aspect.AspectFill:
					desiredBounds = bounds;
					break;
			}
			Console.WriteLine("++++ JUST BEFORE THE CALL TO DRAWIMAGE");
			System.Threading.CancellationToken ct = new System.Threading.CancellationToken();
			page.DrawImage(img, desiredBounds.X, desiredBounds.Y, desiredBounds.Width, desiredBounds.Height, ct);
		}
	}
}
