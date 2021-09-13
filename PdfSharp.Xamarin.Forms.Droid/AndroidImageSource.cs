using System;
using System.IO;
using System.Linq;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Media;
using ExifLib;
using MigraDocCore.DocumentObjectModel.MigraDoc.DocumentObjectModel.Shapes;
using Xamarin.Essentials;
using Xamarin.Forms.Platform.Android;
using static Android.Graphics.Bitmap;
namespace PdfSharp.Xamarin.Forms.Droid
{
	public class AndroidImageSource : ImageSource
	{
		protected override IImageSource FromBinaryImpl(string name, Func<byte[]> imageSource, int? quality = 75)
		{
			return new AndroidImageSourceImpl(name, () => { return new MemoryStream(imageSource.Invoke()); }, (int)quality);
		}

		protected override IImageSource FromFileImpl(string path, int? quality = 75)
		{
			var newFile = System.IO.Path.Combine(FileSystem.CacheDirectory, path);
            Console.WriteLine($"++++++ PATH TO FILE: {newFile}");
			Console.WriteLine($"+++++ IN FILE");
            ExifInterface exif = new ExifInterface(newFile);
            int orientation = exif.GetAttributeInt(ExifInterface.TagOrientation, -1);
            Console.WriteLine($"ORIENTATION {orientation}");
            int rotate = 0;
            //https://www.impulseadventure.com/photo/exif-orientation.html this has corresponding rotation values for jpegs ex. if the case is 6, rotate the jpeg 90 degrees
            switch (orientation)
            {
                case 6:
                    {
                        rotate = 90;
                        break;
                    }
                case 3:
                    {
                        rotate = 180;
                        break;
                    }
                case 8:
                    {
                        rotate = 270;
                        break;
                    }
                default:
                    {
                        rotate = 0;
                        break;
                    }
            }
            exif.Dispose();



            //if (path.Contains("."))
            //{
            //    Console.WriteLine("+++++ IN FIRST IF STATEMENT");
            //    string[] tokens = path.Split('.');
            //    Array.ForEach(tokens, Console.WriteLine);
            //    Console.WriteLine($"+++++ TOKENS: {tokens}");
            //    tokens = tokens.Take(tokens.Length - 1).ToArray();
            //    path = String.Join(".", tokens);
            //    Console.WriteLine($"+++++ FIRST IF PATH: {path}");
            //}
            //var res = global::Xamarin.Forms.Forms.Context.Resources;
            ////Console.WriteLine($"+++++ RES: {res.}");
            //Console.WriteLine($"+++++ PATH: {res.GetResourcePackageName(Android.Resource.Id.Home)}");
            //var resId = res.GetIdentifier(path, "drawable", "ca.sait.ciits.Urchin");
            //Console.WriteLine($"+++++ PATH: {path}");
            //Console.WriteLine($"+++++ RESID: {resId}");
            System.IO.Stream stream = new MemoryStream();
            Console.WriteLine($"+++++ Memory Stream {stream.Length}");
            BitmapFactory.Options bmOptions = new BitmapFactory.Options();
            Console.WriteLine($"+++++ Bitmap options {bmOptions}");
            Console.WriteLine($"FILE LOCATION IN IMAGESOURCE: {newFile}");
            Bitmap bitmap = BitmapFactory.DecodeFile(newFile);
            Console.WriteLine($"+++++ Bitmap its self {bitmap.ByteCount}");
            
            bitmap.Compress(CompressFormat.Jpeg, 75, stream);
            
            Console.WriteLine($"+++++ Bitmap its self after compress {bitmap.ByteCount}");

            return new AndroidImageSourceImpl(newFile, () => stream, quality ?? 75) { Bitmap = bitmap, RotateFactor = rotate};
		}

        protected override IImageSource FromStreamImpl(string name, Func<System.IO.Stream> imageStream, int? quality = 75 )
		{
			Console.WriteLine($"+++++ IN STREAM");
            Console.WriteLine($"+++++ Stream path {name}");
            return new AndroidImageSourceImpl(name, imageStream, (int)quality) ;
		}
	}
}