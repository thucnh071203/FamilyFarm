using AutoMapper;
using FamilyFarm.BusinessLogic.Interfaces;
using FamilyFarm.Models.DTOs.EntityDTO;
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
        private readonly IPaymentRepository _paymenRepository;
        private readonly IMapper _mapper;
        private readonly INotificationService _notificationService;

        public ProcessService(IProcessRepository processRepository, IAccountRepository accountRepository, IServiceRepository serviceRepository, IBookingServiceRepository bookingServiceRepository, IUploadFileService uploadFileService, IProcessStepRepository processStepRepository, IPaymentRepository paymenRepository, IMapper mapper, INotificationService notificationService)
        {
            _processRepository = processRepository;
            _accountRepository = accountRepository;
            _serviceRepository = serviceRepository;
            _bookingServiceRepository = bookingServiceRepository;
            _uploadFileService = uploadFileService;
            _processStepRepository = processStepRepository;
            _paymenRepository = paymenRepository;
            _mapper = mapper;
            _notificationService = notificationService;
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
                        Service = await _serviceRepository.GetServiceById(process.ServiceId),
                        Steps = stepMappers
                    }
                }
            };
        }

        public async Task<ProcessOriginResponseDTO> GetProcessByProcessId(string processId)
        {
            if (string.IsNullOrEmpty(processId))
            {
                return new ProcessOriginResponseDTO
                {
                    Success = false,
                    Message = "Invalid process ID"
                };
            }

            var process = await _processRepository.GetProcessByProcessId(processId);
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
                        Service = await _serviceRepository.GetServiceById(process.ServiceId),
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

            await _serviceRepository.UpdateStatusService(item.ServiceId, item.Status);

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

            var IsExtraProcess = request.IsExtraProcess == null ? false : request.IsExtraProcess;

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
                ContinueStep = 0, //Mặc định đang ở bước 1
                SubProcessStatus = "Not Started", //Đã tạo sub process
                CreatedAt = DateTime.UtcNow,
                IsCompletedByFarmer = false,
                IsDeleted = false,
                IsExtraProcess = IsExtraProcess
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
            var currentBooking = await _bookingServiceRepository.GetById(request.BookingServiceId);
            currentBooking.BookingServiceStatus = "On Process";
            currentBooking.HasExtraProcess = true;

            var updatedBooking = await _bookingServiceRepository.UpdateBooking(request.BookingServiceId, currentBooking);


            //KHI TẠO SUB PROCESS XONG THÌ THÔNG BÁO CHO FARMER
            var expert = await _accountRepository.GetAccountByIdAsync(expertId);

            var notiRequest = new SendNotificationRequestDTO
            {
                ReceiverIds = new List<string> { request.FarmerId},
                SenderId = expertId,
                CategoryNotiId = "685d3f6d1d2b7e9f45ae1c41",
                TargetId = created.SubprocessId,
                TargetType = "Process",
                Content = expert.FullName + " added you to a process."
            };
            var notiResponse = await _notificationService.SendNotificationAsync(notiRequest);


            return true;

        }

        public async Task<ListSubprocessResponseDTO?> GetListSubprocessByExpert(string? expertId)
        {
            if (string.IsNullOrEmpty(expertId))
                return null;

            var listSubprocess = await _processRepository.GetAllSubprocessByExpert(expertId);

            if (listSubprocess == null || listSubprocess.Count <= 0)
                return new ListSubprocessResponseDTO
                {
                    Message = "Cannot get list subprocess",
                    Success = false
                };

            //Neu lay được list subprocess, thì lấy thông tin của step và step image luôn
            var listSubprocessesDTO = new List<SubprocessEntityDTO>();

            foreach(var subprocess in listSubprocess)
            {                
                //LẤY STEPS
                var listSteps = await _processStepRepository.GetStepsBySubprocess(subprocess.SubprocessId);
                if (listSteps == null || listSteps.Count <= 0)
                    continue;

                //Nếu có step thì lấy step image 
                var listProcessStepEntityDTO = new List<ProcessStepEntityDTO>();
                foreach (var step in listSteps)
                {
                    if (step.StepId == null)
                        continue;

                    var listImageSteps = await _processStepRepository.GetStepImagesByStepId(step.StepId);

                    var processStep = new ProcessStepEntityDTO
                    {
                        ProcessStep = step,
                        ProcessStepImages = listImageSteps
                    };

                    listProcessStepEntityDTO.Add(processStep);
                }

                //LẤY THÔNG TIN SERVICE
                var bookingService = await _bookingServiceRepository.GetById(subprocess.BookingServiceId);
                var service = await _serviceRepository.GetServiceById(bookingService.ServiceId);

                //Lấy thông tin farmer
                var accountFarmer = await _accountRepository.GetAccountByAccId(subprocess.FarmerId);
                var farmerProfile = _mapper.Map<MyProfileDTO>(accountFarmer);

                //Lấy thông tin expert
                var accountExpert = await _accountRepository.GetAccountByAccId(subprocess.ExpertId);
                var expertProfile = _mapper.Map<MyProfileDTO>(accountExpert);

                //ADD VÔ LIST SUBPROCESS
                var subprocessData = new SubprocessEntityDTO
                {
                    Service = service,
                    Expert = expertProfile,
                    Farmer = farmerProfile,
                    SubProcess = subprocess,
                    ProcessSteps = listProcessStepEntityDTO
                };
                
                listSubprocessesDTO.Add(subprocessData);
            }

            return new ListSubprocessResponseDTO
            {
                Message = "Get list subprocess successfully.",
                Success = true,
                Count = listSubprocessesDTO.Count,
                Subprocesses = listSubprocessesDTO
            };
            
        }

        public async Task<ListSubprocessResponseDTO?> GetListSubprocessByFarmer(string? farmerId)
        {
            if (string.IsNullOrEmpty(farmerId))
                return null;

            var listSubprocess = await _processRepository.GetAllSubprocessByFarmer(farmerId);

            if (listSubprocess == null || listSubprocess.Count <= 0)
                return new ListSubprocessResponseDTO
                {
                    Message = "Cannot get list subprocess",
                    Success = false
                };

            //Neu lay được list subprocess, thì lấy thông tin của step và step image luôn
            var listSubprocessesDTO = new List<SubprocessEntityDTO>();

            foreach (var subprocess in listSubprocess)
            {
                //LẤY STEPS
                var listSteps = await _processStepRepository.GetStepsBySubprocess(subprocess.SubprocessId);
                if (listSteps == null || listSteps.Count <= 0)
                    continue;

                //Nếu có step thì lấy step image 
                var listProcessStepEntityDTO = new List<ProcessStepEntityDTO>();
                foreach (var step in listSteps)
                {
                    if (step.StepId == null)
                        continue;

                    var listImageSteps = await _processStepRepository.GetStepImagesByStepId(step.StepId);

                    var processStep = new ProcessStepEntityDTO
                    {
                        ProcessStep = step,
                        ProcessStepImages = listImageSteps
                    };

                    listProcessStepEntityDTO.Add(processStep);
                }

                //LẤY THÔNG TIN SERVICE
                var bookingService = await _bookingServiceRepository.GetById(subprocess.BookingServiceId);
                var service = await _serviceRepository.GetServiceById(bookingService.ServiceId);

                //Lấy thông tin farmer
                var accountFarmer = await _accountRepository.GetAccountByAccId(farmerId);
                var farmerProfile = _mapper.Map<MyProfileDTO>(accountFarmer);

                //Lấy thông tin expert
                var accountExpert = await _accountRepository.GetAccountByAccId(subprocess.ExpertId);
                var expertProfile = _mapper.Map<MyProfileDTO>(accountExpert);

                //ADD VÔ LIST SUBPROCESS
                var subprocessData = new SubprocessEntityDTO
                {
                    Service = service,
                    Expert = expertProfile,
                    Farmer = farmerProfile,
                    SubProcess = subprocess,
                    ProcessSteps = listProcessStepEntityDTO
                };

                listSubprocessesDTO.Add(subprocessData);
            }

            return new ListSubprocessResponseDTO
            {
                Message = "Get list subprocess successfully.",
                Success = true,
                Count = listSubprocessesDTO.Count,
                Subprocesses = listSubprocessesDTO
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

        public async Task<SubProcessResponseDTO?> GetListSubProcessCompleted()
        {
            //var listSubProcess = (await _processRepository.GetAllSubProcessCompleted())
            //        .OrderByDescending(x => x.CompleteAt)
            //        .ToList();

            var listSubProcess = await _processRepository.GetAllSubProcessCompleted();

            if (listSubProcess == null)
                return new SubProcessResponseDTO
                {
                    Success = false,
                    Message = "List completed sub process is invalid."
                };

            List<SubProcessMapper> listResponse = new List<SubProcessMapper>();

            foreach (var item in listSubProcess)
            {
                var booking = await _bookingServiceRepository.GetById(item.BookingServiceId);
                var service = await _serviceRepository.GetServiceById(booking.ServiceId);
                var farmer = await _accountRepository.GetAccountById(item.FarmerId);
                var expert = await _accountRepository.GetAccountById(service.ProviderId);
                var payment = await _paymenRepository.GetRepaymentBySubProcessId(item.SubprocessId);
                var mapper = new SubProcessMapper
                {
                    Account = new FriendMapper
                    {
                        AccId = farmer.AccId,
                        RoleId = farmer.RoleId,
                        Username = farmer.Username,
                        FullName = farmer.FullName,
                        Birthday = farmer.Birthday,
                        Gender = farmer.Gender,
                        City = farmer.City,
                        Country = farmer.Country,
                        Address = farmer.Address,
                        Avatar = farmer.Avatar,
                        Background = farmer.Background,
                        WorkAt = farmer.WorkAt,
                        StudyAt = farmer.StudyAt,
                        Status = farmer.Status,
                    },
                    Expert = new ExpertMapper
                    {
                        AccId = expert.AccId,
                        RoleId = expert.RoleId,
                        Username = expert.Username,
                        FullName = expert.FullName,
                        Birthday = expert.Birthday,
                        Gender = expert.Gender,
                        City = expert.City,
                        Country = expert.Country,
                        Address = expert.Address,
                        Avatar = expert.Avatar,
                        Background = expert.Background,
                        WorkAt = expert.WorkAt,
                        StudyAt = expert.StudyAt,
                        Status = expert.Status,
                    },
                    Service = service,
                    SubProcess = item,
                    Payment = payment
                };
                listResponse.Add(mapper);

            }
            return new SubProcessResponseDTO
            {
                Success = true,
                Data = listResponse,
            };
        }

        public async Task<ProcessStepResultResponseDTO> CreateProcessStepResult(ProcessStepResultRequestDTO request)
        {
            if (request == null || string.IsNullOrEmpty(request.StepId))
            {
                return new ProcessStepResultResponseDTO
                {
                    Success = false,
                    Message = "Invalid request data"
                };
            }

            var step = await _processStepRepository.GetProcessStepResultsByStepId(request.StepId);
            if (step == null)
            {
                return new ProcessStepResultResponseDTO
                {
                    Success = false,
                    Message = "Process step not found"
                };
            }

            var newResult = new ProcessStepResults
            {
                StepResultId = null,
                StepId = request.StepId,
                StepResultComment = request.StepResultComment,
                CreatedAt = DateTime.UtcNow
            };

            var createdResult = await _processStepRepository.CreateProcessStepResult(newResult);
            if (createdResult == null)
            {
                return new ProcessStepResultResponseDTO
                {
                    Success = false,
                    Message = "Failed to create process step result"
                };
            }
            // Xử lý upload hình ảnh nếu có
            var images = new List<StepResultImages>();
            if (request.Images != null && request.Images.Any())
            {
                var uploadResults = await _uploadFileService.UploadListImage(request.Images);
                foreach (var uploadResult in uploadResults)
                {
                    if (uploadResult != null)
                    {
                        var stepResultImage = new StepResultImages
                        {
                            StepResultImageId = null,
                            StepResultId = createdResult.StepResultId,
                            ImageUrl = uploadResult.UrlFile
                        };
                        var createdImage = await _processStepRepository.CreateStepResultImage(stepResultImage);
                        if (createdImage != null)
                        {
                            images.Add(createdImage);
                        }
                    }
                }
            }

            //NẾU THEM RESULT THÀNH CÔNG THÌ UPDATE LAI CONTINUE STEP VAO SUBPROCESS CUA NO
            //Lay step dựa trên stepId
            var processStep = await _processStepRepository.GetStepById(createdResult.StepId);

            if(processStep != null)
            {
                var subprocess = await _processRepository.GetSubProcessBySubProcessId(request.SubprocessId);

                //KIỂM TRA XEM STEP CONTINUE HIEN TAI CO LỚN HƠN STEP NUMBER CỦA RESULT MỚI TẠO HAY KHÔNG
                if(subprocess != null && subprocess.ContinueStep < processStep.StepNumber)
                {
                    //NẾU CONTINUE STEP NHỎ HƠN THÌ CẬP NHẬT
                    await _processRepository.UpdateContinueStep(subprocess.SubprocessId, processStep.StepNumber);
                }

                //KIỂM TRA XEM STEP NUMBER CÓ PHẢI LÀ Bước 1 hay không
                if(subprocess != null && processStep.StepNumber == 1)
                {
                    await _processRepository.UpdateStatusSubprocess(subprocess.SubprocessId, "On Process");
                }
            }

            return new ProcessStepResultResponseDTO
            {
                Success = true,
                Message = "Process step result created successfully",
                Data = new List<ProcessStepResultMapper>
                {
                    new ProcessStepResultMapper
                    {
                        Result = createdResult,
                        Images = images
                    }
                }
            };
        }

        public async Task<ProcessStepResultResponseDTO> GetProcessStepResultsByStepId(string stepId)
        {
            if (string.IsNullOrEmpty(stepId))
            {
                return new ProcessStepResultResponseDTO
                {
                    Success = false,
                    Message = "Invalid step ID"
                };
            }

            var results = await _processStepRepository.GetProcessStepResultsByStepId(stepId);
            if (results == null)
            {
                return new ProcessStepResultResponseDTO
                {
                    Success = false,
                    Message = "No results found for this step"
                };
            }

            var resultMappers = new List<ProcessStepResultMapper>();
            foreach (var result in results)
            {
                var images = await _processStepRepository.GetStepResultImagesByStepResultId(result.StepResultId);
                resultMappers.Add(new ProcessStepResultMapper
                {
                    Result = result,
                    Images = images
                });
            }

            return new ProcessStepResultResponseDTO
            {
                Success = true,
                Message = "Retrieved process step results successfully",
                Data = resultMappers
            };
        }

        public async Task<bool?> IsCompletedSubprocess(string? subprocessId)
        {
            //LẤY DANH SÁCH step bằng subprocess id
            var listStep = await _processStepRepository.GetStepsBySubprocess(subprocessId);

            var isCompletedSteps = true;

            if (listStep == null)
                return null;

            if (listStep.Count == 0)
                return false;

            foreach (var step in listStep)
            {
                var listResult = await _processStepRepository.GetProcessStepResultsByStepId(step.StepId);

                if(listResult == null || listResult.Count <= 0)
                {
                    isCompletedSteps = false;
                }
            }

            return isCompletedSteps;
        }

        public async Task<bool?> ConfirmSubprocess(string? subprocessId, string? bookingServiceId)
        {
            if (string.IsNullOrEmpty(subprocessId) || string.IsNullOrEmpty(bookingServiceId))
                return null;

            //UPDATE STATUS CỦA SUBPROCESS THÀNH COMPLETED
            await _processRepository.UpdateStatusSubprocess(subprocessId, "Completed");

            var result = await _bookingServiceRepository.UpdateStatus(bookingServiceId, "Completed");

            return result;
        }
    }
}
