#include "window.h"

#include <defs.h>
#include <globalvars.h>
#include <inputmanager.h>

#ifdef _IMGUI
#include <backends/imgui_impl_sdl.h>
#include <imgui.h>
#endif



Window::Window( uint32_t width, uint32_t height )
{
	SDL_Init( SDL_INIT_VIDEO );

	SDL_WindowFlags windowFlags = ( SDL_WindowFlags )( SDL_WINDOW_VULKAN | SDL_WINDOW_RESIZABLE );

	m_window = SDL_CreateWindow( GAME_NAME, SDL_WINDOWPOS_UNDEFINED, SDL_WINDOWPOS_UNDEFINED, width, height, windowFlags );
}

VkSurfaceKHR Window::CreateSurface( VkInstance instance )
{
	VkSurfaceKHR surface;
	SDL_Vulkan_CreateSurface( m_window, instance, &surface );
	return surface;
}

void Window::Cleanup()
{
	SDL_DestroyWindow( m_window );
}

bool Window::Update()
{
	SDL_Event e;
	bool bQuit = false;

	InputState inputState = g_inputManager->GetState();

	// Clear mouse delta every frame
	inputState.mouseDelta = { 0, 0 };

	SDL_SetRelativeMouseMode( m_captureMouse ? SDL_TRUE : SDL_FALSE );

	while ( SDL_PollEvent( &e ) != 0 )
	{
		if ( e.type == SDL_QUIT )
		{
			return true;
		}
		else if ( e.type == SDL_WINDOWEVENT )
		{
			SDL_WindowEvent we = e.window;

			if ( we.event == SDL_WINDOWEVENT_SIZE_CHANGED )
			{
				auto width = we.data1;
				auto height = we.data2;

				spdlog::info( "Window was resized to {}x{}", width, height );

				// Push event so that renderer etc. knows we've resized the window
				VkExtent2D windowExtents = { width, height };
				m_onWindowResized( windowExtents );
			}
		}
		else if ( e.type == SDL_MOUSEBUTTONDOWN || e.type == SDL_MOUSEBUTTONUP )
		{
			SDL_MouseButtonEvent mbe = e.button;

			bool isDown = mbe.state == SDL_PRESSED;

			if ( inputState.buttons.size() <= mbe.button )
				inputState.buttons.resize( mbe.button + 1 );

			inputState.buttons[mbe.button] = isDown;
		}
		else if ( e.type == SDL_KEYDOWN || e.type == SDL_KEYUP )
		{
			SDL_KeyboardEvent kbe = e.key;

			bool isDown = kbe.state == SDL_PRESSED;
			int scanCode = kbe.keysym.scancode;

			if ( inputState.keys.size() <= scanCode )
				inputState.keys.resize( scanCode + 1 );

			inputState.keys[scanCode] = isDown;

			if ( kbe.keysym.scancode == SDL_SCANCODE_F10 && isDown )
				m_captureMouse = !m_captureMouse;
		}
		else if ( e.type == SDL_MOUSEMOTION )
		{
			SDL_MouseMotionEvent mme = e.motion;

			inputState.mousePosition = { ( float )mme.x, ( float )mme.y };
			inputState.mouseDelta = { ( float )mme.xrel, ( float )mme.yrel };
		}

#ifdef _IMGUI
		// Pipe event to imgui too
		ImGui_ImplSDL2_ProcessEvent( &e );
#endif
	}

	g_inputManager->SetState( inputState );

	return false;
}
