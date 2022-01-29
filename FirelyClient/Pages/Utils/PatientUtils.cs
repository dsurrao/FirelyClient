using System;
using Hl7.Fhir.Model;

namespace FirelyClient.Pages.Utils
{
    public class PatientUtils
    {
        public static string GetPatientDescription(Patient patient)
        {
            string name = "", gender = "", age = "", dob = "";

            if (patient.Name.Count > 0)
            {
                if (patient.Name[0].Family != null)
                {
                    name += patient.Name[0].Family;
                }

                if (!name.Equals("")) name += ", ";

                foreach (var given in patient.Name[0].Given)
                {
                    name += given + " ";
                }
            }

            gender = patient.Gender.ToString();

            try
            {
                var birthDate = DateTime.Parse(patient.BirthDate);
                dob = birthDate.ToShortDateString();
                age = $"{CalculateAge(birthDate)} y";
            }
            catch (FormatException)
            {
                Console.WriteLine("Unable to parse '{0}'", patient.BirthDate);
            }
            catch (ArgumentNullException)
            {
                Console.WriteLine("Birth date is null");
            }

            return $"{name} ({gender}, {age}, {dob})";
        }

        public static int CalculateAge(DateTime dateTime)
        {
            DateTime currentDate = DateTime.Now;
            int age = currentDate.Year - dateTime.Year;

            if (age > 0)
            {
                if (currentDate.Month == dateTime.Month)
                {
                    if (currentDate.Day < dateTime.Day)
                    {
                        age--;
                    }
                }
                else if (currentDate.Month < dateTime.Month)
                {
                    age--;
                }
            }

            return age;
        }
    }
}
