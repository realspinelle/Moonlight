﻿using Logging.Net;
using Moonlight.App.Database;
using Moonlight.App.Database.Entities;
using Moonlight.App.Repositories;
using Moonlight.App.Services.Sessions;

namespace Moonlight.App.Services.Statistics;

public class StatisticsCaptureService
{
    private readonly IServiceScopeFactory ServiceScopeFactory;
    private readonly DateTimeService DateTimeService;
    private readonly PeriodicTimer Timer;
    
    public StatisticsCaptureService(IServiceScopeFactory serviceScopeFactory, ConfigService configService, DateTimeService dateTimeService)
    {
        ServiceScopeFactory = serviceScopeFactory;
        DateTimeService = dateTimeService;

        var config = configService
            .GetSection("Moonlight")
            .GetSection("Statistics");
        
        if(!config.GetValue<bool>("Enabled"))
            return;
        
        var period = TimeSpan.FromMinutes(config.GetValue<int>("Wait"));
        Timer = new(period);

        Logger.Info("Starting statistics system");
        Task.Run(Run);
    }

    private async Task Run()
    {
        try
        {
            while (await Timer.WaitForNextTickAsync())
            {
                Logger.Warn("Creating statistics");
                
                using var scope = ServiceScopeFactory.CreateScope();

                var statisticsRepo = scope.ServiceProvider.GetRequiredService<Repository<StatisticsData>>();
                var usersRepo = scope.ServiceProvider.GetRequiredService<Repository<User>>();
                var serversRepo = scope.ServiceProvider.GetRequiredService<Repository<Server>>();
                var domainsRepo = scope.ServiceProvider.GetRequiredService<Repository<Domain>>();
                var webspacesRepo = scope.ServiceProvider.GetRequiredService<Repository<WebSpace>>();
                var databasesRepo = scope.ServiceProvider.GetRequiredService<Repository<MySqlDatabase>>();
                var sessionService = scope.ServiceProvider.GetRequiredService<SessionService>();
            
                void AddEntry(string chart, int value)
                {
                    statisticsRepo!.Add(new StatisticsData()
                    {
                        Chart = chart,
                        Value = value,
                        Date = DateTimeService.GetCurrent()
                    });
                }
            
                AddEntry("usersCount", usersRepo.Get().Count());
                AddEntry("serversCount", serversRepo.Get().Count());
                AddEntry("domainsCount", domainsRepo.Get().Count());
                AddEntry("webspacesCount", webspacesRepo.Get().Count());
                AddEntry("databasesCount", databasesRepo.Get().Count());
                AddEntry("sessionsCount", sessionService.GetAll().Length);
            }
            
            Logger.Log("Statistics are weird");
        }
        catch (Exception e)
        {
            Logger.Error("An unexpected error occured while capturing statistics");
            Logger.Error(e);
        }
    }
}