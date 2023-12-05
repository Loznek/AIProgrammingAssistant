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
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [Guid(PackageGuids.AIProgrammingAssistantString)]
    public sealed class AIProgrammingAssistantPackage : MicrosoftDIToolkitPackage<AIProgrammingAssistantPackage> //ToolkitPackage 
    {
        public static DTE2 dte;
        public static string ApiKey { get; set; }


        /// <summary>
        /// Register services that this package will use.
        /// </summary>
        protected override void InitializeServices(IServiceCollection services)
        {
            base.InitializeServices(services);
            services.AddTransient<AIConnection.IAIFunctions, AIConnection.AzureApi>();
            services.AddTransient<Optimize>();
            services.AddTransient<CreateQuery>();
            services.AddTransient<GenerateTest>();
            services.AddTransient<GiveFeedback>();
            services.AddTransient<SuggestVariableNames>();
        }  
        
        protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            await base.InitializeAsync(cancellationToken, progress);
            dte = await GetServiceAsync(typeof(DTE)) as DTE2;
            Assumes.Present(dte);

        }  
    }

}