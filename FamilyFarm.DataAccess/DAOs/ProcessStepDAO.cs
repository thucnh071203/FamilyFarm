using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FamilyFarm.Models.Models;
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

        public async Task<ProcessStep?> CreateStep(ProcessStep? request)
        {
            if (request == null)
                return null;

            await _ProcessSteps.InsertOneAsync(request);
            return request;
        }
    }
}
