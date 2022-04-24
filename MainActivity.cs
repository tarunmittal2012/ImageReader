using Android;
using Android.App;
using Android.Content.PM;
using Android.Gms.Vision;
using Android.Gms.Vision.Texts;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.App;
using AndroidX.Core.App;
using System.Text;
using static Android.Gms.Vision.Detector;

namespace OCRReading
{
    [Activity(Label = "@string/app_name", Theme = "@style/Theme.AppCompat.Light.NoActionBar", MainLauncher = true)]
    public class MainActivity : AppCompatActivity,ISurfaceHolderCallback,IProcessor
    {
        public SurfaceView cameraView;
        private TextView scannedText;
        public CameraSource cameraSource;
        public const int RequestCameraID = 1001;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.activity_main);
            cameraView = FindViewById<SurfaceView>(Resource.Id.surfaceView);
            scannedText = FindViewById<TextView>(Resource.Id.textView);

            TextRecognizer textRecognizer = new TextRecognizer.Builder(ApplicationContext).Build();
            if(textRecognizer.IsOperational)
            {
                cameraSource = new CameraSource.Builder(ApplicationContext, textRecognizer)
                    .SetAutoFocusEnabled(true)
                    .SetFacing(CameraFacing.Back)
                    .SetRequestedFps(2.0f)
                    .SetRequestedPreviewSize(1280, 1024)
                    .Build();
                cameraView.Holder.AddCallback(this);
                textRecognizer.SetProcessor(this);
            }

        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Permission[] grantResults)
        {
            switch(requestCode)
            {
                case RequestCameraID:
                    {
                        if(grantResults[0] == Permission.Granted)
                        {
                            cameraSource.Start(cameraView.Holder);
                        }
                        break;
                    }
            }
              
            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }

        public void SurfaceChanged(ISurfaceHolder holder, [GeneratedEnum] Format format, int width, int height)
        {
        }

        public void SurfaceCreated(ISurfaceHolder holder)
        {
            if(ActivityCompat.CheckSelfPermission(ApplicationContext,Manifest.Permission.Camera)!=Android.Content.PM.Permission.Granted)
            {
                ActivityCompat.RequestPermissions(this, new string[]
                {
                    Android.Manifest.Permission.Camera
                }, RequestCameraID);
                return;
            }
            cameraSource.Start(cameraView.Holder);
        }

        public void SurfaceDestroyed(ISurfaceHolder holder)
        {
        }

        public void ReceiveDetections(Detections detections)
        {
            SparseArray items = detections.DetectedItems;
            if(items.Size()!=0)
            {
                scannedText.Post(() => {
                    StringBuilder stringBuilder = new StringBuilder();
                    for(int i=0;i<items.Size();++i)
                    {
                        stringBuilder.Append(((TextBlock)items.ValueAt(i)).Value);
                        stringBuilder.Append("\n");
                    }
                    scannedText.Text = stringBuilder.ToString();
                });

            }
        }

        public void Release()
        {
        }
    }
}