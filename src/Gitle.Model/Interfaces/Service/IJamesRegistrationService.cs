namespace Gitle.Model.Interfaces.Service
{
    using System;

    public interface IJamesRegistrationService
    {
        int GetTotalMinutesForEmployee(int jamesEmployeeId, int year, int week);
        int GetTotalMinutesForEmployee(int jamesEmployeeId, DateTime day);
    }
}