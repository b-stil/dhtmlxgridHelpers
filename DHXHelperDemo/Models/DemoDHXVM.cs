using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using DHXHelperDemo.Code.DHX;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace DHXHelperDemo.Models
{
    public class DemoDHXVM
    {
        [DisplayName("Name")]
        [DHXGridColumn(ColumnWidth = 160, DisplayOrder = 1)]
        public string Name { get; set; }

        [DisplayName("Position")]
        [DHXGridColumn(ColumnWidth = 160, DisplayOrder = 2)]
        public string Position { get; set; }

        [DisplayName("Salary")]
        [DHXGridColumn(ColumnWidth = 100, DisplayOrder = 3)]
        public string Salary { get; set; }

        //Json attribute only here to help with reading from file.
        [JsonProperty(PropertyName = "start_date")]
        [DisplayName("Start Date")]
        [DHXGridColumn(ColumnWidth = 100, DisplayOrder = 4)]
        public string StartDate { get; set; }

        [DisplayName("Office")]
        [DHXGridColumn(ColumnWidth = 225, DisplayOrder = 5)]
        public string Office { get; set; }

        [DisplayName("Ext")]
        [DHXGridColumn(ColumnWidth = 60, DisplayOrder = 6)]
        public string Extension { get; set; }

    }
}