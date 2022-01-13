using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hl7.Fhir.Model;
using Hl7.Fhir.Rest;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;

namespace FirelyClient.Pages
{
    public class PatientModel : PageModel
    {
        private readonly IConfiguration _configuration;

        public string id;
        public List<string> Conditions { get;  private set; }

        public PatientModel(IConfiguration configuration)
        {
            _configuration = configuration;
            Conditions = new List<string>();
        }

        public void OnGet()
        {
            id = Request.Query["id"];

            var settings = new FhirClientSettings
            {
                PreferredFormat = ResourceFormat.Json,
                VerifyFhirVersion = true,
                PreferredReturn = Prefer.ReturnMinimal
            };
            var client = new FhirClient(_configuration["FHIRServer"], settings);
            var result = client.Search<Condition>(new string[] { "patient=" + id });
            foreach (var e in result.Entry)
            {
                var condition = (Condition)e.Resource;
                Conditions.Add(condition.Code.Text);
            }
        }
    }
}
