﻿using System;
using System.Collections.Generic;
using System.Text;

namespace ARPEGOS.Services.Interfaces
{
    public interface IPathService
    {
        string InternalFolder { get; }
        string PublicExternalFolder { get; }
        string PrivateExternalFolder { get; }
    }
}
