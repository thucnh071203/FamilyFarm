using FamilyFarm.BusinessLogic.Interfaces;
using FamilyFarm.Models.DTOs.Request;
using FamilyFarm.Models.DTOs.Response;
using FamilyFarm.Models.Mapper;
using FamilyFarm.Models.Models;
using FamilyFarm.Repositories;
using FamilyFarm.Repositories.Implementations;
using FamilyFarm.Repositories.Interfaces;
using Newtonsoft.Json;
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

        //public async Task<ProcessResponseDTO> GetProcessById(string processId)
        //{
        //    var process = await _processRepository.GetProcessById(processId);

        //    if (process == null)
        //    {
        //        return new ProcessResponseDTO
        //        {
        //            Success = false,
        //            Message = "Process not found"
        //        };
        //    }

        //    return new ProcessResponseDTO
        //    {
        //        Success = true,
        //        Message = "Get process successfully",
        //        Data = new List<ProcessMapper> { new ProcessMapper { process = process } }
        //    };
        //}

        public async Task<ProcessOriginResponseDTO> GetProcessById(string serviceId)
        {
            var process = await _processRepository.GetProcessById(serviceId);

            if (process == null)
            {
                return new ProcessOriginResponseDTO
                {
                    Success = false,
                    Message = "Process not found"
                };
            }

            // Lấy tất cả các bước thuộc về process này
            var steps = await _processStepRepository.GetStepsByProcessId(process.ProcessId);

            var stepMappers = new List<ProcessStepMapper>();

            foreach (var step in steps)
            {
                var images = await _processStepRepository.GetStepImagesByStepId(step.StepId);

                stepMappers.Add(new ProcessStepMapper
                {
                    Step = step,
                    Images = images
                });
            }

            return new ProcessOriginResponseDTO
            {
                Success = true,
                Message = "Get process successfully",
                Data = new List<ProcessOriginMapper>
        {
            new ProcessOriginMapper
            {
                process = process,
                Steps = stepMappers
            }
        }
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
                    if (step.Images != null && step.Images.Count > 0)
                    {
                        foreach (var imageUrl in step.Images)
                        {
                            var stepImage = new ProcessStepImage();
                            stepImage.ProcessStepId = responseStep.StepId;
                            stepImage.ImageUrl = imageUrl ?? "";

                            await _processStepRepository.CreateStepImage(stepImage);
                        }
                    }

                }
            }

            await _serviceRepository.UpdateProcessStatusService(item.ServiceId);
            
            return new ProcessResponseDTO
            {
                Success = true,
                Message = "Process created successfully",
                Data = new List<ProcessMapper> { new ProcessMapper { process = created } }
            };
        }

        //public async Task<ProcessResponseDTO> UpdateProcess(string processId, ProcessRequestDTO item)
        //{
        //    if (item == null)
        //    {
        //        return new ProcessResponseDTO
        //        {
        //            Success = false,
        //            Message = "Request is null"
        //        };
        //    }

        //    var checkService = await _serviceRepository.GetServiceById(item.ServiceId);

        //    if (checkService == null)
        //    {
        //        return new ProcessResponseDTO
        //        {
        //            Success = false,
        //            Message = "Service are null"
        //        };
        //    }

        //    var checkOwner = await _processRepository.GetProcessById(processId);

        //    var updateProcess = new Process
        //    {
        //        ProcessId = null,
        //        ServiceId = item.ServiceId,
        //        ProcessTittle = item.ProcessTittle,
        //        Description = item.Description,
        //        NumberOfSteps = item.NumberOfSteps,
        //    };

        //    var updated = await _processRepository.UpdateProcess(processId, updateProcess);

        //    if (updated == null)
        //    {
        //        return new ProcessResponseDTO
        //        {
        //            Success = false,
        //            Message = "Failed to update process"
        //        };
        //    }

        //    return new ProcessResponseDTO
        //    {
        //        Success = true,
        //        Message = "Service updated successfully",
        //        Data = new List<ProcessMapper> { new ProcessMapper { process = updated } }
        //    };
        //}

        public async Task<ProcessResponseDTO> UpdateProcess(string processId, ProcessUpdateRequestDTO item)
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
                    Message = "Service not found"
                };
            }

            var checkProcess = await _processRepository.GetProcessByProcessId(processId);
            if (checkProcess == null)
            {
                return new ProcessResponseDTO
                {
                    Success = false,
                    Message = "Process not found"
                };
            }

            // Cập nhật thông tin chính của Process
            var updateProcess = new Process
            {
                ProcessId = processId,
                ServiceId = item.ServiceId,
                ProcessTittle = item.ProcessTittle,
                Description = item.Description,
                NumberOfSteps = item.NumberOfSteps,
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

            // Cập nhật Process Steps nếu có
            if (item.ProcessSteps != null && item.ProcessSteps.Count > 0)
            {
                foreach (var step in item.ProcessSteps)
                {
                    ProcessStep? responseStep;

                    if (!string.IsNullOrEmpty(step.StepId))
                    {
                        // Step đã tồn tại → cập nhật
                        var updateStep = new ProcessStep
                        {
                            StepId = step.StepId,
                            ProcessId = processId,
                            StepNumber = step.StepNumber,
                            StepTitle = step.StepTitle,
                            StepDesciption = step.StepDescription
                        };

                        responseStep = await _processStepRepository.UpdateProcessStep(step.StepId, updateStep);
                    }
                    else
                    {
                        // Step mới → thêm mới
                        var newStep = new ProcessStep
                        {
                            ProcessId = processId,
                            StepNumber = step.StepNumber,
                            StepTitle = step.StepTitle,
                            StepDesciption = step.StepDescription
                        };

                        responseStep = await _processStepRepository.CreateProcessStep(newStep);
                    }

                    // Cập nhật ảnh của step
                    if (responseStep != null && step.ImagesWithId != null && step.ImagesWithId.Count > 0)
                    {
                        foreach (var image in step.ImagesWithId)
                        {
                            if (!string.IsNullOrEmpty(image.ProcessStepImageId))
                            {
                                // ảnh đã tồn tại → cập nhật
                                var updateImage = new ProcessStepImage
                                {
                                    ProcessStepImageId = image.ProcessStepImageId,
                                    ProcessStepId = responseStep.StepId,
                                    ImageUrl = image.ImageUrl
                                };

                                await _processStepRepository.UpdateStepImage(image.ProcessStepImageId, updateImage);
                            }
                            else
                            {
                                // ảnh mới → thêm mới
                                var newImage = new ProcessStepImage
                                {
                                    ProcessStepId = responseStep.StepId,
                                    ImageUrl = image.ImageUrl
                                };

                                await _processStepRepository.CreateStepImage(newImage);
                            }
                        }
                    }
                }
            }

            if (item.DeletedImageIds != null && item.DeletedImageIds.Count > 0)
            {
                foreach (var imageId in item.DeletedImageIds)
                {
                    await _processStepRepository.DeleteStepImageById(imageId);
                }
            }

            // Xóa step nếu có
            if (item.DeletedStepIds != null && item.DeletedStepIds.Count > 0)
            {
                foreach (var stepId in item.DeletedStepIds)
                {
                    // Xóa ảnh trước
                    await _processStepRepository.DeleteImagesByStepId(stepId);

                    // Xóa step
                    await _processStepRepository.DeleteStepById(stepId);
                }
            }

            return new ProcessResponseDTO
            {
                Success = true,
                Message = "Process updated successfully",
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

        public async Task<bool?> CreateSubprocess(string? expertId, CreateSubprocessRequestDTO request)
        {
            if (request == null || expertId == null)
                return null;

            //THEM SUBPROCESS MỚI
            var requestSubproccess = new SubProcess
            {
                SubprocessId = null,
                FarmerId = request.FarmerId,
                ExpertId = expertId,
                BookingServiceId = request.BookingServiceId,
                Description = request.Description,
                Title = request.Title,
                NumberOfSteps = request.NumberOfSteps,
                ProcessId = request.ProcessId,
                ContinueStep = 1, //Mặc định đang ở bước 1
                SubProcessStatus = "Created", //Đã tạo sub process
                CreatedAt = DateTime.UtcNow,
                IsCompletedByFarmer = false,
                IsDeleted = false
            };

            var created = await _processRepository.CreateSubprocess(requestSubproccess);

            if(created == null)
                return false;

            //KHI TẠO SUBPROCESS THÀNH CÔNG THÌ TẠO CÁC STEP
            if (request.ProcessSteps != null && request.ProcessSteps.Count > 0)
            {
                foreach (var step in request.ProcessSteps)
                {
                    var newStep = new ProcessStep();
                    newStep.SubprocessId = created.SubprocessId; //Các step chưa subprocess ID
                    newStep.StepNumber = step.StepNumber;
                    newStep.StepTitle = step.StepTitle;
                    newStep.StepDesciption = step.StepDescription;

                    var responseStep = await _processStepRepository.CreateProcessStep(newStep);

                    //Tạo image cho mỗi step
                    if (step.Images != null && step.Images.Count > 0)
                    {
                        foreach (var imageUrl in step.Images)
                        {
                            var stepImage = new ProcessStepImage();
                            stepImage.ProcessStepId = responseStep.StepId;
                            stepImage.ImageUrl = imageUrl ?? "";

                            await _processStepRepository.CreateStepImage(stepImage);
                        }
                    }

                }
            }

            //KHI TẠO THÀNH CÔNG RỒI UPDATE TRẠNG THÁI BOOKING LẠI
            await _bookingServiceRepository.UpdateStatus(request.BookingServiceId, "On Process");
            return true;

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
