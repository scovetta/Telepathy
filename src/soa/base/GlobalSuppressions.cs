//------------------------------------------------------------------------------
// <copyright file="GlobalSuppressions.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//      Suppression of FxCop warning messages
// </summary>
//------------------------------------------------------------------------------

[module: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields", Scope="member", Target="Microsoft.Hpc.RuntimeTrace.EventProviderVersionTwo+EventData.#Reserved", Justification="It is reserved for future use")]
[module: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields", Scope="member", Target="Microsoft.Hpc.RuntimeTrace.EventProviderVersionTwo+EventData.#DataPointer", Justification="The warning is actually incorrect as this field is being used. Seems that FxCop has issues when it comes to unsafe code")]
[module: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields", Scope="member", Target="Microsoft.Hpc.RuntimeTrace.EventProviderVersionTwo+EventData.#Size", Justification="The warning is actually incorrect as this field is being used. Seems that FxCop has issues when it comes to unsafe code")]


