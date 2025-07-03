using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FamilyFarm.Models.Models;
using MongoDB.Bson;
using MongoDB.Driver;

namespace FamilyFarm.DataAccess.DAOs
{
    public class ProcessStepDAO
    {
        private readonly IMongoCollection<ProcessStep> _ProcessSteps;

        public ProcessStepDAO(IMongoDatabase database)
        {
            _ProcessSteps = database.GetCollection<ProcessStep>("ProcessStep");
        }

        public async Task<List<ProcessStep>> GetStepsByProcessId(string processId)
        {
            if (!ObjectId.TryParse(processId, out _)) return null;
            return await _ProcessSteps.Find(p => p.ProcessId == processId).ToListAsync();
        }

        public async Task<ProcessStep?> CreateStep(ProcessStep? request)
        {
            if (request == null)
                return null;

            await _ProcessSteps.InsertOneAsync(request);
            return request;
        }

        public async Task<ProcessStep?> EditStep(string stepId, ProcessStep? request)
        {
            if (!ObjectId.TryParse(stepId, out _)) return null;

            var filter = Builders<ProcessStep>.Filter.Eq(p => p.StepId, stepId);

            if (filter == null) return null;

            var update = Builders<ProcessStep>.Update
                .Set(p => p.StepTitle, request.StepTitle)
                .Set(p => p.StepDesciption, request.StepDesciption);

            var result = await _ProcessSteps.UpdateOneAsync(filter, update);

            var updatedStep = await _ProcessSteps.Find(p => p.StepId == stepId).FirstOrDefaultAsync();

            return updatedStep;
        }

        public async Task DeleteStepById(string stepId)
        {
            var filter = Builders<ProcessStep>.Filter.Eq(p => p.StepId, stepId);
            await _ProcessSteps.DeleteOneAsync(filter);
        }

        public async Task<bool?> HardDeleteByProcess(string? processId)
        {
            if (string.IsNullOrEmpty(processId))
                return null;

            var filter = Builders<ProcessStep>.Filter.Eq(p => p.ProcessId, processId);
            var result = await _ProcessSteps.DeleteManyAsync(filter);

            return result.DeletedCount > 0;
        }

    }
}
