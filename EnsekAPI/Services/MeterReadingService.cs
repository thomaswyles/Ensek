using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.Configuration.Attributes;
using EnsekAPI.Repository;
using EnsekAPI.Repository.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnsekAPI.Services
{
    public class MeterReadingService
    {
        EnsekDbContext _ensekDbContext;

        public MeterReadingService(EnsekDbContext ensekDbContext)
        {
            _ensekDbContext = ensekDbContext;
        }

        public List<MeterReadingsCSV> ReadCSV(IFormFile file)
        {
            List<MeterReadingsCSV> meterReadings = new List<MeterReadingsCSV>();

            CsvConfiguration csvConfiguration = new CsvConfiguration(CultureInfo.InvariantCulture);
            csvConfiguration.HeaderValidated = null;
            csvConfiguration.BadDataFound = null;
            csvConfiguration.MissingFieldFound = null;
            csvConfiguration.Delimiter = ",";
            csvConfiguration.HasHeaderRecord = true;

            using (TextReader textReader = new StreamReader(file.OpenReadStream()))
            using (CsvReader csvReader = new CsvReader(textReader, csvConfiguration))
            {
                csvReader.Context.RegisterClassMap<MeterReadingMap>();
                var records = csvReader.GetRecords<MeterReadingsCSV>();

                foreach (var record in records)
                    meterReadings.Add(ValidateRecord(record));

            };

            return meterReadings;

        }

        private MeterReadingsCSV ValidateRecord(MeterReadingsCSV record)
        {
            if (!int.TryParse(record.AccountId, out int accountId))
            {
                record.Valid = false;
                record.Errors.Add($"{record.AccountId} is not a valid format.");
            }
            else
            {
                if (!AccountExists(accountId))
                {
                    record.Valid = false;
                    record.Errors.Add($"Account {record.AccountId} does not exist.");
                }
            }

            if (!DateTime.TryParse(record.MeterReadingDateTime, out DateTime meterReadingDateTime))
            {
                record.Valid = false;
                record.Errors.Add($"{record.MeterReadingDateTime} is not a valid format.");
            }
            else
            {
                if (record.Valid && _ensekDbContext.MeterReadings.Any(x => x.AccountId == accountId &&
                                                                        x.ReadingDateTime >= meterReadingDateTime))
                {
                    record.Valid = false;
                    record.Errors.Add($"A more recent meter reading already exists for account {record.AccountId}.");
                }
            }

            if (!int.TryParse(record.MeterReadValue, out int meterReadValue))
            {
                record.Valid = false;
                record.Errors.Add($"{record.MeterReadValue} is not a valid format.");
            }

            if (_ensekDbContext.MeterReadings.Any(x => x.AccountId == accountId && x.ReadingDateTime == meterReadingDateTime && x.ReadValue == meterReadValue))
            {
                record.Valid = false;
                record.Errors.Add($"This meter reading already exists in Database.");
            }

            return record;
        }

        private bool AccountExists(int AccountId) => _ensekDbContext.Accounts.Any(x => x.Id == AccountId);
        

        public class MeterReadingsCSV
        {
            [Name("AccountId")]
            public string AccountId { get; set; }
            [Name("MeterReadingDateTime")]
            public string MeterReadingDateTime { get; set; }
            [Name("MeterReadValue")]
            public string MeterReadValue { get; set; }

            public List<string> Errors { get; set; } = new List<string>();
            public bool Valid { get; set; } = true;

        }

        public sealed class MeterReadingMap : ClassMap<MeterReadingsCSV>
        {
            public MeterReadingMap()
            {
                Map(m => m.AccountId);
                Map(m => m.MeterReadingDateTime);
                Map(m => m.MeterReadValue);
            }
        }

    }
}
