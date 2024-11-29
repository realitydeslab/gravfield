/*******************************************************************************
The content of this file includes portions of the proprietary AUDIOKINETIC Wwise
Technology released in source code form as part of the game integration package.
The content of this file may not be used without valid licenses to the
AUDIOKINETIC Wwise Technology.
Note that the use of the game engine is subject to the Unity(R) Terms of
Service at https://unity3d.com/legal/terms-of-service
 
License Usage
 
Licensees holding valid licenses to the AUDIOKINETIC Wwise Technology may use
this file in accordance with the end user license agreement provided with the
software or, alternatively, in accordance with the terms contained
in a written agreement between you and Audiokinetic Inc.
Copyright (c) 2024 Audiokinetic Inc.
*******************************************************************************/

#if UNITY_EDITOR
using System.IO;
using UnityEditor;

[UnityEditor.InitializeOnLoad]
public class AkiOSPluginActivator : AkPlatformPluginActivator
{
	public override string WwisePlatformName => "iOS";
	public override string PluginDirectoryName => "iOS";
	public override string DSPDirectoryPath => Path.Combine("iOS", PlayerSettings.iOS.sdkVersion == iOSSdkVersion.DeviceSDK ? "iphoneos" : "iphonesimulator", "DSP");
	public override string StaticPluginRegistrationName => "AkiOSPlugins";
	public override string StaticPluginDefine => "AK_IOS";
	public override bool RequiresStaticPluginRegistration => true;

	static AkiOSPluginActivator()
	{
		if (UnityEditor.AssetDatabase.IsAssetImportWorkerProcess())
		{
			return;
		}

		AkPluginActivator.RegisterPlatformPluginActivator(BuildTarget.iOS, new AkiOSPluginActivator());
	}

	private const int ARCH_INDEX = 1;
	private const int CONFIG_INDEX = 2;
	public override AkPluginActivator.PluginImporterInformation GetPluginImporterInformation(PluginImporter pluginImporter)
	{
		var parts = GetPluginPathParts(pluginImporter.assetPath);
		return new AkPluginActivator.PluginImporterInformation
		{
			PluginConfig = parts[CONFIG_INDEX],
			PluginArch = parts[ARCH_INDEX]
		};
	}

	internal override bool ConfigurePlugin(PluginImporter pluginImporter, AkPluginActivator.PluginImporterInformation pluginImporterInformation)
	{
		pluginImporter.SetPlatformData(UnityEditor.BuildTarget.iOS, "CPU", "AnyCPU");
		if (pluginImporterInformation.PluginArch == "iphoneos" && PlayerSettings.iOS.sdkVersion != iOSSdkVersion.DeviceSDK)
		{
			// In case the plugin was previously activated, we want to ensure we deactivate it
			return false;
		}
		if (pluginImporterInformation.PluginArch == "iphonesimulator" && PlayerSettings.iOS.sdkVersion != iOSSdkVersion.SimulatorSDK)
		{
			// In case the plugin was previously activated, we want to ensure we deactivate it
			return false;
		}
		return true;
	}
}
#endif