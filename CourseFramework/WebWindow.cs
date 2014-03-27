using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using AwesomiumSharp;
using Mogre;
using System.Runtime.InteropServices;
using ManualResourceLoader = Ogre.ManualResourceLoader;

namespace CourseFramework
{
    class WebWindow : IDisposable
    {
        // Used to give each created OGRE object a unique name
        private static int lastId = 0;
        private int id;

        private static Overlay overlay = null;

        private int x;
        private int y;
        private int width;
        private int height;

        private WebView view;
        private PanelOverlayElement panel;
        private TexturePtr texture;
        private MaterialPtr material;

        public WebWindow(int x, int y, int width, int height)
        {
            id = lastId++;
            this.x = x;
            this.y = y;
            this.width = width;
            this.height = height;

            // Create the WebView
            view = WebCore.CreateWebView(width, height);
            view.SetTransparent(false);
            view.IsDirtyChanged += OnDirty;
            view.LoadCompleted += OnLoaded;

            CreateOverlay();
        }

        void CreateOverlay()
        {
            // Create panel
            if (overlay == null)
            {
                overlay = OverlayManager.Singleton.Create("WebWindowOverlay");
                overlay.ZOrder = 0;
                overlay.Show();
            }
            panel = (PanelOverlayElement)OverlayManager.Singleton.CreateOverlayElement("Panel", "WebWindowPanel" + id);
            panel.MetricsMode = GuiMetricsMode.GMM_PIXELS;
            panel.SetPosition(x, y);
            panel.SetDimensions(width, height);

            // Create a texture to draw the web view to
            texture = TextureManager.Singleton.CreateManual("WebWindowTexture" + id, ResourceGroupManager.DEFAULT_RESOURCE_GROUP_NAME,
                                                            TextureType.TEX_TYPE_2D,
                                                            (uint) width, (uint) height, 0,
                                                            PixelFormat.PF_BYTE_BGRA, (int)TextureUsage.TU_DYNAMIC_WRITE_ONLY_DISCARDABLE);/*
            texture.CreateInternalResources();
            var buffer = texture.GetBuffer();
            unsafe
            {
                buffer.Lock(HardwareBuffer.LockOptions.HBL_NORMAL);
                var bytes = (byte*)buffer.CurrentLock.data;
                for (int i = 0; i < buffer.SizeInBytes; i++)
                    bytes[i] = 255;
            }
            buffer.Unlock();*/

            // Create a material that uses the created texture
            material = (MaterialPtr) MaterialManager.Singleton.Create("WebWindowMaterial" + id, ResourceGroupManager.DEFAULT_RESOURCE_GROUP_NAME);
            var tech = material.CreateTechnique();
            tech.SetDepthCheckEnabled(false);
            
            var pass = tech.CreatePass();
            pass.SetSceneBlending(SceneBlendType.SBT_TRANSPARENT_ALPHA);
            var texUnit = pass.CreateTextureUnitState(texture.Name);
            panel.MaterialName = material.Name;

            overlay.Add2D(panel);
            panel.Show();
        }

        public void LoadUrl(string url)
        {
            view.LoadURL(url);
        }

        public void LoadFile(string fileName)
        {
            view.LoadFile(fileName);
        }

        void OnLoaded(object sender, EventArgs e)
        {
            OnDirty(sender, e);
        }

        void OnDirty(object sender, EventArgs e)
        {
            if (!view.IsDirty)
                return;

            // Draw the new content of the WebView to the overlay
            var viewBuffer = view.Render();
            var textureBuffer = texture.GetBuffer();
            
            // Make sure both buffers are the size we expect them to be
            Debug.Assert(viewBuffer.Width * viewBuffer.Height * 4 == textureBuffer.SizeInBytes && textureBuffer.SizeInBytes == viewBuffer.Rowspan * viewBuffer.Height);

            // Some scary pointer code in order to copy pixels from the view buffer to the texture buffer
            unsafe
            {
                var dst = (IntPtr)textureBuffer.Lock(HardwareBuffer.LockOptions.HBL_DISCARD);

                //var test = new byte[viewBuffer.Rowspan*viewBuffer.Height];
                //Marshal.Copy(viewBuffer.Buffer, test, 0, test.Length);

                viewBuffer.CopyTo(dst, viewBuffer.Rowspan, 4, false, false);
                /*
                for (var i = 0; i < textureBuffer.SizeInBytes; i++)
                {/*
                    dst[i] = src[3];
                    dst[i + 1] = src[2];
                    dst[i + 2] = src[1];
                    dst[1 + 3] = src[0];
                    dst[i] = 0;
                }*/
            }
            textureBuffer.Unlock();
            //texture.CreateInternalResources();
            Console.WriteLine("Texture created");
        }

        public void Dispose()
        {
            MaterialManager.Singleton.Remove(material.Name);
            TextureManager.Singleton.Remove(texture.Name);
            overlay.Remove2D(panel);
            OverlayManager.Singleton.DestroyOverlayElement(panel);

            material.Dispose();
            texture.Dispose();
        }
    }
}
