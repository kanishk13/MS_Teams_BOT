﻿//----------------------------------------------------------------------------
// <copyright company="Microsoft Corporation" file="WorkerStatus.cs">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
//----------------------------------------------------------------------------

namespace MS.GTA.BOTService.Common.Enum
{
    using System.Runtime.Serialization;

    [DataContract]
    public enum WorkerStatus
    {
        Active = 0,
        Inactive = 1
    }
}