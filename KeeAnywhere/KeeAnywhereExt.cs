﻿using System;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using KeeAnywhere.Configuration;
using KeeAnywhere.Forms;
using KeeAnywhere.StorageProviders;
using KeePass.Plugins;
using KeePass.UI;
using KeePassLib.Native;
using KeePassLib.Serialization;
using KeePassLib.Utility;

namespace KeeAnywhere
{
    /// <summary>
    ///     Plugin for KeePass to allow access to cloud storage providers
    /// </summary>
    /// <remarks>KeePass SDK documentation: http://keepass.info/help/v2_dev/plg_index.html</remarks>
    public class KeeAnywhereExt : Plugin
    {
        private ConfigurationService _configService;
        private IPluginHost _host;
        private StorageService _storageService;
        private ToolStripMenuItem _tsOpenFromCloudDrive;
        private ToolStripMenuItem _tsSaveToCloudDrive;

        private ToolStripMenuItem _tsShowSettings;
        private UIService _uiService;


        /// <summary>
        ///     Returns the URL where KeePass can check for updates of this plugin
        /// </summary>
        public override string UpdateUrl
        {
            get { return @"https://raw.githubusercontent.com/Kyrodan/KeeAnywhere/master/version_manifest.txt"; }
        }

        /// <summary>
        ///     Called when the Plugin is being loaded which happens on startup of KeePass
        /// </summary>
        /// <returns>True if the plugin loaded successfully, false if not</returns>
        public override bool Initialize(IPluginHost pluginHost)
        {
            if (_host != null) Terminate();
            if (pluginHost == null) return false;
            //if (NativeLib.IsUnix()) return false;

            _host = pluginHost;

            // Some binding redirection fixes for Google Drive API
            FixGoogleApiDependencyLoading();

            // Load the configuration
            _configService = new ConfigurationService(pluginHost);
            _configService.Load(NativeLib.IsUnix());

            // Initialize storage providers
            _storageService = new StorageService(_configService);
            _storageService.Register();

            // Initialize UIService
            _uiService = new UIService(_configService, _storageService);


            // Add the menu option for configuration under Tools
            var menu = _host.MainWindow.ToolsMenu.DropDownItems;
            _tsShowSettings = new ToolStripMenuItem("KeeAnywhere Settings...", PluginResources.KeeAnywhere_16x16);
            _tsShowSettings.Click += OnShowSetting;
            menu.Add(_tsShowSettings);

            // Add "Open from Cloud Drive..." to File\Open menu.
            var fileMenu = _host.MainWindow.MainMenu.Items["m_menuFile"] as ToolStripMenuItem;
            if (fileMenu != null)
            {
                var openMenu = fileMenu.DropDownItems["m_menuFileOpen"] as ToolStripMenuItem;
                if (openMenu != null)
                {
                    _tsOpenFromCloudDrive = new ToolStripMenuItem("Open from Cloud Drive...",
                        PluginResources.KeeAnywhere_16x16);
                    _tsOpenFromCloudDrive.Click += OnOpenFromCloudDrive;
                    _tsOpenFromCloudDrive.ShortcutKeys = Keys.Control | Keys.Alt | Keys.O;
                    openMenu.DropDownItems.Add(_tsOpenFromCloudDrive);
                }

                var saveMenu = fileMenu.DropDownItems["m_menuFileSaveAs"] as ToolStripMenuItem;
                if (saveMenu != null)
                {
                    var index = saveMenu.DropDownItems.IndexOfKey("m_menuFileSaveAsSep0");

                    _tsSaveToCloudDrive = new ToolStripMenuItem("Save to Cloud Drive...",
                        PluginResources.KeeAnywhere_16x16);
                    _tsSaveToCloudDrive.Click += OnSaveToCloudDrive;
                    saveMenu.DropDownItems.Insert(index, _tsSaveToCloudDrive);
                }
            }

            // Indicate that the plugin started successfully
            return true;
        }

        private void OnSaveToCloudDrive(object sender, EventArgs e)
        {
            if (_host.Database == null) return;

            // First usage: register new account
            if (!HasAccounts()) return;

            var form = new CloudDriveFilePicker();
            form.InitEx(_configService, _storageService, CloudDriveFilePicker.Mode.Save);
            var result = UIUtil.ShowDialogAndDestroy(form);

            if (result != DialogResult.OK)
                return;

            var ci = IOConnectionInfo.FromPath(form.ResultUri);
            ci.CredSaveMode = IOCredSaveMode.SaveCred;
            _host.MainWindow.SaveDatabaseAs(_host.Database, ci, true, null, false);
        }

        private void OnOpenFromCloudDrive(object sender, EventArgs eventArgs)
        {
            // First usage: register new account
            if (!HasAccounts()) return;

            var form = new CloudDriveFilePicker();
            form.InitEx(_configService, _storageService, CloudDriveFilePicker.Mode.Open);
            var result = UIUtil.ShowDialogAndDestroy(form);

            if (result != DialogResult.OK)
                return;

            var ci = IOConnectionInfo.FromPath(form.ResultUri);
            ci.CredSaveMode = IOCredSaveMode.SaveCred;

            _host.MainWindow.OpenDatabase(ci, null, false);
        }

        private bool HasAccounts()
        {
            if (_configService.Accounts.Any()) return true;

            var result = MessageService.Ask(
                "At least one account is required to work with KeeAnywhere.\r\nWould you like to open KeeAnywhere Settings to create a new account?",
                "KeeAnywhere", MessageBoxButtons.YesNo);

            if (result == DialogResult.Yes)
            {
                OnShowSetting(this, EventArgs.Empty);
            }

            return false;

            //AccountConfiguration account = null;

            //while (account == null)
            //{
            //    account = await _uiService.CreateOrUpdateAccount(StorageType.OneDrive);

            //    if (account != null) continue;
            //    var result = MessageService.Ask("You did not created a new account.\nAt least one account is required to work with KeeAnywhere.", "KeeAnywhere", MessageBoxButtons.RetryCancel);

            //    if (result != DialogResult.Retry)
            //    {
            //        return false;
            //    }
            //}

            //return true;
        }

        public override void Terminate()
        {
            if (_host == null) return;

            _configService.Save();

            _host.MainWindow.ToolsMenu.DropDownItems.Remove(_tsShowSettings);

            var fileMenu = _host.MainWindow.MainMenu.Items["m_menuFile"] as ToolStripMenuItem;
            if (fileMenu != null)
            {
                var openMenu = fileMenu.DropDownItems["m_menuFileOpen"] as ToolStripMenuItem;
                if (openMenu != null)
                {
                    openMenu.DropDownItems.Remove(_tsOpenFromCloudDrive);
                }

                var saveAsMenu = fileMenu.DropDownItems["m_menuFileSaveAs"] as ToolStripMenuItem;
                if (saveAsMenu != null)
                {
                    saveAsMenu.DropDownItems.Remove(_tsSaveToCloudDrive);
                }
            }

            _tsShowSettings = null;
            _tsOpenFromCloudDrive = null;
        }

        private void OnShowSetting(object sender, EventArgs e)
        {
            var form = new SettingsForm();
            form.InitEx(_configService, _uiService);
            UIUtil.ShowDialogAndDestroy(form);
        }

        private static void FixGoogleApiDependencyLoading()
        {
            // Google.Api relies on System.Net.Http.Primitives version 1.5.0.0
            // In general a binding redirect is added to the App.config file.
            // Due to this is a KeePass plugin which has no App.config, a workaround is implemented.
            //
            // See https://github.com/google/google-api-dotnet-client/issues/554 for details.

            var httpasm = Assembly.Load("System.Net.Http.Primitives");
            var httpver = new Version(1, 5, 0, 0);

            var jsonasm = Assembly.Load("Newtonsoft.Json");
            var jsonver = new Version(6, 0, 0, 0);


            AppDomain.CurrentDomain.AssemblyResolve += (s, a) =>
            {
                var requestedAssembly = new AssemblyName(a.Name);

                if (requestedAssembly.Name == "System.Net.Http.Primitives" && requestedAssembly.Version == httpver)
                    return httpasm;

                //if (requestedAssembly.Name == "Newtonsoft.Json" && requestedAssembly.Version == jsonver)
                //    return jsonasm;

                return null;
            };
        }
    }
}
