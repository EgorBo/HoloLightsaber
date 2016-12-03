using ProtoBuf;

namespace Shared
{
	[ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
	[ProtoInclude(100, typeof(MotionDto))]
	[ProtoInclude(200, typeof(ColorChangedDto))]
	public class BaseDto { }
	
	[ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
	public class MotionDto : BaseDto
	{
		public Vector3Dto EulerAngles { get; set; }
	}

	[ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
	public class ColorChangedDto : BaseDto
	{
		public Vector3Dto Color { get; set; }
	}




	[ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
	public struct Vector3Dto
	{
		public Vector3Dto(float x, float y, float z)
		{
			X = x;
			Y = y;
			Z = z;
		}

		public float X { get; set; }
		public float Y { get; set; }
		public float Z { get; set; }
	}

	[ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
	public struct Vector4Dto
	{
		public Vector4Dto(float x, float y, float z, float w)
		{
			X = x;
			Y = y;
			Z = z;
			W = w;
		}

		public float W { get; set; }
		public float X { get; set; }
		public float Y { get; set; }
		public float Z { get; set; }
	}
}
