using Career.Domain.Aggregates.CompanyRoot;
using Career.Domain.Repositories;
using Career.Domain.Services;
using Career.Domain.Services.Messaging;

namespace Career.Api.BackgroundServices;

public class CompanyDeleted : BackgroundService
{
    private readonly ILogger<CompanyDeleted> _logger;
    private readonly IServiceProvider _serviceProvider;
    
    public CompanyDeleted(ILogger<CompanyDeleted> logger, IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
    }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var uow =  scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var storageService = scope.ServiceProvider.GetRequiredService<IStorageService>();
                var staffProducer = scope.ServiceProvider.GetRequiredService<IStaffServiceProducer>();
                
                var companies = await uow.CompanyRepository.GetAllDeactivatedCompanies();

                if (companies is not null && companies.Count != 0)
                {
                    _logger.LogInformation($"There are {companies.Count} companies deactivated.");
                    var companiesDeactivated = companies.Where(d => d.UpdatedAt.AddDays(30) <= DateTime.UtcNow).ToList();

                    if (companiesDeactivated.Any())
                    {
                        foreach (var company in companiesDeactivated)
                        {
                            var staffs = await uow.StaffRepository.GetAllStaffsFromACompany(company.Id);
                            if (staffs.Count > 0)
                            {
                                var userIds = staffs.Select(d => d.UserId).ToList();

                                await staffProducer.StaffsRemovedFromCompany(userIds);
                            }
                            await storageService.DeleteCompanyFiles(company.Id);
                            
                            var jobsIds = company.Jobs.Select(d => d.Id).ToList(); 
                            await storageService.DeleteJobsFiles(jobsIds);
                        }
                        
                        uow.GenericRepository.DeleteRange<Company>(companiesDeactivated);
                        await uow.Commit();   
                    }
                }

            }catch (Exception ex)
            {
                _logger.LogError(ex, "Occurred an error while trying to delete companies");
            }

            await Task.Delay(TimeSpan.FromDays(1), stoppingToken);
        }
    }
}