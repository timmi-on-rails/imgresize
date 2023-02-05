using LanguageExt;
using LanguageExt.Effects.Traits;
using LanguageExt.Sys.Traits;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;

using static LanguageExt.Prelude;

namespace ImageResizer;

public static class Img<RT>
    where RT : struct, HasCancel<RT>, HasFile<RT>
{
    public static Aff<RT, Image> LoadImage(string path)
        => default(RT).FileEff.Bind(fileIO => Aff<RT, Image>(async rt =>
    {
        using var stream = fileIO.OpenRead(path);
        var image = await Image.LoadAsync(stream, rt.CancellationToken);
        return image;
    }));

    public static Aff<RT, Unit> SaveImage(string path, Image image)
        => default(RT).FileEff.Bind(fileIO => Aff<RT, Unit>(async rt =>
        {
            using var stream = fileIO.OpenWrite(path);
            await image.SaveAsync(
                stream,
                new JpegEncoder { ColorType = JpegColorType.Rgb, Quality = 85 },
                rt.CancellationToken);
            return unit;
        }));

    public static Aff<RT, Unit> CopyImageResized(
        string src,
        string dest,
        int width,
        int height,
        bool keepRatio)
        => from image in LoadImage(src)
           from resizedImage in ResizeImage(image, width, height, keepRatio)
           from _ in SaveImage(dest, resizedImage)
           select unit;

    // TODO avoid mutable variables
    public static Eff<Image> ResizeImage(
        Image image,
        int newWidth,
        int newHeight,
        bool keepRatio) => Eff(() =>
    {
        if (keepRatio)
        {
            double ratio = image.Width / image.Height;
            if (ratio > 1.0)
            {
                // landscape
                newHeight = (int)Math.Round(newWidth / ratio);
            }
            else
            {
                // portrait
                newWidth = (int)Math.Round(newHeight / ratio);
            }
        }

        image.Mutate(x => x.Resize(newWidth, newHeight));


        return image;
    });
}
