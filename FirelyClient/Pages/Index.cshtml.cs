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

namespace FirelyClient.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly IConfiguration _configuration;

        public List<string> Names;

        public IndexModel(ILogger<IndexModel> logger,
            IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
            Names = new List<string>();
        }

        public void OnGet()
        {
            Console.WriteLine("OnGet");

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
                // Let's write the fully qualified url for the resource to the console:
                Console.WriteLine("Full url for this resource: " + e.FullUrl);

                var pat_entry = (Patient)e.Resource;

                // Do something with this patient, for example write the family name that's in the first
                // element of the name list to the console:
                //Console.WriteLine("Patient's last name: " + pat_entry.Name[0].Family);
                if (pat_entry.Name.Count > 0)
                {
                    Names.Add(pat_entry.Name[0].Family + ", "
                        + pat_entry.Name[0].Given.First());
                    Console.WriteLine("Patient's last name: " + pat_entry.Name[0].Family);
                }
            }
        }
    }
}
