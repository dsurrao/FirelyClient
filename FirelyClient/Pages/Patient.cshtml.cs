using System.Collections.Generic;
using System.Threading.Tasks;
using Hl7.Fhir.Model;
using Hl7.Fhir.Rest;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using FirelyClient.Pages.Utils;

namespace FirelyClient.Pages
{
    public class PatientModel : PageModel
    {
        private readonly IConfiguration _configuration;

        public string id;
        public string Patient;
        public List<string> Conditions { get;  private set; }

        public PatientModel(IConfiguration configuration)
        {
            _configuration = configuration;
            Patient = "";
            Conditions = new List<string>();
        }

        public async System.Threading.Tasks.Task OnGetAsync()
        {
            id = Request.Query["id"];

            var settings = new FhirClientSettings
            {
                PreferredFormat = ResourceFormat.Json,
                VerifyFhirVersion = true,
                PreferredReturn = Prefer.ReturnMinimal
            };
            var client = new FhirClient(_configuration["FHIRServer"], settings);


            Task<Patient> patientTask = client.ReadAsync<Patient>("Patient/" + id);
            Task<Bundle> conditionTask = client.SearchAsync<Condition>(
                new string[] { "patient=" + id });
            var patientBundle = await patientTask;
            var conditionBundle = await conditionTask;

            Patient = PatientUtils.GetPatientDescription(patientBundle);

            foreach (var e in conditionBundle.Entry)
            {
                var condition = (Condition)e.Resource;
                Conditions.Add(condition.Code.Text);
            }
        }
    }
}
