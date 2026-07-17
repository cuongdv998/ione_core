using AutoMapper;
using iOne.Customer.ResIndustries;
using iOne.Customer.ResCustomers;
using iOne.ResIndustries;
using iOne.ResCustomers;

namespace iOne.Customer;

public class iOneCustomerApplicationAutoMapperProfile : Profile
{
    public iOneCustomerApplicationAutoMapperProfile()
    {
        /* You can configure your AutoMapper mapping configuration here.
         * Alternatively, you can split your mapping configurations
         * into multiple profile classes for a better organization. */

        // ResIndustry mappings
        CreateMap<ResIndustry, ResIndustryDto>();
        CreateMap<CreateResIndustryDto, ResIndustry>();
        CreateMap<UpdateResIndustryDto, ResIndustry>();

        // ResCustomer mappings
        CreateMap<ResCustomer, ResCustomerDto>();
        CreateMap<CreateResCustomerDto, ResCustomer>();
        CreateMap<UpdateResCustomerDto, ResCustomer>();
    }
}

