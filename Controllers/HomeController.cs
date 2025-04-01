using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using CADProjectsHub.Models;
using CADProjectsHub.Helpers;
using Microsoft.Extensions.Options;

namespace CADProjectsHub.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IOptions<CryptoSettings> _cryptoSettings;
        public HomeController(ILogger<HomeController> logger, IOptions<CryptoSettings> cryptoSettings)
        {
            _logger = logger;
            _cryptoSettings = cryptoSettings;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult RunBenchmark()
        {
            var rsaHelper = new Helpers.RSAHelper(_cryptoSettings);
            rsaHelper.RunAllTests();
            return Content("Benchmark completed. Check /wwwroot/logs/crypto_benchmark_log.txt");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
