using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FamilyFarm.BusinessLogic.Interfaces;

namespace FamilyFarm.BusinessLogic.Services
{
    public class RepaymentCacheService : IRepaymentCacheService
    {
        private readonly ConcurrentDictionary<string, string> _cache = new();

        public void SaveAdminId(string txnRef, string adminId)
        {
            Console.WriteLine($"[CACHE] SAVE: {txnRef} → {adminId}");
            _cache[txnRef] = adminId;
        }

        public string? GetAdminId(string txnRef)
        {
            _cache.TryGetValue(txnRef, out var adminId);
            Console.WriteLine($"[CACHE] GET: {txnRef} → {adminId ?? "NOT FOUND"}");
            return adminId;
        }

        public void RemoveAdminId(string txnRef)
        {
            _cache.TryRemove(txnRef, out _);
        }
    }
}
