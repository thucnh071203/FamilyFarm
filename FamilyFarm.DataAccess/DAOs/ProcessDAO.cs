﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using FamilyFarm.Models.Models;
using MongoDB.Bson;
using MongoDB.Driver;

namespace FamilyFarm.DataAccess.DAOs
{
    public class ProcessDAO
    {
        private readonly IMongoCollection<Process> _Processes;

        public ProcessDAO(IMongoDatabase database)
        {
            _Processes = database.GetCollection<Process>("Process");
        }

        public async Task<List<Process>> GetAllAsync()
        {
            return await _Processes.Find(p => p.IsDelete != true).ToListAsync();
        }

        public async Task<List<Process>> GetAllByExpertAsync(string accountId)
        {
            return await _Processes.Find(p => p.ExpertId == accountId && p.IsDelete != true).ToListAsync();
        }

        public async Task<List<Process>> GetAllByFarmerAsync(string accountId)
        {
            return await _Processes.Find(p => p.FarmerId == accountId && p.IsDelete != true).ToListAsync();
        }

        public async Task<Process> GetByIdAsync(string processId)
        {
            if (!ObjectId.TryParse(processId, out _)) return null;

            return await _Processes.Find(p => p.ProcessId == processId && p.IsDelete != true).FirstOrDefaultAsync();
        }

        public async Task<Process> CreateAsync(Process process)
        {
            process.ProcessId = ObjectId.GenerateNewId().ToString();
            process.CreateAt = DateTime.UtcNow;
            process.UpdateAt = null;
            process.DeleteAt = null;
            process.IsCompletedByExpert = false;
            process.IsCompletedByFarmer = false;
            process.IsDelete = false;
            await _Processes.InsertOneAsync(process);
            return process;
        }

        public async Task<Process> UpdateAsync(string processId, Process updateProcess)
        {
            if (!ObjectId.TryParse(processId, out _)) return null;

            var filter = Builders<Process>.Filter.Eq(p => p.ProcessId, processId);

            if (filter == null) return null;

            var update = Builders<Process>.Update
                .Set(p => p.ProcessTittle, updateProcess.ProcessTittle)
                .Set(p => p.Description, updateProcess.Description)
                .Set(p => p.NumberOfSteps, updateProcess.NumberOfSteps)
                .Set(p => p.ContinueStep, updateProcess.ContinueStep)
                .Set(p => p.ProcessStatus, updateProcess.ProcessStatus)
                .Set(p => p.UpdateAt, DateTime.UtcNow)
                .Set(p => p.IsCompletedByExpert, updateProcess.IsCompletedByExpert)
                .Set(p => p.IsCompletedByFarmer, updateProcess.IsCompletedByFarmer);

            var result = await _Processes.UpdateOneAsync(filter, update);

            var updatedProcess = await _Processes.Find(p => p.ProcessId == processId && p.IsDelete != true).FirstOrDefaultAsync();

            return updatedProcess;
        }

        public async Task<long> DeleteAsync(string processId)
        {
            if (!ObjectId.TryParse(processId, out _)) return 0;

            var filter = Builders<Process>.Filter.Eq(p => p.ProcessId, processId);

            var update = Builders<Process>.Update
                .Set(g => g.DeleteAt, DateTime.UtcNow)
                .Set(g => g.IsDelete, true);

            var result = await _Processes.UpdateOneAsync(filter, update);

            return result.ModifiedCount;
        }

        public async Task<List<Process>> SearchProcessKeywordAsync(string? keyword, string accountId, string roleId)
        {
            if (string.IsNullOrWhiteSpace(keyword))
            {
                // If the keyword is null or whitespace, return an empty list.
                return new List<Process>();
            }

            var filterBuilder = Builders<Process>.Filter;

            FilterDefinition<Process> filter;

            if (roleId == "68007b2a87b41211f0af1d57") // Expert
            {
                filter = filterBuilder.Eq(p => p.IsDelete, false) &
                             filterBuilder.Eq(p => p.ExpertId, accountId) &
                             filterBuilder.Regex(p => p.ProcessTittle, new BsonRegularExpression(keyword, "i"));
            }
            else if (roleId == "68007b0387b41211f0af1d56") // Farmer
            {
                filter = filterBuilder.Eq(p => p.IsDelete, false) &
                             filterBuilder.Eq(p => p.FarmerId, accountId) &
                             filterBuilder.Regex(p => p.ProcessTittle, new BsonRegularExpression(keyword, "i"));
            } else
            {
                return null;
            }    

            var matchProcesses = await _Processes.Find(filter).ToListAsync();
            return matchProcesses;
        }

        public async Task<List<Process>> FitlerStatusAsync(string? status, string accountId, string roleId)
        {
            if (string.IsNullOrWhiteSpace(status))
            {
                // If the keyword is null or whitespace, return an empty list.
                return new List<Process>();
            }

            var filterBuilder = Builders<Process>.Filter;

            FilterDefinition<Process> filter;

            if (roleId == "68007b2a87b41211f0af1d57") // Expert
            {
                filter = filterBuilder.Eq(p => p.IsDelete, false) &
                             filterBuilder.Eq(p => p.ExpertId, accountId) &
                             filterBuilder.Eq(p => p.ProcessStatus, status);
            }
            else if (roleId == "68007b0387b41211f0af1d56") // Farmer
            {
                filter = filterBuilder.Eq(p => p.IsDelete, false) &
                             filterBuilder.Eq(p => p.FarmerId, accountId) &
                             filterBuilder.Eq(p => p.ProcessStatus, status);
            }
            else
            {
                return null;
            }

            var matchProcesses = await _Processes.Find(filter).ToListAsync();
            return matchProcesses;
        }
    }
}
