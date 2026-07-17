using AutoMapper;
using iOne.Report.ReportTemplateParameters;
using iOne.Report.ReportTemplates;
using iOne.Report.ReportTemplateSqls;
using iOne.Reports;

namespace iOne.Report;

public class iOneReportApplicationAutoMapperProfile : Profile
{
    public iOneReportApplicationAutoMapperProfile()
    {
        /* You can configure your AutoMapper mapping configuration here.
         * Alternatively, you can split your mapping configurations
         * into multiple profile classes for a better organization. */

        CreateMap<ReportTemplateSql, ReportTemplateSqlDto>();
        CreateMap<CreateReportTemplateSqlDto, ReportTemplateSql>();
        CreateMap<UpdateReportTemplateSqlDto, ReportTemplateSql>();
        CreateMap<ReportTemplateSqlInputDto, ReportTemplateSql>();

        CreateMap<ReportTemplate, ReportTemplateDto>();

        CreateMap<CreateReportTemplateDto, ReportTemplate>();
        CreateMap<UpdateReportTemplateDto, ReportTemplate>();

        CreateMap<ReportTemplateParameter, ReportTemplateParameterDto>();
        CreateMap<CreateReportTemplateParameterDto, ReportTemplateParameter>();
        CreateMap<UpdateReportTemplateParameterDto, ReportTemplateParameter>();
        CreateMap<UpdateReportTemplateParameterInputDto, ReportTemplateParameter>();
    }
}

