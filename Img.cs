using LanguageExt;
using LanguageExt.Sys.Traits;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;

using static LanguageExt.Prelude;

namespace ImageResizer;

public static class Img
{
    public static Aff<RT, Image> LoadImage<RT>(string path)
        where RT : struct, HasFile<RT>
        => default(RT).FileEff.Bind(fileIO => Aff<RT, Image>(async rt =>
    {
        using var stream = fileIO.OpenRead(path);
        var image = await Image.LoadAsync(stream, rt.CancellationToken);
        return image;
    }));

    public static Aff<RT, Unit> SaveImage<RT>(string path, Image image)
        where RT : struct, HasFile<RT>
        => default(RT).FileEff.Bind(fileIO => Aff<RT, Unit>(async rt =>
        {
            using var stream = fileIO.OpenWrite(path);
            await image.SaveAsync(
                stream,
                new JpegEncoder { ColorType = JpegColorType.Rgb, Quality = 85 },
                rt.CancellationToken);
            return unit;
        }));

    public static Aff<RT, Unit> CopyImageResized<RT>(
        string src,
        string dest,
        int width,
        int height,
        bool keepRatio)
        where RT : struct, HasFile<RT>
        => from image in LoadImage<RT>(src)
           from resizedImage in ResizeImage(image, width, height, keepRatio)
           from _ in SaveImage<RT>(dest, resizedImage)
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
