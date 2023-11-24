using System;
using CsvHelper.Configuration;
using CsvHelper.Configuration.Attributes;

namespace SatApiTest.Models
{

    public class SelfAssessment
    {
        [Name("Id")]
        public Guid Id { get; set; }
        [Name("Created")]
        public DateTime Created { get; set; }

        public SelfAssessment()
        {
            Id = Guid.NewGuid();
            Created = DateTime.UtcNow;
        }
        [Name("Institution")]
        public string Institution { get; set; }
        public int Domain1Competency1 { get; set; }
        public int Domain1Competency2 { get; set; }
        public int Domain1Competency3 { get; set; }
        public int Domain1Competency4 { get; set; }
        public int Domain1Competency5 { get; set; }
        public int Domain1Competency6 { get; set; }
        public bool Domain1Competency1Important { get; set; }
        public bool Domain1Competency2Important { get; set; }
        public bool Domain1Competency3Important { get; set; }
        public bool Domain1Competency4Important { get; set; }
        public bool Domain1Competency5Important { get; set; }
        public bool Domain1Competency6Important { get; set; }

        public int Domain2Competency1 { get; set; }
        public int Domain2Competency2 { get; set; }
        public int Domain2Competency3 { get; set; }
        public int Domain2Competency4 { get; set; }
        public bool Domain2Competency1Important { get; set; }
        public bool Domain2Competency2Important { get; set; }
        public bool Domain2Competency3Important { get; set; }
        public bool Domain2Competency4Important { get; set; }


        public int Domain3Competency1 { get; set; }
        public int Domain3Competency2 { get; set; }
        public int Domain3Competency3 { get; set; }
        public int Domain3Competency4 { get; set; }

        public bool Domain3Competency1Important { get; set; }
        public bool Domain3Competency2Important { get; set; }
        public bool Domain3Competency3Important { get; set; }
        public bool Domain3Competency4Important { get; set; }


        public int Domain4Competency1 { get; set; }
        public int Domain4Competency2 { get; set; }
        public int Domain4Competency3 { get; set; }
        public int Domain4Competency4 { get; set; }
        public int Domain4Competency5 { get; set; }
        public bool Domain4Competency1Important { get; set; }
        public bool Domain4Competency2Important { get; set; }
        public bool Domain4Competency3Important { get; set; }
        public bool Domain4Competency4Important { get; set; }
        public bool Domain4Competency5Important { get; set; }


        public int Domain5Competency1 { get; set; }
        public int Domain5Competency2 { get; set; }
        public int Domain5Competency3 { get; set; }
        public int Domain5Competency4 { get; set; }
        public bool Domain5Competency1Important { get; set; }
        public bool Domain5Competency2Important { get; set; }
        public bool Domain5Competency3Important { get; set; }
        public bool Domain5Competency4Important { get; set; }

    }

    public sealed class SelfAssessmentMap : ClassMap<SelfAssessment>
    {
        public SelfAssessmentMap()
        {
            Map(m => m.Id);
            Map(m => m.Created);
            Map(m => m.Institution);
            Map(m => m.Domain1Competency1);
            Map(m => m.Domain1Competency2);
            Map(m => m.Domain1Competency3);
            Map(m => m.Domain1Competency4);
            Map(m => m.Domain1Competency5);
            Map(m => m.Domain1Competency6);
            Map(m => m.Domain2Competency1);
            Map(m => m.Domain2Competency2);
            Map(m => m.Domain2Competency3);
            Map(m => m.Domain2Competency4);
            Map(m => m.Domain3Competency1);
            Map(m => m.Domain3Competency2);
            Map(m => m.Domain3Competency3);
            Map(m => m.Domain3Competency4);
            Map(m => m.Domain4Competency1);
            Map(m => m.Domain4Competency2);
            Map(m => m.Domain4Competency3);
            Map(m => m.Domain4Competency4);
            Map(m => m.Domain4Competency5);
            Map(m => m.Domain5Competency1);
            Map(m => m.Domain5Competency2);
            Map(m => m.Domain5Competency3);
            Map(m => m.Domain5Competency4);
            Map(m => m.Domain1Competency1Important);
            Map(m => m.Domain1Competency2Important);
            Map(m => m.Domain1Competency3Important);
            Map(m => m.Domain1Competency4Important);
            Map(m => m.Domain1Competency5Important);
            Map(m => m.Domain1Competency6Important);
            Map(m => m.Domain2Competency1Important);
            Map(m => m.Domain2Competency2Important);
            Map(m => m.Domain2Competency3Important);
            Map(m => m.Domain2Competency4Important);
            Map(m => m.Domain3Competency1Important);
            Map(m => m.Domain3Competency2Important);
            Map(m => m.Domain3Competency3Important);
            Map(m => m.Domain3Competency4Important);
            Map(m => m.Domain4Competency1Important);
            Map(m => m.Domain4Competency2Important);
            Map(m => m.Domain4Competency3Important);
            Map(m => m.Domain4Competency4Important);
            Map(m => m.Domain4Competency5Important);
            Map(m => m.Domain5Competency1Important);
            Map(m => m.Domain5Competency2Important);
            Map(m => m.Domain5Competency3Important);
            Map(m => m.Domain5Competency4Important);
        }
    }
}