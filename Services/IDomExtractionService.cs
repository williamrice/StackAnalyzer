using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using StackAnalyzer.Models;

namespace StackAnalyzer.Services
{
    public interface IDomExtractionService
    {
        Task<DomExtractionResult> ExtractDomDataAsync(string url);
    }
}