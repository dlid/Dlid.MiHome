using System;
using System.Collections.Generic;
using System.Text;

namespace Dlid.MiHome.Vacuum
{
    public enum VacuumState
    {
        Unknown = 0,
        Initiating = 1,
        Sleeping = 2,
        Waiting = 3,
        Cleaning = 5,
        ReturningHome = 6,
        RemoteControl = 7,
        Charging = 8,
        ChargingError = 9,
        Pause = 10,
        SpotCleaning = 11,
        InError = 12,
        ShuttingDown = 13,
        Updating = 14,
        Docking = 15,
        GoTo = 16,
        ZoneCleaning = 17,
        RoomCleaning = 18,
        Full = 100
    }
}
