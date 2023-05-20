using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using StockIndicatorsAnalyzer.BLL;
using StockIndicatorsAnalyzer.Models;

namespace StockIndicatorsAnalyzer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StockInformationController : ControllerBase
    {
        private static IStockInfoService _stockInfoService;
        public StockInformationController(IStockInfoService StockInfoService)
        {
            _stockInfoService = StockInfoService;
        }

        [HttpGet("/downloadDailyStockFiles")]
        public async Task<IActionResult> DownloadDailyStatData([FromBody] FileDownloadRequest request)
        {
            try
            {
                await _stockInfoService.GetDailyFileFromNSEAsync(request.StartDate,request.EndDate,request.DelayBetweenRequests);
                return Ok("Files are downloading in the background. Check after sometime");
            }
            catch(Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("/downloadedFilesList")]
        public IActionResult DownloadedFiles()
        {
            try
            {
                var filesList = _stockInfoService.GetDownloadedFilesStatus();
                return Ok(new
                {
                    filesList = filesList,
                    count = filesList.Count
                });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("/UnzipFiles")]
        public IActionResult UnzipFiles()
        {
            try
            {
                _stockInfoService.UnZipFiles();
                return Ok("Files Unzipped Successfully");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("/savedatatomasterJson")]
        public async Task<IActionResult> SaveDataToMasterJson()
        {
            try
            {
                await _stockInfoService.SaveDataToMasterJsonAsync();
                return Ok("All data is saved to Master json");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("/getIndicators")]
        public async Task<IActionResult> GetIndicators([FromBody]Request req)
        {
            try
            {
                var data = await _stockInfoService.GetTechnicalIndicatorsAsync(req);
                return Ok(data);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
