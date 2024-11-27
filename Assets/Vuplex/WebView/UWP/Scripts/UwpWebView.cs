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
#if UNITY_WSA && !UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using UnityEngine;
using Vuplex.WebView.Internal;

namespace Vuplex.WebView {

    /// <summary>
    /// The IWebView implementation for 3D WebView for UWP / Hololens.
    /// This class also includes extra methods for UWP-specific functionality.
    /// </summary>
    public class UwpWebView : BaseWebView,
                              IWebView,
                              IWithMovablePointer,
                              IWithNative2DMode,
                              IWithPointerDownAndUp {

        /// <see cref="IWithNative2DMode"/>
        public bool Native2DModeEnabled { get; private set; }

        public WebPluginType PluginType { get; } = WebPluginType.UniversalWindowsPlatform;

        /// <see cref="IWithNative2DMode"/>
        public Rect Rect { get { return _rect; }}

        /// <see cref="IWithNative2DMode"/>
        public bool Visible { get; private set; }

        /// <seealso cref="IWithNative2DMode"/>
        public void BringToFront() => WebViewLogger.LogWarning("IWithNative2DMode.BringToFront() was called, but is not currently implemented for UWP.");

        // Overrides because BaseWebView.CaptureScreenshot() leaks memory on UWP.
        public override Task<byte[]> CaptureScreenshot() {

            var taskSource = new TaskCompletionSource<byte[]>();
            _invokeDataOperation(taskSource.SetResult, WebView_captureScreenshot);
            return taskSource.Task;
        }

        public static void ClearAllData() => WebView_clearAllData();

        public static Task<bool> DeleteCookies(string url, string cookieName = null) {

            if (url == null) {
                throw new ArgumentException("The url cannot be null.");
            }
            WebView_deleteCookies(url, cookieName);
            return Task.FromResult(true);
        }

        public override void Dispose() {

            // Cancel the render if it has been scheduled via GL.IssuePluginEvent().
            WebView_removePointer(_nativeWebViewPtr);
            base.Dispose();
        }

        public static Task<Cookie[]> GetCookies(string url, string cookieName = null) {

            if (url == null) {
                throw new ArgumentException("The url cannot be null.");
            }
            var serializedCookies = WebView_getCookies(url, cookieName);
            var cookies = Cookie.ArrayFromJson(serializedCookies);
            return Task.FromResult(cookies);
        }

        // Override because BaseWebView.GetRawTextureData() leaks memory on UWP.
        public override Task<byte[]> GetRawTextureData() {

            var taskSource = new TaskCompletionSource<byte[]>();
            _invokeDataOperation(taskSource.SetResult, WebView_getRawTextureData);
            return taskSource.Task;
        }

        public static void GloballySetUserAgent(bool mobile) => WebView_globallySetUserAgentToMobile(mobile);

        public static void GloballySetUserAgent(string userAgent) => WebView_globallySetUserAgent(userAgent);

        /// <summary>
        /// This method automatically gets called when the app changes focus.
        /// </summary>
        public static void HandleAppFocusChanged(bool focused) {

            if (VXUtils.XRSettings.enabled) {
                WebView_handleMixedRealityAppFocusChanged(focused);
            }
        }

        /// <summary>
        /// This method automatically gets called when the application starts.
        /// </summary>
        public static void InitializePlugin() {

            var sendMessageFunction = Marshal.GetFunctionPointerForDelegate<Action<string, string, string>>(_unitySendMessage);
            WebView_setSendMessageFunction(sendMessageFunction);

            var dataResultCallback = Marshal.GetFunctionPointerForDelegate<DataResultCallback>(_dataResultCallback);
            WebView_setDataResultCallback(dataResultCallback);

            var logInfo = Marshal.GetFunctionPointerForDelegate<Action<string>>(_logInfo);
            var logWarning = Marshal.GetFunctionPointerForDelegate<Action<string>>(_logWarning);
            var logError = Marshal.GetFunctionPointerForDelegate<Action<string>>(_logError);
            WebView_setLogFunctions(logInfo, logWarning, logError);
            WebView_logVersionInfo();
        }

        public async Task Init(int width, int height) {

            await _initBase(width, height);
            _nativeWebViewPtr = WebView_new(gameObject.name, width, height, VXUtils.XRSettings.enabled);
            if (_nativeWebViewPtr == IntPtr.Zero) {
                throw new WebViewUnavailableException("Failed to instantiate a new webview. This could indicate that you're using an expired trial version of 3D WebView.");
            }
        }

        /// <see cref="IWithNative2DMode"/>
        public async Task InitInNative2DMode(Rect rect) {

            Native2DModeEnabled = true;
            _rect = rect;
            Visible = true;
            await _initBase((int)rect.width, (int)rect.height, createTexture: false);
            _nativeWebViewPtr = WebView_newInNative2DMode(
                gameObject.name,
                (int)rect.x,
                (int)rect.y,
                (int)rect.width,
                (int)rect.height
            );
        }

        public static UwpWebView Instantiate() => new GameObject().AddComponent<UwpWebView>();

        /// <see cref="IWithMovablePointer"/>
        public void MovePointer(Vector2 normalizedPoint) {

            _assertValidState();
            var pixelsPoint = _convertNormalizedToPixels(normalizedPoint);
            WebView_movePointer(_nativeWebViewPtr, pixelsPoint.x, pixelsPoint.y);
        }

        /// <see cref="IWithPointerDownAndUp"/>
        public void PointerDown(Vector2 point) => _pointerDown(point, MouseButton.Left, 1);

        /// <see cref="IWithPointerDownAndUp"/>
        public void PointerDown(Vector2 point, PointerOptions options) {

            if (options == null) {
                options = new PointerOptions();
            }
            _pointerDown(point, options.Button, options.ClickCount);
        }

        /// <see cref="IWithPointerDownAndUp"/>
        public void PointerUp(Vector2 point) => _pointerUp(point, MouseButton.Left, 1);

        /// <see cref="IWithPointerDownAndUp"/>
        public void PointerUp(Vector2 point, PointerOptions options) {

            if (options == null) {
                options = new PointerOptions();
            }
            _pointerUp(point, options.Button, options.ClickCount);
        }

        public static Task<bool> SetCookie(Cookie cookie) {

            if (cookie == null) {
                throw new ArgumentException("Cookie cannot be null.");
            }
            if (!cookie.IsValid) {
                throw new ArgumentException("Cannot set invalid cookie: " + cookie);
            }
            var success = WebView_setCookie(cookie.ToJson());
            return Task.FromResult(success);
        }

        /// <summary>
        /// By default, web pages cannot access the device's geolocation via JavaScript. However, geolocation
        /// access can be granted to **all web pages** via the following steps:
        ///
        /// - Call `UwpWebView.SetGeolocationPermissionEnabled(true)` at the start of the app.
        /// - Enable the "Location" capability in "UWP Player Settings" -> "Publishing Settings" -> "Capabilities".
        /// - Enable the "Allow apps to access your location" setting in the device's Location Privacy Settings.
        ///
        /// If all three of those conditions are met, then when a web page tries to access a location API, the system will
        /// present the user with a popup to allow or deny location access. If the user allows access, then
        /// **all web pages** will be able to access the device location.
        /// </summary>
        /// <example>
        /// <code>
        /// void Awake() {
        ///     #if UNITY_WSA &amp;&amp; !UNITY_EDITOR
        ///         UwpWebView.SetGeolocationPermissionEnabled(true);
        ///     #endif
        /// }
        /// </code>
        /// </example>
        public static void SetGeolocationPermissionEnabled(bool enabled) => WebView_setGeolocationPermissionEnabled(enabled);

        /// <see cref="IWithNative2DMode"/>
        public void SetNativeZoomEnabled(bool enabled) {

            WebViewLogger.LogWarning("3D WebView for UWP doesn't support native zooming, so the call to IWithNative2DMode.SetNativeZoomEnabled() will be ignored.");
        }

        /// <see cref="IWithNative2DMode"/>
        public void SetRect(Rect rect) {

            _assertValidState();
            _assertNative2DModeEnabled();
            _rect = rect;
            WebView_setRect(_nativeWebViewPtr, (int)rect.x, (int)rect.y, (int)rect.width, (int)rect.height);
        }

        public static void SetStorageEnabled(bool enabled) => WebView_setStorageEnabled(enabled);

        /// <see cref="IWithNative2DMode"/>
        public void SetVisible(bool visible) {

            _assertValidState();
            _assertNative2DModeEnabled();
            Visible = visible;
            WebView_setVisible(_nativeWebViewPtr, visible);
        }

        public static bool ValidateGraphicsApi() {

            var isValid = SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.Direct3D11;
            if (!isValid) {
                WebViewLogger.LogError("Unsupported graphics API: 3D WebView for UWP requires Direct3D11. Please go to Player Settings and set \"Graphics APIs\" to Direct3D11.");
            }
            return isValid;
        }

    #region Non-public members
        delegate void DataResultCallback(string gameObjectName, string resultCallbackId, IntPtr imageBytes, int imageBytesLength);
        Dictionary<string, Action<byte[]>> _pendingDataResultCallbacks = new Dictionary<string, Action<byte[]>>();
        readonly WaitForEndOfFrame _waitForEndOfFrame = new WaitForEndOfFrame();

        void _assertNative2DModeEnabled() {

            if (!Native2DModeEnabled) {
                throw new InvalidOperationException("IWithNative2DMode methods can only be called on a webview with Native 2D Mode enabled.");
            }
        }

        [AOT.MonoPInvokeCallback(typeof(DataResultCallback))]
        static void _dataResultCallback(string gameObjectName, string resultCallbackId, IntPtr unmanagedBytes, int bytesLength) {

            // Load the results into a managed array.
            var managedBytes = new byte[bytesLength];
            Marshal.Copy(unmanagedBytes, managedBytes, 0, bytesLength);

            Dispatcher.RunOnMainThread(() => {
                var gameObj = GameObject.Find(gameObjectName);
                if (gameObj == null) {
                    WebViewLogger.LogError($"Unable to process the data result, because there is no GameObject named '{gameObjectName}'");
                    return;
                }
                var webView = gameObj.GetComponent<UwpWebView>();
                webView._handleDataResult(resultCallbackId, managedBytes);
            });
        }

        void _handleDataResult(string resultCallbackId, byte[] bytes) {

            var callback = _pendingDataResultCallbacks[resultCallbackId];
            _pendingDataResultCallbacks.Remove(resultCallbackId);
            callback(bytes);
        }

        void _invokeDataOperation(Action<byte[]> callback, Action<IntPtr, string> nativeDataMethod) {

            _assertValidState();
            try {
                string resultCallbackId = null;
                if (callback != null) {
                    resultCallbackId = Guid.NewGuid().ToString();
                    _pendingDataResultCallbacks[resultCallbackId] = callback;
                }
                nativeDataMethod(_nativeWebViewPtr, resultCallbackId);
            } catch (Exception e) {
                WebViewLogger.LogError("An exception occurred in while getting the webview data: " + e);
                callback(new byte[0]);
            }
        }

        [AOT.MonoPInvokeCallback(typeof(Action<string>))]
        static void _logInfo(string message) => WebViewLogger.Log(message, false);

        [AOT.MonoPInvokeCallback(typeof(Action<string>))]
        static void _logWarning(string message) => WebViewLogger.LogWarning(message, false);

        [AOT.MonoPInvokeCallback(typeof(Action<string>))]
        static void _logError(string message) => WebViewLogger.LogError(message, false);

        // Start the coroutine from OnEnable so that the coroutine
        // is restarted if the object is deactivated and then reactivated.
        void OnEnable() => StartCoroutine(_renderPluginOncePerFrame());

        void _pointerDown(Vector2 normalizedPoint, MouseButton mouseButton, int clickCount) {

            _assertValidState();
            var pixelsPoint = _convertNormalizedToPixels(normalizedPoint);
            WebView_pointerDown(_nativeWebViewPtr, pixelsPoint.x, pixelsPoint.y, (int)mouseButton, clickCount);
        }

        void _pointerUp(Vector2 normalizedPoint, MouseButton mouseButton, int clickCount) {

            _assertValidState();
            var pixelsPoint = _convertNormalizedToPixels(normalizedPoint);
            WebView_pointerUp(_nativeWebViewPtr, pixelsPoint.x, pixelsPoint.y, (int)mouseButton, clickCount);
        }

        IEnumerator _renderPluginOncePerFrame() {

            while (true) {
                yield return _waitForEndOfFrame;
                if (Native2DModeEnabled) {
                    break;
                }
                if (_nativeWebViewPtr != IntPtr.Zero && !IsDisposed) {
                    int pointerId = WebView_depositPointer(_nativeWebViewPtr);
                    GL.IssuePluginEvent(WebView_getRenderFunction(), pointerId);
                }
            }
        }

        [AOT.MonoPInvokeCallback(typeof(Action<string, string, string>))]
        static void _unitySendMessage(string gameObjectName, string methodName, string message) {

            Dispatcher.RunOnMainThread(() => {
                var gameObj = GameObject.Find(gameObjectName);
                if (gameObj == null) {
                    WebViewLogger.LogWarning($"Unable to deliver a message from the native plugin to a webview GameObject because there is no longer a GameObject named '{gameObjectName}'. This can sometimes happen directly after destroying a webview. In that case, it is benign and this message can be ignored.");
                    return;
                }
                gameObj.SendMessage(methodName, message);
            });
        }

        [DllImport(_dllName)]
        static extern void WebView_captureScreenshot(IntPtr webViewPtr, string resultCallbackId);

        [DllImport(_dllName)]
        static extern void WebView_clearAllData();

        [DllImport(_dllName)]
        static extern void WebView_deleteCookies(string url, string cookieName);

        [DllImport(_dllName)]
        static extern int WebView_depositPointer(IntPtr pointer);

        [DllImport(_dllName)]
        static extern string WebView_getCookies(string url, string cookieName);

        [DllImport(_dllName)]
        static extern void WebView_getRawTextureData(IntPtr webViewPtr, string resultCallbackId);

        [DllImport(_dllName)]
        static extern IntPtr WebView_getRenderFunction();

        [DllImport(_dllName)]
        static extern void WebView_globallySetUserAgentToMobile(bool mobile);

        [DllImport(_dllName)]
        static extern void WebView_globallySetUserAgent(string userAgent);

        [DllImport(_dllName)]
        static extern void WebView_handleMixedRealityAppFocusChanged(bool focused);

        [DllImport(_dllName)]
        static extern IntPtr WebView_logVersionInfo();

        [DllImport (_dllName)]
        static extern void WebView_movePointer(IntPtr webViewPtr, int x, int y);

        [DllImport(_dllName)]
        static extern IntPtr WebView_new(string gameObjectName, int width, int height, bool mixedRealityEnabled);

        [DllImport(_dllName)]
        static extern IntPtr WebView_newInNative2DMode(string gameObjectName, int x, int y, int width, int height);

        [DllImport (_dllName)]
        static extern void WebView_pointerDown(IntPtr webViewPtr, int x, int y, int mouseButton, int clickCount);

        [DllImport (_dllName)]
        static extern void WebView_pointerUp(IntPtr webViewPtr, int x, int y, int mouseButton, int clickCount);

        [DllImport(_dllName)]
        static extern void WebView_removePointer(IntPtr pointer);

        [DllImport(_dllName)]
        static extern bool WebView_setCookie(string serializedCookie);

        [DllImport(_dllName)]
        static extern int WebView_setDataResultCallback(IntPtr callback);

        [DllImport(_dllName)]
        static extern int WebView_setGeolocationPermissionEnabled(bool enabled);

        [DllImport (_dllName)]
        static extern void WebView_setRect(IntPtr webViewPtr, int x, int y, int width, int height);

        [DllImport(_dllName)]
        static extern int WebView_setLogFunctions(
            IntPtr logInfoFunction,
            IntPtr logWarningFunction,
            IntPtr logErrorFunction
        );

        [DllImport(_dllName)]
        static extern int WebView_setSendMessageFunction(IntPtr sendMessageFunction);

        [DllImport(_dllName)]
        static extern void WebView_setStorageEnabled(bool enabled);

        [DllImport (_dllName)]
        static extern void WebView_setVisible(IntPtr webViewPtr, bool visible);
    #endregion

    #region Obsolete APIs
        // Added in v3.13, deprecated in v4.0.
        [Obsolete("UwpWebView.GetCookies(url, callback) is now deprecated. Please use Web.CookieManager.GetCookies() instead: https://developer.vuplex.com/webview/CookieManager#GetCookies")]
        public static async void GetCookies(string url, Action<Cookie[]> callback) {
            var cookies = await GetCookies(url);
            callback(cookies);
        }

        // Added in v3.9, deprecated in v4.0.
        [Obsolete("UwpWebView.DeleteCookies(url, callback) is now deprecated. Please use Web.CookieManager.DeleteCookies() instead: https://developer.vuplex.com/webview/CookieManager#DeleteCookies")]
        public static async void DeleteCookies(string url, Action callback) {
            await DeleteCookies(url);
            callback();
        }

        // Added in v3.13, deprecated in v4.0.
        [Obsolete("UwpWebView.SetCookie(cookie, callback) is now deprecated. Please use Web.CookieManager.SetCookie() instead: https://developer.vuplex.com/webview/CookieManager#SetCookie")]
        public static async void SetCookie(Cookie cookie, Action<bool> callback) {
            var result = await SetCookie(cookie);
            callback(result);
        }
    #endregion
    }
}
#endif
