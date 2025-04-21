using System.Configuration;
using SkiaSharp;

public interface IImageService
{
    Task<string> UploadImage(IFormFile formFile, string username);
    bool DeleteImage(string imageUrl);
    Task<string> GenerateImage(string username);
}

public class ImageService : IImageService
{
    private static readonly Dictionary<string, List<byte[]>> _fileSignature =
    new Dictionary<string, List<byte[]>>
    {
        { ".jpeg", new List<byte[]>
            {
                new byte[] { 0xFF, 0xD8, 0xFF, 0xE0 },
                new byte[] { 0xFF, 0xD8, 0xFF, 0xE2 },
                new byte[] { 0xFF, 0xD8, 0xFF, 0xE3 },
            }
        },
        { ".png", new List<byte[]>
            {
                new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A },
            }
        },
    };

    private static bool validateFile(IFormFile formFile)
    {
        var ext = Path.GetExtension(formFile.FileName);
        if (!_fileSignature.ContainsKey(ext))
            return false;

        using (var reader = new BinaryReader(formFile.OpenReadStream()))
        {
            var signatures = _fileSignature[ext];
            var headerBytes = reader.ReadBytes(signatures.Max(m => m.Length));

            return signatures.Any(signature =>
                headerBytes.Take(signature.Length).SequenceEqual(signature));
        }
    }

    public async Task<string> UploadImage(IFormFile formFile, string username)
    {
        if (!validateFile(formFile)) 
            throw new ArgumentException("Invalid file type");

        var ext = Path.GetExtension(formFile.FileName);
        var fileName = $"{Guid.NewGuid()}{ext}";
        var path = Path.Combine(Environment.CurrentDirectory, "wwwroot/images/uploads/", fileName);
        
        using (var stream = new FileStream(path, FileMode.Create))
        {
            await formFile.CopyToAsync(stream);
        }

        Console.WriteLine("Hello we did it!");
        return $"/images/uploads/{fileName}";
    }

    public bool DeleteImage(string imageUrl)
    {
        if (string.IsNullOrEmpty(imageUrl))
            return false;

        var fileName = Path.GetFileName(imageUrl);
        if (string.IsNullOrEmpty(fileName))
            return false;

        var path = Path.Combine(Environment.CurrentDirectory, "wwwroot/images/uploads/", fileName);
        
        if (!File.Exists(path))
            return false;

        File.Delete(path);
        return true;
    }

    public async Task<string> GenerateImage(string displayName)
    {
        if (string.IsNullOrEmpty(displayName))
            throw new ArgumentException("Display Name cannot be empty");

        var fileName = $"{Guid.NewGuid()}.png";
        var path = Path.Combine(Environment.CurrentDirectory, "wwwroot/images/uploads/", fileName);
        
        var firstLetters = displayName.Split(' ').Select(w => w[0].ToString()).Take(2).Aggregate((a, b) => $"{a}{b}");

        GenerateImageWithText(firstLetters, path);

        return $"/images/uploads/{fileName}";
    }

    private static void GenerateImageWithText(string text, string filePath, int width = 256, int height = 256)
    {
        // Create a new image surface
        using (var surface = SKSurface.Create(new SKImageInfo(width, height)))
        {
            var canvas = surface.Canvas;
            
            // Clear the canvas with white background
            var randomColor = new Random();
            var color = SKColor.FromHsl((byte)randomColor.Next(360), (byte)randomColor.Next(100), 70);
            canvas.Clear(color);
            
            // Create paint for drawing text
            var paint = new SKPaint
            {
                Color = SKColors.Black,
                TextSize = 96,
                IsAntialias = true,
                Typeface = SKTypeface.FromFamilyName("Sans", SKFontStyleWeight.Bold, SKFontStyleWidth.Normal, SKFontStyleSlant.Upright)
            };
            
            // Measure text to center it
            SKRect textBounds = new SKRect();
            paint.MeasureText(text, ref textBounds);
            
            // Calculate position to center the text
            float x = (width - textBounds.Width) / 2 - textBounds.Left;
            float y = (height - textBounds.Height) / 2 - textBounds.Top;
            
            // Draw the text
            canvas.DrawText(text, x, y, paint);
            
            // Save the image
            using (var image = surface.Snapshot())
            using (var data = image.Encode(SKEncodedImageFormat.Png, 100))
            using (var stream = File.OpenWrite(filePath))
            {
                data.SaveTo(stream);
            }
        }
    }

}
