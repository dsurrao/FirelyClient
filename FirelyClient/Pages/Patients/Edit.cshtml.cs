using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hl7.Fhir.Model;
using Hl7.Fhir.Rest;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;

namespace FirelyClient.Pages.Patients
{
    public class EditModel : PageModel
    {
        private string id;
        private FhirClient client;

        public EditModel(IConfiguration configuration)
        {
            var settings = new FhirClientSettings
            {
                PreferredFormat = ResourceFormat.Json,
                VerifyFhirVersion = true,
                PreferredReturn = Prefer.ReturnMinimal
            };
            client = new FhirClient(configuration["FHIRServer"], settings);
        }

        public class PatientModel
        {
            public string Id { get; set; }
            public string GivenName { get; set; }
            public string FamilyName { get; set; }
            public AdministrativeGender Gender { get; set; }
            public DateTime DateOfBirth { get; set; }
        }

        public async Task<IActionResult> OnGetAsync()
        {
            id = Request.Query["id"];
            var patient = await client.ReadAsync<Patient>("Patient/" + id);
            Model = new PatientModel();
            Model.Id = id;
            Model.GivenName = patient.Name[0].GivenElement[0].Value;
            Model.FamilyName = patient.Name[0].Family;
            Model.Gender = (AdministrativeGender)patient.Gender;
            Model.DateOfBirth = DateTime.Parse(patient.BirthDate);

            return Page();
        }

        [BindProperty]
        public PatientModel Model { get; set; }

        public async Task<IActionResult> OnPostAsync()
        {
            var yyyy = Model.DateOfBirth.Year.ToString();
            var mm = Model.DateOfBirth.Month.ToString().PadLeft(2, '0');
            var dd = Model.DateOfBirth.Day.ToString().PadLeft(2, '0');

            var patient = await client.ReadAsync<Patient>("Patient/" + Model.Id);
            patient.Name = new List<HumanName>()
                {
                    new HumanName()
                    {
                        Given = new List<string>() { Model.GivenName },
                        Family = Model.FamilyName
                    }
                };
            patient.Gender = Model.Gender;
            patient.BirthDate = $"{yyyy}-{mm}-{dd}";

            var updatedPatient = await client.UpdateAsync<Patient>(patient);

            return RedirectToPage("/Index");
        }
    }

}
