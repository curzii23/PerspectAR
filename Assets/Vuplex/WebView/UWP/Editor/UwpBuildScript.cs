// Copyright (c) 2022 Vuplex Inc. All rights reserved.
//
// Licensed under the Vuplex Commercial Software Library License, you may
// not use this file except in compliance with the License. You may obtain
// a copy of the License at
//
//     https://vuplex.com/commercial-library-license
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
#if UNITY_WSA && UNITY_EDITOR
#pragma warning disable CS0618
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Callbacks;
using UnityEngine;
using UnityEngine.Rendering;
using Vuplex.WebView.Internal;

namespace Vuplex.WebView.Editor {

    /// <summary>
    /// UWP build script that copies the UWP plugin's Windows Runtime
    /// component to the generated Visual Studio project and adds
    /// a reference to it.
    /// </summary>
    public class UwpBuildScript : IPreprocessBuild {

        public int callbackOrder { get { return 0; } }

        public void OnPreprocessBuild(BuildTarget buildTarget, string buildPath) {

            if (buildTarget != BuildTarget.WSAPlayer) {
                return;
            }
            // Validate the graphics API.
            #if !VUPLEX_DISABLE_GRAPHICS_API_WARNING
                var selectedGraphicsApi = PlayerSettings.GetGraphicsAPIs(buildTarget)[0];
                var error = VXUtils.GetGraphicsApiErrorMessage(selectedGraphicsApi, new GraphicsDeviceType[] { GraphicsDeviceType.Direct3D11 });
                if (error != null) {
                    throw new BuildFailedException(error);
                }
            #endif

            // Validate the build type.
            if (EditorUserBuildSettings.wsaUWPBuildType != WSAUWPBuildType.XAML) {
                throw new BuildFailedException($"Unsupported build type: 3D WebView for UWP requires a project Built Type of \"XAML Project\", but the built type is currently set to {EditorUserBuildSettings.wsaUWPBuildType}. Please set the project build type to \"XAML Project\". For more information, see https://developer.vuplex.com/webview/getting-started#uwp .");
            }

            var mixedRealityEnabled = EditorUtils.XRSdkIsEnabled("WindowsMR");
            if (mixedRealityEnabled && PlayerSettings.graphicsJobs) {
                throw new BuildFailedException("3D WebView for UWP requires that Graphics Jobs be disabled when targeting Windows Mixed Reality. Please disable \"Graphics Jobs\" in Player Settings.");
            }
        }

        [PostProcessBuild(700)]
        public static void OnPostProcessBuild(BuildTarget buildTarget, string pathToBuiltProject) {

            if (buildTarget != BuildTarget.WSAPlayer) {
                return;
            }
            _copyRuntimeComponent(pathToBuiltProject);
            _updateVisualStudioProject(pathToBuiltProject);
            _updateAppManifest(pathToBuiltProject);
        }

        const string WINDOWS_RUNTIME_COMPONENT_NAME = "VuplexWebViewRuntimeComponent.winmd";

        /// <summary>
        /// Unity doesn't automatically copy the winmd file to the project, so this method does that.
        /// </summary>
        static void _copyRuntimeComponent(string pathToBuiltProject) {

            // Create the project's Plugins directory if it doesn't yet exist.
            var destinationPluginsDirectoryPath = Path.Combine(pathToBuiltProject, Application.productName, "Plugins");
            Directory.CreateDirectory(destinationPluginsDirectoryPath);
            var sourceRuntimeComponentPath = _getRuntimeComponentPath();
            var destinationRuntimeComponentPath = Path.Combine(destinationPluginsDirectoryPath, WINDOWS_RUNTIME_COMPONENT_NAME);
            File.Copy(sourceRuntimeComponentPath, destinationRuntimeComponentPath, true);
        }

        static string _getRuntimeComponentPath() {

            var expectedPath = Path.Combine(new string[] { Application.dataPath, "Vuplex", "WebView", "UWP", "Plugins", WINDOWS_RUNTIME_COMPONENT_NAME });
            if (File.Exists(expectedPath)) {
                return expectedPath;
            }
            // The Windows Runtime component isn't in the default location (Assets/Vuplex/WebView/UWP/Plugins/{name}).
            // So, let's try to find where it is in the Assets directory.
            var files = Directory.GetFiles(Application.dataPath, WINDOWS_RUNTIME_COMPONENT_NAME, SearchOption.AllDirectories);
            if (files.Count() == 1) {
                return files[0];
            }
            throw new Exception($"Vuplex.WebView build error: unable to locate the {WINDOWS_RUNTIME_COMPONENT_NAME} file in the Assets folder. It's not in the default location ({expectedPath}), and {files.Count()} instances of the directory were found in Assets folder.");
        }

        /// <summary>
        /// Updates the app manifest to register the JavaScriptBridge class included in
        /// the runtime component.
        /// https://github.com/microsoft/cppwinrt/issues/425#issuecomment-558212560
        ///
        /// It does this by adding an <Extension> element that indicates the library
        /// path and the name of the class to register. If the manifest already has
        /// an existing <Extensions> element that isn't nested inside an <Application>,
        /// then it adds the new <Extension> to it. If the manifest doesn't have an
        /// existing <Extensions> element, then this method adds one before nesting the
        /// <Extension> inside of it.
        /// </summary>
        static void _updateAppManifest(string pathToBuiltProject) {

            var filePath = Path.Combine(pathToBuiltProject, Application.productName, "Package.appxmanifest");
            var fileXml = File.ReadAllText(filePath, Encoding.UTF8);
            var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(WINDOWS_RUNTIME_COMPONENT_NAME);

            if (fileXml.Contains("VuplexWebViewRuntimeComponent.JavaScriptBridge")) {
                // The manifest already has a reference to the Windows Runtime component.
                return;
            }

            var packageContentsRegex = new Regex("<Package.*?>(.*)</Package>", RegexOptions.Singleline);
            var packageContentsResult = packageContentsRegex.Match(fileXml);
            if (!packageContentsResult.Success) {
                throw new Exception($"Vuplex.WebView build error: the Visual Studio project at {filePath} has no <Package> element.");
            }
            var originalPackageContentsXml = packageContentsResult.Groups[1].Captures[0].Value;
            // xmlToModify is the XML that gets replaced
            var xmlToModify = originalPackageContentsXml;
            // An <Application> element can contain an unrelated <Extensions> element, but that's
            // not the kind of <Extensions> we're targeting. So, we deliberately need to ensure we're
            // not adding to an <Extensions> in an <Application>.
            var applicationsRegex = new Regex("(<Applications.*?>.*</Applications>)", RegexOptions.Singleline);
            var applicationsElementResult = applicationsRegex.Match(originalPackageContentsXml);
            if (applicationsElementResult.Success) {
                // There is an <Applications> element in the middle of the XML, so pick which half
                // we should add the new <Extension> element to. If one of the halves already has
                // an <Extensions> element, then go with that side.
                var applicationsXml = applicationsElementResult.Groups[1].Captures[0].Value;
                var potentialSectionsToModify = originalPackageContentsXml.Split(new string[] { applicationsXml }, 2, StringSplitOptions.RemoveEmptyEntries)
                                                                          .Where(section => section.Trim().Length > 0)
                                                                          .ToList();
                if (potentialSectionsToModify.Count > 1) {
                    xmlToModify = potentialSectionsToModify[0].Contains("<Extensions") ? potentialSectionsToModify[0] : potentialSectionsToModify[1];
                }
            }

            var extensionsContentsRegex = new Regex("<Extensions.*?>(.*)</Extensions>", RegexOptions.Singleline);
            var extensionsContentsResult = extensionsContentsRegex.Match(xmlToModify);

            // Match the file's expected indentation.
            var extensionElementXml = @"
    <Extension Category=""windows.activatableClass.inProcessServer"">
      <InProcessServer>
        <Path>VuplexWebViewRuntimeComponent2.dll</Path>
        <ActivatableClass ActivatableClassId=""VuplexWebViewRuntimeComponent.JavaScriptBridge"" ThreadingModel=""both""/>
      </InProcessServer>
    </Extension>
";
            string updatedFileXml = null;
            if (extensionsContentsResult.Success) {
                // The file already has a Package/Extensions element, so add to it.
                var existingExtensionsElementsXml = extensionsContentsResult.Groups[1].Captures[0].Value;
                var updatedExtensionsElementsXml = existingExtensionsElementsXml + extensionElementXml;
                updatedFileXml = fileXml.Replace(existingExtensionsElementsXml, updatedExtensionsElementsXml);
            } else {
                // The file doesn't yet have a Package/Extensions element, so add one.
                var extensionXml = $"  <Extensions>{extensionElementXml}  </Extensions>\n";
                var updatedXml = xmlToModify + extensionXml;
                updatedFileXml = fileXml.Replace(xmlToModify, updatedXml);
            }

            File.WriteAllText(filePath, updatedFileXml, Encoding.UTF8);
        }

        /// <summary>
        /// Updates the VS project to add a file reference to VuplexWebViewRuntimeComponent.winmd.
        /// </summary>
        static void _updateVisualStudioProject(string pathToBuiltProject) {

            var filePath = Path.Combine(pathToBuiltProject, Application.productName, Application.productName + ".vcxproj");
            var fileXml = File.ReadAllText(filePath, Encoding.UTF8);
            var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(WINDOWS_RUNTIME_COMPONENT_NAME);

            if (fileXml.Contains($"Include=\"{fileNameWithoutExtension}\"")) {
                // The Visual Studio project already has a reference to the Windows Runtime component.
                return;
            }
            var index = fileXml.IndexOf("</Project>");
            if (index == -1) {
                throw new Exception($"Vuplex.WebView build error: the Visual Studio project at {filePath} has no closing </Project> tag.");
            }
            // Match the file's expected indentation.
            var xmlTemplate = @"
  <ItemGroup>
    <Reference Include=""{0}"">
      <HintPath>Plugins\{1}</HintPath>
      <IsWinMDFile>true</IsWinMDFile>
    </Reference>
  </ItemGroup>
";
            var xmlToInsert = String.Format(xmlTemplate, fileNameWithoutExtension, WINDOWS_RUNTIME_COMPONENT_NAME);
            var updatedFileXml = fileXml.Insert(index, xmlToInsert);
            File.WriteAllText(filePath, updatedFileXml, Encoding.UTF8);
        }
    }
}
#endif
