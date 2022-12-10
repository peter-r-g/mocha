#include "root.h"

#include <defs.h>
#include <edict.h>
#include <globalvars.h>
#include <logmanager.h>
#include <managed/hostmanager.h>
#include <renderdocmanager.h>
#include <vulkan/rendermanager.h>
#include <physicsmanager.h>

//
// These global variables are all defined in globalvars.h,
// because the naming makes more sense (imagine if we
// included Root.h everywhere!)
//

VmaAllocator* g_allocator; // Ugly duckling

RenderManager* g_renderManager;
LogManager* g_logManager;
HostManager* g_hostManager;
RenderdocManager* g_renderdocManager;
EntityManager* g_entityDictionary;
PhysicsManager* g_physicsManager;

float g_curTime;
float g_frameTime;
Vector3 g_cameraPos;
Quaternion g_cameraRot;
float g_cameraFov;
float g_cameraZNear;
float g_cameraZFar;

void Root::Startup()
{
	// TODO: How do we start up g_allocator like this?
	//		 Should we have a wrapper around VmaAllocator?
	//		 Should it be part of a 'RenderSystem'?

	g_logManager = new LogManager();
	g_logManager->Startup();

#if __ATTACH_RENDERDOC
	g_renderdocManager = new RenderdocManager();
	g_renderdocManager->Startup();
#endif

	g_entityDictionary = new EntityManager();
	g_entityDictionary->Startup();

	g_physicsManager = new PhysicsManager();
	g_physicsManager->Startup();

	g_renderManager = new RenderManager();
	g_renderManager->Startup();

	g_hostManager = new HostManager( MANAGED_PATH, MANAGED_CLASS );
	g_hostManager->Startup();
}

void Root::Shutdown()
{
	g_hostManager->Shutdown();
	g_renderManager->Shutdown();
	g_physicsManager->Shutdown();
	g_entityDictionary->Shutdown();

#if __ATTACH_RENDERDOC
	g_renderdocManager->Shutdown();
#endif

	g_logManager->Shutdown();
}

void Root::Run()
{
	g_renderManager->Run();
}