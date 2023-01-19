using Mocha.Common;

namespace Mocha.AssetCompiler;

internal struct TextureMetadata
{
	public TextureFormat Format { get; set; } = TextureFormat.BC3;

	public TextureMetadata() { }
}
