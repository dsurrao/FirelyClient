using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Hl7.Fhir.Model;
using Hl7.Fhir.Rest;
using Microsoft.Extensions.Configuration;
using System.Globalization;

namespace FirelyClient.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly IConfiguration _configuration;

        public List<string> Patients;

        public IndexModel(ILogger<IndexModel> logger,
            IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
            Patients = new List<string>();
        }

        public void OnGet()
        {
            var settings = new FhirClientSettings
            {
                PreferredFormat = ResourceFormat.Json,
                VerifyFhirVersion = true,
                PreferredReturn = Prefer.ReturnMinimal
            };
            var client = new FhirClient(_configuration["FHIRServer"], settings);
            var result = client.Search<Patient>();

            foreach (var e in result.Entry)
            {
                Patients.Add(GetPatientDescription((Patient)e.Resource));
            }
        }

        private string GetPatientDescription(Patient patient)
        {
            string name = "", gender = "", age = "";

            if (patient.Name.Count > 0)
            {
                if (patient.Name[0].Family != null)
                {
                    name += patient.Name[0].Family;
                }

                if (!name.Equals("")) name += ", ";

                foreach (var given in patient.Name[0].Given)
                {
                    name += given + " ";
                }
            }

            gender = patient.Gender + ", ";

            try
            {
                var birthDate = DateTime.Parse(patient.BirthDate);
                age = $"{CalculateAge(birthDate)} y";
            }
            catch (FormatException)
            {
                Console.WriteLine("Unable to parse '{0}'", patient.BirthDate);
            }

            return $"{name} ({gender}{age})";
        }

        private int CalculateAge(DateTime dateTime)
        {
            DateTime currentDate = DateTime.Now;
            int age = currentDate.Year - dateTime.Year;

            if (age > 0)
            {
                if (currentDate.Month == dateTime.Month)
                {
                    if (currentDate.Day < dateTime.Day)
                    {
                        age--;
                    }
                }
                else if (currentDate.Month < dateTime.Month)
                {
                    age--;
                }
            }

            return age;
        }
    }
}
