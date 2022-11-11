#pragma once
#include "vk_types.h"

namespace vkinit
{
	inline VkPipelineShaderStageCreateInfo PipelineShaderStageCreateInfo(
	    VkShaderStageFlagBits stage, VkShaderModule shaderModule )
	{
		VkPipelineShaderStageCreateInfo shaderStageInfo{};
		shaderStageInfo.sType = VK_STRUCTURE_TYPE_PIPELINE_SHADER_STAGE_CREATE_INFO;
		shaderStageInfo.pNext = nullptr;

		shaderStageInfo.stage = stage;
		shaderStageInfo.module = shaderModule;
		shaderStageInfo.pName = "main";

		return shaderStageInfo;
	}

	inline VkPipelineVertexInputStateCreateInfo PipelineVertexInputStateCreateInfo()
	{
		VkPipelineVertexInputStateCreateInfo vertexInputInfo{};
		vertexInputInfo.sType = VK_STRUCTURE_TYPE_PIPELINE_VERTEX_INPUT_STATE_CREATE_INFO;
		vertexInputInfo.pNext = nullptr;

		vertexInputInfo.vertexBindingDescriptionCount = 0;
		vertexInputInfo.vertexAttributeDescriptionCount = 0;

		return vertexInputInfo;
	}

	inline VkPipelineInputAssemblyStateCreateInfo PipelineInputAssemblyStateCreateInfo( VkPrimitiveTopology topology )
	{
		VkPipelineInputAssemblyStateCreateInfo inputAssembly{};
		inputAssembly.sType = VK_STRUCTURE_TYPE_PIPELINE_INPUT_ASSEMBLY_STATE_CREATE_INFO;
		inputAssembly.pNext = nullptr;

		inputAssembly.topology = topology;
		inputAssembly.primitiveRestartEnable = VK_FALSE;

		return inputAssembly;
	}

	inline VkPipelineRasterizationStateCreateInfo PipelineRasterizationStateCreateInfo( VkPolygonMode polygonMode )
	{
		VkPipelineRasterizationStateCreateInfo rasterizer{};
		rasterizer.sType = VK_STRUCTURE_TYPE_PIPELINE_RASTERIZATION_STATE_CREATE_INFO;
		rasterizer.pNext = nullptr;

		rasterizer.depthClampEnable = VK_FALSE;
		rasterizer.rasterizerDiscardEnable = VK_FALSE;
		rasterizer.polygonMode = polygonMode;
		rasterizer.lineWidth = 1.0f;
		rasterizer.cullMode = VK_CULL_MODE_NONE;
		rasterizer.frontFace = VK_FRONT_FACE_CLOCKWISE;
		rasterizer.depthBiasEnable = VK_FALSE;
		rasterizer.depthBiasConstantFactor = 0.0f;
		rasterizer.depthBiasClamp = 0.0f;
		rasterizer.depthBiasSlopeFactor = 0.0f;

		return rasterizer;
	}

	inline VkPipelineMultisampleStateCreateInfo PipelineMultisampleStateCreateInfo()
	{
		VkPipelineMultisampleStateCreateInfo multisampling{};
		multisampling.sType = VK_STRUCTURE_TYPE_PIPELINE_MULTISAMPLE_STATE_CREATE_INFO;
		multisampling.pNext = nullptr;

		multisampling.sampleShadingEnable = VK_FALSE;
		multisampling.rasterizationSamples = VK_SAMPLE_COUNT_1_BIT;
		multisampling.minSampleShading = 1.0f;
		multisampling.pSampleMask = nullptr;
		multisampling.alphaToCoverageEnable = VK_FALSE;
		multisampling.alphaToOneEnable = VK_FALSE;

		return multisampling;
	}

	inline VkPipelineColorBlendAttachmentState PipelineColorBlendAttachmentState()
	{
		VkPipelineColorBlendAttachmentState colorBlendAttachment{};
		colorBlendAttachment.colorWriteMask =
		    VK_COLOR_COMPONENT_R_BIT | VK_COLOR_COMPONENT_G_BIT | VK_COLOR_COMPONENT_B_BIT | VK_COLOR_COMPONENT_A_BIT;
		colorBlendAttachment.blendEnable = VK_FALSE;

		return colorBlendAttachment;
	}

	inline VkPipelineLayoutCreateInfo PipelineLayoutCreateInfo()
	{
		VkPipelineLayoutCreateInfo pipelineLayoutInfo{};
		pipelineLayoutInfo.sType = VK_STRUCTURE_TYPE_PIPELINE_LAYOUT_CREATE_INFO;
		pipelineLayoutInfo.pNext = nullptr;

		pipelineLayoutInfo.flags = 0;
		pipelineLayoutInfo.setLayoutCount = 0;
		pipelineLayoutInfo.pSetLayouts = nullptr;
		pipelineLayoutInfo.pushConstantRangeCount = 0;
		pipelineLayoutInfo.pPushConstantRanges = nullptr;

		return pipelineLayoutInfo;
	}

	inline VkRenderingAttachmentInfo RenderingAttachmentInfo( VkImageView imageView )
	{
		VkRenderingAttachmentInfo renderingAttachmentInfo = {};
		renderingAttachmentInfo.sType = VK_STRUCTURE_TYPE_RENDERING_ATTACHMENT_INFO;
		renderingAttachmentInfo.pNext = nullptr;

		renderingAttachmentInfo.imageLayout = VK_IMAGE_LAYOUT_COLOR_ATTACHMENT_OPTIMAL;
		renderingAttachmentInfo.resolveMode = VK_RESOLVE_MODE_NONE;
		renderingAttachmentInfo.loadOp = VK_ATTACHMENT_LOAD_OP_CLEAR;
		renderingAttachmentInfo.storeOp = VK_ATTACHMENT_STORE_OP_STORE;
		renderingAttachmentInfo.imageView = imageView;
		renderingAttachmentInfo.clearValue = { { { 0.0f, 0.0f, 0.0f } } };

		return renderingAttachmentInfo;
	}

	inline VkRenderingInfo RenderingInfo( VkRenderingAttachmentInfo renderingAttachmentInfo, VkExtent2D extent )
	{
		VkRenderingInfo renderInfo = {};
		renderInfo.sType = VK_STRUCTURE_TYPE_RENDERING_INFO;
		renderInfo.pNext = nullptr;
		renderInfo.layerCount = 1;
		renderInfo.renderArea = VkRect2D{ VkOffset2D{}, extent };
		renderInfo.colorAttachmentCount = 1;
		renderInfo.pColorAttachments = &renderingAttachmentInfo;

		return renderInfo;
	}

	inline VkSubmitInfo SubmitInfo( VkPipelineStageFlags* waitStage, VkSemaphore* presentSemaphore,
	    VkSemaphore* renderSemaphore, VkCommandBuffer* commandBuffer )
	{
		VkSubmitInfo submit = {};
		submit.sType = VK_STRUCTURE_TYPE_SUBMIT_INFO;
		submit.pNext = nullptr;

		submit.pWaitDstStageMask = waitStage;

		submit.waitSemaphoreCount = 1;
		submit.pWaitSemaphores = presentSemaphore;

		submit.signalSemaphoreCount = 1;
		submit.pSignalSemaphores = renderSemaphore;

		submit.commandBufferCount = 1;
		submit.pCommandBuffers = commandBuffer;

		return submit;
	}

	inline VkPresentInfoKHR PresentInfo( VkSwapchainKHR* swapchain, VkSemaphore* waitSemaphore, uint32_t* swapchainImageIndex )
	{
		VkPresentInfoKHR presentInfo = {};
		presentInfo.sType = VK_STRUCTURE_TYPE_PRESENT_INFO_KHR;
		presentInfo.pNext = nullptr;

		presentInfo.pSwapchains = swapchain;
		presentInfo.swapchainCount = 1;

		presentInfo.pWaitSemaphores = waitSemaphore;
		presentInfo.waitSemaphoreCount = 1;

		presentInfo.pImageIndices = swapchainImageIndex;

		return presentInfo;
	}

	inline VkCommandBufferBeginInfo CommandBufferBeginInfo()
	{
		VkCommandBufferBeginInfo cmdBeginInfo = {};
		
		cmdBeginInfo.sType = VK_STRUCTURE_TYPE_COMMAND_BUFFER_BEGIN_INFO;
		cmdBeginInfo.pNext = nullptr;
		cmdBeginInfo.pInheritanceInfo = nullptr;
		cmdBeginInfo.flags = VK_COMMAND_BUFFER_USAGE_ONE_TIME_SUBMIT_BIT;

		return cmdBeginInfo;
	}
} // namespace vkinit