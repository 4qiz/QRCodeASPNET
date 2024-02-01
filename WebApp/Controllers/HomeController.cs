using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Drawing;
using WebApp.Models;
using ZXing;
using ZXing.QrCode;
using ZXing.Windows.Compatibility;

namespace WebApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IWebHostEnvironment _hostEnvironment;

        public HomeController(ILogger<HomeController> logger, IWebHostEnvironment hostEnvironment)
        {
            _logger = logger;
            _hostEnvironment = hostEnvironment;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Index(IFormCollection formCollection)
        {
            var writer = new QRCodeWriter();
            var resBit = writer.encode(formCollection["QRCodeString"].ToString(), BarcodeFormat.QR_CODE, 200, 200);
            var matrix = resBit;
            Bitmap result = new Bitmap(matrix.Width, matrix.Height);
            for (int x = 0; x < matrix.Height; x++)
            {
                for (int y = 0; y < matrix.Width; y++)
                {
                    Color pixel = matrix[x, y] ? Color.Black : Color.White;
                    result.SetPixel(x, y, pixel);
                }
            }
            string webRootPath = _hostEnvironment.WebRootPath;
            result.Save(webRootPath + "\\Images\\Qrcode.png");
            ViewBag.URL = "\\Images\\Qrcode.png";

            return View();
        }

        public IActionResult ReadQRCode(IFormCollection formCollection)
        {
            string webRootPath = _hostEnvironment.WebRootPath;
            var path = webRootPath + "\\Images\\Qrcode.png";
            var reader = new BarcodeReaderGeneric();

            Bitmap image = (Bitmap)Image.FromFile(path);
            using (image)
            {
                var source = new BitmapLuminanceSource(image);
                Result res = reader.Decode(source);
                ViewBag.Text = res?.Text ?? "Не удалось считать QR code";
            }
            return View("Privacy");
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Privacy(IFormFile file)
        { // код загружает файл на сервер
            if (file == null)
            {
                return BadRequest("No file provided.");
            }

            string webRootPath = _hostEnvironment.WebRootPath;
            string userDirectory = Path.Combine(webRootPath, "Images");

            if (!Directory.Exists(userDirectory))
            {
                Directory.CreateDirectory(userDirectory);
            }

            string filePath = Path.Combine(userDirectory, "Qrcode.png");

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                file.CopyToAsync(stream).Wait();
            }

            return View("Privacy");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
