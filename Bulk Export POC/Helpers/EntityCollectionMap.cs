using LeapPatientService.Application.Models;
using LeapPatientService.Application.Models.Entities;
using LeapPatientService.Application.Models.Entities.Common;
using Newtonsoft.Json;

namespace Bulk_Export_POC.Helpers
{
    public static class EntityCollectionMap
    {
        public static readonly IReadOnlyDictionary<ResourceType, Func<EntityCollection, List<BaseEntity>>> Dictionary =
            new Dictionary<ResourceType, Func<EntityCollection, List<BaseEntity>>>
            {
                { ResourceType.Appointments, e => ((AppointmentCollection)e).Appointments.Cast<BaseEntity>().ToList() },
                { ResourceType.Patient, e => ((PatientCollection)e).Patients.Cast<BaseEntity>().ToList() },
                { ResourceType.AllergyIntolerance, e => ((AllergyIntoleranceCollection)e).AllergyIntolerances.Cast<BaseEntity>().ToList() },
                { ResourceType.Organization, e => ((OrganizationCollection)e).Organizations.Cast<BaseEntity>().ToList() },
                { ResourceType.Practitioner, e => ((PractitionerCollection)e).Practitioners.Cast<BaseEntity>().ToList() },
                { ResourceType.Location, e => ((LocationCollection)e).Locations.Cast<BaseEntity>().ToList() },
                { ResourceType.Condition, e => ((ConditionCollection)e).Conditions.Cast<BaseEntity>().ToList() },
                { ResourceType.Goals, e => ((GoalCollection)e).Goals.Cast<BaseEntity>().ToList() },
                { ResourceType.BodyWeight, e => ((BodyWeightCollection)e).BodyWeights.Cast<BaseEntity>().ToList() },
                { ResourceType.ImplantableDevice, e => ((ImplantableDeviceCollection)e).ImplantableDevices.Cast<BaseEntity>().ToList() },
                { ResourceType.BodyTemperature, e => ((BodyTemperatureCollection)e).BodyTemperatures.Cast<BaseEntity>().ToList() },
                { ResourceType.RespiratoryRate, e => ((RespiratoryRateCollection)e).RespiratoryRates.Cast<BaseEntity>().ToList() },
                { ResourceType.BloodPressure, e => ((BloodPressureCollection)e).BloodPressures.Cast<BaseEntity>().ToList() },
                { ResourceType.HeartRate, e => ((HeartRateCollection)e).HeartRates.Cast<BaseEntity>().ToList() },
                { ResourceType.PulseOximetry, e => ((PulseOximetryCollection)e).PulseOximetries.Cast<BaseEntity>().ToList() },
                { ResourceType.BodyHeight, e => ((BodyHeightCollection)e).BodyHeights.Cast<BaseEntity>().ToList() },
                { ResourceType.HeadCircumference, e => ((HeadCircumferenceCollection)e).HeadCircumferences.Cast<BaseEntity>().ToList() },
                { ResourceType.Bmi, e => ((BMICollection)e).BMIs.Cast<BaseEntity>().ToList() },
                { ResourceType.Coverage, e => ((CoverageCollection)e).Coverages.Cast<BaseEntity>().ToList() },
                { ResourceType.LabObservation, e => ((LabObservationCollection)e).LabObservations.Cast<BaseEntity>().ToList() },
                { ResourceType.Immunization, e => ((ImmunizationCollection)e).Immunizations.Cast<BaseEntity>().ToList() },
                { ResourceType.Encounter, e => ((EncounterCollection)e).Encounters.Cast<BaseEntity>().ToList() },
                { ResourceType.CarePlan, e => ((CarePlanCollection)e).CarePlans.Cast<BaseEntity>().ToList() },
                { ResourceType.MedicationRequest, e => ((MedicationRequestCollection)e).MedicationRequests.Cast<BaseEntity>().ToList() },
                { ResourceType.SmokingStatus, e => ((SmokingStatusCollection)e).SmokingStatuses.Cast<BaseEntity>().ToList() },
                { ResourceType.Procedure, e => ((ProcedureCollection)e).Procedures.Cast<BaseEntity>().ToList() },
                { ResourceType.CareTeam, e => ((CareTeamCollection)e).CareTeams.Cast<BaseEntity>().ToList() },
                { ResourceType.WeightForLengthPercentile, e => ((WeightForLengthPercentileCollection)e).WeightForLengthPercentiles.Cast<BaseEntity>().ToList() },
                { ResourceType.OFCPercentile, e => ((OfcPercentileCollection)e).OfcPercentiles.Cast<BaseEntity>().ToList() },
                { ResourceType.PediametricBmiAgeObservation, e => ((PediametricBmiAgeObservationCollection)e).PediametricBmiAgeObservations.Cast<BaseEntity>().ToList() },
                { ResourceType.DiagnosticReportForLabortaryResult, e => ((DiagnosticReportForLabortaryResultCollection)e).DiagnosticReportForLabortaryResults.Cast<BaseEntity>().ToList() },
                { ResourceType.DiagnosticReportForNoteExchange, e => ((DiagnosticReportForNoteExchangeCollection)e).DiagnosticReportForNoteExchanges.Cast<BaseEntity>().ToList() },
                { ResourceType.Medication, e => ((MedicationCollection)e).Medications.Cast<BaseEntity>().ToList() },
                { ResourceType.DiagnosticDocumentReference, e => ((DiagnosticDocumentReferenceCollection)e).DiagnosticDocumentReferences.Cast<BaseEntity>().ToList() },
                { ResourceType.EncounterDocumentReference, e => ((EncounterDocumentReferenceCollection)e).EncounterDocumentReferences.Cast<BaseEntity>().ToList() },
                { ResourceType.Occupation, e => ((OccupationCollection)e).Occupations.Cast<BaseEntity>().ToList() },
                { ResourceType.ServiceRequest, e => ((ServiceRequestCollection)e).ServiceRequests.Cast<BaseEntity>().ToList() },
                { ResourceType.PregnancyStatus, e => ((PregnancyStatusCollection)e).PregnancyStatuses.Cast<BaseEntity>().ToList() },
                { ResourceType.TravelHistory, e => ((TravelHistoryCollection)e).TravelHistories.Cast<BaseEntity>().ToList() },
                { ResourceType.Education, e => ((EducationCollection)e).Educations.Cast<BaseEntity>().ToList() }
            };
       
        public static readonly IReadOnlyDictionary<ResourceType, Func<BaseEntity, string>> BaseEntityDictionary =
            new Dictionary<ResourceType, Func<BaseEntity, string>>
            {
                { ResourceType.Appointments, e => JsonConvert.SerializeObject((Appointment)e) },
                { ResourceType.Patient, e => JsonConvert.SerializeObject((Patient)e) },
                { ResourceType.AllergyIntolerance, e => JsonConvert.SerializeObject((AllergyIntolerance)e) },
                { ResourceType.Organization, e => JsonConvert.SerializeObject((Organization)e) },
                { ResourceType.Practitioner, e => JsonConvert.SerializeObject((Practitioner)e) },
                { ResourceType.Location, e => JsonConvert.SerializeObject((Location)e) },
                { ResourceType.Condition, e => JsonConvert.SerializeObject((Condition)e) },
                { ResourceType.Goals, e => JsonConvert.SerializeObject((Goal)e) },
                { ResourceType.BodyWeight, e => JsonConvert.SerializeObject((BodyWeight)e) },
                { ResourceType.ImplantableDevice, e => JsonConvert.SerializeObject((ImplantableDevice)e) },
                { ResourceType.BodyTemperature, e => JsonConvert.SerializeObject((BodyTemperature)e) },
                { ResourceType.RespiratoryRate, e => JsonConvert.SerializeObject((RespiratoryRate)e) },
                { ResourceType.BloodPressure, e => JsonConvert.SerializeObject((BloodPressure)e) },
                { ResourceType.HeartRate, e => JsonConvert.SerializeObject((HeartRate)e) },
                { ResourceType.PulseOximetry, e => JsonConvert.SerializeObject((PulseOximetry)e) },
                { ResourceType.BodyHeight, e => JsonConvert.SerializeObject((BodyHeight)e) },
                { ResourceType.HeadCircumference, e => JsonConvert.SerializeObject((HeadCircumference)e) },
                { ResourceType.Bmi, e => JsonConvert.SerializeObject((BMI)e) },
                { ResourceType.Coverage, e => JsonConvert.SerializeObject((Coverage)e) },
                { ResourceType.LabObservation, e => JsonConvert.SerializeObject((LabObservation)e) },
                { ResourceType.Immunization, e => JsonConvert.SerializeObject((Immunization)e) },
                { ResourceType.Encounter, e => JsonConvert.SerializeObject((Encounter)e) },
                { ResourceType.CarePlan, e => JsonConvert.SerializeObject((CarePlan)e) },
                { ResourceType.MedicationRequest, e => JsonConvert.SerializeObject((MedicationRequest)e) },
                { ResourceType.SmokingStatus, e => JsonConvert.SerializeObject((SmokingStatus)e) },
                { ResourceType.Procedure, e => JsonConvert.SerializeObject((Procedure)e) },
                { ResourceType.CareTeam, e => JsonConvert.SerializeObject((CareTeam)e) },
                { ResourceType.WeightForLengthPercentile, e => JsonConvert.SerializeObject((WeightForLengthPercentile)e) },
                { ResourceType.OFCPercentile, e => JsonConvert.SerializeObject((OfcPercentile)e) },
                { ResourceType.PediametricBmiAgeObservation, e => JsonConvert.SerializeObject((PediametricBmiAgeObservation)e) },
                { ResourceType.DiagnosticReportForLabortaryResult, e => JsonConvert.SerializeObject((DiagnosticReportForLabortaryResult)e) },
                { ResourceType.DiagnosticReportForNoteExchange, e => JsonConvert.SerializeObject((DiagnosticReportForNoteExchange)e) },
                { ResourceType.Medication, e => JsonConvert.SerializeObject((Medication)e) },
                { ResourceType.DiagnosticDocumentReference, e => JsonConvert.SerializeObject((DiagnosticDocumentReference)e) },
                { ResourceType.EncounterDocumentReference, e => JsonConvert.SerializeObject((EncounterDocumentReference)e) },
                { ResourceType.Occupation, e => JsonConvert.SerializeObject((Occupation)e) },
                { ResourceType.ServiceRequest, e => JsonConvert.SerializeObject((ServiceRequest)e) },
                { ResourceType.PregnancyStatus, e => JsonConvert.SerializeObject((PregnancyStatus)e) },
                { ResourceType.TravelHistory, e => JsonConvert.SerializeObject((TravelHistory)e) },
                { ResourceType.Education, e => JsonConvert.SerializeObject((Education)e) }
            };
    }
}
