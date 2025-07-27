using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StackAnalyzer.Models
{
    public record StackAnalysisResult(
    string Framework,
    List<string> Libraries,
    List<string> CssFrameworks,
    List<string> BuildTools,
    string HostingProvider,
    List<string> Analytics,
    List<string> CdnServices,
    string Confidence,
    string RawDomData
);
}