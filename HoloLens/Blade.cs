using System;
using Urho;
using Urho.Actions;
using Urho.Shapes;

namespace Lightsaber.HoloLens
{
	public class Blade : Component
	{
		Box glowModel;
		Node bladeNode;
		StaticModel bladeModel;

		[Preserve] public Blade() { }
		[Preserve] public Blade(IntPtr ptr) : base(ptr) { }

		public bool Active { get; private set; }

		public void SetColor(Color c)
		{
			//float glowF = 10;
			//bladeModel.GetMaterial(0).SetShaderParameter("MatDiffColor", new Color(c.R * glowF + 1, c.G * glowF + 1, c.B * glowF + 1));
			glowModel.GetMaterial(0).SetShaderParameter("MatDiffColor", new Color(c, 0.4f));
			Toggle();
		}

		public void Toggle()
		{
			Active = !Active;
			var to = new Vector3(1f, 1f, 7) / 1.75f;
			bladeNode.Scale = new Vector3(1, 1, 0.1f) / 1.75f;
			bladeNode.Position = new Vector3(0f, 0f, 0.6f);

			if (Active)
			{
				bladeNode.RunActions(new ScaleTo(0.5f, to.X, to.Y, to.Z));
				bladeNode.RunActions(new MoveTo(0.5f, new Vector3(0f, 0f, 2.5f)));
			}
		}

		public override void OnAttachedToNode(Node node)
		{
			base.OnAttachedToNode(node);

			bladeNode = node.CreateChild();
			bladeNode.SetScale(0);
			bladeModel = bladeNode.CreateComponent<StaticModel>();
			bladeModel.Model = CoreAssets.Models.Box;
			bladeModel.SetMaterial(Material.FromColor(Color.White));

			var glowNode = bladeNode.CreateChild();
			glowNode.Scale = new Vector3(3f, 3f, 1f);
			glowNode.Position = new Vector3(0.02f, 0, 0.02f);
			glowModel = glowNode.CreateComponent<Box>();
		}
	}
}