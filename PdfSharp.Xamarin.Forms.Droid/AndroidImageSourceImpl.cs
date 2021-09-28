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
				//var jpegInfo = ExifReader.ReadJpeg(stream);

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
                //switch (RotateFactor)
                //            {
                //	case 90:
                //		mx.PostRotate(90);
                //		Console.WriteLine($"ROTATING 90 DEGREES Rotate FACTOR: {RotateFactor}");
                //                    break;
                //	case 180:
                //		Console.WriteLine($"ROTATING 180 DEGREES Rotate FACTOR: {RotateFactor}");
                //		mx.PostRotate(180);
                //		break;
                //	case 270:
                //		Console.WriteLine($"ROTATING 270 DEGREES Rotate FACTOR: {RotateFactor}");
                //		mx.PostRotate(270);
                //		break;
                //	default:
                //		ct.ThrowIfCancellationRequested();
                //		Bitmap.Compress(CompressFormat.Jpeg, _quality, ms);
                //		ct.ThrowIfCancellationRequested();
                //		break;
                //}
                switch (Orientation)
                {
                    case Orientation.Rotate90:
                        mx.PostRotate(90);
                        break;
                    case Orientation.Rotate180:
                        mx.PostRotate(180);
                        break;
                    case Orientation.Rotate270:
                        mx.PostRotate(270);
                        break;
                    default:
                        ct.ThrowIfCancellationRequested();
                        Bitmap.Compress(CompressFormat.Jpeg, _quality, ms);
                        ct.ThrowIfCancellationRequested();
                        return;
                }
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
		
		public void rotate(float degrees, MemoryStream ms)
		{
			Console.WriteLine($"Bitmap height: {Bitmap.Height} Bitmap width: {Bitmap.Width}");
			float totalRotated = 0;
			if (Bitmap != null)
			{
				Matrix matrix = new Matrix();
				// compute the absolute rotation
				totalRotated = (totalRotated + degrees) % 360;
				// precompute some trig functions
				double radians = Math.PI / 180 *totalRotated;
				double sin = Math.Abs(Math.Sin(radians));
				double cos = Math.Abs(Math.Cos(radians));
				// figure out total width and height of new bitmap
				double newWidth = Bitmap.Width * cos + Bitmap.Height * sin;
				double newHeight = Bitmap.Width * sin + Bitmap.Height * cos;
				Console.WriteLine($"Bitmap new height: {newHeight} Bitmap new width: {newWidth}");
				// set up matrix
				matrix.Reset();
				matrix.PreRotate(totalRotated, (float)(newWidth / 2) , (float)(newHeight / 2));
				// create new bitmap by rotating mBitmap

				using (var rotated = CreateBitmap(Bitmap, 0, 0,
											  Bitmap.Width, Bitmap.Height, matrix, true)) 
				{
					var test = CreateScaledBitmap(rotated, (int)newWidth, (int)newHeight, true);
					test.Compress(CompressFormat.Jpeg, _quality, ms);
				} ;
			}
		}
	}
}