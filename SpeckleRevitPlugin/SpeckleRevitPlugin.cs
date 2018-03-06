﻿using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.Attributes;
using SpeckleRevitTest;
using CefSharp;
using System.IO;
using System.Reflection;
using CefSharp.WinForms;
using System.Windows.Forms;

[TransactionAttribute(TransactionMode.Manual)]
[RegenerationAttribute(RegenerationOption.Manual)]
public class SpeckleRevit : IExternalCommand
{

    public static ChromiumWebBrowser Browser;

    public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
    {
        //Get application and document objects
        UIApplication uiApp = commandData.Application;
        Document doc = uiApp.ActiveUIDocument.Document;

        // initialise cef
        if (!Cef.IsInitialized)
            InitializeCef();

        // initialise one browser instance
        InitializeChromium();

        var form = new SpeckleRevitForm(commandData);

        form.Controls.Add(Browser);

        form.Show();

        return Result.Succeeded;
    }



    void InitializeCef()
    {

        Cef.EnableHighDPISupport();

        var assemblyLocation = Assembly.GetExecutingAssembly().Location;
        var assemblyPath = Path.GetDirectoryName(assemblyLocation);
        var pathSubprocess = Path.Combine(assemblyPath, "CefSharp.BrowserSubprocess.exe");
        CefSharpSettings.LegacyJavascriptBindingEnabled = true;
        var settings = new CefSettings
        {
            LogSeverity = LogSeverity.Verbose,
            LogFile = "ceflog.txt",
            BrowserSubprocessPath = pathSubprocess
        };

        // Initialize cef with the provided settings

        Cef.Initialize(settings);

    }


    public void InitializeChromium()
    {

#if DEBUG

        Browser = new ChromiumWebBrowser(@"http://localhost:9090/");

#else
        var path = Directory.GetParent(Assembly.GetExecutingAssembly().Location);
        Debug.WriteLine(path, "SPK");

        var indexPath = string.Format(@"{0}\app\index.html", path);

        if (!File.Exists(indexPath))
            Debug.WriteLine("Speckle for Revit: Error. The html file doesn't exists : {0}", "SPK");

        indexPath = indexPath.Replace("\\", "/");

        Browser = new ChromiumWebBrowser(indexPath);
#endif
        // Allow the use of local resources in the browser
        Browser.BrowserSettings = new BrowserSettings
        {
            FileAccessFromFileUrls = CefState.Enabled,
            UniversalAccessFromFileUrls = CefState.Enabled
        };


        Browser.Dock = DockStyle.Fill;
    }

}