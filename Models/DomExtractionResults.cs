using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StackAnalyzer.Models
{
    public record DomExtractionResult(
    string Html,
    string[] Scripts,
    string[] Stylesheets,
    object[] MetaTags,
    string[] Headers,
    string[] CookieNames
);
}