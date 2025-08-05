using System;
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
        private readonly IMongoCollection<SubProcess> _Subprocesses;

        public ProcessDAO(IMongoDatabase database)
        {
            _Processes = database.GetCollection<Process>("Process");
            _Subprocesses = database.GetCollection<SubProcess>("SubProcess");
        }

        public async Task<List<Process>> GetAllAsync()
        {
            return await _Processes.Find(p => p.IsDelete != true).ToListAsync();
        }

        //public async Task<List<Process>> GetAllByExpertAsync(string accountId)
        //{
        //    return await _Processes.Find(p => p.ExpertId == accountId && p.IsDelete != true).ToListAsync();
        //}

        //public async Task<List<Process>> GetAllByFarmerAsync(string accountId)
        //{
        //    return await _Processes.Find(p => p.FarmerId == accountId && p.IsDelete != true).ToListAsync();
        //}

        public async Task<Process> GetByIdAsync(string processId)
        {
            if (!ObjectId.TryParse(processId, out _)) return null;

            return await _Processes.Find(p => p.ProcessId == processId && p.IsDelete != true).FirstOrDefaultAsync();
        }

        public async Task<Process?> GetLatestByServiceIdAsync(string serviceId)
        {
            if (!ObjectId.TryParse(serviceId, out _)) return null;

            return await _Processes
                .Find(p => p.ServiceId == serviceId && p.IsDelete != true)
                .SortByDescending(p => p.CreateAt)
                .FirstOrDefaultAsync();
        }

        public async Task<Process> CreateAsync(Process process)
        {
            process.ProcessId = ObjectId.GenerateNewId().ToString();
            process.CreateAt = DateTime.UtcNow;
            process.UpdateAt = null;
            process.DeleteAt = null;
            //process.IsCompletedByExpert = false;
            //process.IsCompletedByFarmer = false;
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
                .Set(p => p.UpdateAt, DateTime.UtcNow);

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

        public async Task<List<Process>> SearchProcessKeywordAsync(string? keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword))
            {
                // If the keyword is null or whitespace, return an empty list.
                return new List<Process>();
            }

            var filterBuilder = Builders<Process>.Filter;

            FilterDefinition<Process> filter;

            filter = filterBuilder.Eq(p => p.IsDelete, false) &
                             filterBuilder.Regex(p => p.ProcessTittle, new BsonRegularExpression(keyword, "i"));

            //if (roleId == "68007b2a87b41211f0af1d57") // Expert
            //{
            //    filter = filterBuilder.Eq(p => p.IsDelete, false) &
            //                 filterBuilder.Eq(p => p.ExpertId, accountId) &
            //                 filterBuilder.Regex(p => p.ProcessTittle, new BsonRegularExpression(keyword, "i"));
            //}
            //else if (roleId == "68007b0387b41211f0af1d56") // Farmer
            //{
            //    filter = filterBuilder.Eq(p => p.IsDelete, false) &
            //                 filterBuilder.Eq(p => p.FarmerId, accountId) &
            //                 filterBuilder.Regex(p => p.ProcessTittle, new BsonRegularExpression(keyword, "i"));
            //} else
            //{
            //    return null;
            //}    

            var matchProcesses = await _Processes.Find(filter).ToListAsync();
            return matchProcesses;
        }

        //public async Task<List<Process>> FitlerStatusAsync(string? status, string accountId, string roleId)
        //{
        //    if (string.IsNullOrWhiteSpace(status))
        //    {
        //        // If the keyword is null or whitespace, return an empty list.
        //        return new List<Process>();
        //    }

        //    var filterBuilder = Builders<Process>.Filter;

        //    FilterDefinition<Process> filter;

        //    if (roleId == "68007b2a87b41211f0af1d57") // Expert
        //    {
        //        filter = filterBuilder.Eq(p => p.IsDelete, false) &
        //                     filterBuilder.Eq(p => p.ExpertId, accountId) &
        //                     filterBuilder.Eq(p => p.ProcessStatus, status);
        //    }
        //    else if (roleId == "68007b0387b41211f0af1d56") // Farmer
        //    {
        //        filter = filterBuilder.Eq(p => p.IsDelete, false) &
        //                     filterBuilder.Eq(p => p.FarmerId, accountId) &
        //                     filterBuilder.Eq(p => p.ProcessStatus, status);
        //    }
        //    else
        //    {
        //        return null;
        //    }

        //    var matchProcesses = await _Processes.Find(filter).ToListAsync();
        //    return matchProcesses;
        //}

        public async Task<Process?> GetProcessByServiceId(string? serviceId)
        {
            if (string.IsNullOrEmpty(serviceId))
                return null;

            var filter = Builders<Process>.Filter.Eq(p => p.ServiceId, serviceId);
            return await _Processes.Find(filter).FirstOrDefaultAsync();
        }

        public async Task<bool?> HardDeleteByServiceId(string? serviceId)
        {
            if (string.IsNullOrEmpty(serviceId))
                return null;

            var filter = Builders<Process>.Filter.Eq(p => p.ServiceId, serviceId);
            var result = await _Processes.DeleteManyAsync(filter);

            return result.DeletedCount > 0;
        }

        public async Task<SubProcess?> CreateSubprocess(SubProcess? subProcess)
        {
            if (subProcess == null)
                return null;

            try
            {
                await _Subprocesses.InsertOneAsync(subProcess);
                return subProcess;
            }
            catch (Exception ex)
            {
                
                return null;
            }
        }

        public async Task<List<SubProcess>?> GetSubprocessesByExpert(string? expertId)
        {
            if (string.IsNullOrEmpty(expertId))
                return null;

            var filter = Builders<SubProcess>.Filter.Eq(sp => sp.ExpertId, expertId);

            var subprocesses = await _Subprocesses.Find(filter).ToListAsync();

            return subprocesses;
        }

        public async Task<List<SubProcess>?> GetSubprocessesByFarmer(string? farmerId)
        {
            if (string.IsNullOrEmpty(farmerId))
                return null;

            var filter = Builders<SubProcess>.Filter.Eq(sp => sp.FarmerId, farmerId);

            var subprocesses = await _Subprocesses.Find(filter).ToListAsync();

            return subprocesses;
        }

        public async Task<SubProcess?> GetBySubProcessId(string subprocessId)
        {
            if (string.IsNullOrEmpty(subprocessId))
                return null;

            if (!ObjectId.TryParse(subprocessId, out _)) return null;

            var filter = Builders<SubProcess>.Filter.Eq(sp => sp.SubprocessId, subprocessId);

            var subprocess = await _Subprocesses.Find(filter).FirstOrDefaultAsync();

            return subprocess;
        }

        public async Task<SubProcess?> UpdateSubProcess(string subprocessId, SubProcess updateSubProcess)
        {
            if (string.IsNullOrEmpty(subprocessId) || !ObjectId.TryParse(subprocessId, out _))
                return null;

            var filter = Builders<SubProcess>.Filter.Eq(sp => sp.SubprocessId, subprocessId);

            // Cập nhật các trường mà bạn muốn thay đổi.
            var update = Builders<SubProcess>.Update
                .Set(sp => sp.Title, updateSubProcess.Title)
                .Set(sp => sp.Description, updateSubProcess.Description)
                .Set(sp => sp.NumberOfSteps, updateSubProcess.NumberOfSteps)
                .Set(sp => sp.ContinueStep, updateSubProcess.ContinueStep)
                .Set(sp => sp.SubProcessStatus, updateSubProcess.SubProcessStatus)
                .Set(sp => sp.IsCompletedByFarmer, updateSubProcess.IsCompletedByFarmer)
                .Set(sp => sp.Price, updateSubProcess.Price)
                .Set(sp => sp.IsAccept, updateSubProcess.IsAccept)
                .Set(sp => sp.PayAt, updateSubProcess.PayAt)
                .Set(sp => sp.IsExtraProcess, updateSubProcess.IsExtraProcess)
                .Set(sp => sp.UpdatedAt, DateTime.UtcNow); // Cập nhật thời gian

            var result = await _Subprocesses.UpdateOneAsync(filter, update);

            if (result.ModifiedCount > 0)
            {
                return await _Subprocesses.Find(filter).FirstOrDefaultAsync();
            }

            return null;
        }

        public async Task<List<SubProcess>?> GetAllSubProcessComplete()
        {
            var filter = Builders<SubProcess>.Filter.And(
                Builders<SubProcess>.Filter.Eq(c => c.IsDeleted, false),
                Builders<SubProcess>.Filter.Eq(c => c.IsExtraProcess, true),
                Builders<SubProcess>.Filter.Eq(c => c.IsCompletedByFarmer, true)
            );

            return await _Subprocesses.Find(filter).ToListAsync();
        }

        public async Task<bool?> UpdateContinueStep(string? subprocessId, int stepNumber)
        {
            if (string.IsNullOrEmpty(subprocessId))
                return null;

            var filter = Builders<SubProcess>.Filter.Eq(s => s.SubprocessId, subprocessId);
            var update = Builders<SubProcess>.Update.Set(s => s.ContinueStep, stepNumber);

            var result = await _Subprocesses.UpdateOneAsync(filter, update);

            return result.ModifiedCount > 0;
        }

        public async Task UpdateStatusSubprocess(string? subprocessId, string? status)
        {
            if (string.IsNullOrEmpty(subprocessId) || string.IsNullOrEmpty(status))
                return;

            var filter = Builders<SubProcess>.Filter.Eq(x => x.SubprocessId, subprocessId);
            var update = Builders<SubProcess>.Update.Set(x => x.SubProcessStatus, status);

            await _Subprocesses.UpdateOneAsync(filter, update);
        }
    }
}
