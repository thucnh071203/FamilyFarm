﻿using FamilyFarm.DataAccess.DAOs;
using FamilyFarm.Models.Models;
using FamilyFarm.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyFarm.Repositories.Implementations
{
    public class BookingServiceRepository:IBookingServiceRepository
    {
        private readonly BookingServiceDAO _dao;
        public BookingServiceRepository(BookingServiceDAO dao)
        {
            _dao = dao;
        }
        public async Task<BookingService?> GetById(string id)
        {
            return await _dao.GetById(id);
        }
        public async Task<List<BookingService>?> GetAllBookingByAccid(string id)
        {
            return await _dao.GetAllBookingByAccid(id);
        }
        public async Task<List<BookingService>?> GetListRequestBookingByAccid(string id)
        {
            return await _dao.GetListRequestBookingByAccid(id);
        }
        public async Task<List<BookingService>?> GetAllBookingByServiceId(string id)
        {
            return await _dao.GetAllBookingByServiceId(id);
        }
        public async Task<List<BookingService>?> GetListRequestBookingByServiceId(string id)
        {
            return await _dao.GetListRequestBookingByServiceId(id);
        }
        public async Task Create(BookingService bookingService)
        {
            await _dao.Create(bookingService);
        }
        public async Task UpdateStatus(BookingService bookingService)
        {
            await _dao.UpdateStatus(bookingService);
        }

    }
}
