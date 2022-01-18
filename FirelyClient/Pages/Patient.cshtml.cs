using System.Collections.Generic;
using System.Threading.Tasks;
using Hl7.Fhir.Model;
using Hl7.Fhir.Rest;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using FirelyClient.Pages.Utils;
using System;

namespace FirelyClient.Pages
{
    public class PatientModel : PageModel
    {
        private readonly IConfiguration _configuration;

        public string id;
        public string Patient;
        public List<string> Conditions { get;  private set; }
        public List<string> Medications { get; private set; }
        public List<string> Observations { get; private set; }
        public List<string> Encounters { get; private set; }

        public PatientModel(IConfiguration configuration)
        {
            _configuration = configuration;
            Patient = "";
            Conditions = new List<string>();
            Medications = new List<string>();
            Observations = new List<string>();
            Encounters = new List<string>();
        }

        /*
         * Annotation, Signature, Account, AdverseEvent, AllergyIntolerance, Appointment, AppointmentResponse, AuditEvent, Basic, BiologicallyDerivedProduct, BodyStructure, CarePlan, CareTeam, ChargeItem, Claim, ClaimResponse, ClinicalImpression, Communication, CommunicationRequest, Composition, Condition, Consent, Contract, Coverage, CoverageEligibilityRequest, CoverageEligibilityResponse, DetectedIssue, Device, DeviceRequest, DeviceUseStatement, DiagnosticReport, DocumentManifest, DocumentReference, Encounter, EnrollmentRequest, EpisodeOfCare, ExplanationOfBenefit, FamilyMemberHistory, Flag, Goal, Group, GuidanceResponse, ImagingStudy, Immunization, ImmunizationEvaluation, ImmunizationRecommendation, Invoice, List, MeasureReport, Media, MedicationAdministration, MedicationDispense, MedicationRequest, MedicationStatement, MolecularSequence, NutritionOrder, Observation, itself, Person, Procedure, Provenance, QuestionnaireResponse, RelatedPerson, RequestGroup, ResearchSubject, RiskAssessment, Schedule, ServiceRequest, Specimen, SupplyDelivery, SupplyRequest, Task and VisionPrescription
         */
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

            // Firely server bug reported:
            // https://github.com/FirelyTeam/spark/issues/436#issuecomment-1013732001
            Task<Bundle> medicationTask = client.SearchAsync<Medication>(
                new string[] { "patient=" + id });

            Task<Bundle> observationTask = client.SearchAsync<Observation>(
                new string[] { "patient=" + id });
            Task<Bundle> encounterTask = client.SearchAsync<Encounter>(
                new string[] { "patient=" + id });

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
                if (condition.Code.Coding.Count > 0)
                {
                    conditionText = condition.Code.Coding[0].Display;
                }
                else
                {
                    conditionText = condition.Code.Text;
                }
                Conditions.Add(conditionText);
            }

            foreach (var e in medicationBundle.Entry)
            {
                try
                {
                    var medication = (Medication)e.Resource;
                    var medicationText = "";
                    if (medication.Code != null)
                    {
                        if (medication.Code.Coding.Count > 0)
                        {
                            medicationText = medication.Code.Coding[0].Display;
                        }
                        else if (medication.Code.Text != null)
                        {
                            medicationText = medication.Code.Text;
                        }
                    }

                    Medications.Add(medicationText);
                }
                catch (InvalidCastException)
                {
                    Console.WriteLine($"Could not cast {e.FullUrl} to medication.");
                }
            }

            foreach (var e in observationBundle.Entry)
            {
                Observations.Add(((Observation)e.Resource).Text.Div);
            }

            foreach (var e in encounterBundle.Entry)
            {
                Encounters.Add(((Encounter)e.Resource).Text.Div);
            }
        }
    }
}

// todo: add dates and times to all observations