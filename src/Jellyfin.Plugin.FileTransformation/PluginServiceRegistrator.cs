﻿using System.Reflection;
using Jellyfin.Plugin.FileTransformation.Controller;
using Jellyfin.Plugin.FileTransformation.Infrastructure;
using Jellyfin.Plugin.FileTransformation.Services;
using Jellyfin.Plugin.Referenceable.Helpers;
using MediaBrowser.Controller;
using MediaBrowser.Controller.Configuration;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;

namespace Jellyfin.Plugin.FileTransformation
{
    public class FileTransformPluginServiceRegistrator : PluginServiceRegistrator
    {
        public override void ConfigureServices(IServiceCollection serviceCollection, IServerApplicationHost applicationHost)
        {
            StartupHelper.WebDefaultFilesFileProvider = GetFileTransformationFileProvider;
            StartupHelper.WebStaticFilesFileProvider = GetFileTransformationFileProvider;
            
            serviceCollection.AddSingleton<WebFileTransformationService>()
                .AddSingleton<IWebFileTransformationReadService>(s => s.GetRequiredService<WebFileTransformationService>())
                .AddSingleton<IWebFileTransformationWriteService>(s => s.GetRequiredService<WebFileTransformationService>());
        }

        ~FileTransformPluginServiceRegistrator()
        {
            if (Assembly.GetExecutingAssembly().IsCollectible)
            {
                // We don't care about the collectible version being destroyed
                return;
            }
            
            StartupHelper.WebDefaultFilesFileProvider = null;
            StartupHelper.WebStaticFilesFileProvider = null;
        }
        
        private IFileProvider GetFileTransformationFileProvider(IServerConfigurationManager serverConfigurationManager, IApplicationBuilder mainApplicationBuilder)
        {
            return new PhysicalTransformedFileProvider(
                new PhysicalFileProvider(serverConfigurationManager.ApplicationPaths.WebPath),
                mainApplicationBuilder.ApplicationServices.GetRequiredService<IWebFileTransformationReadService>());
        }
    }
}