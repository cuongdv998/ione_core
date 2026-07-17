using System;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Domain.Services;

namespace iOne.Policies;

public class PolicyCertificateManager : DomainService
{
    protected IPolicyCertificateRepository Repository { get; }

    public PolicyCertificateManager(IPolicyCertificateRepository repository)
    {
        Repository = repository;
    }

    public virtual async Task CreateAsync(PolicyCertificate policyCertificate)
    {
        await Repository.InsertAsync(policyCertificate);
    }

    public virtual async Task UpdateAsync(
        PolicyCertificate policyCertificate,
        string? certificateNo = null,
        string? url = null)
    {
        if (certificateNo != null)
        {
            policyCertificate.UpdateCertificateNo(certificateNo);
        }
        
        if (url != null)
        {
            policyCertificate.UpdateUrl(url);
        }
        
        await Repository.UpdateAsync(policyCertificate);
    }
}
