#if __ANDROID__
using Com.Google.Vrtoolkit.Cardboard.Sensors;
using Shared;
using Urho;
using System;
using System.Threading.Tasks;

namespace Lightsaber
{
	public class MotionDetector
	{
		HeadTracker headTracker;

		public async void StartListening(Action<Vector3Dto> listener)
		{
			headTracker = HeadTracker.CreateFromContext(Android.App.Application.Context);
			headTracker.StartTracking();
			await Task.Delay(1000);
			while (true)
			{
				await Task.Delay(16);

				var view = new float[16];
				headTracker.GetLastHeadView(view, 0);
				var m4 = new Matrix4(
					view[0], view[1], view[2], view[3],
					view[4], view[5], view[6], view[7],
					view[8], view[9], view[10], view[11],
					view[12], view[13], view[14], view[15]);
				var rot = m4.Rotation;
				listener(new Vector3Dto(rot.PitchAngle, rot.YawAngle, 0));
			}
		}
	}
}
#elif __IOS__
using CoreMotion;
using Shared;
using Urho;
using System;

namespace Lightsaber
{
	public class MotionDetector
	{
		CMMotionManager manager;

		public void StartListening(Action<Vector3Dto> listener)
		{
			manager = new CMMotionManager();
			if (!manager.DeviceMotionAvailable)
				throw new InvalidOperationException("DeviceMotion is not available");

			manager.DeviceMotionUpdateInterval = 1 / 60f; // 60fps
			manager.StartDeviceMotionUpdates(Foundation.NSOperationQueue.CurrentQueue, (motion, error) =>
			{
				var att = manager.DeviceMotion.Attitude;
				listener(new Vector3Dto(
					MathHelper.RadiansToDegrees((float)-att.Pitch),
					MathHelper.RadiansToDegrees((float)-att.Yaw),
					0));
			});
		}
	}
}
#endif
