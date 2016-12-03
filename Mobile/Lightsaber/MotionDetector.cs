#if __ANDROID__
using System.Threading.Tasks;
using Com.Google.Vrtoolkit.Cardboard.Sensors;
using Shared;
using Urho;

namespace Lightsaber
{
	public class MotionDetector
	{
		HeadTracker headTracker;

		public async Task StartListening()
		{
			headTracker = HeadTracker.CreateFromContext(Android.App.Application.Context);
			headTracker.StartTracking();
		}

		public Vector4Dto GetLastQuaternion()
		{
			var view = new float[16];
			headTracker.GetLastHeadView(view, 0);
			var m4 = new Matrix4(
				view[0],  view[1],  view[2],  view[3],
				view[4],  view[5],  view[6],  view[7],
				view[8],  view[9],  view[10], view[11],
				view[12], view[13], view[14], view[15]);
			var rot = m4.Rotation;
			return new Vector4Dto(-rot.X, -rot.Y, rot.Z, rot.W);
		}
	}
}
#elif __IOS__
using System.Threading.Tasks;
using CoreMotion;
using Shared;
using Urho;

namespace Lightsaber
{
	public class MotionDetector
	{
		CMMotionManager manager;

		public async Task StartListening()
		{
			var tcs = new TaskCompletionSource<bool>();
			manager = new CMMotionManager();
			manager.StartAccelerometerUpdates();
			manager.StartDeviceMotionUpdates(Foundation.NSOperationQueue.CurrentQueue, (motion, error) => { tcs?.TrySetResult(true); });
			await tcs.Task;
			tcs = null;
		}

		public Vector4Dto GetLastQuaternion()
		{
			var q = manager.DeviceMotion.Attitude.Quaternion;
			return new Vector4Dto((float)q.x, (float)q.y, (float)q.z, (float)q.w);
		}
	}
}
#endif
