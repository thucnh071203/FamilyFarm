using FamilyFarm.BusinessLogic.Interfaces;
using FamilyFarm.Models.DTOs.Request;
using FamilyFarm.Models.DTOs.Response;
using FamilyFarm.Models.Mapper;
using FamilyFarm.Models.Models;
using FamilyFarm.Repositories;
using FamilyFarm.Repositories.Implementations;
using FamilyFarm.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyFarm.BusinessLogic.Services
{
    public class ProcessService : IProcessService
    {
        private readonly IProcessRepository _processRepository;
        private readonly IAccountRepository _accountRepository;
        private readonly IServiceRepository _serviceRepository;
        private readonly IBookingServiceRepository _bookingServiceRepository;
        private readonly IUploadFileService _uploadFileService;
        private readonly IProcessStepRepository _processStepRepository;

        public ProcessService(IProcessRepository processRepository, IAccountRepository accountRepository, IServiceRepository serviceRepository, IBookingServiceRepository bookingServiceRepository, IUploadFileService uploadFileService, IProcessStepRepository processStepRepository)
        {
            _processRepository = processRepository;
            _accountRepository = accountRepository;
            _serviceRepository = serviceRepository;
            _bookingServiceRepository = bookingServiceRepository;
            _uploadFileService = uploadFileService;
            _processStepRepository = processStepRepository;
        }

        public async Task<ProcessResponseDTO> GetAllProcess()
        {
            var listAllProcess = await _processRepository.GetAllProcess();

            if (listAllProcess.Count == 0 || listAllProcess == null)
            {
                return new ProcessResponseDTO
                {
                    Success = false,
                    Message = "Process list is empty"
                };
            }

            var processMappers = listAllProcess.Select(p => new ProcessMapper { process = p }).ToList();

            return new ProcessResponseDTO
            {
                Success = true,
                Message = "Get all process successfully",
                Count = listAllProcess.Count,
                Data = processMappers
            };
        }

        //public async Task<ProcessResponseDTO> GetAllProcessByExpert(string accountId)
        //{
        //    var checkAccount = await _accountRepository.GetAccountById(accountId);

        //    if (checkAccount == null)
        //    {
        //        return new ProcessResponseDTO
        //        {
        //            Success = false,
        //            Message = "Account is null"
        //        };
        //    }
        //    else if (checkAccount.RoleId != "68007b2a87b41211f0af1d57")
        //    {
        //        return new ProcessResponseDTO
        //        {
        //            Success = false,
        //            Message = "Account is not expert"
        //        };
        //    }

        //    var listAllProcessExpert = await _processRepository.GetAllProcessByExpert(accountId);

        //    if (listAllProcessExpert.Count == 0 || listAllProcessExpert == null)
        //    {
        //        return new ProcessResponseDTO
        //        {
        //            Success = false,
        //            Message = "Process list is empty"
        //        };
        //    }

        //    var processMappers = listAllProcessExpert.Select(p => new ProcessMapper { process = p }).ToList();

        //    return new ProcessResponseDTO
        //    {
        //        Success = true,
        //        Message = "Get all process successfully",
        //        Count = listAllProcessExpert.Count,
        //        Data = processMappers
        //    };
        //}

        //public async Task<ProcessResponseDTO> GetAllProcessByFarmer(string accountId)
        //{
        //    var checkAccount = await _accountRepository.GetAccountById(accountId);

        //    if (checkAccount == null)
        //    {
        //        return new ProcessResponseDTO
        //        {
        //            Success = false,
        //            Message = "Account is null"
        //        };
        //    }
        //    else if (checkAccount.RoleId != "68007b0387b41211f0af1d56")
        //    {
        //        return new ProcessResponseDTO
        //        {
        //            Success = false,
        //            Message = "Account is not farmer"
        //        };
        //    }

        //    var listAllProcessFarmer = await _processRepository.GetAllProcessByFarmer(accountId);

        //    if (listAllProcessFarmer.Count == 0 || listAllProcessFarmer == null)
        //    {
        //        return new ProcessResponseDTO
        //        {
        //            Success = false,
        //            Message = "Process list is empty"
        //        };
        //    }

        //    var processMappers = listAllProcessFarmer.Select(p => new ProcessMapper { process = p }).ToList();

        //    return new ProcessResponseDTO
        //    {
        //        Success = true,
        //        Message = "Get all process successfully",
        //        Count= listAllProcessFarmer.Count,
        //        Data = processMappers
        //    };
        //}

        public async Task<ProcessResponseDTO> GetProcessById(string processId)
        {
            var process = await _processRepository.GetProcessById(processId);

            if (process == null)
            {
                return new ProcessResponseDTO
                {
                    Success = false,
                    Message = "Process not found"
                };
            }

            return new ProcessResponseDTO
            {
                Success = true,
                Message = "Get process successfully",
                Data = new List<ProcessMapper> { new ProcessMapper { process = process } }
            };
        }

        public async Task<ProcessResponseDTO> CreateProcess(ProcessRequestDTO item)
        {
            if (item == null)
            {
                return new ProcessResponseDTO
                {
                    Success = false,
                    Message = "Request is null"
                };
            }

            var checkService = await _serviceRepository.GetServiceById(item.ServiceId);

            if (checkService == null)
            {
                return new ProcessResponseDTO
                {
                    Success = false,
                    Message = "Service are null"
                };
            }

            var addNewProcess = new Process
            {
                ProcessId = null,
                ServiceId = item.ServiceId,
                ProcessTittle = item.ProcessTittle,
                Description = item.Description,
                NumberOfSteps = item.NumberOfSteps,
            };

            var created = await _processRepository.CreateProcess(addNewProcess);

            if (created == null)
            {
                return new ProcessResponseDTO
                {
                    Success = false,
                    Message = "Failed to create service"
                };
            }

            //KHI TẠO PROCESS THÀNH CÔNG THÌ TẠO CÁC PROCESS STEP OGIRINAL
            if(item.ProcessSteps != null && item.ProcessSteps.Count > 0)
            {
                foreach (var step in item.ProcessSteps)
                {
                    var newStep = new ProcessStep();
                    newStep.ProcessId = created.ProcessId;
                    newStep.StepNumber = step.StepNumber;
                    newStep.StepTitle = step.StepTitle;
                    newStep.StepDesciption = step.StepDescription;

                    var responseStep = await _processStepRepository.CreateProcessStep(newStep);

                    //Tạo image cho mỗi step
                    if(step.Images != null && step.Images.Count > 0)
                    {
                        //Goi method upload List image tu Upload file service
                        List<FileUploadResponseDTO> listImageUrl = await _uploadFileService.UploadListImage(step.Images);

                        if (listImageUrl != null && listImageUrl.Count > 0)
                        {
                            foreach (var image in listImageUrl)
                            {
                                var stepImage = new ProcessStepImage();
                                stepImage.ProcessStepId = responseStep.StepId;
                                stepImage.ImageUrl = image.UrlFile ?? "";

                                await _processStepRepository.CreateStepImage(stepImage);
                            }
                        }
                        
                    }
                    
                }
            }
            
            return new ProcessResponseDTO
            {
                Success = true,
                Message = "Process created successfully",
                Data = new List<ProcessMapper> { new ProcessMapper { process = created } }
            };
        }

        public async Task<ProcessResponseDTO> UpdateProcess(string processId, ProcessRequestDTO item)
        {
            if (item == null)
            {
                return new ProcessResponseDTO
                {
                    Success = false,
                    Message = "Request is null"
                };
            }

            //var checkAccountExpert = await _accountRepository.GetAccountById(item.ExpertId);
            //var checkAccountFarmer = await _accountRepository.GetAccountById(item.FarmerId);

            var checkService = await _serviceRepository.GetServiceById(item.ServiceId);
            //var checkBooking = await _bookingServiceRepository.GetById(item.BookingServiceId);

            if (checkService == null)
            {
                return new ProcessResponseDTO
                {
                    Success = false,
                    Message = "Service are null"
                };
            }

            //if (checkAccountExpert == null || checkAccountFarmer == null)
            //{
            //    return new ProcessResponseDTO
            //    {
            //        Success = false,
            //        Message = "Account is null"
            //    };
            //}
            //else if (checkAccountExpert.RoleId != "68007b2a87b41211f0af1d57")
            //{
            //    return new ProcessResponseDTO
            //    {
            //        Success = false,
            //        Message = "Creator is not expert"
            //    };
            //}
            //else if (checkAccountFarmer.RoleId != "68007b0387b41211f0af1d56")
            //{
            //    return new ProcessResponseDTO
            //    {
            //        Success = false,
            //        Message = "Customer is not farmer"
            //    };
            //}

            var checkOwner = await _processRepository.GetProcessById(processId);

            //if (checkOwner.ExpertId != item.ExpertId)
            //{
            //    return new ProcessResponseDTO
            //    {
            //        Success = false,
            //        Message = "Expert does not match"
            //    };
            //}

            var updateProcess = new Process
            {
                ProcessId = null,
                //ExpertId = item.ExpertId,
                //FarmerId = item.FarmerId,
                ServiceId = item.ServiceId,
                //BookingServiceId = item.BookingServiceId,
                ProcessTittle = item.ProcessTittle,
                Description = item.Description,
                NumberOfSteps = item.NumberOfSteps,
                //ContinueStep = item.ContinueStep,
                //ProcessStatus = "InProgress"
            };

            var updated = await _processRepository.UpdateProcess(processId, updateProcess);

            if (updated == null)
            {
                return new ProcessResponseDTO
                {
                    Success = false,
                    Message = "Failed to update process"
                };
            }

            return new ProcessResponseDTO
            {
                Success = true,
                Message = "Service updated successfully",
                Data = new List<ProcessMapper> { new ProcessMapper { process = updated } }
            };
        }

        public async Task<ProcessResponseDTO> DeleteProcess(string processId)
        {
            var deletedCount = await _processRepository.DeleteProcess(processId);

            if (deletedCount == 0)
            {
                return new ProcessResponseDTO
                {
                    Success = false,
                    Message = "Failed to delete process"
                };
            }

            return new ProcessResponseDTO
            {
                Success = true,
                Message = "Process deleted successfully",
                Data = null
            };
        }

        public async Task<ProcessResponseDTO> GetAllProcessByKeyword(string? keyword)
        {
            //var checkAccount = await _accountRepository.GetAccountById(accountId);

            //if (checkAccount == null)
            //{
            //    return new ProcessResponseDTO
            //    {
            //        Success = false,
            //        Message = "Account is null"
            //    };
            //}

            var searchAllProcess = await _processRepository.GetAllProcessByKeyword(keyword);

            if (searchAllProcess.Count == 0 || searchAllProcess == null)
            {
                return new ProcessResponseDTO
                {
                    Success = false,
                    Message = "Process list is empty"
                };
            }

            var processMappers = searchAllProcess.Select(p => new ProcessMapper { process = p }).ToList();

            return new ProcessResponseDTO
            {
                Success = true,
                Message = "Get all process successfully",
                Count = searchAllProcess.Count,
                Data = processMappers
            };
        }

        //public async Task<ProcessResponseDTO> FilterProcessByStatus(string? status, string accountId)
        //{
        //    var checkAccount = await _accountRepository.GetAccountById(accountId);

        //    if (checkAccount == null)
        //    {
        //        return new ProcessResponseDTO
        //        {
        //            Success = false,
        //            Message = "Account is null"
        //        };
        //    }

        //    var filterAllProcess = await _processRepository.FilterProcessByStatus(status, accountId, checkAccount.RoleId);

        //    if (filterAllProcess.Count == 0 || filterAllProcess == null)
        //    {
        //        return new ProcessResponseDTO
        //        {
        //            Success = false,
        //            Message = "Process list is empty"
        //        };
        //    }

        //    var processMappers = filterAllProcess.Select(p => new ProcessMapper { process = p }).ToList();

        //    return new ProcessResponseDTO
        //    {
        //        Success = true,
        //        Message = "Get all process successfully",
        //        Count = filterAllProcess.Count,
        //        Data = processMappers
        //    };
        //}
    }
}
