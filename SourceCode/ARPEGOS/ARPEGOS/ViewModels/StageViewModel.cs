﻿using ARPEGOS.Models;
using ARPEGOS.ViewModels.Base;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace ARPEGOS.ViewModels
{
    public abstract class StageViewModel: BaseViewModel
    {
        public static ObservableCollection<Stage> CreationScheme = null;
        public static int CurrentStep { get; set; }
        public static double GeneralLimit { get; set; }
        public static double GeneralMaximum { get; set; }
        public static double GeneralProgress { get; set; }
        public static string GeneralLimitProperty { get; set; } = null;
        public static string RootStage { get; set; }
        public static bool ApplyOnCharacter { get; set; } = false;
        public static void Reset()
        {
            CreationScheme = null;
            GeneralLimit = 0;
            GeneralMaximum = 0;
            GeneralProgress = 0;
            GeneralLimitProperty = null;
            RootStage = null;
            ApplyOnCharacter = false;
        }
    }
}
