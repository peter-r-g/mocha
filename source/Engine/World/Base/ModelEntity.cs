﻿namespace Mocha;

[Category( "World" ), Title( "Model Entity" ), Icon( FontAwesome.Cube )]
public partial class ModelEntity : BaseEntity
{
	[HideInInspector]
	public SceneObject SceneObject { get; set; }

	public ModelEntity()
	{
	}

	protected override void CreateNativeEntity()
	{
		NativeHandle = Glue.Entities.CreateModelEntity();
	}

	public void SetModel( Model model )
	{
		Glue.Entities.SetModel( NativeHandle, model.NativeModel.NativePtr );
	}
}
