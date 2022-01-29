using System.Collections.Generic;
using System.Threading.Tasks;
using Hl7.Fhir.Model;
using Hl7.Fhir.Rest;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using FirelyClient.Pages.Utils;
using System;
using Microsoft.AspNetCore.Mvc;

namespace FirelyClient.Pages
{
    public class PatientModel : PageModel
    {
        private readonly FhirClient _client;

        public string Id { get; private set; }
        public string Patient { get; private set; }
        public List<string> Conditions { get;  private set; }
        public List<string> Medications { get; private set; }
        public List<string> Observations { get; private set; }
        public List<string> Encounters { get; private set; }

        public PatientModel(IConfiguration configuration)
        {
            Patient = "";
            Conditions = new List<string>();
            Medications = new List<string>();
            Observations = new List<string>();
            Encounters = new List<string>();

            _client = new FhirClient(configuration["FHIRServer"],
                new FhirClientSettings
                {
                    PreferredFormat = ResourceFormat.Json,
                    VerifyFhirVersion = true,
                    PreferredReturn = Prefer.ReturnMinimal
                });
        }

        /*
         * Annotation, Signature, Account, AdverseEvent, AllergyIntolerance, Appointment, AppointmentResponse, AuditEvent, Basic, BiologicallyDerivedProduct, BodyStructure, CarePlan, CareTeam, ChargeItem, Claim, ClaimResponse, ClinicalImpression, Communication, CommunicationRequest, Composition, Condition, Consent, Contract, Coverage, CoverageEligibilityRequest, CoverageEligibilityResponse, DetectedIssue, Device, DeviceRequest, DeviceUseStatement, DiagnosticReport, DocumentManifest, DocumentReference, Encounter, EnrollmentRequest, EpisodeOfCare, ExplanationOfBenefit, FamilyMemberHistory, Flag, Goal, Group, GuidanceResponse, ImagingStudy, Immunization, ImmunizationEvaluation, ImmunizationRecommendation, Invoice, List, MeasureReport, Media, MedicationAdministration, MedicationDispense, MedicationRequest, MedicationStatement, MolecularSequence, NutritionOrder, Observation, itself, Person, Procedure, Provenance, QuestionnaireResponse, RelatedPerson, RequestGroup, ResearchSubject, RiskAssessment, Schedule, ServiceRequest, Specimen, SupplyDelivery, SupplyRequest, Task and VisionPrescription
         */
        public async Task<IActionResult> OnGetAsync()
        {
            Id = Request.Query["id"];

            Task<Patient> patientTask = _client.ReadAsync<Patient>("Patient/" + Id);
            Task<Bundle> conditionTask = _client.SearchAsync<Condition>(
                new string[] { "patient=" + Id });
            Task<Bundle> medicationTask = _client.SearchAsync<MedicationRequest>(
                new string[] { "patient=" + Id });

            Task<Bundle> observationTask = _client.SearchAsync<Observation>(
                new string[] { "patient=" + Id });
            Task<Bundle> encounterTask = _client.SearchAsync<Encounter>(
                new string[] { "patient=" + Id });

            var patientBundle = await patientTask;
            var conditionBundle = await conditionTask;
            var medicationBundle = await medicationTask;
            var observationBundle = await observationTask;
            var encounterBundle = await encounterTask;

            Patient = PatientUtils.GetPatientDescription(patientBundle);

            foreach (var e in conditionBundle.Entry)
            {
                var condition = (Condition)e.Resource;
                var conditionText = "";
                if (condition.RecordedDate != null)
                {
                    conditionText += condition.RecordedDate.ToString() + " ";
                }
                if (condition.Code != null)
                {

                }
                if (condition.Code.Coding.Count > 0)
                {
                    conditionText += condition.Code.Coding[0].Display;
                }
                else
                {
                    conditionText += condition.Code.Text;
                }
                Conditions.Add(conditionText);
            }

            foreach (var e in medicationBundle.Entry)
            {
                try
                {
                    var medication = (MedicationRequest)e.Resource;
                    var medicationText = medication.AuthoredOn + " "
                        + ((ResourceReference) medication.Medication).Display;
                    Medications.Add(medicationText);
                }
                catch (InvalidCastException)
                {
                    Console.WriteLine($"Could not cast {e.FullUrl} to medication.");
                }
            }

            foreach (var e in observationBundle.Entry)
            {
                Observations.Add(((Observation)e.Resource).Effective.ToString() + ((Observation)e.Resource).Text.Div);
            }

            foreach (var e in encounterBundle.Entry)
            {
                Encounters.Add(((Encounter)e.Resource).Period.Start
                    + ((Encounter)e.Resource).Text.Div);
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var id = Request.Form["Id"];
            await _client.DeleteAsync($"Patient/{id}");

            return RedirectToPage("/Index");
        }
    }
}

// todo: add dates and times to all observations