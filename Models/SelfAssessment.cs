using System;
using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.Configuration.Attributes;
using Microsoft.CodeAnalysis.Elfie.Serialization;
using SatApiTest.Controllers;
using SatApiTest.Models;

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

        internal static SelfAssessment FromCsv(string selfAssessmentCsv)
        {
            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = false,
            };

            using (var reader = new StringReader(selfAssessmentCsv))
            using (var csvReader = new CsvHelper.CsvReader(reader, config))
            {
                csvReader.Context.RegisterClassMap<SelfAssessmentMap>();
                var selfAssessments = csvReader.GetRecords<SelfAssessment>();
                return selfAssessments.FirstOrDefault();
            }

            
        }
    }

    public sealed class SelfAssessmentMap : ClassMap<SelfAssessment>
    {
        public SelfAssessmentMap()
        {
            Map(m => m.Id).Index(0);
            Map(m => m.Created).Index(1);
            Map(m => m.Institution).Index(2);
            Map(m => m.Domain1Competency1).Index(3);
            Map(m => m.Domain1Competency2).Index(4);
            Map(m => m.Domain1Competency3).Index(5);
            Map(m => m.Domain1Competency4).Index(6);
            Map(m => m.Domain1Competency5).Index(7);
            Map(m => m.Domain1Competency6).Index(8);
            Map(m => m.Domain2Competency1).Index(9);
            Map(m => m.Domain2Competency2).Index(10);
            Map(m => m.Domain2Competency3).Index(11);
            Map(m => m.Domain2Competency4).Index(12);
            Map(m => m.Domain3Competency1).Index(13);
            Map(m => m.Domain3Competency2).Index(14);
            Map(m => m.Domain3Competency3).Index(15);
            Map(m => m.Domain3Competency4).Index(16);
            Map(m => m.Domain4Competency1).Index(17);
            Map(m => m.Domain4Competency2).Index(18);
            Map(m => m.Domain4Competency3).Index(19);
            Map(m => m.Domain4Competency4).Index(20);
            Map(m => m.Domain4Competency5).Index(21);
            Map(m => m.Domain5Competency1).Index(22);
            Map(m => m.Domain5Competency2).Index(23);
            Map(m => m.Domain5Competency3).Index(24);
            Map(m => m.Domain5Competency4).Index(25);
            Map(m => m.Domain1Competency1Important).Index(26);
            Map(m => m.Domain1Competency2Important).Index(27);
            Map(m => m.Domain1Competency3Important).Index(28);
            Map(m => m.Domain1Competency4Important).Index(29);
            Map(m => m.Domain1Competency5Important).Index(30);
            Map(m => m.Domain1Competency6Important).Index(31);
            Map(m => m.Domain2Competency1Important).Index(32);
            Map(m => m.Domain2Competency2Important).Index(33);
            Map(m => m.Domain2Competency3Important).Index(34);
            Map(m => m.Domain2Competency4Important).Index(35);
            Map(m => m.Domain3Competency1Important).Index(36);
            Map(m => m.Domain3Competency2Important).Index(37);
            Map(m => m.Domain3Competency3Important).Index(38);
            Map(m => m.Domain3Competency4Important).Index(39);
            Map(m => m.Domain4Competency1Important).Index(40);
            Map(m => m.Domain4Competency2Important).Index(41);
            Map(m => m.Domain4Competency3Important).Index(42);
            Map(m => m.Domain4Competency4Important).Index(43);
            Map(m => m.Domain4Competency5Important).Index(44);
            Map(m => m.Domain5Competency1Important).Index(45);
            Map(m => m.Domain5Competency2Important).Index(46);
            Map(m => m.Domain5Competency3Important).Index(47);
            Map(m => m.Domain5Competency4Important).Index(48);
        }
    }
}