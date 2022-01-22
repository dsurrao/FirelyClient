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
using Microsoft.Extensions.Primitives;

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
            var settings = new FhirClientSettings
            {
                PreferredFormat = ResourceFormat.Json,
                VerifyFhirVersion = true,
                PreferredReturn = Prefer.ReturnMinimal
            };
            var client = new FhirClient(_configuration["FHIRServer"], settings);

            var searchParams = new string[] { };

            var skip = Request.Query["_skip"];
            if (skip != StringValues.Empty)
            {
                searchParams = searchParams.Append($"_skip={skip}").ToArray();
            }

            var count = Request.Query["_count"];
            if (count != StringValues.Empty)
            {
                searchParams = searchParams.Append($"_count={count}").ToArray();
            }

            var result = client.Search<Patient>(searchParams);
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
