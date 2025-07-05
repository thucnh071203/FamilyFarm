using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FamilyFarm.DataAccess.DAOs;
using FamilyFarm.Models.Models;
using FamilyFarm.Repositories.Interfaces;

namespace FamilyFarm.Repositories.Implementations
{
    public class PaymentRepository : IPaymentRepository
    {
        private readonly PaymentDAO _dao;
        public PaymentRepository(PaymentDAO dao)
        {
            _dao = dao;
        }
        public async Task<List<PaymentTransaction>> GetAllPayment()
        {
            return await _dao.GetAllAsync();
        }

        public async Task<PaymentTransaction> GetPayment(string id)
        {
            return await _dao.GetByIdAsync(id);
        }

        public async Task<PaymentTransaction> CreatePayment(PaymentTransaction payment)
        {
            return await _dao.CreateAsync(payment);
        }
    }
}
