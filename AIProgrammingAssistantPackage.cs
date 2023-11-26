//global using Community.VisualStudio.Toolkit;
global using Microsoft.VisualStudio.Shell;
global using System;
global using Community.VisualStudio.Toolkit.DependencyInjection.Microsoft;
global using Task = System.Threading.Tasks.Task;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using AIProgrammingAssistant.Commands.Optimize;
using Community.VisualStudio.Toolkit;
using AIProgrammingAssistant.Commands.CreateQuery;
using AIProgrammingAssistant.Commands.GenerateTest;
using AIProgrammingAssistant.Commands.GiveFeedBack;
using AIProgrammingAssistant.Commands.SuggestVariableNames;
using EnvDTE;
using EnvDTE80;
using Microsoft;

namespace AIProgrammingAssistant
{
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [InstalledProductRegistration(Vsix.Name, Vsix.Description, Vsix.Version)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [Guid(PackageGuids.AIProgrammingAssistantString)]
    public sealed class AIProgrammingAssistantPackage : MicrosoftDIToolkitPackage<AIProgrammingAssistantPackage>
    {
        public static DTE2 _dte;
        public static string apiKey { get; set; }
        protected override void InitializeServices(IServiceCollection services)
        {
            services.AddSingleton<AIConnection.IAIFunctions, AIConnection.AzureApi>();
            //services.AddScoped<Optimize>();
            //services.AddScoped<CreateQuery>();
            //services.AddScoped<GenerateTest>();
            //services.AddScoped<GiveFeedback>();
            //services.AddScoped<SuggestVariableNames>();
            services.RegisterCommands(ServiceLifetime.Singleton);
        }  

        
        protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            await base.InitializeAsync(cancellationToken, progress);
            _dte = await GetServiceAsync(typeof(DTE)) as DTE2;
            Assumes.Present(_dte);
        }  
    }

}