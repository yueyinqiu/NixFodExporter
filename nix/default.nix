{
  lib,
  buildDotnetModule,
  fetchFromGitHub,
  dotnetCorePackages,
}:

buildDotnetModule (finalAttrs: {
  pname = "nix-fod-exporter";
  version = "0.0.1";

  src = fetchFromGitHub {
    owner = "yueyinqiu";
    repo = "NixFodExporter";
    rev = "v${finalAttrs.version}";
    hash = "sha256-TA/MUSjoe5mknXxqkW8jpcgpoC7164mhoR4Ansx7tRM=";
  };

  projectFile = "src/NixFodExporter/NixFodExporter.csproj";
  dotnet-sdk = dotnetCorePackages.sdk_10_0;

  nugetDeps = ./deps.nix;

  strictDeps = true;
  __structuredAttrs = true;

  meta = {
    description = "";
    homepage = "https://github.com/yueyinqiu/NixFodExporter";
    license = lib.licenses.mit;
    mainProgram = "NixFodExporter";
    maintainers = [ ];
  };
})
