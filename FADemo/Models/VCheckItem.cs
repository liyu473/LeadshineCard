using System;
using Avalonia.Media.Imaging;
using Metalama.Patterns.Observability;

namespace FADemo.Models;

[Observable]
public partial class VCheckItem
{
    public Bitmap? ImageSource { get; set; }
    
    public Uri? Path { get; set; }
    
    public Bitmap? CheckResult { get; set; }
}
