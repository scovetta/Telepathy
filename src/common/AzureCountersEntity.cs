//-------------------------------------------------------------------------------------------------
// <copyright file="AzureCountersEntity.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// 
// <summary>
//     Defines the entity (row) for the Azure performance counter table
// </summary>
//-------------------------------------------------------------------------------------------------
namespace Microsoft.Hpc.Common.Azure
{
    using System;
    using System.Diagnostics;
    using Microsoft.WindowsAzure.Storage.Table;

    /// <summary>
    /// This class defines the entity for an Azure performance counter
    /// </summary>
    internal class CountersEntity : TableEntity
    {
        public const string CounterValuePropertyNamingFormat = "Counter{0}Value";

        public CountersEntity()
        {
            RowKey = string.Format("{0:10}_{1}", DateTime.MaxValue.Ticks - DateTime.Now.Ticks, Guid.NewGuid());
            Version = 1;
        }

        public int Version { get; set; }

        public double Counter0Value { get; set; }
        public double Counter1Value { get; set; }
        public double Counter2Value { get; set; }
        public double Counter3Value { get; set; }
        public double Counter4Value { get; set; }
        public double Counter5Value { get; set; }
        public double Counter6Value { get; set; }
        public double Counter7Value { get; set; }
        public double Counter8Value { get; set; }
        public double Counter9Value { get; set; }

        public void UpdateValues(double[] values)
        {
            Debug.Assert(values.Length == (int)AzureCounterEnum.AZURE_COUNTER_MAX);

            //
            //  Update counter values
            //
            Counter0Value = values[0];
            Counter1Value = values[1];
            Counter2Value = values[2];
            Counter3Value = values[3];
            Counter4Value = values[4];
            Counter5Value = values[5];
            Counter6Value = values[6];
            Counter7Value = values[7];
            Counter8Value = values[8];
            Counter9Value = values[9];
        }
    }
}