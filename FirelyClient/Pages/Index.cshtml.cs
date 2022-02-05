using System;
using System.Collections.Generic;
using System.Linq;
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

        public List<Patient> Patients;
        public string First;
        public string Prev;
        public string Next;
        public string Last;
        public string NameParam;

        public IndexModel(ILogger<IndexModel> logger,
            IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
            Patients = new List<Patient>();
            First = "";
            Prev = "";
            Next = "";
            Last = "";
        }

        public void OnGet()
        {
            NameParam = Request.Query["NameParam"];

            var settings = new FhirClientSettings
            {
                PreferredFormat = ResourceFormat.Json,
                VerifyFhirVersion = true,
                PreferredReturn = Prefer.ReturnMinimal
            };
            var client = new FhirClient(_configuration["FHIRServer"], settings);

            var searchParams = new List<string>();
            if (NameParam != null && !NameParam.Trim().Equals(""))
            {
                searchParams.Add($"name:contains={NameParam}");
            }

            var allowedKeys = new string[] { "_count", "_skip" };
            foreach (var key in Request.Query.Keys.Where(
                key => allowedKeys.Contains(key)))
            {
                searchParams.Add($"{key}={Request.Query[key]}");
            }

            var result = client.Search<Patient>(searchParams.ToArray());
            foreach (var e in result.Entry)
            {
                Patients.Add((Patient)e.Resource);
            }

            if (result.FirstLink != null) First = result.FirstLink.Query;
            if (result.PreviousLink != null) Prev = result.PreviousLink.Query;
            if (result.NextLink != null) Next = result.NextLink.Query;
            if (result.LastLink != null) Last = result.LastLink.Query;
        }
    }
}
