using System.Diagnostics;
using FileHosting.Models;
using FileHosting.Models.Database;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.IO;

namespace FileHosting.Controllers
{
    public class FileController : Controller
    {
        private readonly AppDbContext _context;

        public FileController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult ImageUpload()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ImageUpload(FileUploadViewModel model)
        {
            const long MaxFileSize = 10 * 1024 * 1024; // 10 MB
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".bmp", ".gif" };
            var allowedContentTypes = new[] { "image/jpeg", "image/png", "image/gif", "image/bmp" };

            if (!ModelState.IsValid) return View(model);

            if (model.UploadedFile == null)
            {
                ModelState.AddModelError("UploadedFile", "Bitte eine Datei auswählen.");
                return View(model);
            }

            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login", "Auth");

            if (model.UploadedFile.Length > MaxFileSize)
            {
                ModelState.AddModelError("UploadedFile", "Datei darf maximal 10 MB groß sein.");
                return View(model);
            }

            var extension = Path.GetExtension(model.UploadedFile.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(extension))
            {
                ModelState.AddModelError("UploadedFile", "Nur Bilddateien (.jpg, .jpeg, .png, .bmp, .gif) sind erlaubt.");
                return View(model);
            }

            if (!allowedContentTypes.Contains(model.UploadedFile.ContentType))
            {
                ModelState.AddModelError("UploadedFile", "Ungültiger Dateityp.");
                return View(model);
            }

            var storedFileName = Guid.NewGuid().ToString() + extension;
            var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "uploads", storedFileName);

            using (var stream = new FileStream(uploadPath, FileMode.Create))
            {
                await model.UploadedFile.CopyToAsync(stream);
            }

            var file = new Models.Database.File
            {
                OriginalName = model.FileName + extension,
                StoredName = storedFileName,
                ContentType = model.UploadedFile.ContentType,
                FileSize = model.UploadedFile.Length,
                UserId = userId.Value,
                UploadDate = DateTime.UtcNow
            };

            _context.Files.Add(file);
            await _context.SaveChangesAsync();

            return RedirectToAction("FileList");
        }

        public async Task<IActionResult> FileList()
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login", "Auth");

            var files = await _context.Files
                .Where(f => f.UserId == userId.Value)
                .OrderByDescending(f => f.UploadDate)
                .ToListAsync();

            return View(files);
        }

        [HttpGet]
        public async Task<IActionResult> Download(int id)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login", "Auth");

            var file = await _context.Files.FirstOrDefaultAsync(f => f.Id == id && f.UserId == userId);
            if (file == null) return NotFound();

            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "uploads", file.StoredName);
            if (!System.IO.File.Exists(filePath)) return NotFound();

            return PhysicalFile(filePath, file.ContentType, file.OriginalName);
        }
    }
}
