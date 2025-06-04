using System;
using System.Collections.Generic;

namespace FileHosting.Models.Database;

public partial class File
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public string OriginalName { get; set; } = null!;

    public string StoredName { get; set; } = null!;

    public long FileSize { get; set; }

    public string? ContentType { get; set; }

    public DateTime? UploadDate { get; set; }

    public virtual User User { get; set; } = null!;
}
