#include <globalvars.h>
#include <iostream>
#include <root.h>
#include <Windows.h>

int APIENTRY WinMain( HINSTANCE hInst, HINSTANCE hInstPrev, PSTR cmdline, int cmdshow )
{
	g_executingRealm = REALM_CLIENT;

	auto& root = Root::GetInstance();

	root.Startup();
	root.Run();
	root.Shutdown();

	return 0;
}