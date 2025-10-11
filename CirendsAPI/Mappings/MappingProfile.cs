using AutoMapper;
using CirendsAPI.DTOs;
using CirendsAPI.Models;

namespace CirendsAPI.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // User mappings
            CreateMap<User, UserDto>();

            // Activity mappings
            CreateMap<Activity, ActivityDto>()
                .ForMember(dest => dest.CreatedBy, opt => opt.MapFrom(src => src.CreatedBy))
                .ForMember(dest => dest.Tasks, opt => opt.MapFrom(src => src.Tasks))
                .ForMember(dest => dest.Participants, opt => opt.MapFrom(src => src.ActivityUsers));

            CreateMap<CreateActivityDto, Activity>();
            CreateMap<UpdateActivityDto, Activity>()
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));

            // ActivityUser mappings
            CreateMap<ActivityUser, ActivityUserDto>()
                .ForMember(dest => dest.User, opt => opt.MapFrom(src => src.User));

            // Task mappings
            CreateMap<TaskItem, TaskItemDto>()
                .ForMember(dest => dest.AssignedTo, opt => opt.MapFrom(src => src.AssignedTo))
                .ForMember(dest => dest.CreatedBy, opt => opt.MapFrom(src => src.CreatedBy))
                .ForMember(dest => dest.Expenses, opt => opt.MapFrom(src => src.Expenses));

            CreateMap<CreateTaskDto, TaskItem>();
            CreateMap<UpdateTaskDto, TaskItem>()
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));

            // Expense mappings
            CreateMap<Expense, ExpenseDto>()
                .ForMember(dest => dest.PaidBy, opt => opt.MapFrom(src => src.PaidBy))
                .ForMember(dest => dest.ExpenseShares, opt => opt.MapFrom(src => src.ExpenseShares));

            CreateMap<CreateExpenseDto, Expense>();
            CreateMap<UpdateExpenseDto, Expense>()
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));

            // ExpenseShare mappings
            CreateMap<ExpenseShare, ExpenseShareDto>()
                .ForMember(dest => dest.User, opt => opt.MapFrom(src => src.User));

            CreateMap<CreateExpenseShareDto, ExpenseShare>();
            
            // Backward compatibility mapping
            CreateMap<ExpenseShareRequest, ExpenseShare>();
        }
    }
}