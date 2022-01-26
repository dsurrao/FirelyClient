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
    public class CreateModel : PageModel
    {
        private IConfiguration _configuration;

        public class PatientModel
        {
            public string GivenName { get; set; }
            public string FamilyName { get; set; }
            public AdministrativeGender Gender { get; set; }
            public DateTime DateOfBirth { get; set; }
        }

        public CreateModel(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [BindProperty]
        public PatientModel Model { get; set; }

        public async Task<IActionResult> OnPostAsync()
        {
            var yyyy = Model.DateOfBirth.Year.ToString();
            var mm = Model.DateOfBirth.Month.ToString().PadLeft(2, '0');
            var dd = Model.DateOfBirth.Day.ToString().PadLeft(2, '0');
            var settings = new FhirClientSettings
            {
                PreferredFormat = ResourceFormat.Json,
                VerifyFhirVersion = true,
                PreferredReturn = Prefer.ReturnMinimal
            };
            var client = new FhirClient(_configuration["FHIRServer"], settings);
            var createdPatient = await client.CreateAsync<Patient>(
                new Patient()
                {
                    Name = new List<HumanName>()
                    {
                        new HumanName()
                        {
                            Given = new List<string>() { Model.GivenName },
                            Family = Model.FamilyName
                        }
                    },
                    Gender = Model.Gender,
                    BirthDate = $"{yyyy}-{mm}-{dd}"
                }
            );

            return RedirectToPage("/Index");
        }
    }

}
