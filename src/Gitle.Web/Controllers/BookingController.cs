﻿using Gitle.Model;
using NHibernate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using NHibernate.Linq;

namespace Gitle.Web.Controllers
{
    using Castle.MonoRail.Framework;
    using Helpers;

    public class BookingController : SecureController
    {
        public BookingController(ISessionFactory sessionFactory) : base(sessionFactory)
        {
        }

        [BookHours]
        public void Index()
        {
            Index(DateTime.Today);
        }

        [BookHours]
        public void Index(DateTime date)
        {
            var bookings = session.Query<Booking>()
                .Where(x => x.IsActive && x.User == CurrentUser && x.Date > DateTime.Today.AddDays(-14))
                .OrderByDescending(x => x.Date)
                .GroupBy(x => x.Date.Date)
                .ToDictionary(g => new { date = g.Key, total = g.ToList().Sum(x => x.Minutes) }, g => g.ToList());
            PropertyBag.Add("bookings", bookings);
            PropertyBag.Add("today", date.ToString("dd-MM-yyyy"));
            PropertyBag.Add("admins", session.Query<User>().Where(x => x.IsActive && x.IsAdmin));
        }

        [BookHours]
        public void Overview()
        {
            var bookingsByDate = session.Query<Booking>()
                .Where(x => x.IsActive)
                .OrderByDescending(x => x.Date)
                .GroupBy(x => x.Date.Date);
            //.ToDictionary(g => new { date = g.Key, total = g.ToList().Sum(x => x.Minutes) }, g => g.ToList());
            var dayList = new List<BookingDay>();
            foreach (var bookingDateGroup in bookingsByDate)
            {
                var bookingDay = new BookingDay { Day = bookingDateGroup.Key, Users = new List<BookingUser>() };
                var bookingsByUser = bookingDateGroup.GroupBy(x => x.User);
                foreach (var bookingUserGroup in bookingsByUser)
                {
                    var bookingUser = new BookingUser
                    {
                        User = bookingUserGroup.Key,
                        Bookings = bookingUserGroup.ToList()
                    };
                    bookingDay.Users.Add(bookingUser);
                }
                dayList.Add(bookingDay);
            }
            PropertyBag.Add("dayList", dayList);
        }

        [BookHours]
        [return: JSONReturnBinder]
        public object CheckBookingHours(long issueId, double minutes)
        {
            CancelView();
            var currentIssue = session.Get<Issue>(issueId);
            var allBookings = session.Query<Booking>().Where(x => x.Issue.Id == currentIssue.Id && x.IsActive);
            var totalMinutes = 0.0;
            var availableMinutes = currentIssue.Hours * 60;

            foreach (var allBooking in allBookings)
            {
                totalMinutes = allBooking.Minutes + totalMinutes;
            }

            return new { value = !(availableMinutes > 0) || !(totalMinutes + minutes > availableMinutes) };
        }

        [BookHours]
        public void MoveBookings(int[] selectedBookings, DateTime moveDate)
        {
            using (var transaction = session.BeginTransaction())
            {
                foreach (var bookingId in selectedBookings)
                {
                    var booking = session.Get<Booking>(Convert.ToInt64(bookingId));
                    booking.Date = moveDate;

                    session.SaveOrUpdate(booking);
                }

                transaction.Commit();
            }

            RedirectToReferrer();
        }

        [BookHours]
        public void Save(int adminId = 0)
        {
            var booking = BindObject<Booking>("booking");
            
            if (adminId > 0)
            {
                if (CurrentUser.IsDanielle)
                {
                    booking.User = session.Query<User>()
                        .FirstOrDefault(x => x.IsActive && x.Id == adminId && x.IsAdmin);
                }
                else
                {
                    return;
                }
            }
            else
            {
                booking.User = CurrentUser;
            }
            if (!CurrentUser.IsAdmin && !CurrentUser.Projects.Select(up => up.Id).Contains(booking.Project.Id))
            {
                RedirectToAction("index", new { date = booking.Date.ToShortDateString() });
                return;
            }
            if (booking.Issue.Id == 0 || booking.Project.Id == 0) //project moet nu ook verplicht zijn
            {
                //issue is verplicht!
                RedirectToAction("index", new { date = booking.Date.ToShortDateString() });
                return;
                //booking.Issue = null;
            }

            using (var transaction = session.BeginTransaction())
            {
                session.SaveOrUpdate(booking);
                transaction.Commit();
            }
            RedirectToAction("index", new { date = booking.Date.ToShortDateString() });
        }

        [BookHours]
        public void Save(int id, int projectId, int issueId)
        {
            var booking = session.Query<Booking>().FirstOrDefault(x => x.IsActive && x.Id == id);

            if (booking != null)
            {
                BindObjectInstance(booking, "booking");
            }
            if (issueId == 0)
            {
                booking.Issue = null;
            }
            else
            {
                booking.Issue = session.Query<Issue>().FirstOrDefault(x => x.IsActive && x.Id == issueId);
            }

            booking.Project = session.Query<Project>().FirstOrDefault(x => x.IsActive && x.Id == projectId);


            using (var transaction = session.BeginTransaction())
            {
                session.SaveOrUpdate(booking);
                transaction.Commit();
            }
            RedirectToReferrer();
        }
        [BookHours]
        public void Delete(int id)
        {
            var booking = session.Query<Booking>().FirstOrDefault(x => x.IsActive && x.Id == id && !x.InvoiceLines.Any());
            if (booking != null)
            {
                booking.Deactivate();

                using (var transaction = session.BeginTransaction())
                {
                    session.SaveOrUpdate(booking);
                    transaction.Commit();
                }
            }
            RedirectToReferrer();
        }
        [BookHours]
        public void Edit(int id)
        {
            var booking = session.Query<Booking>().FirstOrDefault(x => x.IsActive && x.Id == id); //TODO: admin moet ook kunnen ophalen = andere user
            if (booking == null)
            {
                RedirectToReferrer();
            }
            else
            {
                var suggestion = new Suggestion(booking.Project.Name, booking.Project.Id.ToString());
                PropertyBag.Add("booking", booking);
                PropertyBag.Add("suggestion", suggestion);
                RenderView("_row");
                CancelLayout();
            }
        }

    }

    public class BookingDay
    {
        public DateTime Day;
        public IList<BookingUser> Users;
        public double Minutes { get { return Users.Sum(u => u.Bookings.Sum(x => x.Minutes)); } }
    }

    public class BookingUser
    {
        public User User;
        public IList<Booking> Bookings;
        public double Minutes { get { return Bookings.Sum(x => x.Minutes); } }
    }
}