using System.Runtime.InteropServices;

[StructLayout( LayoutKind.Sequential )]
public struct UnmanagedArgs
{
    public IntPtr __LogManager_ManagedInfoMethodPtr;
	public IntPtr __LogManager_ManagedWarningMethodPtr;
	public IntPtr __LogManager_ManagedErrorMethodPtr;
	public IntPtr __LogManager_ManagedTraceMethodPtr;
	public IntPtr __LogManager_GetLogHistoryMethodPtr;
	public IntPtr __LogManager_CtorMethodPtr;
	public IntPtr __Engine_QuitMethodPtr;
	public IntPtr __Engine_GetCurrentTickMethodPtr;
	public IntPtr __Engine_GetFrameDeltaTimeMethodPtr;
	public IntPtr __Engine_GetTickDeltaTimeMethodPtr;
	public IntPtr __Engine_GetFramesPerSecondMethodPtr;
	public IntPtr __Engine_GetTimeMethodPtr;
	public IntPtr __Engine_GetProjectPathMethodPtr;
	public IntPtr __Texture_CtorMethodPtr;
	public IntPtr __Texture_SetDataMethodPtr;
	public IntPtr __Texture_CopyMethodPtr;
	public IntPtr __Material_CreateResourcesMethodPtr;
	public IntPtr __Material_ReloadMethodPtr;
	public IntPtr __Material_CtorMethodPtr;
	public IntPtr __Model_AddMeshMethodPtr;
	public IntPtr __Model_CtorMethodPtr;
	public IntPtr __Editor_GetContextPointerMethodPtr;
	public IntPtr __Editor_TextBoldMethodPtr;
	public IntPtr __Editor_TextSubheadingMethodPtr;
	public IntPtr __Editor_TextHeadingMethodPtr;
	public IntPtr __Editor_TextMonospaceMethodPtr;
	public IntPtr __Editor_TextLightMethodPtr;
	public IntPtr __Editor_GetGPUNameMethodPtr;
	public IntPtr __Editor_InputTextMethodPtr;
	public IntPtr __Editor_RenderViewDropdownMethodPtr;
	public IntPtr __Editor_GetWindowSizeMethodPtr;
	public IntPtr __Editor_GetRenderSizeMethodPtr;
	public IntPtr __Editor_GetVersionNameMethodPtr;
	public IntPtr __Editor_ImageMethodPtr;
	public IntPtr __Editor_BeginMainStatusBarMethodPtr;
	public IntPtr __Editor_DrawGraphMethodPtr;
	public IntPtr __Entities_CreateBaseEntityMethodPtr;
	public IntPtr __Entities_CreateModelEntityMethodPtr;
	public IntPtr __Entities_SetViewmodelMethodPtr;
	public IntPtr __Entities_SetUIMethodPtr;
	public IntPtr __Entities_SetPositionMethodPtr;
	public IntPtr __Entities_SetRotationMethodPtr;
	public IntPtr __Entities_SetScaleMethodPtr;
	public IntPtr __Entities_SetNameMethodPtr;
	public IntPtr __Entities_GetPositionMethodPtr;
	public IntPtr __Entities_GetRotationMethodPtr;
	public IntPtr __Entities_GetScaleMethodPtr;
	public IntPtr __Entities_GetNameMethodPtr;
	public IntPtr __Entities_SetModelMethodPtr;
	public IntPtr __Entities_SetCameraPositionMethodPtr;
	public IntPtr __Entities_GetCameraPositionMethodPtr;
	public IntPtr __Entities_SetCameraRotationMethodPtr;
	public IntPtr __Entities_GetCameraRotationMethodPtr;
	public IntPtr __Entities_SetCameraFieldOfViewMethodPtr;
	public IntPtr __Entities_GetCameraFieldOfViewMethodPtr;
	public IntPtr __Entities_SetCameraZNearMethodPtr;
	public IntPtr __Entities_GetCameraZNearMethodPtr;
	public IntPtr __Entities_SetCameraZFarMethodPtr;
	public IntPtr __Entities_GetCameraZFarMethodPtr;
	public IntPtr __Entities_SetCubePhysicsMethodPtr;
	public IntPtr __Entities_SetSpherePhysicsMethodPtr;
	public IntPtr __Entities_SetMeshPhysicsMethodPtr;
	public IntPtr __Entities_SetVelocityMethodPtr;
	public IntPtr __Entities_GetVelocityMethodPtr;
	public IntPtr __Entities_GetMassMethodPtr;
	public IntPtr __Entities_GetFrictionMethodPtr;
	public IntPtr __Entities_GetRestitutionMethodPtr;
	public IntPtr __Entities_SetMassMethodPtr;
	public IntPtr __Entities_SetFrictionMethodPtr;
	public IntPtr __Entities_SetRestitutionMethodPtr;
	public IntPtr __Entities_GetIgnoreRigidbodyPositionMethodPtr;
	public IntPtr __Entities_GetIgnoreRigidbodyRotationMethodPtr;
	public IntPtr __Entities_SetIgnoreRigidbodyPositionMethodPtr;
	public IntPtr __Entities_SetIgnoreRigidbodyRotationMethodPtr;
	public IntPtr __ConsoleSystem_RunMethodPtr;
	public IntPtr __Input_IsButtonDownMethodPtr;
	public IntPtr __Input_GetMousePositionMethodPtr;
	public IntPtr __Input_GetMouseDeltaMethodPtr;
	public IntPtr __Input_IsKeyDownMethodPtr;
	public IntPtr __Physics_TraceMethodPtr;
}
