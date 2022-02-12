using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Hl7.Fhir.Model;
using Hl7.Fhir.Rest;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using static Hl7.Fhir.Model.Observation;

namespace FirelyClient.Pages.Observations
{
    public class BloodPressureModel : PageModel
    {
        private readonly FhirClient _client;

        public BloodPressureModel(IConfiguration configuration)
        {
            _client = new FhirClient(configuration["FHIRServer"],
                new FhirClientSettings
                {
                    PreferredFormat = ResourceFormat.Json,
                    VerifyFhirVersion = true,
                    PreferredReturn = Prefer.ReturnMinimal
                });
        }

        public class BloodPressureFormModel
        {
            [Required]
            public string PatientId { get; set; }
            [Required]
            public int Systolic { get; set; }
            [Required]
            public int Diastolic { get; set; }
        }

        [BindProperty]
        public BloodPressureFormModel Model { get; set; }

        public ActionResult OnGet()
        {
            Model = new BloodPressureFormModel();
            Model.PatientId = Request.Query["PatientId"];

            // https://server.fire.ly/Observation/1203ccc6-8e25-4a66-adcc-e4875bc05a17
            //var bp = _client.Read<Observation>("Observation/1203ccc6-8e25-4a66-adcc-e4875bc05a17");

            return Page();
        }

        public async Task<ActionResult> OnPostAsync()
        {
            var bp = new Observation
            {
                //BodySite = new CodeableConcept
                //{
                //    Coding = new List<Coding>
                //    {
                //        new Coding
                //        {
                //            Display = "Right arm",
                //            System = "http://snomed.info/sct"
                //        }
                //    }
                //},
                Code = new CodeableConcept
                {
                    Coding = new List<Coding>
                    {
                        new Coding
                        {
                            Code = "85354-9",
                            System = "http://loinc.org",
                            Display = "Blood pressure panel with all children optional"
                        }
                    }
                },
                Category = new List<CodeableConcept>
                {
                    new CodeableConcept
                    {
                        Coding = new List<Coding>
                        {
                            new Coding
                            {
                                Code = "vital-signs",
                                System = "http://terminology.hl7.org/CodeSystem/observation-category"
                            }
                        }
                    }
                },
                Component = new List<ComponentComponent>
                {
                    new ComponentComponent
                    {
                        Code = new CodeableConcept
                        {
                            Coding = new List<Coding>
                            {
                                new Coding
                                {
                                    Code = "8480-6",
                                    Display = "Systolic blood pressure",
                                    System = "http://loinc.org"
                                }
                            }
                        },
                        Value = new Quantity
                        {
                            Code = "mm[Hg]",
                            System = "http://unitsofmeasure.org",
                            Unit = "mmHg",
                            Value = Model.Systolic
                        }
                    },
                    new ComponentComponent
                    {
                        Code = new CodeableConcept
                        {
                            Coding = new List<Coding>
                            {
                                new Coding
                                {
                                    Code = "8462-4",
                                    Display = "Diastolic blood pressure",
                                    System = "http://loinc.org"
                                }
                            }
                        },
                        Value = new Quantity
                        {
                            Code = "mm[Hg]",
                            System = "http://unitsofmeasure.org",
                            Unit = "mmHg",
                            Value = Model.Diastolic
                        }
                    }
                },
                Effective = new FhirDateTime(DateTimeOffset.Now),
                Performer = new List<ResourceReference>
                {
                    new ResourceReference
                    {
                        Url = new Uri("https://server.fire.ly/Practitioner/b34fa134-4512-4008-afa3-492a1a1b0f9a")
                    }
                },
                Subject = new ResourceReference
                {
                    Url = new Uri($"https://server.fire.ly/Patient/{Model.PatientId}")
                },
                Status = ObservationStatus.Final,
                Text = new Narrative
                {
                    Status = Narrative.NarrativeStatus.Generated,
                    Div = $"<div xmlns=\"http://www.w3.org/1999/xhtml\"><pre>Blood Presssure<br/>Systolic: {Model.Systolic.ToString()} <br/>Diastolic: {Model.Diastolic.ToString()}</pre></div>"
                }
            };

            try
            {
                var savedBp = await _client.CreateAsync<Observation>(bp);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            return RedirectToPage("/Patient", new RouteValueDictionary {
                { "id", Model.PatientId } });
        }
    }
}

/*
 * {
  "resourceType": "Observation",
  "id": "blood-pressure",
  "meta": {
    "profile": [
      "http://hl7.org/fhir/StructureDefinition/vitalsigns"
    ]
  },
  "text": {
    "status": "generated",
    "div": "<div xmlns=\"http://www.w3.org/1999/xhtml\"><p><b>Generated Narrative with Details</b></p><p><b>id</b>: blood-pressure</p><p><b>meta</b>: </p><p><b>identifier</b>: urn:uuid:187e0c12-8dd2-67e2-99b2-bf273c878281</p><p><b>basedOn</b>: </p><p><b>status</b>: final</p><p><b>category</b>: Vital Signs <span>(Details : {http://terminology.hl7.org/CodeSystem/observation-category code 'vital-signs' = 'Vital Signs', given as 'Vital Signs'})</span></p><p><b>code</b>: Blood pressure systolic &amp; diastolic <span>(Details : {LOINC code '85354-9' = 'Blood pressure panel with all children optional', given as 'Blood pressure panel with all children optional'})</span></p><p><b>subject</b>: <a>Patient/example</a></p><p><b>effective</b>: 17/09/2012</p><p><b>performer</b>: <a>Practitioner/example</a></p><p><b>interpretation</b>: Below low normal <span>(Details : {http://terminology.hl7.org/CodeSystem/v3-ObservationInterpretation code 'L' = 'Low', given as 'low'})</span></p><p><b>bodySite</b>: Right arm <span>(Details : {SNOMED CT code '368209003' = 'Right upper arm', given as 'Right arm'})</span></p><blockquote><p><b>component</b></p><p><b>code</b>: Systolic blood pressure <span>(Details : {LOINC code '8480-6' = 'Systolic blood pressure', given as 'Systolic blood pressure'}; {SNOMED CT code '271649006' = 'Systolic blood pressure', given as 'Systolic blood pressure'}; {http://acme.org/devices/clinical-codes code 'bp-s' = 'bp-s', given as 'Systolic Blood pressure'})</span></p><p><b>value</b>: 107 mmHg<span> (Details: UCUM code mm[Hg] = 'mmHg')</span></p><p><b>interpretation</b>: Normal <span>(Details : {http://terminology.hl7.org/CodeSystem/v3-ObservationInterpretation code 'N' = 'Normal', given as 'normal'})</span></p></blockquote><blockquote><p><b>component</b></p><p><b>code</b>: Diastolic blood pressure <span>(Details : {LOINC code '8462-4' = 'Diastolic blood pressure', given as 'Diastolic blood pressure'})</span></p><p><b>value</b>: 60 mmHg<span> (Details: UCUM code mm[Hg] = 'mmHg')</span></p><p><b>interpretation</b>: Below low normal <span>(Details : {http://terminology.hl7.org/CodeSystem/v3-ObservationInterpretation code 'L' = 'Low', given as 'low'})</span></p></blockquote></div>"
  },
  "identifier": [
    {
      "system": "urn:ietf:rfc:3986",
      "value": "urn:uuid:187e0c12-8dd2-67e2-99b2-bf273c878281"
    }
  ],
  "basedOn": [
    {
      "identifier": {
        "system": "https://acme.org/identifiers",
        "value": "1234"
      }
    }
  ],
  "status": "final",
  "category": [
    {
      "coding": [
        {
          "system": "http://terminology.hl7.org/CodeSystem/observation-category",
          "code": "vital-signs",
          "display": "Vital Signs"
        }
      ]
    }
  ],
  "code": {
    "coding": [
      {
        "system": "http://loinc.org",
        "code": "85354-9",
        "display": "Blood pressure panel with all children optional"
      }
    ],
    "text": "Blood pressure systolic & diastolic"
  },
  "subject": {
    "reference": "Patient/example"
  },
  "effectiveDateTime": "2012-09-17",
  "performer": [
    {
      "reference": "Practitioner/example"
    }
  ],
  "interpretation": [
    {
      "coding": [
        {
          "system": "http://terminology.hl7.org/CodeSystem/v3-ObservationInterpretation",
          "code": "L",
          "display": "low"
        }
      ],
      "text": "Below low normal"
    }
  ],
  "bodySite": {
    "coding": [
      {
        "system": "http://snomed.info/sct",
        "code": "368209003",
        "display": "Right arm"
      }
    ]
  },
  "component": [
    {
      "code": {
        "coding": [
          {
            "system": "http://loinc.org",
            "code": "8480-6",
            "display": "Systolic blood pressure"
          },
          {
            "system": "http://snomed.info/sct",
            "code": "271649006",
            "display": "Systolic blood pressure"
          },
          {
            "system": "http://acme.org/devices/clinical-codes",
            "code": "bp-s",
            "display": "Systolic Blood pressure"
          }
        ]
      },
      "valueQuantity": {
        "value": 107,
        "unit": "mmHg",
        "system": "http://unitsofmeasure.org",
        "code": "mm[Hg]"
      },
      "interpretation": [
        {
          "coding": [
            {
              "system": "http://terminology.hl7.org/CodeSystem/v3-ObservationInterpretation",
              "code": "N",
              "display": "normal"
            }
          ],
          "text": "Normal"
        }
      ]
    },
    {
      "code": {
        "coding": [
          {
            "system": "http://loinc.org",
            "code": "8462-4",
            "display": "Diastolic blood pressure"
          }
        ]
      },
      "valueQuantity": {
        "value": 60,
        "unit": "mmHg",
        "system": "http://unitsofmeasure.org",
        "code": "mm[Hg]"
      },
      "interpretation": [
        {
          "coding": [
            {
              "system": "http://terminology.hl7.org/CodeSystem/v3-ObservationInterpretation",
              "code": "L",
              "display": "low"
            }
          ],
          "text": "Below low normal"
        }
      ]
    }
  ]
}
 */