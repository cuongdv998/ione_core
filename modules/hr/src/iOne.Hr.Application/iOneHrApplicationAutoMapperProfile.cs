using AutoMapper;
using iOne.Hr.HrDepartmentTypes;
using iOne.Hr.HrDepartments;
using iOne.Hr.HrEmployeeRoles;
using iOne.Hr.HrEmployeeLevels;
using iOne.Hr.HrEmployeePositions;
using iOne.Hr.HrEmployees;
using iOne.HrDepartmentTypes;
using iOne.HrDepartments;
using iOne.HrEmployeeRoles;
using iOne.HrEmployeeLevels;
using iOne.HrEmployeePositions;
using iOne.HrEmployees;

namespace iOne.Hr;

public class iOneHrApplicationAutoMapperProfile : Profile
{
    public iOneHrApplicationAutoMapperProfile()
    {
        /* You can configure your AutoMapper mapping configuration here.
         * Alternatively, you can split your mapping configurations
         * into multiple profile classes for a better organization. */

        // HrDepartmentType mappings
        CreateMap<HrDepartmentType, HrDepartmentTypeDto>();
        CreateMap<CreateHrDepartmentTypeDto, HrDepartmentType>();
        CreateMap<UpdateHrDepartmentTypeDto, HrDepartmentType>();

        // HrDepartment mappings
        CreateMap<HrDepartment, HrDepartmentDto>();
        CreateMap<CreateHrDepartmentDto, HrDepartment>()
            .ForMember(dest => dest.Code, opt => opt.MapFrom(src => src.Code))
            .ForMember(dest => dest.PartnerId, opt => opt.Ignore()); // PartnerId will be set in AppService
        // Note: UpdateHrDepartmentDto không có Code, nên không map trực tiếp

        // HrEmployeeRole mappings
        CreateMap<HrEmployeeRole, HrEmployeeRoleDto>();
        CreateMap<CreateHrEmployeeRoleDto, HrEmployeeRole>();
        CreateMap<UpdateHrEmployeeRoleDto, HrEmployeeRole>();

        // HrEmployeeLevel mappings
        CreateMap<HrEmployeeLevel, HrEmployeeLevelDto>();
        CreateMap<CreateHrEmployeeLevelDto, HrEmployeeLevel>();
        CreateMap<UpdateHrEmployeeLevelDto, HrEmployeeLevel>();

        // HrEmployeePosition mappings
        CreateMap<HrEmployeePosition, HrEmployeePositionDto>();
        CreateMap<CreateHrEmployeePositionDto, HrEmployeePosition>();
        CreateMap<UpdateHrEmployeePositionDto, HrEmployeePosition>();

        // HrEmployee mappings
        CreateMap<HrEmployee, HrEmployeeDto>();
        CreateMap<CreateHrEmployeeDto, HrEmployee>();
        CreateMap<UpdateHrEmployeeDto, HrEmployee>();

        // HrEmployeeRoleRel mappings
        CreateMap<HrEmployeeRoleRel, HrEmployeeRoleRelDto>();
        CreateMap<CreateHrEmployeeRoleRelDto, HrEmployeeRoleRel>();
        CreateMap<UpdateHrEmployeeRoleRelDto, HrEmployeeRoleRel>();
    }
}

