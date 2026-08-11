{
  description = "Minimal test case for NixFodExporter";

  inputs = {
    nixpkgs.url = "github:nixos/nixpkgs/nixos-unstable";
  };

  outputs = { nixpkgs, ... }:
    let
      system = "x86_64-linux";
      pkgs = nixpkgs.legacyPackages.${system};
    in {
      packages.${system} = {
        flatFod = pkgs.fetchurl {
          url = "https://raw.githubusercontent.com/NixOS/nix/master/README.md";
          sha256 = "sha256-5zQ7fQCHY1R4YKoy+HpYDJWBkSPdB8nLP+mkfw0jwV8=";
        };
        recursiveFod = pkgs.fetchzip {
          url = "https://github.com/NixOS/patchelf/archive/refs/tags/0.18.0.tar.gz";
          sha256 = "sha256-5zQ7fQCHY1R4YKoy+HpYDJWBkSPdB8nLP+mkfw0jwV8=";
        };
      };
    };
}