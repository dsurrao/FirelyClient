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
using FirelyClient.Pages.Utils;

namespace FirelyClient.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly IConfiguration _configuration;

        //public List<string> Patients;
        public List<Patient> Patients;

        public IndexModel(ILogger<IndexModel> logger,
            IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
            //Patients = new List<string>();
            Patients = new List<Patient>();
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
                //Patients.Add(GetPatientDescription((Patient)e.Resource));
                Patients.Add((Patient)e.Resource);
            }
        }
    }
}
