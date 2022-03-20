using EnsekAPI.Repository;
using EnsekAPI.Repository.Models;
using EnsekAPI.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static EnsekAPI.Services.MeterReadingService;

namespace EnsekAPI.Controllers
{
    [ApiController]
    public class MeterReadingController : ControllerBase
    {
        private readonly ILogger<MeterReadingController> _logger;

        EnsekDbContext _context = new EnsekDbContext();

        public MeterReadingController(ILogger<MeterReadingController> logger)
        {
            _logger = logger;
        }

        [HttpPost("meter-reading-upload")]
        public IActionResult POST(IFormFile file)
        {
            if (file == null)
                return BadRequest("A File must be uploaded");
            else if (!file.FileName.EndsWith(".csv"))
                return BadRequest("Only CSV files are supported");


            MeterReadingService meterReadingService = new MeterReadingService(_context);
            List<MeterReadingsCSV> meterReadings = meterReadingService.ReadCSV(file);

            if (!meterReadings.Any())
                return Ok("File contains no meter readings");

            if (meterReadings.Any(x => x.Valid))
            {
                    List<MeterReading> newMeterReadings = new List<MeterReading>();

                    foreach (MeterReadingsCSV meterReadingCSV in meterReadings.Where(x => x.Valid))
                    {
                        MeterReading meterReading = new MeterReading()
                        {
                            AccountId = int.Parse(meterReadingCSV.AccountId),
                            ReadingDateTime = DateTime.Parse(meterReadingCSV.MeterReadingDateTime),
                            ReadValue = int.Parse(meterReadingCSV.MeterReadValue)
                        };

                        newMeterReadings.Add(meterReading);
                    }

                _context.MeterReadings.AddRange(newMeterReadings);
                _context.SaveChanges();
            }

            if (meterReadings.Any(x => !x.Valid))
            {
                string resultMessage = "Following Meter Readings Failed Validation:\r\n";

                foreach (MeterReadingsCSV meterReadingsCSV in meterReadings.Where(x => !x.Valid))
                {
                    resultMessage += $"{meterReadingsCSV.AccountId} | {meterReadingsCSV.MeterReadingDateTime} | {meterReadingsCSV.MeterReadValue}\r\n";
                    resultMessage += $"Reasons:\r\n";
                    resultMessage += string.Join("\r\n", meterReadingsCSV.Errors);
                    resultMessage += "\r\n-----------------------------------\r\n";

                }

                return Ok(resultMessage);
            }
            else return Ok("All Meter Readings Uploaded");
               

        }
    }
}
