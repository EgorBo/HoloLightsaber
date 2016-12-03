#if __ANDROID__
using Com.Google.Vrtoolkit.Cardboard.Sensors;
using Shared;
using Urho;
using System;

namespace Lightsaber
{
	public class MotionDetector
	{
		HeadTracker headTracker;
		Action<Vector3Dto> listener;
		DateTime lastUpdate;

		public void StartListening(Action<Vector3Dto> listener)
		{
			throw new NotImplementedException();
			this.listener = listener;
			headTracker = HeadTracker.CreateFromContext(Android.App.Application.Context);
			headTracker.StartTracking();
		}

		Vector3Dto GetRotation()
		{
			var view = new float[16];
			headTracker.GetLastHeadView(view, 0);
			var m4 = new Matrix4(
				view[0],  view[1],  view[2],  view[3],
				view[4],  view[5],  view[6],  view[7],
				view[8],  view[9],  view[10], view[11],
				view[12], view[13], view[14], view[15]);
			var rot = m4.Rotation;
			return new Vector3Dto(); //TODO: pitch, roll - ToEulerAngles?
			//return new Vector4Dto(-rot.X, -rot.Y, rot.Z, rot.W);
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
		Action<Vector3Dto> listener;
		DateTime lastUpdate;

		public async void StartListening(Action<Vector3Dto> listener)
		{
			this.listener = listener;
			manager = new CMMotionManager();
			manager.StartDeviceMotionUpdates(Foundation.NSOperationQueue.CurrentQueue, (motion, error) =>
			{
				var duration = DateTime.UtcNow - lastUpdate;
				var att = manager.DeviceMotion.Attitude;
				listener(new Vector3Dto(
					MathHelper.RadiansToDegrees((float)-att.Pitch),
					MathHelper.RadiansToDegrees((float)-att.Yaw),
					0));
				lastUpdate = DateTime.UtcNow;
			});
		}
	}
}
#endif
