namespace Gitle.Model.Interfaces.Service
{
    using System.Collections;
    using System.Collections.Generic;

    public interface IBookingQueryParserService
    {
        IList<Booking> GetBookingsByQuery(string query);
    }
}