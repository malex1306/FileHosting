namespace FileHosting.Models;
using System.ComponentModel.DataAnnotations;

public class FileUploadViewModel
{
 
    [Required]
    [Display(Name = "Dateiname (ohne Endung)")]
    public string FileName { get; set; }

    [Required]
    [Display(Name = "Datei auswählen")]
    public IFormFile UploadedFile { get; set; }
}