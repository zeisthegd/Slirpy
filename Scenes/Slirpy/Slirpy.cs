using Godot;
using System;

public class Slirpy : KinematicBody
{
	const float CAMERA_MOUSE_ROTATION_SPEED = 0.001F;
	const float CAMERA_CONTROLLER_ROTATION_SPEED = 3.0F;
	const float CAMERA_X_ROTATION_MIN = -40F;
	const float CAMERA_X_ROTATION_MAX = 30F;

	const float DIRECTION_INTERPOLATE_SPEED = 1F;
	const float MOTION_INTERPOLATE_SPEED = 10F;
	const float ROTATION_INTERPOLATE_SPEED = 10F;

	Transform orientation = new Transform();
	Transform rootMotion = new Transform();
	Vector2 motion = new Vector2();
	Vector3 velocity = new Vector3();

	float cameraXRotation = 0.0F;

	Vector3 initialPosition;
	Vector3 gravity = (float)ProjectSettings.GetSetting("physics/3d/default_gravity") * (Vector3)ProjectSettings.GetSetting("physics/3d/default_gravity_vector");

	Spatial playerModel;

	Spatial cameraBase;
	Spatial cameraRot;
	AnimationPlayer cameraAnimation;
	SpringArm cameraSpringArm;
	Camera cameraCamera;

	AnimationTree animationTree;




	public override void _Ready()
	{
		Input.SetMouseMode(Input.MouseMode.Captured);

		initialPosition = Transform.origin;

		GetComponents();

		PreInitOrientationTransform();


	}
	public override void _PhysicsProcess(float delta)
	{
		var cameraMove = new Vector2(
			Input.GetActionStrength("view_right") - Input.GetActionStrength("view_left"),
			Input.GetActionStrength("view_up") - Input.GetActionStrength("view_down"));
		var cameraSpeedThisFrame = delta * CAMERA_CONTROLLER_ROTATION_SPEED;

		RotateCamera(cameraMove * cameraSpeedThisFrame);

		var motionTarget = new Vector2(
			Input.GetActionStrength("move_right") - Input.GetActionStrength("move_left"),
			Input.GetActionStrength("move_back") - Input.GetActionStrength("move_forward"));
		motion = motion.LinearInterpolate(motionTarget, MOTION_INTERPOLATE_SPEED * delta);

		var cameraBasis = cameraRot.GlobalTransform.basis;
		var cameraZ = cameraBasis.z;
		var cameraX = cameraBasis.x;

		cameraZ.y = 0;
		cameraZ = cameraZ.Normalized();
		cameraX.y = 0;
		cameraX = cameraX.Normalized();

		//Convert orientation to quarternion for interpolation rotation
		var target = cameraX * motion.x + cameraZ * motion.y;
		if (target.Length() > 0.001F)
		{
			var qFrom = orientation.basis.Quat();
			var qTo = Transform.LookingAt(target, Vector3.Up).basis.Quat();
			//Interpolate current rotation with desired one
			orientation.basis = new Basis(qFrom.Slerp(qTo, delta * ROTATION_INTERPOLATE_SPEED));
		}

		rootMotion = animationTree.GetRootMotionTransform();

		animationTree.Set("parameters/state/current", 1);

		orientation *= rootMotion;

		var horiVelocity = orientation.origin / GetPhysicsProcessDeltaTime();
		velocity.x = horiVelocity.x;
		velocity.z = horiVelocity.z;
		velocity += (gravity * delta);
		velocity = MoveAndSlide(velocity, Vector3.Up);

		orientation.origin = new Vector3();//Clear accumulated root motion displacement (was applied to speed)
		orientation = orientation.Orthonormalized();

		Transform playerModelGlbTransform = playerModel.GlobalTransform;
		playerModelGlbTransform.basis = orientation.basis;
		playerModel.GlobalTransform = playerModelGlbTransform;

	}

	public override void _Input(InputEvent @event)
	{
		InputEventMouseMotion inputEventMouseMotion;
		if(@event is InputEventMouseMotion)
		{
			var cameraSpeedThisFrame = CAMERA_MOUSE_ROTATION_SPEED;
			inputEventMouseMotion = (InputEventMouseMotion)@event;
			RotateCamera(inputEventMouseMotion.Relative * cameraSpeedThisFrame);

		}
		
	}


	private void PreInitOrientationTransform()
	{
		orientation = playerModel.GlobalTransform;
		orientation.origin = new Vector3();
	}

	private void GetComponents()
	{
		playerModel = (Spatial)GetNode("PlayerModel");
		cameraBase = (Spatial)GetNode("CameraBase");
		cameraRot = (Spatial)cameraBase.GetNode("CameraRot");
		cameraSpringArm = (SpringArm)cameraRot.GetNode("SpringArm");
		cameraAnimation = (AnimationPlayer)cameraBase.GetNode("AnimationPlayer");

		animationTree = (AnimationTree)GetNode("AnimationTree");
	}

	private void RotateCamera(Vector2 move)
	{
		cameraBase.RotateY(-move.x);
		//Alter relative transforms, camera needs to be renormaliedl
		cameraBase.Orthonormalize();
		cameraXRotation += move.y;
		cameraXRotation = Mathf.Clamp(cameraXRotation, Mathf.Deg2Rad(CAMERA_X_ROTATION_MIN), Mathf.Deg2Rad(CAMERA_X_ROTATION_MAX));

		Vector3 camCrtRot = cameraRot.Rotation;
		cameraRot.Rotation = new Vector3(cameraXRotation, camCrtRot.y, camCrtRot.z);
	}
}
