using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Android.Graphics;
using ExifLib;
using static Android.Graphics.Bitmap;
using static Android.Graphics.BitmapFactory;
using static MigraDocCore.DocumentObjectModel.MigraDoc.DocumentObjectModel.Shapes.ImageSource;

namespace PdfSharp.Xamarin.Forms.Droid
{
	internal class AndroidImageSourceImpl : IImageSource
	{
		internal Bitmap Bitmap { get; set; }
		internal Stream Stream { get; set; }
		private Orientation Orientation { get; }

		private readonly Func<Stream> _streamSource;
		private readonly int _quality;
		public int RotateFactor { get; set; }
		public int Width { get; }
		public int Height { get; }
		public string Name { get; }

		public AndroidImageSourceImpl(string name, Func<Stream> streamSource, int quality)
		{
			Name = name;
			_streamSource = streamSource;
			_quality = quality;
			
			using (var stream = streamSource.Invoke())
			{
				var jpegInfo = ExifReader.ReadJpeg(stream);

				Orientation = Orientation.Normal;
				stream.Seek(0, SeekOrigin.Begin);
				var options = new Options { InJustDecodeBounds = true };
#pragma warning disable CS0642 // Possible mistaken empty statement
				using (DecodeStream(stream, null, options))
					;
#pragma warning restore CS0642 // Possible mistaken empty statement
				Width = Orientation == Orientation.Normal || Orientation == Orientation.Rotate180 ? options.OutWidth : options.OutHeight;
				Height = Orientation == Orientation.Normal || Orientation == Orientation.Rotate180 ? options.OutHeight : options.OutWidth;
			}
		}

		public void SaveAsJpeg(MemoryStream ms, CancellationToken ct)
		{
			TaskCompletionSource<object> tcs = new TaskCompletionSource<object>();
			ct.Register(() => {
				tcs.TrySetCanceled();
			});
			var task = Task.Run(() =>
			{

				Matrix mx = new Matrix();
				ct.ThrowIfCancellationRequested();

                switch (RotateFactor)
                {
					case 90:
						mx.PostRotate(90);
						Console.WriteLine($"ROTATING 90 DEGREES Rotate FACTOR: {RotateFactor}");
						ct.ThrowIfCancellationRequested();
                        Bitmap.Compress(CompressFormat.Jpeg, _quality, ms);
                        ct.ThrowIfCancellationRequested();
                        break;
					case 180:
						Console.WriteLine($"ROTATING 180 DEGREES Rotate FACTOR: {RotateFactor}");
						mx.PostRotate(180);
						break;
					case 270:
						Console.WriteLine($"ROTATING 270 DEGREES Rotate FACTOR: {RotateFactor}");
						mx.PostRotate(270);
						break;
					default:
						break;
				}
					//switch (Orientation)
					//{
					//	case Orientation.Rotate90:
					//		mx.PostRotate(90);
					//		break;
					//	case Orientation.Rotate180:
					//		mx.PostRotate(180);
					//		break;
					//	case Orientation.Rotate270:
					//		mx.PostRotate(270);
					//		break;
					//	default:
					//	//Console.WriteLine($"BITMAP{}");
					//	ct.ThrowIfCancellationRequested();
					//		Bitmap.Compress(CompressFormat.Jpeg, _quality, ms);
					//	ct.ThrowIfCancellationRequested();
					//		return;
					//}
					ct.ThrowIfCancellationRequested();
					using (var flip = CreateBitmap(Bitmap, 0, 0, Bitmap.Width, Bitmap.Height, mx, true))
					{
						flip.Compress(CompressFormat.Jpeg, _quality, ms);
					}
					ct.ThrowIfCancellationRequested();
			});
			Task.WaitAny(task, tcs.Task);
			tcs.TrySetCanceled();
			ct.ThrowIfCancellationRequested();
			if (task.IsFaulted)
            {
				foreach(var ex in task.Exception.InnerExceptions)
                {
					Console.WriteLine($"++++ Exception: {ex}");
                }
				throw task.Exception;
			}
				
		}
	}
}